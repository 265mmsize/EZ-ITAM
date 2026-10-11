using EzItam.Api.Models.Entities;
using EzItam.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EzItam.Tests;

public class LicenseServiceTests : IDisposable
{
    private readonly TestDb _testDb = new();

    private LicenseService CreateService() => new(_testDb.Create(), _testDb.CurrentUser);

    private async Task<int> LicenseId(string name) => (await _testDb.Create().SoftwareLicenses.SingleAsync(l => l.Name == name)).Id;
    private async Task<int> EmployeeId(string no) => (await _testDb.Create().Employees.SingleAsync(e => e.EmployeeNo == no)).Id;

    [Fact]
    public async Task Assign_consumes_seats_and_rejects_when_exhausted()
    {
        var lic = await LicenseId("Office");
        await CreateService().AssignAsync(lic, await EmployeeId("E1"), null, null);
        await CreateService().AssignAsync(lic, await EmployeeId("E2"), null, null);

        var asset = await _testDb.Create().Assets.SingleAsync();
        var ex = await Assert.ThrowsAsync<DomainException>(() => CreateService().AssignAsync(lic, null, asset.Id, null));
        Assert.Contains("잔여 수량", ex.Message);

        var reloaded = await CreateService().GetAsync(lic);
        Assert.Equal(2, reloaded!.UsedSeats);
        Assert.Equal(0, reloaded.AvailableSeats);
    }

    [Fact]
    public async Task Assign_same_target_twice_is_rejected()
    {
        var lic = await LicenseId("Office");
        var emp = await EmployeeId("E1");
        await CreateService().AssignAsync(lic, emp, null, null);
        await Assert.ThrowsAsync<DomainException>(() => CreateService().AssignAsync(lic, emp, null, null));
    }

    [Fact]
    public async Task Assign_requires_employee_or_asset()
    {
        var lic = await LicenseId("Office");
        await Assert.ThrowsAsync<DomainException>(() => CreateService().AssignAsync(lic, null, null, null));
    }

    [Fact]
    public async Task Assign_expired_license_is_rejected()
    {
        var lic = await LicenseId("Expired");
        var emp = await EmployeeId("E1");
        await Assert.ThrowsAsync<DomainException>(() => CreateService().AssignAsync(lic, emp, null, null));
    }

    [Fact]
    public async Task Site_license_has_unlimited_seats()
    {
        var lic = await LicenseId("SiteWide");
        await CreateService().AssignAsync(lic, await EmployeeId("E1"), null, null);
        await CreateService().AssignAsync(lic, await EmployeeId("E2"), null, null);
        var reloaded = await CreateService().GetAsync(lic);
        Assert.Equal(2, reloaded!.UsedSeats);
        Assert.True(reloaded.AvailableSeats > 1000);
    }

    [Fact]
    public async Task Release_frees_seat_and_cannot_release_twice()
    {
        var lic = await LicenseId("Office");
        var a = await CreateService().AssignAsync(lic, await EmployeeId("E1"), null, null);
        await CreateService().ReleaseAsync(a.Id);

        var reloaded = await CreateService().GetAsync(lic);
        Assert.Equal(0, reloaded!.UsedSeats);
        await Assert.ThrowsAsync<DomainException>(() => CreateService().ReleaseAsync(a.Id));
    }

    public void Dispose() => _testDb.Dispose();
}
