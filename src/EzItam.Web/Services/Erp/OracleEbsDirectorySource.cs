using System.Data;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;

namespace EzItam.Web.Services.Erp;

/// <summary>ODP.NET Core 로 Oracle EBS 11i HR 테이블을 직접 조회하는 구현</summary>
public class OracleEbsDirectorySource : IErpDirectorySource
{
    private readonly OracleEbsOptions _options;
    private readonly ILogger<OracleEbsDirectorySource> _logger;

    public OracleEbsDirectorySource(IOptions<OracleEbsOptions> options, ILogger<OracleEbsDirectorySource> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool IsAvailable => _options.Enabled && !string.IsNullOrWhiteSpace(_options.ConnectionString);

    public async Task<IReadOnlyList<ErpDepartment>> GetDepartmentsAsync(CancellationToken ct = default)
    {
        var list = new List<ErpDepartment>();
        await using var conn = new OracleConnection(_options.ConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = _options.DepartmentQuery;
        cmd.CommandTimeout = _options.CommandTimeoutSeconds;
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(new ErpDepartment(
                OrganizationId: Convert.ToInt64(reader["ORGANIZATION_ID"]),
                Code: reader["ORG_CODE"]?.ToString()?.Trim() ?? string.Empty,
                Name: reader["ORG_NAME"]?.ToString()?.Trim() ?? string.Empty,
                IsActive: string.Equals(reader["ACTIVE_FLAG"]?.ToString(), "Y", StringComparison.OrdinalIgnoreCase)));
        }
        _logger.LogInformation("Oracle EBS 부서 {Count}건 조회", list.Count);
        return list;
    }

    public async Task<IReadOnlyList<ErpEmployee>> GetEmployeesAsync(CancellationToken ct = default)
    {
        var list = new List<ErpEmployee>();
        await using var conn = new OracleConnection(_options.ConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = _options.EmployeeQuery;
        cmd.CommandTimeout = _options.CommandTimeoutSeconds;
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var orgRaw = reader["ORGANIZATION_ID"];
            list.Add(new ErpEmployee(
                PersonId: Convert.ToInt64(reader["PERSON_ID"]),
                EmployeeNo: reader["EMPLOYEE_NUMBER"]?.ToString()?.Trim() ?? string.Empty,
                Name: reader["FULL_NAME"]?.ToString()?.Trim() ?? string.Empty,
                Email: NullIfEmpty(reader["EMAIL_ADDRESS"]),
                Title: NullIfEmpty(reader["JOB_NAME"]),
                OrganizationId: orgRaw is null or DBNull ? null : Convert.ToInt64(orgRaw),
                IsActive: string.Equals(reader["ACTIVE_FLAG"]?.ToString(), "Y", StringComparison.OrdinalIgnoreCase)));
        }
        _logger.LogInformation("Oracle EBS 사원 {Count}건 조회", list.Count);
        return list;
    }

    private static string? NullIfEmpty(object? value)
    {
        if (value is null or DBNull) return null;
        var s = value.ToString()?.Trim();
        return string.IsNullOrEmpty(s) ? null : s;
    }
}
