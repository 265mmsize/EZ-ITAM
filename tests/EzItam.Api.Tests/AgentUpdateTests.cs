using EzItam.Api.Models.Entities;
using EzItam.Api.Services;
using EzItam.Contracts;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EzItam.Tests;

/// <summary>테스트용 빈 배포 카탈로그</summary>
internal sealed class NoUpdates : IUpdateCatalog
{
    public UpdateManifest? GetManifest(AgentRing ring) => null;
    public string? GetPackagePath(AgentRing ring) => null;
}

internal sealed class FixedUpdates(UpdateManifest manifest) : IUpdateCatalog
{
    public UpdateManifest? GetManifest(AgentRing ring) => manifest;
    public string? GetPackagePath(AgentRing ring) => null;
}

public class AgentUpdateTests : IDisposable
{
    private readonly TestDb _testDb = new();
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "ezitam-tests-" + Guid.NewGuid().ToString("N"));
    private readonly IConfiguration _config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["Agent:EnrollmentKey"] = "test-key" })
        .Build();

    public void Dispose()
    {
        _testDb.Dispose();
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    private AgentService Svc(IUpdateCatalog updates) => new(_testDb.Create(), _config, _testDb.CurrentUser, updates);

    private static EnrollRequest Enroll(string version)
        => new("test-key", "UUID-U", "SN-U", "PC-U", "Dell", "Latitude", "Windows 11", "10.0", new[] { "AA-BB" }, "10.0.0.7", version);

    [Fact]
    public async Task Heartbeat_offers_update_only_to_approved_agents_on_an_older_version()
    {
        var updates = new FixedUpdates(new UpdateManifest("1.0.1", "ABCDEF", "EzItam.Agent-1.0.1.zip"));
        var resp = await Svc(updates).EnrollAsync(Enroll("1.0.0"));
        var agent = (await Svc(updates).AuthenticateAsync(resp.Token))!;

        var before = await Svc(updates).HeartbeatAsync(agent, new HeartbeatRequest("1.0.0", null, false, null));
        Assert.Null(before.TargetVersion);

        await Svc(updates).ApproveAsync(agent.Id);
        agent = (await Svc(updates).AuthenticateAsync(resp.Token))!;
        var hb = await Svc(updates).HeartbeatAsync(agent, new HeartbeatRequest("1.0.0", null, false, null));
        Assert.Equal("1.0.1", hb.TargetVersion);
        Assert.Equal("ABCDEF", hb.UpdateSha256);

        var upToDate = await Svc(updates).HeartbeatAsync(agent, new HeartbeatRequest("1.0.1", null, false, null));
        Assert.Null(upToDate.TargetVersion);
    }

    [Fact]
    public void Catalog_rejects_manifest_with_path_in_file_name()
    {
        var ring = Path.Combine(_dir, "Stable");
        Directory.CreateDirectory(ring);
        File.WriteAllText(Path.Combine(ring, "manifest.json"),
            """{"Version":"9.9.9","Sha256":"00","FileName":"..\\..\\Windows\\evil.zip"}""");

        Assert.Null(new UpdateCatalog(_dir).GetManifest(AgentRing.Stable));
    }

    [Fact]
    public void Catalog_reads_valid_manifest_and_package_path()
    {
        var ring = Path.Combine(_dir, "Pilot");
        Directory.CreateDirectory(ring);
        File.WriteAllText(Path.Combine(ring, "manifest.json"),
            """{"Version":"1.0.1","Sha256":"ABC","FileName":"EzItam.Agent-1.0.1.zip"}""");

        var catalog = new UpdateCatalog(_dir);
        Assert.Equal("1.0.1", catalog.GetManifest(AgentRing.Pilot)!.Version);
        Assert.Equal(Path.Combine(ring, "EzItam.Agent-1.0.1.zip"), catalog.GetPackagePath(AgentRing.Pilot));
        Assert.Null(catalog.GetManifest(AgentRing.Stable));
    }
}
