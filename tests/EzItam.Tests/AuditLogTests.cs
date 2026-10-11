using System.Text.Json;
using EzItam.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EzItam.Tests;

public class AuditLogTests : IDisposable
{
    private readonly TestDb _testDb = new();

    [Fact]
    public async Task Update_writes_audit_log_with_old_and_new_values()
    {
        using (var db = _testDb.Create())
        {
            var asset = await db.Assets.SingleAsync();
            asset.Name = "변경된 이름";
            await db.SaveChangesAsync();
        }

        using var verify = _testDb.Create();
        var log = await verify.AuditLogs
            .Where(l => l.EntityName == nameof(Asset) && l.Action == AuditAction.Updated)
            .OrderByDescending(l => l.Id).FirstAsync();

        Assert.Equal("tester", log.UserName);
        using var doc = JsonDocument.Parse(log.Changes!);
        var name = doc.RootElement.GetProperty("Name");
        Assert.Equal("테스트 노트북", name.GetProperty("Old").GetString());
        Assert.Equal("변경된 이름", name.GetProperty("New").GetString());
        Assert.False(doc.RootElement.TryGetProperty("UpdatedAt", out _));
    }

    [Fact]
    public async Task Create_and_delete_write_audit_logs()
    {
        int id;
        using (var db = _testDb.Create())
        {
            var v = new Vendor { Name = "테스트업체" };
            db.Vendors.Add(v);
            await db.SaveChangesAsync();
            id = v.Id;
            db.Vendors.Remove(v);
            await db.SaveChangesAsync();
        }

        using var verify = _testDb.Create();
        var actions = await verify.AuditLogs
            .Where(l => l.EntityName == nameof(Vendor) && l.EntityKey == id.ToString())
            .OrderBy(l => l.Id).Select(l => l.Action).ToListAsync();
        Assert.Equal(new[] { AuditAction.Created, AuditAction.Deleted }, actions);
    }

    [Fact]
    public async Task Unchanged_save_writes_no_update_log()
    {
        using var db = _testDb.Create();
        var before = await db.AuditLogs.CountAsync();
        var asset = await db.Assets.SingleAsync();
        asset.Name = asset.Name;
        await db.SaveChangesAsync();
        Assert.Equal(before, await db.AuditLogs.CountAsync());
    }

    public void Dispose() => _testDb.Dispose();
}
