using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EzItam.Api.Models.Entities;


/// <summary>PC 에이전트 등록 정보. 자산(Asset)과 1:1로 연결될 수 있습니다.</summary>
public class Agent
{
    public int Id { get; set; }

    [StringLength(64)]
    public string DeviceUuid { get; set; } = string.Empty;

    [StringLength(100)]
    public string? SerialNumber { get; set; }

    [StringLength(100)]
    public string HostName { get; set; } = string.Empty;

    [StringLength(100)] public string? Manufacturer { get; set; }
    [StringLength(100)] public string? Model { get; set; }
    [StringLength(100)] public string? CpuName { get; set; }
    public int? CpuCores { get; set; }
    public long? TotalMemoryMb { get; set; }
    [StringLength(100)] public string? OsName { get; set; }
    [StringLength(100)] public string? OsVersion { get; set; }
    [StringLength(20)] public string? OsBuild { get; set; }
    [StringLength(100)] public string? LoggedOnUser { get; set; }
    [StringLength(300)] public string? MacAddresses { get; set; }
    [StringLength(100)] public string? IpAddress { get; set; }
    public bool? BitLockerOn { get; set; }
    public long? DiskTotalGb { get; set; }

    [StringLength(32)]
    public string AgentVersion { get; set; } = string.Empty;

    public AgentStatus Status { get; set; } = AgentStatus.Pending;

    /// <summary>토큰 원문은 저장하지 않고 SHA-256 해시만 저장합니다.</summary>
    [StringLength(64)]
    public string? TokenHash { get; set; }

    public DateTime EnrolledAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime? LastInventoryAt { get; set; }
    public DateTime? BootTimeUtc { get; set; }
    public bool RebootPending { get; set; }

    public int? AssetId { get; set; }
    public Asset? Asset { get; set; }

    public ICollection<AgentSoftware> Software { get; set; } = new List<AgentSoftware>();

    [NotMapped]
    public bool IsOnline => Status != AgentStatus.Disabled && LastSeenAt.HasValue && LastSeenAt.Value > DateTime.Now.AddMinutes(-15);
}

/// <summary>에이전트가 보고한 설치 소프트웨어 (최신 인벤토리 기준 전체 교체)</summary>
public class AgentSoftware
{
    public long Id { get; set; }
    public int AgentId { get; set; }
    public Agent? Agent { get; set; }

    [StringLength(300)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Publisher { get; set; }

    [StringLength(100)]
    public string? Version { get; set; }

    [StringLength(20)]
    public string? InstallDate { get; set; }

    [StringLength(20)]
    public string Source { get; set; } = string.Empty;
}
