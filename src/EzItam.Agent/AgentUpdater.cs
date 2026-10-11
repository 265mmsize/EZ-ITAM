using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using EzItam.Agent.Transport;
using Microsoft.Extensions.Options;

namespace EzItam.Agent;

/// <summary>
/// 서버가 알려 준 패키지를 내려받아 SHA-256 을 검증하고, 교체 스크립트를 분리된 프로세스로 실행합니다.
/// 교체 스크립트는 서비스를 멈추고 파일을 바꾼 뒤 다시 시작하며, 시작에 실패하면 백업으로 되돌립니다.
/// 설치(최초 배포)는 이 클래스의 범위가 아닙니다.
/// </summary>
public sealed class AgentUpdater
{
    private const string ServiceName = "EzItamAgent";
    private const string ExeName = "EzItam.Agent.exe";

    private readonly AgentApiClient _api;
    private readonly AgentStateStore _store;
    private readonly AgentOptions _options;
    private readonly AgentHistoryStore _history;
    private readonly ILogger<AgentUpdater> _logger;

    public AgentUpdater(AgentApiClient api, AgentStateStore store, AgentHistoryStore history, IOptions<AgentOptions> options, ILogger<AgentUpdater> logger)
    {
        _api = api;
        _store = store;
        _history = history;
        _options = options.Value;
        _logger = logger;
    }

    private string UpdateDir => Path.Combine(_options.ResolveStateDirectory(), "update");

    /// <returns>교체 스크립트를 실행했으면 true (곧 서비스가 재시작됩니다).</returns>
    public async Task<bool> TryApplyAsync(AgentState state, string targetVersion, string expectedSha256, CancellationToken ct)
    {
        if (!OperatingSystem.IsWindows())
        {
            _logger.LogInformation("Windows 가 아니므로 업데이트 적용을 건너뜁니다.");
            return false;
        }
        if (state.LastUpdateAttemptVersion == targetVersion) return false;

        _history.Add("Update", "Downloading", targetVersion);
        var dir = UpdateDir;
        if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        Directory.CreateDirectory(dir);

        var zip = Path.Combine(dir, "package.zip");
        await _api.DownloadUpdateAsync(state.Token!, zip, ct);

        // 해시가 다르면 같은 파일을 반복 받지 않도록 이 버전은 시도 완료로 기록합니다.
        var actual = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(zip, ct)));
        if (!string.Equals(actual, expectedSha256, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("업데이트 패키지 해시가 일치하지 않습니다. {Version} 은 적용하지 않습니다.", targetVersion);
            _history.Add("Update", "HashMismatch", targetVersion);
            MarkAttempted(state, targetVersion);
            return false;
        }

        var staging = Path.Combine(dir, "staging");
        ZipFile.ExtractToDirectory(zip, staging);
        if (!File.Exists(Path.Combine(staging, ExeName)))
        {
            _logger.LogError("패키지에 {Exe} 가 없습니다. {Version} 은 적용하지 않습니다.", ExeName, targetVersion);
            _history.Add("Update", "Invalid", targetVersion);
            MarkAttempted(state, targetVersion);
            return false;
        }

        var script = Path.Combine(dir, "apply-update.ps1");
        await File.WriteAllTextAsync(script, ApplyScript, ct);

        var install = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        var backup = Path.Combine(dir, "backup");
        MarkAttempted(state, targetVersion);

        // 서비스가 멈추면 이 프로세스도 종료되므로, 스크립트는 독립 프로세스로 띄웁니다.
        var psi = new ProcessStartInfo("powershell.exe")
        {
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var arg in new[] { "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", script,
                     "-ServiceName", ServiceName, "-Staging", staging, "-Install", install, "-Backup", backup })
            psi.ArgumentList.Add(arg);
        Process.Start(psi);
        _history.Add("Update", "Scheduled", $"{AgentOptions.AgentVersion} → {targetVersion}");
        _logger.LogInformation("업데이트 스크립트를 실행했습니다. 대상 버전 {Version}", targetVersion);
        return true;
    }

    private void MarkAttempted(AgentState state, string version)
    {
        state.LastUpdateAttemptVersion = version;
        _store.Save(state);
    }

    // 설치 폴더의 appsettings*.json 은 운영 설정(서버 주소 등)이므로 덮어쓰지 않습니다.
    private const string ApplyScript = """
        param([string]$ServiceName, [string]$Staging, [string]$Install, [string]$Backup)
        $ErrorActionPreference = 'Stop'
        $log = Join-Path (Split-Path $Backup -Parent) 'update.log'
        function Log([string]$m) { "$(Get-Date -Format s) $m" | Add-Content $log }
        function Wait-Status([string]$want, [int]$sec) {
            $end = (Get-Date).AddSeconds($sec)
            while ((Get-Date) -lt $end) {
                if ((Get-Service $ServiceName).Status -eq $want) { return $true }
                Start-Sleep -Seconds 1
            }
            return $false
        }
        function Copy-Files([string]$src, [string]$dst, [bool]$keepSettings) {
            Get-ChildItem $src -Recurse -File | Where-Object { -not ($keepSettings -and $_.Name -like 'appsettings*') } | ForEach-Object {
                $rel = $_.FullName.Substring($src.Length).TrimStart('\')
                $target = Join-Path $dst $rel
                New-Item -ItemType Directory -Force (Split-Path $target -Parent) | Out-Null
                Copy-Item $_.FullName $target -Force
            }
        }
        try {
            Log 'update start'
            Start-Sleep -Seconds 3
            Stop-Service $ServiceName -Force
            if (-not (Wait-Status 'Stopped' 60)) { throw 'service did not stop' }
            if (Test-Path $Backup) { Remove-Item $Backup -Recurse -Force }
            Copy-Item $Install $Backup -Recurse -Force
            Copy-Files $Staging $Install $true
            Start-Service $ServiceName
            Start-Sleep -Seconds 20
            if ((Get-Service $ServiceName).Status -ne 'Running') { throw 'service not running after update' }
            Log 'update ok'
        } catch {
            Log "update failed: $_ ; rolling back"
            try {
                Stop-Service $ServiceName -Force -ErrorAction SilentlyContinue
                Wait-Status 'Stopped' 60 | Out-Null
                if (Test-Path $Backup) { Copy-Files $Backup $Install $false }
                Start-Service $ServiceName
                Log 'rollback done'
            } catch {
                Log "rollback failed: $_"
            }
        }
        """;
}
