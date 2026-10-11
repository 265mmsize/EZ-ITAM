using System.Security.Cryptography;
using System.Text;
using EzItam.Contracts;
using EzItam.Web.Data;
using EzItam.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Web.Services;

public interface IAgentService
{
    Task<EnrollResponse> EnrollAsync(EnrollRequest request, CancellationToken ct = default);
    Task<Agent?> AuthenticateAsync(string? token, CancellationToken ct = default);
    Task<HeartbeatResponse> HeartbeatAsync(Agent agent, HeartbeatRequest request, CancellationToken ct = default);
    Task ApplyInventoryAsync(Agent agent, InventoryPayload payload, CancellationToken ct = default);
    Task ApproveAsync(int agentId, CancellationToken ct = default);
    Task DisableAsync(int agentId, CancellationToken ct = default);
    Task LinkAssetAsync(int agentId, int? assetId, CancellationToken ct = default);
    Task<Asset?> FindMatchingAssetAsync(Agent agent, CancellationToken ct = default);
}

public class AgentService : IAgentService
{
    /// <summary>에이전트가 서버에 접속하는 기본 주기(분). 서버가 응답으로 조정할 수 있습니다.</summary>
    public const int DefaultHeartbeatSeconds = 300;
    public const int InventoryIntervalHours = 24;

    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly ICurrentUserService _currentUser;

    public AgentService(AppDbContext db, IConfiguration config, ICurrentUserService currentUser)
    {
        _db = db;
        _config = config;
        _currentUser = currentUser;
    }

    public async Task<EnrollResponse> EnrollAsync(EnrollRequest req, CancellationToken ct = default)
    {
        var expected = _config["Agent:EnrollmentKey"];
        if (string.IsNullOrWhiteSpace(expected) || !FixedEquals(req.EnrollmentKey, expected))
            throw new UnauthorizedAccessException("등록 키가 올바르지 않습니다.");
        if (string.IsNullOrWhiteSpace(req.DeviceUuid))
            throw new DomainException("장치 UUID가 필요합니다.");

        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.DeviceUuid == req.DeviceUuid, ct);
        if (agent is not null && agent.Status == AgentStatus.Disabled)
            throw new UnauthorizedAccessException("사용 중지된 장치입니다. 관리자가 다시 승인해야 합니다.");

        var isNew = agent is null;
        agent ??= new Agent { DeviceUuid = Truncate(req.DeviceUuid, 64)!, EnrolledAt = DateTime.Now, Status = AgentStatus.Pending };
        ApplyEnrollFields(agent, req);

        var token = CreateToken();
        agent.TokenHash = Hash(token);
        agent.LastSeenAt = DateTime.Now;

        if (isNew) _db.Agents.Add(agent);
        await _db.SaveChangesAsync(ct);
        return new EnrollResponse(agent.Id, token, agent.Status.ToString());
    }

    public async Task<Agent?> AuthenticateAsync(string? token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        var hash = Hash(token);
        return await _db.Agents.FirstOrDefaultAsync(a => a.TokenHash == hash, ct);
    }

    public async Task<HeartbeatResponse> HeartbeatAsync(Agent agent, HeartbeatRequest req, CancellationToken ct = default)
    {
        Track(agent);
        agent.LastSeenAt = DateTime.Now;
        agent.AgentVersion = Truncate(req.AgentVersion, 32) ?? string.Empty;
        agent.BootTimeUtc = req.BootTimeUtc;
        agent.RebootPending = req.RebootPending;
        if (!string.IsNullOrWhiteSpace(req.IpAddress)) agent.IpAddress = Truncate(req.IpAddress, 100);
        await _db.SaveChangesAsync(ct);

        var inventoryDue = agent.Status == AgentStatus.Approved
            && (agent.LastInventoryAt is null || agent.LastInventoryAt < DateTime.Now.AddHours(-InventoryIntervalHours));
        var interval = _config.GetValue("Agent:HeartbeatSeconds", DefaultHeartbeatSeconds);
        return new HeartbeatResponse(interval, inventoryDue, agent.Status.ToString());
    }

    public async Task ApplyInventoryAsync(Agent agent, InventoryPayload payload, CancellationToken ct = default)
    {
        Track(agent);
        var hw = payload.Hardware;
        agent.SerialNumber = Truncate(hw.SerialNumber, 100);
        agent.HostName = Truncate(hw.HostName, 100) ?? agent.HostName;
        agent.Manufacturer = Truncate(hw.Manufacturer, 100);
        agent.Model = Truncate(hw.Model, 100);
        agent.CpuName = Truncate(hw.CpuName, 100);
        agent.CpuCores = hw.CpuCores;
        agent.TotalMemoryMb = hw.TotalMemoryMb;
        agent.OsName = Truncate(hw.OsName, 100);
        agent.OsVersion = Truncate(hw.OsVersion, 100);
        agent.OsBuild = Truncate(hw.OsBuild, 20);
        agent.LoggedOnUser = Truncate(hw.LoggedOnUser, 100);
        agent.MacAddresses = JoinMacs(hw.MacAddresses);
        agent.IpAddress = Truncate(hw.IpAddress, 100);
        agent.BitLockerOn = hw.BitLockerOn;
        agent.DiskTotalGb = hw.DiskTotalGb;
        // 클라이언트 시계 오차에 영향받지 않도록 서버 시각을 기록합니다.
        agent.LastInventoryAt = DateTime.Now;
        agent.LastSeenAt = DateTime.Now;

        // 설치 소프트웨어는 최신 인벤토리 기준으로 전체 교체합니다.
        var old = await _db.AgentSoftware.Where(s => s.AgentId == agent.Id).ToListAsync(ct);
        _db.AgentSoftware.RemoveRange(old);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var sw in payload.Software)
        {
            if (string.IsNullOrWhiteSpace(sw.Name)) continue;
            var key = $"{sw.Name.Trim()}|{sw.Version}";
            if (!seen.Add(key)) continue;
            _db.AgentSoftware.Add(new AgentSoftware
            {
                AgentId = agent.Id,
                Name = Truncate(sw.Name.Trim(), 300)!,
                Publisher = Truncate(sw.Publisher, 200),
                Version = Truncate(sw.Version, 100),
                InstallDate = Truncate(sw.InstallDate, 20),
                Source = Truncate(sw.Source, 20) ?? "Registry"
            });
        }

        if (agent.AssetId is null)
        {
            var match = await FindMatchingAssetAsync(agent, ct);
            if (match is not null) agent.AssetId = match.Id;
        }
        if (agent.AssetId is not null)
            await SyncAssetFromAgentAsync(agent, ct);

        await _db.SaveChangesAsync(ct);
    }

    public async Task ApproveAsync(int agentId, CancellationToken ct = default)
    {
        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.Id == agentId, ct)
            ?? throw new DomainException("에이전트를 찾을 수 없습니다.");
        agent.Status = AgentStatus.Approved;
        if (agent.AssetId is null)
        {
            var match = await FindMatchingAssetAsync(agent, ct);
            if (match is not null) agent.AssetId = match.Id;
        }
        Audit(agent, "Approve");
        await _db.SaveChangesAsync(ct);
    }

    public async Task DisableAsync(int agentId, CancellationToken ct = default)
    {
        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.Id == agentId, ct)
            ?? throw new DomainException("에이전트를 찾을 수 없습니다.");
        agent.Status = AgentStatus.Disabled;
        agent.TokenHash = null;
        Audit(agent, "Disable");
        await _db.SaveChangesAsync(ct);
    }

    public async Task LinkAssetAsync(int agentId, int? assetId, CancellationToken ct = default)
    {
        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.Id == agentId, ct)
            ?? throw new DomainException("에이전트를 찾을 수 없습니다.");
        if (assetId is not null)
        {
            if (await _db.Agents.AnyAsync(a => a.AssetId == assetId && a.Id != agentId, ct))
                throw new DomainException("해당 자산은 이미 다른 에이전트와 연결되어 있습니다.");
            if (!await _db.Assets.AnyAsync(a => a.Id == assetId, ct))
                throw new DomainException("자산을 찾을 수 없습니다.");
        }
        agent.AssetId = assetId;
        if (assetId is not null) await SyncAssetFromAgentAsync(agent, ct);
        Audit(agent, assetId is null ? "Unlink" : $"Link:{assetId}");
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>매칭 우선순위: 시리얼번호 → 호스트명. 이미 다른 에이전트가 쓰는 자산은 제외합니다.</summary>
    public async Task<Asset?> FindMatchingAssetAsync(Agent agent, CancellationToken ct = default)
    {
        var taken = _db.Agents.Where(a => a.AssetId != null).Select(a => a.AssetId);

        if (!string.IsNullOrWhiteSpace(agent.SerialNumber))
        {
            var bySerial = await _db.Assets
                .FirstOrDefaultAsync(a => a.SerialNumber == agent.SerialNumber && !taken.Contains(a.Id), ct);
            if (bySerial is not null) return bySerial;
        }
        if (!string.IsNullOrWhiteSpace(agent.HostName))
        {
            return await _db.Assets
                .FirstOrDefaultAsync(a => a.HostName == agent.HostName && !taken.Contains(a.Id), ct);
        }
        return null;
    }

    /// <summary>자산 대장에 값이 비어 있는 항목만 에이전트 값으로 채웁니다. 기존 값은 덮어쓰지 않습니다.</summary>
    private async Task SyncAssetFromAgentAsync(Agent agent, CancellationToken ct)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == agent.AssetId, ct);
        if (asset is null) return;
        asset.HostName ??= agent.HostName;
        asset.IpAddress ??= agent.IpAddress;
        asset.OperatingSystem ??= agent.OsName;
        asset.Manufacturer ??= agent.Manufacturer;
        asset.Model ??= agent.Model;
        asset.SerialNumber ??= agent.SerialNumber;
        asset.MacAddress ??= agent.MacAddresses?.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        // UpdatedAt/UpdatedBy 는 AppDbContext 가 저장 시 자동으로 기록합니다.
    }

    /// <summary>에이전트는 heartbeat 마다 바뀌어 자동 감사 대상에서 제외했으므로, 관리자 조작만 직접 기록합니다.</summary>
    private void Audit(Agent agent, string action)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(Agent),
            EntityKey = agent.Id.ToString(),
            Action = AuditAction.Updated,
            Changes = System.Text.Json.JsonSerializer.Serialize(new { Action = new { New = action }, Host = new { New = agent.HostName } }),
            UserName = _currentUser.UserName,
            Timestamp = DateTime.Now
        });
    }

    private static string JoinMacs(IEnumerable<string>? macs)
    {
        var joined = string.Join(",", (macs ?? Array.Empty<string>()).Where(m => !string.IsNullOrWhiteSpace(m)));
        return Truncate(joined, 300) ?? string.Empty;
    }

    private static void ApplyEnrollFields(Agent agent, EnrollRequest req)
    {
        agent.SerialNumber = Truncate(req.SerialNumber, 100);
        agent.HostName = Truncate(req.HostName, 100) ?? string.Empty;
        agent.Manufacturer = Truncate(req.Manufacturer, 100);
        agent.Model = Truncate(req.Model, 100);
        agent.OsName = Truncate(req.OsName, 100);
        agent.OsVersion = Truncate(req.OsVersion, 100);
        agent.MacAddresses = JoinMacs(req.MacAddresses);
        agent.IpAddress = Truncate(req.IpAddress, 100);
        agent.AgentVersion = Truncate(req.AgentVersion, 32) ?? string.Empty;
    }

    /// <summary>다른 컨텍스트에서 읽어 온(추적되지 않는) 엔티티도 변경이 저장되도록 현재 컨텍스트에 연결합니다.</summary>
    private void Track(Agent agent)
    {
        if (_db.Entry(agent).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
            _db.Agents.Attach(agent);
    }

    private static string CreateToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string Hash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    private static bool FixedEquals(string a, string b)
        => System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));

    private static string? Truncate(string? s, int max)
        => s is null ? null : (s.Length <= max ? s : s[..max]);
}
