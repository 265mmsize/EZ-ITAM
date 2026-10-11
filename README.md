# EZ-ITAM — IT 자산 관리 (IT Asset Management)

사내 하드웨어 자산, 소프트웨어 라이선스, 유지보수 이력을 한 곳에서 추적·관리하는 웹 애플리케이션입니다.

- **ASP.NET Core 8 MVC** (C#) · **Entity Framework Core 8** · **MS SQL Server**
- **ASP.NET Core Identity** 로그인 / 역할(Admin · Manager · Viewer) 기반 권한
- 한국어 UI, Bootstrap 5
- ERP(Oracle EBS) 연동은 범위에서 제외했습니다.

## 주요 기능

| 영역 | 기능 |
|---|---|
| 대시보드 | 자산 현황(분류/상태/부서별), 보증 만료 임박, 라이선스 만료 임박, 진행중 유지보수, 최근 불출/반납 |
| 하드웨어 자산 | 등록/수정/삭제, 자산번호 자동 채번(`분류코드-연도-일련번호`), 검색·필터·정렬·페이징, CSV 내보내기, 정액법 장부가액 |
| 불출 / 반납 | 사용자에게 불출 → 반납(상태·위치 변경) 이력 관리, 상태 변경 사유 기록 |
| 유지보수 | 수리/업그레이드/점검/교체 이력, 비용·업체, 미완료 시 자산 '수리중' 자동 전환 |
| 소프트웨어 라이선스 | 라이선스 유형(Per Seat/Per Device/구독/사이트), 보유·사용 수량, 사용자/장비 할당·해제, 초과 할당 방지 |
| 조직 / 기준정보 | 사원, 부서, 자산 분류, 위치, 공급업체 CRUD |
| 변경 이력 | 모든 자산·조직·라이선스 변경 사항을 사용자·시각·이전/이후 값으로 자동 기록 (감사 로그) |
| 사용자 관리 | 계정 생성, 권한 변경, 비밀번호 초기화, 잠금 해제 — 관리자 메뉴 |

### 권한

| 역할 | 가능 작업 |
|---|---|
| Viewer | 조회만 |
| Manager | 자산/라이선스/유지보수/기준정보 등록·수정, 불출·반납·할당 |
| Admin | Manager 권한 + 삭제, 사용자 관리 |

## 프로젝트 구조

```
EZ-ITAM.sln
├─ src/EzItam.Web/              ASP.NET Core MVC 웹 애플리케이션
│  ├─ Controllers/              화면별 컨트롤러
│  ├─ Data/                     AppDbContext(감사 로그 자동 기록), 시드, EF 마이그레이션
│  ├─ Models/Entities/          엔티티 (Asset, Employee, SoftwareLicense …)
│  ├─ Services/                 AssetService, LicenseService, DashboardService, CSV
│  ├─ ViewModels/, Views/, wwwroot/
│  └─ appsettings*.json
├─ tests/EzItam.Tests/          xUnit 단위 테스트 (SQLite in-memory)
├─ db/schema.sql                SQL Server 용 DDL (EF 마이그레이션에서 생성, 멱등)
└─ docker-compose.yml           SQL Server + 앱 빠른 실행
```

## 시작하기 (Visual Studio 2022)

1. `EZ-ITAM.sln` 을 Visual Studio 2022 (17.8 이상, .NET 8 SDK) 로 엽니다.
2. `src/EzItam.Web/appsettings.Development.json` 의 연결 문자열을 환경에 맞게 수정합니다.
   ```json
   "ConnectionStrings": {
     "Default": "Server=localhost;Database=EzItam;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```
   SQL 인증을 사용하는 경우: `Server=db-host,1433;Database=EzItam;User Id=ezitam;Password=***;TrustServerCertificate=True`
3. **F5** 로 실행합니다. 시작 시 자동으로 마이그레이션이 적용되고(`Database:AutoMigrate`), 기본 데이터가 생성됩니다.
   - 개발 환경(`SampleData: true`)에서는 샘플 자산/사원/라이선스도 함께 생성됩니다.
4. 기본 관리자 계정으로 로그인합니다.

   | 아이디 | 비밀번호 |
   |---|---|
   | `admin` | `Admin!2345` |

   > 운영 배포 전 `Seed:AdminPassword` 를 반드시 변경하거나, 로그인 후 **비밀번호** 메뉴에서 변경하세요.

### DB 를 직접 생성하려는 경우

- `db/schema.sql` 을 SSMS 에서 실행하면 전체 스키마가 생성됩니다 (여러 번 실행해도 안전).
- 또는 패키지 관리자 콘솔에서 `Update-Database`, CLI 에서는 `dotnet ef database update --project src/EzItam.Web`.
- 운영 환경에서 앱의 자동 마이그레이션을 끄려면 `"Database": { "AutoMigrate": false }` 로 설정합니다.

### 명령줄 (CLI)

```bash
dotnet build
dotnet test                                  # 단위 테스트
dotnet run --project src/EzItam.Web          # http://localhost:5006
```

### SQL Server 없이 바로 체험 (SQLite 데모 프로필)

```bash
cd src/EzItam.Web
EZITAM_PROFILE=Sqlite ASPNETCORE_ENVIRONMENT=Development dotnet run
```
Visual Studio 에서는 실행 프로필 **sqlite-demo** 를 선택하면 됩니다. (`ezitam.db` 파일이 생성됩니다. 데모 용도이며 운영은 SQL Server 를 사용하세요.)

### Docker 로 실행

```bash
docker compose up --build
# http://localhost:8080  (admin / Admin!2345)
```


## 설정 항목 요약

| 키 | 설명 | 기본값 |
|---|---|---|
| `ConnectionStrings:Default` | DB 연결 문자열 | localhost / EzItam |
| `Database:Provider` | `SqlServer` 또는 `Sqlite`(데모) | SqlServer |
| `Database:AutoMigrate` | 시작 시 마이그레이션·시드 적용 | true |
| `Seed:AdminUser` / `Seed:AdminPassword` | 최초 관리자 계정 | admin / Admin!2345 |
| `Seed:SampleData` | 샘플 데이터 생성 | false (Development: true) |
## 라이선스 / 기타

사내 사용 목적의 프로젝트입니다. 기여·이슈는 GitHub 저장소를 이용해 주세요.
