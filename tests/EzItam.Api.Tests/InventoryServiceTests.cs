using EzItam.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Tests;

public sealed class InventoryServiceTests : IDisposable
{
    private readonly TestDb _testDb = new();

    [Fact]
    public async Task CreateInventory_persists_inventory_and_returns_its_id()
    {
        var service = new InventoryService(_testDb.Create(), _testDb.CurrentUser);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var id = await service.CreateInventoryAsync(new CreateInventoryDto
        {
            Name = "연간 자산 조사",
            DateFrom = today,
            DateTo = today.AddDays(30)
        });

        using var db = _testDb.Create();
        var inventory = await db.Inventories.SingleAsync(item => item.Id == id);
        Assert.True(id > 0);
        Assert.Equal("연간 자산 조사", inventory.Name);
    }

    [Fact]
    public async Task DeleteInventory_for_missing_id_returns_false()
    {
        var service = new InventoryService(_testDb.Create(), _testDb.CurrentUser);

        Assert.False(await service.DeleteInventoryAsync(999));
    }

    public void Dispose() => _testDb.Dispose();
}
