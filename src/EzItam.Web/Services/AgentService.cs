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
        var isNew = agent is null;
        agent ??= new Agent { DeviceUuid = req.DeviceUuid, EnrolledAt = DateTime.UtcNow, Status = AgentStatus.Pending };
        ApplyEnrollFields(agent, req);

        var token = CreateToken();
        agent.TokenHash = Hash(token);
        agent.LastSeenAt = DateTime.UtcNow;

        if (isNew) _db.Agents.Add(agent);
        await _db.SaveChangesAsync(ct);
        return new EnrollResponse(agent.Id, token, agent.Status.ToString());
    }

    public async Task<Agent?> AuthenticateAsync(string? token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        var hash = Hash(token);
        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.TokenHash == hash, ct);
        return agent is not null && FixedEquals(agent.TokenHash!, hash) ? agent : null;
    }

    public async Task<HeartbeatResponse> HeartbeatAsync(Agent agent, HeartbeatRequest req, CancellationToken ct = default)
    {
        agent.LastSeenAt = DateTime.UtcNow;
        agent.AgentVersion = req.AgentVersion;
        agent.BootTimeUtc = req.BootTimeUtc;
        agent.RebootPending = req.RebootPending;
        if (!string.IsNullOrWhiteSpace(req.IpAddress)) agent.IpAddress = req.IpAddress;
        await _db.SaveChangesAsync(ct);

        var inventoryDue = agent.Status == AgentStatus.Approved
            && (agent.LastInventoryAt is null || agent.LastInventoryAt < DateTime.UtcNow.AddHours(-InventoryIntervalHours));
        var interval = _config.GetValue("Agent:HeartbeatSeconds", DefaultHeartbeatSeconds);
        return new HeartbeatResponse(interval, inventoryDue, agent.Status.ToString());
    }

    public async Task ApplyInventoryAsync(Agent agent, InventoryPayload payload, CancellationToken ct = default)
    {
        var hw = payload.Hardware;
        agent.SerialNumber = hw.SerialNumber;
        agent.HostName = hw.HostName;
        agent.Manufacturer = hw.Manufacturer;
        agent.Model = hw.Model;
        agent.CpuName = hw.CpuName;
        agent.CpuCores = hw.CpuCores;
        agent.TotalMemoryMb = hw.TotalMemoryMb;
        agent.OsName = hw.OsName;
        agent.OsVersion = hw.OsVersion;
        agent.OsBuild = hw.OsBuild;
        agent.LoggedOnUser = hw.LoggedOnUser;
        agent.MacAddresses = string.Join(",", hw.MacAddresses);
        agent.IpAddress = hw.IpAddress;
        agent.BitLockerOn = hw.BitLockerOn;
        agent.DiskTotalGb = hw.DiskTotalGb;
        agent.LastInventoryAt = payload.CollectedAtUtc;
        agent.LastSeenAt = DateTime.UtcNow;

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
                Name = Truncate(sw.Name.Trim(), 300),
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
        await _db.SaveChangesAsync(ct);
    }

    public async Task DisableAsync(int agentId, CancellationToken ct = default)
    {
        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.Id == agentId, ct)
            ?? throw new DomainException("에이전트를 찾을 수 없습니다.");
        agent.Status = AgentStatus.Disabled;
        agent.TokenHash = null;
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
        asset.MacAddress ??= agent.MacAddresses?.Split(',').FirstOrDefault();
        asset.UpdatedBy = _currentUser.UserName;
        asset.UpdatedAt = DateTime.Now;
    }

    private static void ApplyEnrollFields(Agent agent, EnrollRequest req)
    {
        agent.SerialNumber = req.SerialNumber;
        agent.HostName = req.HostName;
        agent.Manufacturer = req.Manufacturer;
        agent.Model = req.Model;
        agent.OsName = req.OsName;
        agent.OsVersion = req.OsVersion;
        agent.MacAddresses = string.Join(",", req.MacAddresses);
        agent.IpAddress = req.IpAddress;
        agent.AgentVersion = req.AgentVersion;
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
