using EzItam.Web.Data;
using EzItam.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EzItam.Web.Services.Erp;

public interface IErpSyncService
{
    bool IsAvailable { get; }
    ErpSyncResult? LastResult { get; }
    Task<ErpSyncResult> SyncAsync(CancellationToken ct = default);
}

/// <summary>ERP 조직/사원 마스터를 ITAM 부서/사원 테이블에 업서트합니다.</summary>
public class ErpSyncService : IErpSyncService
{
    private static ErpSyncResult? _lastResult;

    private readonly AppDbContext _db;
    private readonly IErpDirectorySource _source;
    private readonly OracleEbsOptions _options;
    private readonly ILogger<ErpSyncService> _logger;

    public ErpSyncService(AppDbContext db, IErpDirectorySource source, IOptions<OracleEbsOptions> options, ILogger<ErpSyncService> logger)
    {
        _db = db;
        _source = source;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsAvailable => _source.IsAvailable;
    public ErpSyncResult? LastResult => _lastResult;

    public async Task<ErpSyncResult> SyncAsync(CancellationToken ct = default)
    {
        var result = new ErpSyncResult();
        try
        {
            if (!_source.IsAvailable)
                throw new DomainException("ERP 연동이 설정되지 않았습니다. appsettings의 OracleEbs 섹션을 확인하세요.");

            var erpDepts = await _source.GetDepartmentsAsync(ct);
            var erpEmps = await _source.GetEmployeesAsync(ct);

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            // 1) 부서
            var depts = await _db.Departments.ToListAsync(ct);
            var deptByErpId = depts.Where(d => d.ErpOrganizationId.HasValue).ToDictionary(d => d.ErpOrganizationId!.Value);
            var deptByCode = depts.ToDictionary(d => d.Code, StringComparer.OrdinalIgnoreCase);

            foreach (var ed in erpDepts.Where(d => !string.IsNullOrWhiteSpace(d.Code)))
            {
                if (deptByErpId.TryGetValue(ed.OrganizationId, out var dept) || deptByCode.TryGetValue(ed.Code, out dept))
                {
                    var changed = dept.Name != ed.Name || dept.IsActive != ed.IsActive || dept.ErpOrganizationId != ed.OrganizationId || dept.Code != ed.Code;
                    if (changed)
                    {
                        dept.Name = ed.Name;
                        dept.IsActive = ed.IsActive;
                        dept.ErpOrganizationId = ed.OrganizationId;
                        if (!deptByCode.ContainsKey(ed.Code)) dept.Code = ed.Code;
                        result.DepartmentsUpdated++;
                    }
                }
                else
                {
                    dept = new Department { Code = ed.Code, Name = ed.Name, IsActive = ed.IsActive, ErpOrganizationId = ed.OrganizationId };
                    _db.Departments.Add(dept);
                    deptByCode[ed.Code] = dept;
                    result.DepartmentsInserted++;
                }
                deptByErpId[ed.OrganizationId] = dept;
            }
            await _db.SaveChangesAsync(ct);

            // 2) 사원
            var emps = await _db.Employees.ToListAsync(ct);
            var empByErpId = emps.Where(e => e.ErpPersonId.HasValue).ToDictionary(e => e.ErpPersonId!.Value);
            var empByNo = emps.ToDictionary(e => e.EmployeeNo, StringComparer.OrdinalIgnoreCase);
            var seen = new HashSet<int>();

            foreach (var ee in erpEmps.Where(e => !string.IsNullOrWhiteSpace(e.EmployeeNo)))
            {
                int? deptId = ee.OrganizationId.HasValue && deptByErpId.TryGetValue(ee.OrganizationId.Value, out var d) ? d.Id : null;

                if (empByErpId.TryGetValue(ee.PersonId, out var emp) || empByNo.TryGetValue(ee.EmployeeNo, out emp))
                {
                    var changed = emp.Name != ee.Name || emp.Email != ee.Email || emp.Title != ee.Title
                        || emp.DepartmentId != deptId || emp.IsActive != ee.IsActive || emp.ErpPersonId != ee.PersonId;
                    if (changed)
                    {
                        emp.Name = ee.Name;
                        emp.Email = ee.Email;
                        emp.Title = ee.Title;
                        emp.DepartmentId = deptId;
                        emp.IsActive = ee.IsActive;
                        emp.ErpPersonId = ee.PersonId;
                        result.EmployeesUpdated++;
                    }
                }
                else
                {
                    emp = new Employee
                    {
                        EmployeeNo = ee.EmployeeNo, Name = ee.Name, Email = ee.Email, Title = ee.Title,
                        DepartmentId = deptId, IsActive = ee.IsActive, ErpPersonId = ee.PersonId
                    };
                    _db.Employees.Add(emp);
                    empByNo[ee.EmployeeNo] = emp;
                    result.EmployeesInserted++;
                }
                empByErpId[ee.PersonId] = emp;
                if (emp.Id != 0) seen.Add(emp.Id);
            }

            if (_options.DeactivateMissingEmployees && erpEmps.Count > 0)
            {
                foreach (var emp in emps.Where(e => e.ErpPersonId.HasValue && e.IsActive && !seen.Contains(e.Id)))
                {
                    emp.IsActive = false;
                    result.EmployeesDeactivated++;
                }
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            _logger.LogInformation("ERP 동기화 완료: 부서 +{DI}/~{DU}, 사원 +{EI}/~{EU}/-{ED}",
                result.DepartmentsInserted, result.DepartmentsUpdated, result.EmployeesInserted, result.EmployeesUpdated, result.EmployeesDeactivated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERP 동기화 실패");
            result.Error = ex.Message;
        }
        finally
        {
            result.FinishedAt = DateTime.Now;
            _lastResult = result;
        }
        return result;
    }
}
