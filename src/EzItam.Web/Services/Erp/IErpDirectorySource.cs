namespace EzItam.Web.Services.Erp;

/// <summary>ERP(Oracle EBS 11i)에서 조직/사원 마스터를 읽어오는 소스</summary>
public interface IErpDirectorySource
{
    bool IsAvailable { get; }
    Task<IReadOnlyList<ErpDepartment>> GetDepartmentsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ErpEmployee>> GetEmployeesAsync(CancellationToken ct = default);
}
