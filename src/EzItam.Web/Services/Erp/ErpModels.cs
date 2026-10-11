namespace EzItam.Web.Services.Erp;

public record ErpDepartment(long OrganizationId, string Code, string Name, bool IsActive);

public record ErpEmployee(long PersonId, string EmployeeNo, string Name, string? Email, string? Title, long? OrganizationId, bool IsActive);

public class ErpSyncResult
{
    public int DepartmentsInserted { get; set; }
    public int DepartmentsUpdated { get; set; }
    public int EmployeesInserted { get; set; }
    public int EmployeesUpdated { get; set; }
    public int EmployeesDeactivated { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.Now;
    public DateTime? FinishedAt { get; set; }
    public string? Error { get; set; }
    public bool Succeeded => Error is null;
}

public class OracleEbsOptions
{
    public const string SectionName = "OracleEbs";

    /// <summary>true 이면 ERP 동기화 메뉴가 활성화됩니다.</summary>
    public bool Enabled { get; set; }

    /// <summary>ODP.NET 연결 문자열. 예: User Id=apps;Password=***;Data Source=//ebs-db:1521/PROD</summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// 부서 조회 SQL. 컬럼: ORGANIZATION_ID, ORG_CODE, ORG_NAME, ACTIVE_FLAG('Y'/'N')
    /// </summary>
    public string DepartmentQuery { get; set; } =
        @"SELECT hou.organization_id AS ORGANIZATION_ID,
                 NVL(hou.attribute1, TO_CHAR(hou.organization_id)) AS ORG_CODE,
                 hou.name AS ORG_NAME,
                 CASE WHEN hou.date_to IS NULL OR hou.date_to >= SYSDATE THEN 'Y' ELSE 'N' END AS ACTIVE_FLAG
            FROM hr_all_organization_units hou
           WHERE hou.type IS NOT NULL";

    /// <summary>
    /// 사원 조회 SQL. 컬럼: PERSON_ID, EMPLOYEE_NUMBER, FULL_NAME, EMAIL_ADDRESS, JOB_NAME, ORGANIZATION_ID, ACTIVE_FLAG('Y'/'N')
    /// </summary>
    public string EmployeeQuery { get; set; } =
        @"SELECT ppf.person_id AS PERSON_ID,
                 ppf.employee_number AS EMPLOYEE_NUMBER,
                 ppf.full_name AS FULL_NAME,
                 ppf.email_address AS EMAIL_ADDRESS,
                 pj.name AS JOB_NAME,
                 paf.organization_id AS ORGANIZATION_ID,
                 NVL(ppf.current_employee_flag, 'N') AS ACTIVE_FLAG
            FROM per_all_people_f ppf
            LEFT JOIN per_all_assignments_f paf
              ON paf.person_id = ppf.person_id
             AND paf.primary_flag = 'Y'
             AND paf.assignment_type = 'E'
             AND SYSDATE BETWEEN paf.effective_start_date AND paf.effective_end_date
            LEFT JOIN per_jobs pj ON pj.job_id = paf.job_id
           WHERE ppf.employee_number IS NOT NULL
             AND SYSDATE BETWEEN ppf.effective_start_date AND ppf.effective_end_date";

    /// <summary>ERP에서 사라진 사원을 자동으로 퇴직 처리할지 여부</summary>
    public bool DeactivateMissingEmployees { get; set; } = true;

    public int CommandTimeoutSeconds { get; set; } = 120;
}
