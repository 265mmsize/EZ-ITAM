# EZ-ITAM — 데이터베이스 DBA 검토

## 1. 개요

- **기본 DBMS:** Microsoft SQL Server 2025 RTM (17.0.1000.7), Express Edition (64-bit). 데이터베이스: `EzItam`, 호환성 수준: 170, 복구 모델: SIMPLE, 사용자 스키마: `dbo`.
- **데이터베이스 연결:** 업무/Identity DB는 `ConnectionStrings:Default`, Hangfire SQL Server 저장소는 `ConnectionStrings:Hangfire`, HRIS 원본은 `ConnectionStrings:HRIS_Db`를 사용한다. Hangfire 스키마는 애플리케이션 EF Core 마이그레이션과 별도로 패키지가 관리한다.
- **EF 마이그레이션의 테이블:** 업무 테이블 16개, ASP.NET Core Identity 테이블 7개, `__EFMigrationsHistory` 1개로 총 24개. 별도 Hangfire 데이터베이스의 테이블은 포함하지 않는다.

| 구분 | 테이블 | 기능 |
|---|---|---|
| 자산 | `Assets` | 자산 정보, 분류, 상태, 구매, 위치 및 현재 사용자 |
| 자산 | `AssetCategories` | 자산 분류 및 감가상각 기간 |
| 자산 | `AssetAssignments` | 자산 지급/반납 이력 |
| 자산 | `MaintenanceRecords` | 수리/유지보수 이력 |
| 자산 | `Locations` | 위치 목록 |
| 자산 | `Vendors` | 공급업체 |
| 인사 | `Departments` | 부서 목록 |
| 인사 | `Employees` | HRIS 동기화가 가능한 내부 직원 |
| HRIS 연동 | `EmployeeTemps` | HRIS 직원/부서 스냅샷을 수신하는 스테이징 테이블 |
| 라이선스 | `SoftwareLicenses` | 소프트웨어 라이선스 목록 |
| 라이선스 | `LicenseAssignments` | 직원 또는 자산에 대한 라이선스 할당/회수 이력 |
| 에이전트 | `Agents` | 등록된 장치 및 최근 인벤토리 정보 |
| 에이전트 | `AgentSoftware` | 에이전트가 보고한 현재 소프트웨어 |
| 실사 | `Inventories` | 자산 실사 회차 및 상태 |
| 실사 | `InventoryDetails` | 시스템 정보와 실사 정보를 비교한 결과 |
| 감사 | `AuditLogs` | `AppDbContext`가 기록하는 변경 이력 |
| Identity | `AspNetUsers` | ASP.NET Core Identity 사용자 및 인증 정보 |
| Identity | `AspNetRoles` | 역할 정의 |
| Identity | `AspNetUserRoles` | 사용자와 역할의 연결 |
| Identity | `AspNetUserClaims` | 사용자 클레임 |
| Identity | `AspNetRoleClaims` | 역할 클레임 |
| Identity | `AspNetUserLogins` | 외부 로그인 |
| Identity | `AspNetUserTokens` | Identity 토큰 |
| EF Core | `__EFMigrationsHistory` | 적용된 마이그레이션 추적 |

## 2. 구조, 키 및 제약 조건

컬럼, SQL Server 타입, 길이, NULL 여부, PK/FK, 인덱스, 기본값 및 업무 설명은 [데이터 사전](EZITAM_Data_Dictionary.ko.xlsx)을 참조한다. `Hangfire Storage` 시트는 패키지가 관리하는 테이블 목록이며 Hangfire 전체 컬럼 사전을 대체하지 않는다. ERD는 [ERD.ko.md](ERD.ko.md), 애플리케이션 DDL은 공통 SQL 파일 [EZITAM.SqlServer.Migrations.sql](EZITAM.SqlServer.Migrations.sql)에 있다. Hangfire는 별도 패키지 스키마와 스크립트를 사용한다.

업무 enum (`AssetStatus`, `MaintenanceType`, `LicenseType`, `AuditAction`, `AgentStatus`, `AgentRing`, `InventoryStatus`)은 `int`로 저장된다. 값과 순서는 `EzItam.Contracts/Enums.cs`에 정의되어 있다. 주요 고유 인덱스는 `Assets.AssetTag`, `AssetCategories.Code`, `Departments.Code`, `Employees.EmployeeNo`, `Agents.DeviceUuid`, `Inventories.Name`, `InventoryDetails(InventoryId, IdAsset)`, `EmployeeTemps(OrgCode, EmployeeNo)`이다.

## 3. 데이터 처리, 성능 및 동시성

- **인덱스:** EF Core 인덱스는 Excel의 `Indexes` 시트와 DDL에 수록되어 있다. 실제 통계, 실행 계획, 테이블 크기, 선택도 및 운영 워크로드가 없어 인덱스 추가/삭제나 실제 성능을 단정할 수 없다.
- **View/Stored Procedure/Function/Trigger:** 애플리케이션 코드나 마이그레이션에서 정의를 찾지 못했다. HRIS는 `HrService`의 인라인 SQL CTE를 사용한다. 운영 DB에서 별도로 생성한 객체는 카탈로그 확인이 필요하다.
- **INSERT/UPDATE/DELETE:** API는 EF Core `AppDbContext`를 사용하며 일반적으로 `SaveChangesAsync`를 호출한다. 지급, 유지보수, 라이선스 및 실사 이력은 별도 테이블에 저장된다. 일부 API는 물리 삭제를 제공하며 cascade/restrict/set-null 동작은 FK에 정의되어 있다.
- **트랜잭션:** EF Core는 각 `SaveChanges`에 기본 트랜잭션을 사용한다. HR 동기화는 HRIS를 먼저 읽고 애플리케이션 DB의 명시적 트랜잭션에서 스테이징 교체, 부서/직원 동기화 및 퇴직 상태 갱신을 처리한다. HRIS와 EZ-ITAM 사이의 분산 트랜잭션은 아니다. 일부 실사 작업도 명시적 트랜잭션을 사용한다. SQL Server 일시적 오류 재시도는 최대 3회다. 신규 엔티티의 사용자 정의 감사는 엔티티를 먼저 저장하고 두 번째 `SaveChanges`에서 로그를 기록한다. 두 저장을 감싸는 외부 트랜잭션이 없는 경우 로그 저장 실패로 감사 기록이 누락될 수 있다.
- **동시성:** 업무 테이블에 `rowversion`/낙관적 동시성 토큰은 확인되지 않았다. Identity에는 `ConcurrencyStamp`가 있다. 추가 잠금/검사가 없다면 업무 갱신이 마지막 쓰기 우선 방식으로 처리될 수 있으므로 재고, 라이선스 좌석 및 실사에 대해 실제 워크로드 검토가 필요하다.
- **성능/확장성:** 실행 계획, Query Store/DMV 및 용량을 수집하고 워크로드에 따라 인덱스를 검토한다. 대규모 이력의 페이지 처리와 `AuditLogs`, `AgentSoftware`, 할당 이력 및 스테이징의 보관 정책을 검토한다.

## 4. 보안 및 관리

- **SQL Server 계정 및 권한:** `sa`는 SQL Server 관리자 계정이며 `itasset`은 `EzItam` DB의 `dbo` 스키마에 대한 전체 접근 권한을 가진다. 실제 login, database user, 권한 및 role을 SQL Server에서 대조해야 한다. 암호와 연결 정보는 문서에 포함하지 않는다.
- **향후 권한:** 읽기 전용 또는 업무 데이터 읽기/수정 없이 데이터베이스만 관리하는 계정/role을 필요에 따라 생성할 수 있다. 업무별 최소 권한을 부여하고 role에서 상속된 권한도 확인한다. 일반적인 접근에 `sa` 사용을 기본으로 하지 않는다.
- **감사:** `dbo.AuditLogs`는 `AppDbContext`가 추적하는 엔티티의 entity/key, action, actor, timestamp 및 변경 JSON을 저장한다. `Agent`, `AgentSoftware`, Identity 및 `EmployeeTemp`는 자동 감사 대상이 아니다. 이는 애플리케이션 감사이며 SQL Server Audit이 아니므로 DB에 직접 수행한 변경은 포함하지 않는다. 향후 SQL Server 수준 감사를 검토할 수 있다.
- **백업 및 복구:** Maintenance Plan을 통한 전체 백업 예약을 계획한다. 다만 SQL Server 2025 Express에는 Maintenance Plan 예약 실행에 필요한 SQL Server Agent가 없으므로 Windows 작업 스케줄러와 `BACKUP DATABASE`를 대안으로 사용할 수 있다.

## 5. 연동

- **HRIS:** `HrService`는 SQL Server `Microsoft.Data.SqlClient`와 `HRIS_Db` 연결을 사용하고 `@OrgCode`를 SQL 매개변수로 전달한다. 재귀 CTE가 `D03_OrgStructure`에서 기준 부서와 하위 부서를 조회하고 `D04_Employee`, `C04_JobTitle`, `C04_Position`을 조인한다. 논리 삭제된 직원을 제외하고 직원/부서/직책/직위와 퇴직 여부를 매핑한다. 쿼리는 Email에 `NULL`을 반환하고 `DateValue1..9`를 선택하지 않는다. 자세한 쿼리는 [Stored-Objects.ko.md](Stored-Objects.ko.md)를 참조한다.
- **HRIS 동기화:** Hangfire가 `EmployeeSyncBackgroundService.SyncEmployeeDataAsync`를 호출한다. HRIS 결과가 비어 있으면 warning을 기록하고 스냅샷과 마스터를 유지한다. 결과가 있으면 애플리케이션 DB 트랜잭션에서 `EmployeeTemps` 전체를 삭제/재적재하고 `Departments`를 `Code`로 동기화한 뒤 `Employees`를 `EmployeeNo`로 갱신한다. `N`인 직원은 생성/갱신하고 `IsActive`를 활성화하며, `Y`인 기존 직원은 물리 삭제하지 않고 비활성화한다. `EmployeeTemps`는 이력이 아닌 현재 스냅샷이다. 고유 키는 `Departments.Code`, `Employees.EmployeeNo`, `EmployeeTemps(OrgCode, EmployeeNo)`이며 스테이징에는 HRIS 코드에 대한 FK가 없다.
- **확인 사항:** 실제 `HrService` 읽기 경로는 `HrEmployeeDto.OrgCode`를 설정하지 않아 job이 `EmployeeTemps.OrgCode`에 기본 빈 문자열을 저장한다. mock 경로는 값을 설정한다. 여러 조직을 구분해야 한다면 원본 org code를 DTO에 설정해야 한다. 쿼리의 `NULL AS Email`은 mapper에서 빈 문자열로 변환되어 `Employees`에 기록될 수 있으므로 기존 이메일을 지우는 것이 의도된 동작인지 확인한다. 이 문서는 처리 로직을 변경하지 않는다.
- **HRIS API:** `GET /api/v1/external/hr/employees?orgCode=...`는 HRIS를 직접 읽으며 `[AllowAnonymous]`가 지정되어 있다. 스테이징/직원 테이블을 갱신하는 동기화 endpoint는 아니다.
- **Agent:** 에이전트는 API를 통해 인벤토리를 전송하고 백엔드는 `Agents`를 갱신하며 `AgentSoftware`의 현재 목록을 교체한다. 소프트웨어 스냅샷 이력은 아니다.
- **ERP:** repository에서 명확한 ERP 연결이나 스키마, contract, external ID mapping, outbox, CDC 또는 메시지 브로커를 확인하지 못했다. HRIS를 ERP 연동으로 간주해서는 안 된다.
- **API–DB:** 웹 클라이언트는 API를 호출하며 controllers/services는 EF Core를 사용한다. HRIS 동기화는 별도 SQL 연결을 사용하고 Hangfire job 상태는 별도 SQL Server 저장소에 보관한다.

### Hangfire SQL Server 저장소

- **패키지/설정:** `Hangfire.SqlServer` 1.8.25, 데이터 호환성 `Version_180`, recommended serializer settings 및 simple assembly type names를 사용한다. `AddHangfireServer`는 `Environment.ProcessorCount * 2` workers를 실행한다. 연결 키는 `ConnectionStrings:Hangfire`이며 애플리케이션의 `Default` DB와 별도다.
- **스키마:** 패키지 스크립트는 `[HangFire]` 스키마와 `Schema`, `Job`, `State`, `JobParameter`, `JobQueue`, `Server`, `Hash`, `List`, `Set`, `Value`, `Counter`, `AggregatedCounter` 테이블 및 저장소 인덱스를 생성한다. 각 테이블의 일반적인 용도는 workbook의 `Hangfire Storage` 시트에 있다. Hangfire가 관리하는 내부 구조이므로 수동 변경하거나 EF 업무 마이그레이션에 합치지 않는다.
- **반복 job:** `HangFireJobSetting:HrSync`의 `JobId`, `CronExpression`, `OrgCode`를 사용한다. repository 기본 cron은 서버의 `TimeZoneInfo.Local` 기준 일요일 03:00 (`0 3 * * 0`)이다. 60/300/600초 간격으로 최대 3회 재시도하며 소진 시 failed 상태가 된다. 실제 `OrgCode` 값은 문서에 기재하지 않는다.
- **Dashboard:** `/hangfire`에 연결되어 있으며 repository에서 custom authorization filter를 등록하지 않는다. 신뢰 영역 외부에 노출하기 전에 배포 환경의 실제 접근 구성을 확인한다.

## 산출물

1. `EZITAM.SqlServer.Migrations.sql` — 빈 데이터베이스부터 최신 마이그레이션까지의 SQL Server DDL/migration script. **빈 DB 또는 DBA 검토 후 테스트 환경에서만 실행**한다. 멱등 스크립트가 아니며 운영 DB에서 직접 추출한 스키마가 아니다.
2. `ERD.ko.md` — 업무 및 Identity 테이블의 Mermaid ERD.
3. `EZITAM_Data_Dictionary.ko.xlsx` — 테이블/컬럼/인덱스/관계 데이터 사전.
4. `Stored-Objects.ko.md` — source/migrations의 view, procedure, function, trigger 검토와 HRIS 및 Hangfire 설명.
5. `Optimization-Preflight.ko.sql` — 최적화 마이그레이션 적용 전 읽기 전용 데이터 점검.
6. `sql_schema.sql` — 공통 스키마 스크립트.
