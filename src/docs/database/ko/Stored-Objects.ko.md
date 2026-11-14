# 저장 객체 및 연동

## Repository 검토 결과

| 객체 유형 | 결과 |
|---|---|
| View | EF 마이그레이션이나 애플리케이션 코드에서 정의를 찾지 못함 |
| Stored procedure | 애플리케이션에서 정의 또는 호출을 찾지 못함 |
| User-defined function | 애플리케이션에서 정의 또는 호출을 찾지 못함 |
| Trigger | EF 마이그레이션이나 애플리케이션 코드에서 정의를 찾지 못함 |

Hangfire는 이러한 객체로 source의 업무 쿼리를 대체하지 않는다. `Hangfire.SqlServer` 1.8.25는 별도의 `[HangFire]` 저장소 스키마(테이블 및 인덱스)를 관리하며 테이블 목록은 workbook의 `Hangfire Storage` 시트에 있다. 저장소 DDL의 수명 주기는 패키지가 관리하므로 애플리케이션 마이그레이션 스크립트에 복사하지 않았다. 운영 DB에는 repository 외부에서 수동 생성한 객체가 있을 수 있으므로 카탈로그를 별도 확인해야 한다.

Repository에 해당 객체가 없어 별도로 제공할 SQL 정의가 없다. 이것이 운영 DB에 수동 생성된 객체가 없다는 증거는 아니다. 읽기 전용 계정으로 각 DB의 `sys.views`, `sys.procedures`, `sys.objects`/`sys.triggers`를 확인한다.

## 애플리케이션의 HRIS 처리 SQL

다음은 저장 프로시저가 아닌 `EzItam.Api/ExternalService/HrService.cs`의 인라인 쿼리에 대한 원문 문서의 기록이다. HRIS를 읽으며 애플리케이션 DB의 동기화 및 트랜잭션은 `EmployeeSyncBackgroundService`에서 처리한다. 연결 키는 `HRIS_Db`이고 `@OrgCode`는 `SqlCommand.Parameters`로 바인딩한다. **참고:** 아래 `@OrgCode AS OrgCode`는 현재 코드의 쿼리와 다르므로 실제 동작은 코드와 대조해야 한다.

```sql
WITH TreeDept AS (
    SELECT ID, ParentID, Code, OrgStructureName, OrgStructureLevel
    FROM D03_OrgStructure
    WHERE Code = @OrgCode

    UNION ALL

    SELECT d.ID, d.ParentID, d.Code, d.OrgStructureName, d.OrgStructureLevel
    FROM D03_OrgStructure d
    INNER JOIN TreeDept t ON d.ParentID = t.ID
)
SELECT
    @OrgCode AS OrgCode,
    e.Code,
    e.EmployeeName,
    null AS Email,
    td.Code AS DeptCode,
    td.OrgStructureName AS DeptName,
    td.OrgStructureLevel AS DeptLevel,
    tt.Code AS TitleCode,
    tt.JobTitleName AS TitleName,
    p.Code AS PositionCode,
    p.PositionName AS PositionName,
    CASE WHEN e.ResignNo IS NOT NULL THEN 'Y' ELSE 'N' END AS IsResigned
FROM D04_Employee e
INNER JOIN TreeDept td ON e.OrgStructureID = td.ID
LEFT JOIN C04_JobTitle tt ON e.JobTitleID = tt.ID
LEFT JOIN C04_Position p ON e.PositionID = p.ID
WHERE e.IsDelete != 1 OR e.IsDelete IS NULL
ORDER BY td.OrgStructureLevel, e.Code ASC;
```

## 직원 동기화 흐름

1. `Program.cs`는 Hangfire를 통해 `EmployeeSyncBackgroundService.SyncEmployeeDataAsync` 반복 job을 등록한다. Job ID, cron 및 org code는 `HangFireJobSetting:HrSync`에서 가져온다. repository 기본 일정은 서버 로컬 시간대 일요일 03:00이며 60, 300, 600초 간격으로 3회 재시도하도록 설정되어 있다.
2. 애플리케이션 DB 트랜잭션을 열기 전에 HRIS 목록을 읽는다. 목록이 비어 있으면 warning을 기록하고 종료하여 기존 스테이징/마스터 데이터를 유지한다.
3. 데이터가 있으면 애플리케이션 DB 트랜잭션에서 `EmployeeTemps`를 삭제/재적재하고 `Departments`를 생성/갱신한 후 `Employees`를 갱신한다. HRIS와 애플리케이션 DB를 함께 묶는 분산 트랜잭션은 없다.
4. `EmployeeTemps`는 성공할 때마다 교체되는 스테이징 스냅샷이며 이력을 저장하지 않는다. `IsResigned`는 `Y`/`N`으로 처리되며 스테이징 고유 키는 `(OrgCode, EmployeeNo)`다. `Departments.Code`와 `Employees.EmployeeNo`도 고유하다.
5. `N` 직원은 `EmployeeNo`로 upsert하고 이름/이메일/직책/부서를 갱신하며 `IsActive`를 활성화한다. 기존 `Y` 직원은 비활성화하며 물리 삭제하지 않는다. 부서는 재직 직원에서 추출하여 코드가 없으면 생성하고 이름이 바뀌면 갱신한다.

## 데이터 확인 사항

- HRIS SQL은 현재 `NULL AS Email`을 사용한다. mapper가 읽은 값을 빈 문자열로 변환하므로 job이 기존 `Employees.Email`을 빈 문자열로 덮어쓸 수 있다. 현재 쿼리에서 HRIS는 이메일 원본이 아니다.
- `HrController`는 `[AllowAnonymous]`가 지정된 `GET /api/v1/external/hr/employees`를 제공한다. 이 endpoint는 HRIS 직원 데이터를 직접 반환하며 스테이징 동기화를 실행하지 않는다. 배포 요구 사항에 따라 접근 권한을 확인한다.
- 이 동기화 쿼리는 `EmployeeTemps.DateValue1..9`를 채우지 않는다. 해당 문자열 스테이징 컬럼을 정규화된 SQL 날짜로 간주하지 않는다.
