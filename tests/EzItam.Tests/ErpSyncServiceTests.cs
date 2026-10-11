using EzItam.Web.Models.Entities;
using EzItam.Web.Services.Erp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EzItam.Tests;

public class ErpSyncServiceTests : IDisposable
{
    private readonly TestDb _testDb = new();

    private sealed class FakeSource : IErpDirectorySource
    {
        public List<ErpDepartment> Departments { get; } = new();
        public List<ErpEmployee> Employees { get; } = new();
        public bool IsAvailable => true;
        public Task<IReadOnlyList<ErpDepartment>> GetDepartmentsAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<ErpDepartment>>(Departments);
        public Task<IReadOnlyList<ErpEmployee>> GetEmployeesAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<ErpEmployee>>(Employees);
    }

    private ErpSyncService CreateService(FakeSource source, bool deactivate = true)
        => new(_testDb.Create(), source, Options.Create(new OracleEbsOptions { Enabled = true, ConnectionString = "x", DeactivateMissingEmployees = deactivate }), NullLogger<ErpSyncService>.Instance);

    [Fact]
    public async Task Sync_upserts_departments_and_employees_and_deactivates_missing()
    {
        var source = new FakeSource();
        source.Departments.Add(new ErpDepartment(100, "IT", "정보시스템팀(ERP)", true));   // 기존 부서 코드 매칭 → 갱신
        source.Departments.Add(new ErpDepartment(200, "RND", "연구소", true));           // 신규
        source.Employees.Add(new ErpEmployee(1, "E1", "김철수", "cs@corp.com", "과장", 100, true)); // 기존 사번 매칭 → 갱신
        source.Employees.Add(new ErpEmployee(3, "E3", "신입사원", null, "사원", 200, true));        // 신규

        // E2 를 ERP 연동 사원으로 표시해 두면, ERP 결과에 없으므로 퇴직 처리되어야 함
        using (var db = _testDb.Create())
        {
            var e2 = await db.Employees.SingleAsync(e => e.EmployeeNo == "E2");
            e2.ErpPersonId = 2;
            await db.SaveChangesAsync();
        }

        var result = await CreateService(source).SyncAsync();

        Assert.True(result.Succeeded, result.Error);
        Assert.Equal(1, result.DepartmentsInserted);
        Assert.Equal(1, result.DepartmentsUpdated);
        Assert.Equal(1, result.EmployeesInserted);
        Assert.Equal(1, result.EmployeesUpdated);
        Assert.Equal(1, result.EmployeesDeactivated);

        using var verify = _testDb.Create();
        var it = await verify.Departments.SingleAsync(d => d.Code == "IT");
        Assert.Equal(100, it.ErpOrganizationId);
        Assert.Equal("정보시스템팀(ERP)", it.Name);
        var rnd = await verify.Departments.SingleAsync(d => d.Code == "RND");
        var e3 = await verify.Employees.SingleAsync(e => e.EmployeeNo == "E3");
        Assert.Equal(rnd.Id, e3.DepartmentId);
        Assert.Equal(3, e3.ErpPersonId);
        Assert.False((await verify.Employees.SingleAsync(e => e.EmployeeNo == "E2")).IsActive);
        Assert.Equal("cs@corp.com", (await verify.Employees.SingleAsync(e => e.EmployeeNo == "E1")).Email);
    }

    [Fact]
    public async Task Sync_is_idempotent()
    {
        var source = new FakeSource();
        source.Departments.Add(new ErpDepartment(200, "RND", "연구소", true));
        source.Employees.Add(new ErpEmployee(3, "E3", "신입사원", null, null, 200, true));

        await CreateService(source).SyncAsync();
        var second = await CreateService(source).SyncAsync();

        Assert.True(second.Succeeded);
        Assert.Equal(0, second.DepartmentsInserted + second.DepartmentsUpdated + second.EmployeesInserted + second.EmployeesUpdated + second.EmployeesDeactivated);
    }

    public void Dispose() => _testDb.Dispose();
}
