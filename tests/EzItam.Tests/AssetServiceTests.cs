using EzItam.Web.Models.Entities;
using EzItam.Web.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EzItam.Tests;

public class AssetServiceTests : IDisposable
{
    private readonly TestDb _testDb = new();

    private AssetService CreateService() => new(_testDb.Create(), _testDb.CurrentUser);

    [Fact]
    public async Task CheckOut_assigns_employee_and_creates_open_assignment()
    {
        var svc = CreateService();
        var asset = await _testDb.Create().Assets.SingleAsync();
        var emp = await _testDb.Create().Employees.FirstAsync(e => e.EmployeeNo == "E1");

        var assignment = await svc.CheckOutAsync(asset.Id, emp.Id, "신규 지급");

        using var db = _testDb.Create();
        var reloaded = await db.Assets.SingleAsync(a => a.Id == asset.Id);
        Assert.Equal(AssetStatus.Assigned, reloaded.Status);
        Assert.Equal(emp.Id, reloaded.CurrentEmployeeId);
        Assert.Null(assignment.ReturnedAt);
        Assert.Equal("tester", assignment.AssignedBy);
        Assert.Equal(1, await db.AssetAssignments.CountAsync(x => x.AssetId == asset.Id && x.ReturnedAt == null));
    }

    [Fact]
    public async Task CheckOut_twice_is_rejected()
    {
        var svc = CreateService();
        var asset = await _testDb.Create().Assets.SingleAsync();
        var emps = await _testDb.Create().Employees.Where(e => e.IsActive).OrderBy(e => e.EmployeeNo).ToListAsync();

        await svc.CheckOutAsync(asset.Id, emps[0].Id, null);
        var ex = await Assert.ThrowsAsync<DomainException>(() => CreateService().CheckOutAsync(asset.Id, emps[1].Id, null));
        Assert.Contains("이미 사용중", ex.Message);
    }

    [Fact]
    public async Task CheckOut_to_inactive_employee_is_rejected()
    {
        var svc = CreateService();
        var asset = await _testDb.Create().Assets.SingleAsync();
        var retired = await _testDb.Create().Employees.FirstAsync(e => e.EmployeeNo == "E9");

        await Assert.ThrowsAsync<DomainException>(() => svc.CheckOutAsync(asset.Id, retired.Id, null));
    }

    [Fact]
    public async Task CheckIn_closes_assignment_and_updates_status_and_location()
    {
        var asset = await _testDb.Create().Assets.SingleAsync();
        var emp = await _testDb.Create().Employees.FirstAsync(e => e.EmployeeNo == "E1");
        var loc = await _testDb.Create().Locations.SingleAsync();
        await CreateService().CheckOutAsync(asset.Id, emp.Id, null);

        var closed = await CreateService().CheckInAsync(asset.Id, AssetStatus.Idle, loc.Id, "퇴사 반납");

        using var db = _testDb.Create();
        var reloaded = await db.Assets.SingleAsync(a => a.Id == asset.Id);
        Assert.Equal(AssetStatus.Idle, reloaded.Status);
        Assert.Null(reloaded.CurrentEmployeeId);
        Assert.Equal(loc.Id, reloaded.LocationId);
        Assert.NotNull(closed.ReturnedAt);
        Assert.Contains("퇴사 반납", closed.Notes);
        Assert.Equal(0, await db.AssetAssignments.CountAsync(x => x.AssetId == asset.Id && x.ReturnedAt == null));
    }

    [Fact]
    public async Task CheckIn_on_unassigned_asset_is_rejected()
    {
        var asset = await _testDb.Create().Assets.SingleAsync();
        await Assert.ThrowsAsync<DomainException>(() => CreateService().CheckInAsync(asset.Id, AssetStatus.InStock, null, null));
    }

    [Fact]
    public async Task ChangeStatus_cannot_set_assigned_directly()
    {
        var asset = await _testDb.Create().Assets.SingleAsync();
        await Assert.ThrowsAsync<DomainException>(() => CreateService().ChangeStatusAsync(asset.Id, AssetStatus.Assigned, null));
    }

    [Fact]
    public async Task ChangeStatus_appends_reason_to_notes()
    {
        var asset = await _testDb.Create().Assets.SingleAsync();
        await CreateService().ChangeStatusAsync(asset.Id, AssetStatus.Retired, "노후 장비");

        var reloaded = await _testDb.Create().Assets.SingleAsync(a => a.Id == asset.Id);
        Assert.Equal(AssetStatus.Retired, reloaded.Status);
        Assert.Contains("노후 장비", reloaded.Notes);
        Assert.Contains("tester", reloaded.Notes);
    }

    [Fact]
    public async Task Create_with_duplicate_tag_is_rejected()
    {
        var cat = await _testDb.Create().AssetCategories.SingleAsync();
        var ex = await Assert.ThrowsAsync<DomainException>(() => CreateService().CreateAsync(new Asset { AssetTag = "nb-2025-0001", Name = "중복", CategoryId = cat.Id }));
        Assert.Contains("이미 사용 중", ex.Message);
    }

    [Fact]
    public async Task Create_with_employee_sets_assigned_and_records_history()
    {
        var cat = await _testDb.Create().AssetCategories.SingleAsync();
        var emp = await _testDb.Create().Employees.FirstAsync(e => e.EmployeeNo == "E2");

        var created = await CreateService().CreateAsync(new Asset { AssetTag = "NB-2025-0002", Name = "지급 노트북", CategoryId = cat.Id, CurrentEmployeeId = emp.Id });

        using var db = _testDb.Create();
        Assert.Equal(AssetStatus.Assigned, created.Status);
        Assert.Equal(1, await db.AssetAssignments.CountAsync(x => x.AssetId == created.Id && x.EmployeeId == emp.Id));
        Assert.Equal("tester", (await db.Assets.SingleAsync(a => a.Id == created.Id)).CreatedBy);
    }

    [Fact]
    public async Task GenerateAssetTag_increments_within_category_and_year()
    {
        var cat = await _testDb.Create().AssetCategories.SingleAsync();
        var svc = CreateService();
        var year = DateTime.Today.Year;

        var tag1 = await svc.GenerateAssetTagAsync(cat.Id);
        Assert.Equal(year == 2025 ? $"NB-{year}-0002" : $"NB-{year}-0001", tag1);

        await svc.CreateAsync(new Asset { AssetTag = tag1, Name = "A", CategoryId = cat.Id });
        var tag2 = await CreateService().GenerateAssetTagAsync(cat.Id);
        Assert.Equal(int.Parse(tag1[^4..]) + 1, int.Parse(tag2[^4..]));
    }

    [Fact]
    public async Task Delete_assigned_asset_is_rejected()
    {
        var asset = await _testDb.Create().Assets.SingleAsync();
        var emp = await _testDb.Create().Employees.FirstAsync(e => e.EmployeeNo == "E1");
        await CreateService().CheckOutAsync(asset.Id, emp.Id, null);

        await Assert.ThrowsAsync<DomainException>(() => CreateService().DeleteAsync(asset.Id));
    }

    [Fact]
    public async Task Search_filters_by_text_status_and_department()
    {
        var emp = await _testDb.Create().Employees.FirstAsync(e => e.EmployeeNo == "E1");
        var asset = await _testDb.Create().Assets.SingleAsync();
        await CreateService().CheckOutAsync(asset.Id, emp.Id, null);

        var svc = CreateService();
        Assert.Equal(1, (await svc.SearchAsync(new AssetFilter { Search = "철수" })).TotalCount);
        Assert.Equal(1, (await svc.SearchAsync(new AssetFilter { Status = AssetStatus.Assigned })).TotalCount);
        Assert.Equal(0, (await svc.SearchAsync(new AssetFilter { Status = AssetStatus.InStock })).TotalCount);
        Assert.Equal(1, (await svc.SearchAsync(new AssetFilter { DepartmentId = emp.DepartmentId })).TotalCount);
        Assert.Equal(0, (await svc.SearchAsync(new AssetFilter { Search = "없는자산" })).TotalCount);
    }

    [Fact]
    public void BookValue_uses_straight_line_depreciation()
    {
        var asset = new Asset
        {
            PurchasePrice = 3_600_000,
            PurchaseDate = new DateTime(2024, 1, 1),
            Category = new AssetCategory { DepreciationMonths = 36 }
        };
        Assert.Equal(3_600_000m, asset.CurrentBookValue(new DateTime(2024, 1, 1)));
        Assert.Equal(2_400_000m, asset.CurrentBookValue(new DateTime(2025, 1, 1)));
        Assert.Equal(0m, asset.CurrentBookValue(new DateTime(2027, 6, 1)));
    }

    public void Dispose() => _testDb.Dispose();
}
