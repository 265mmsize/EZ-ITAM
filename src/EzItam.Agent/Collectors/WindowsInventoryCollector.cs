using System.Runtime.Versioning;
using EzItam.Contracts;
using Microsoft.Win32;
using System.Management;

namespace EzItam.Agent.Collectors;

/// <summary>WMI 와 레지스트리로 실제 Windows PC 정보를 수집합니다. 관리자 권한(LocalSystem 서비스)을 전제로 합니다.</summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsInventoryCollector : IInventoryCollector
{
    private static readonly string[] UninstallRoots =
    {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    };

    public Task<HardwareInfo> CollectHardwareAsync(CancellationToken ct) => Task.Run(() =>
    {
        var uuid = FirstString("SELECT UUID FROM Win32_ComputerSystemProduct", "UUID") ?? string.Empty;
        var cs = FirstObject("SELECT Name, Manufacturer, Model, TotalPhysicalMemory, UserName FROM Win32_ComputerSystem");
        var os = FirstObject("SELECT Caption, Version, BuildNumber FROM Win32_OperatingSystem");
        var bios = FirstString("SELECT SerialNumber FROM Win32_BIOS", "SerialNumber");

        var cpus = QueryRows("SELECT Name, NumberOfCores FROM Win32_Processor");
        var cpu = cpus.FirstOrDefault();
        var cores = cpus.Sum(r => ToInt(r["NumberOfCores"]));

        var nics = QueryRows("SELECT MACAddress, IPAddress FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = TRUE").ToList();
        var macs = nics.Select(n => n["MACAddress"]?.ToString()).Where(m => !string.IsNullOrWhiteSpace(m)).Select(m => m!).ToArray();
        var ip = nics.SelectMany(n => n["IPAddress"] as string[] ?? Array.Empty<string>())
            .FirstOrDefault(a => a.Contains('.'));

        var diskBytes = QueryRows("SELECT Size FROM Win32_DiskDrive").Sum(r => ToLong(r["Size"]));

        return new HardwareInfo(
            DeviceUuid: uuid,
            SerialNumber: string.IsNullOrWhiteSpace(bios) ? null : bios,
            HostName: Environment.MachineName,
            Manufacturer: cs?["Manufacturer"]?.ToString()?.Trim(),
            Model: cs?["Model"]?.ToString()?.Trim(),
            CpuName: cpu?["Name"]?.ToString()?.Trim(),
            CpuCores: cores > 0 ? cores : null,
            TotalMemoryMb: cs?["TotalPhysicalMemory"] is null ? null : ToLong(cs["TotalPhysicalMemory"]) / (1024 * 1024),
            OsName: os?["Caption"]?.ToString()?.Trim(),
            OsVersion: os?["Version"]?.ToString(),
            OsBuild: os?["BuildNumber"]?.ToString(),
            LoggedOnUser: cs?["UserName"]?.ToString(),
            MacAddresses: macs,
            IpAddress: ip,
            BitLockerOn: ReadBitLocker(),
            DiskTotalGb: diskBytes > 0 ? diskBytes / (1024L * 1024 * 1024) : null);
    }, ct);

    public Task<IReadOnlyList<SoftwareItem>> CollectSoftwareAsync(CancellationToken ct) => Task.Run(() =>
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var list = new List<SoftwareItem>();

        foreach (var root in UninstallRoots)
        {
            ReadUninstall(Registry.LocalMachine, root, list, seen);
        }
        ReadUninstall(Registry.CurrentUser, UninstallRoots[0], list, seen);
        return (IReadOnlyList<SoftwareItem>)list.OrderBy(s => s.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
    }, ct);

    private static void ReadUninstall(RegistryKey hive, string path, List<SoftwareItem> list, HashSet<string> seen)
    {
        using var key = hive.OpenSubKey(path);
        if (key is null) return;
        foreach (var subName in key.GetSubKeyNames())
        {
            using var sub = key.OpenSubKey(subName);
            if (sub is null) continue;
            var name = sub.GetValue("DisplayName") as string;
            if (string.IsNullOrWhiteSpace(name)) continue;
            if (Convert.ToInt32(sub.GetValue("SystemComponent") ?? 0) == 1) continue;
            if (sub.GetValue("ParentKeyName") is not null) continue; // 업데이트 하위 항목 제외

            var version = sub.GetValue("DisplayVersion") as string;
            if (!seen.Add($"{name.Trim()}|{version}")) continue;

            var msi = Convert.ToInt32(sub.GetValue("WindowsInstaller") ?? 0) == 1;
            list.Add(new SoftwareItem(
                Name: name.Trim(),
                Publisher: (sub.GetValue("Publisher") as string)?.Trim(),
                Version: version,
                InstallDate: sub.GetValue("InstallDate") as string,
                Source: msi ? "MSI" : "Registry"));
        }
    }

    private static bool? ReadBitLocker()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\CIMV2\Security\MicrosoftVolumeEncryption",
                "SELECT ProtectionStatus FROM Win32_EncryptableVolume WHERE DriveLetter = 'C:'");
            foreach (ManagementObject mo in searcher.Get())
            {
                return Convert.ToInt32(mo["ProtectionStatus"] ?? 0) == 1;
            }
            return null;
        }
        catch (ManagementException)
        {
            return null;
        }
    }

    private static List<Dictionary<string, object?>> QueryRows(string wql)
    {
        var rows = new List<Dictionary<string, object?>>();
        try
        {
            using var searcher = new ManagementObjectSearcher(wql);
            foreach (ManagementObject mo in searcher.Get())
            {
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var p in mo.Properties) row[p.Name] = p.Value;
                rows.Add(row);
            }
        }
        catch (ManagementException)
        {
            // WMI 개별 조회 실패는 전체 수집을 막지 않습니다.
        }
        return rows;
    }

    private static Dictionary<string, object?>? FirstObject(string wql) => QueryRows(wql).FirstOrDefault();

    private static string? FirstString(string wql, string column)
        => FirstObject(wql)?[column]?.ToString();

    private static int ToInt(object? v) => v is null ? 0 : Convert.ToInt32(v);
    private static long ToLong(object? v) => v is null ? 0 : Convert.ToInt64(v);
}
