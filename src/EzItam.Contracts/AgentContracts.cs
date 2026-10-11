namespace EzItam.Contracts;

/// <summary>에이전트 → 서버: 최초 등록 요청. 서버는 Pending 상태로 저장하고 토큰을 발급합니다.</summary>
public sealed record EnrollRequest(
    string EnrollmentKey,
    string DeviceUuid,
    string? SerialNumber,
    string HostName,
    string? Manufacturer,
    string? Model,
    string? OsName,
    string? OsVersion,
    string[] MacAddresses,
    string? IpAddress,
    string AgentVersion);

public sealed record EnrollResponse(int AgentId, string Token, string Status);

public sealed record HeartbeatRequest(string AgentVersion, DateTime? BootTimeUtc, bool RebootPending, string? IpAddress);

/// <summary>서버 → 에이전트: 다음 접속 간격과 인벤토리 전송 필요 여부. 서버가 주기를 제어합니다.</summary>
public sealed record HeartbeatResponse(
    int NextHeartbeatSeconds,
    bool InventoryDue,
    string Status,
    string? TargetVersion = null,
    string? UpdateSha256 = null);

/// <summary>업데이트 패키지 매니페스트. 링 폴더의 manifest.json 과 같은 형식입니다.</summary>
public sealed record UpdateManifest(string Version, string Sha256, string FileName);

public sealed record InventoryPayload(
    DateTime CollectedAtUtc,
    HardwareInfo Hardware,
    IReadOnlyList<SoftwareItem> Software);

public sealed record HardwareInfo(
    string DeviceUuid,
    string? SerialNumber,
    string HostName,
    string? Manufacturer,
    string? Model,
    string? CpuName,
    int? CpuCores,
    long? TotalMemoryMb,
    string? OsName,
    string? OsVersion,
    string? OsBuild,
    string? LoggedOnUser,
    string[] MacAddresses,
    string? IpAddress,
    bool? BitLockerOn,
    long? DiskTotalGb);

public sealed record SoftwareItem(string Name, string? Publisher, string? Version, string? InstallDate, string Source);
