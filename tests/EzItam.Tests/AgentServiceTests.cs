using EzItam.Contracts;
using EzItam.Web.Models.Entities;
using EzItam.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EzItam.Tests;

public class AgentServiceTests : IDisposable
{
    private readonly TestDb _testDb = new();
    private readonly IConfiguration _config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["Agent:EnrollmentKey"] = "test-key" })
        .Build();

    private AgentService Svc() => new(_testDb.Create(), _config, _testDb.CurrentUser);

    private static EnrollRequest Enroll(string uuid = "UUID-1", string key = "test-key", string host = "PC-KIM", string? serial = "SN-1")
        => new(key, uuid, serial, host, "Dell", "Latitude", "Windows 11", "10.0", new[] { "AA-BB" }, "10.0.0.5", "1.0.0");

    private static InventoryPayload Inventory(string host, string? serial, params SoftwareItem[] sw)
        => new(DateTime.UtcNow,
            new HardwareInfo("UUID-1", serial, host, "Dell", "Latitude", "i7", 10, 16384, "Windows 11", "10.0", "26100", "kim", new[] { "AA-BB" }, "10.0.0.5", true, 512),
            sw);

    [Fact]
    public async Task Enroll_with_wrong_key_is_rejected()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Svc().EnrollAsync(Enroll(key: "bad")));
    }

    [Fact]
    public async Task Enroll_creates_pending_agent_and_returns_token_that_authenticates()
    {
        var resp = await Svc().EnrollAsync(Enroll());
        Assert.Equal("Pending", resp.Status);

        var agent = await Svc().AuthenticateAsync(resp.Token);
        Assert.NotNull(agent);
        Assert.Equal(AgentStatus.Pending, agent!.Status);

        // 원문 토큰은 저장되지 않고 해시만 저장됩니다.
        using var db = _testDb.Create();
        var stored = await db.Agents.SingleAsync();
        Assert.NotEqual(resp.Token, stored.TokenHash);
    }

    [Fact]
    public async Task Re_enroll_same_device_keeps_identity_and_rotates_token()
    {
        var first = await Svc().EnrollAsync(Enroll());
        var second = await Svc().EnrollAsync(Enroll());

        Assert.Equal(first.AgentId, second.AgentId);
        Assert.NotEqual(first.Token, second.Token);
        Assert.Null(await Svc().AuthenticateAsync(first.Token));
        Assert.NotNull(await Svc().AuthenticateAsync(second.Token));
    }

    [Fact]
    public async Task Inventory_is_not_due_until_approved()
    {
        var resp = await Svc().EnrollAsync(Enroll());
        var agent = (await Svc().AuthenticateAsync(resp.Token))!;

        var hb = await Svc().HeartbeatAsync(agent, new HeartbeatRequest("1.0.0", null, false, null));
        Assert.False(hb.InventoryDue);
        Assert.Equal("Pending", hb.Status);

        await Svc().ApproveAsync(agent.Id);
        agent = (await Svc().AuthenticateAsync(resp.Token))!;
        hb = await Svc().HeartbeatAsync(agent, new HeartbeatRequest("1.0.0", null, false, null));
        Assert.True(hb.InventoryDue);
    }

    [Fact]
    public async Task Inventory_replaces_software_and_deduplicates()
    {
        var resp = await Svc().EnrollAsync(Enroll());
        await Svc().ApproveAsync((await Svc().AuthenticateAsync(resp.Token))!.Id);
        var agent = (await Svc().AuthenticateAsync(resp.Token))!;

        await Svc().ApplyInventoryAsync(agent, Inventory("PC-KIM", "SN-1",
            new SoftwareItem("7-Zip", "Igor Pavlov", "24.08", null, "Registry"),
            new SoftwareItem("7-Zip", "Igor Pavlov", "24.08", null, "Registry"),
            new SoftwareItem("Edge", "Microsoft", "131", null, "Registry")));
        await Svc().ApplyInventoryAsync(agent, Inventory("PC-KIM", "SN-1",
            new SoftwareItem("Edge", "Microsoft", "132", null, "Registry")));

        using var db = _testDb.Create();
        var sw = await db.AgentSoftware.ToListAsync();
        Assert.Single(sw);
        Assert.Equal("132", sw[0].Version);
    }

    [Fact]
    public async Task Inventory_auto_links_asset_by_serial_and_fills_empty_fields_only()
    {
        using (var db = _testDb.Create())
        {
            var asset = await db.Assets.SingleAsync();
            asset.SerialNumber = "SN-1";
            asset.HostName = null;
            asset.Model = "기존 모델";
            await db.SaveChangesAsync();
        }

        var resp = await Svc().EnrollAsync(Enroll());
        await Svc().ApproveAsync((await Svc().AuthenticateAsync(resp.Token))!.Id);
        var agent = (await Svc().AuthenticateAsync(resp.Token))!;
        await Svc().ApplyInventoryAsync(agent, Inventory("PC-KIM", "SN-1"));

        using var verify = _testDb.Create();
        var linked = await verify.Agents.SingleAsync();
        Assert.NotNull(linked.AssetId);
        var asset2 = await verify.Assets.SingleAsync();
        Assert.Equal("PC-KIM", asset2.HostName);          // 빈 값은 채움
        Assert.Equal("기존 모델", asset2.Model);            // 기존 값은 유지
    }

    [Fact]
    public async Task Link_rejects_asset_already_linked_to_another_agent()
    {
        var a1 = await Svc().EnrollAsync(Enroll("UUID-A", host: "A", serial: "X1"));
        var a2 = await Svc().EnrollAsync(Enroll("UUID-B", host: "B", serial: "X2"));
        var asset = await _testDb.Create().Assets.SingleAsync();

        var agent1 = (await Svc().AuthenticateAsync(a1.Token))!;
        var agent2 = (await Svc().AuthenticateAsync(a2.Token))!;
        await Svc().LinkAssetAsync(agent1.Id, asset.Id);
        await Assert.ThrowsAsync<DomainException>(() => Svc().LinkAssetAsync(agent2.Id, asset.Id));
    }

    [Fact]
    public async Task Disable_revokes_token()
    {
        var resp = await Svc().EnrollAsync(Enroll());
        var agent = (await Svc().AuthenticateAsync(resp.Token))!;
        await Svc().DisableAsync(agent.Id);
        Assert.Null(await Svc().AuthenticateAsync(resp.Token));
    }

    [Fact]
    public async Task Disabled_agent_cannot_re_enroll_with_shared_key()
    {
        var resp = await Svc().EnrollAsync(Enroll());
        var agent = (await Svc().AuthenticateAsync(resp.Token))!;
        await Svc().DisableAsync(agent.Id);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Svc().EnrollAsync(Enroll()));
        Assert.Null(await Svc().AuthenticateAsync(resp.Token));
    }

    [Fact]
    public async Task Long_agent_values_are_truncated_to_column_limits()
    {
        var macs = Enumerable.Range(0, 40).Select(i => $"00-11-22-33-44-{i:00}").ToArray();
        var longText = new string('x', 500);
        var req = new EnrollRequest("test-key", "UUID-L", longText, longText, longText, longText, longText, longText, macs, longText, longText);
        var resp = await Svc().EnrollAsync(req);
        await Svc().ApproveAsync((await Svc().AuthenticateAsync(resp.Token))!.Id);
        var agent = (await Svc().AuthenticateAsync(resp.Token))!;
        await Svc().ApplyInventoryAsync(agent, new InventoryPayload(DateTime.UtcNow,
            new HardwareInfo("UUID-L", longText, longText, longText, longText, longText, 4, 8, longText, longText, longText, longText, macs, longText, null, null),
            new[] { new SoftwareItem(new string('n', 400), new string('p', 400), new string('v', 400), "2026", "Registry") }));

        using var db = _testDb.Create();
        var a = await db.Agents.SingleAsync(x => x.DeviceUuid == "UUID-L");
        Assert.True(a.MacAddresses!.Length <= 300);
        Assert.True(a.HostName.Length <= 100 && a.CpuName!.Length <= 100 && a.OsBuild!.Length <= 20 && a.AgentVersion.Length <= 32);
        var sw = await db.AgentSoftware.SingleAsync(x => x.AgentId == a.Id);
        Assert.True(sw.Name.Length <= 300 && sw.Publisher!.Length <= 200 && sw.Version!.Length <= 100);
    }

    [Fact]
    public async Task Heartbeat_and_inventory_do_not_write_audit_logs_but_admin_actions_do()
    {
        var resp = await Svc().EnrollAsync(Enroll());
        var agent = (await Svc().AuthenticateAsync(resp.Token))!;
        int Count() => _testDb.Create().AuditLogs.Count(l => l.EntityName == nameof(Agent));

        await Svc().HeartbeatAsync(agent, new HeartbeatRequest("1.0.0", DateTime.UtcNow, false, "10.0.0.9"));
        Assert.Equal(0, Count());

        await Svc().ApproveAsync(agent.Id);
        Assert.Equal(1, Count());

        agent = (await Svc().AuthenticateAsync(resp.Token))!;
        await Svc().ApplyInventoryAsync(agent, Inventory("PC-KIM", "SN-9"));
        Assert.Equal(1, Count());
    }

    [Fact]
    public async Task Inventory_due_is_computed_from_server_time_not_client_clock()
    {
        var resp = await Svc().EnrollAsync(Enroll());
        await Svc().ApproveAsync((await Svc().AuthenticateAsync(resp.Token))!.Id);
        var agent = (await Svc().AuthenticateAsync(resp.Token))!;

        // 클라이언트 시계가 10년 앞서 있어도 서버 기준으로 "방금 수신"으로 기록되어야 합니다.
        var skewed = Inventory("PC-KIM", "SN-1") with { CollectedAtUtc = DateTime.UtcNow.AddYears(10) };
        await Svc().ApplyInventoryAsync(agent, skewed);

        agent = (await Svc().AuthenticateAsync(resp.Token))!;
        Assert.True(agent.LastInventoryAt <= DateTime.Now.AddMinutes(1));
        var hb = await Svc().HeartbeatAsync(agent, new HeartbeatRequest("1.0.0", null, false, null));
        Assert.False(hb.InventoryDue);
    }

    public void Dispose() => _testDb.Dispose();
}
