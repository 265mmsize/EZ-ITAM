using EzItam.Contracts;
using Microsoft.Extensions.Options;

namespace EzItam.Agent.Collectors;

/// <summary>개발과 프로토콜 검증용. 실제 PC 정보를 읽지 않고 고정된 샘플을 돌려줍니다.</summary>
public sealed class FakeInventoryCollector : IInventoryCollector
{
    private readonly AgentOptions _options;

    public FakeInventoryCollector(IOptions<AgentOptions> options) => _options = options.Value;

    public Task<HardwareInfo> CollectHardwareAsync(CancellationToken ct)
        => Task.FromResult(new HardwareInfo(
            DeviceUuid: _options.FakeDeviceUuid,
            SerialNumber: "FAKE-SN-0001",
            HostName: Environment.MachineName,
            Manufacturer: "Dell Inc.",
            Model: "Latitude 5540",
            CpuName: "Intel(R) Core(TM) i7-1365U",
            CpuCores: 10,
            TotalMemoryMb: 16384,
            OsName: "Microsoft Windows 11 Pro",
            OsVersion: "10.0",
            OsBuild: "26100",
            LoggedOnUser: Environment.UserName,
            MacAddresses: new[] { "00-11-22-33-44-55" },
            IpAddress: "10.0.0.25",
            BitLockerOn: true,
            DiskTotalGb: 512));

    public Task<IReadOnlyList<SoftwareItem>> CollectSoftwareAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<SoftwareItem>>(new List<SoftwareItem>
        {
            new("Microsoft Edge", "Microsoft Corporation", "131.0.2903.70", null, "Registry"),
            new("7-Zip 24.08 (x64)", "Igor Pavlov", "24.08", "20260101", "Registry"),
            new("Visual Studio Community 2022", "Microsoft Corporation", "17.12.0", null, "MSI")
        });
}
