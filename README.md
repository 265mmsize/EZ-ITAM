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

## 솔루션 구조

서버와 본사 화면은 **REST API로만** 연결됩니다. 화면은 DB나 서버 코드를 참조하지 않습니다.

| 프로젝트 | 역할 | 실행 위치 |
|---|---|---|
| `src/EzItam.Api` | 서버: REST API, EF Core, SQL Server, 계정·JWT 인증, 에이전트 API | 본사 서버 |
| `src/EzItam.Web` | 본사 관리 화면 (MVC). API만 호출 | 본사 (3~4명 사용) |
| `src/EzItam.Agent` | PC 에이전트 (Windows 서비스). API만 호출 | 지사 PC 전체 |
| `src/EzItam.Contracts` | API·화면·에이전트가 공유하는 DTO와 열거형 | 공용 라이브러리 |
| `tests/EzItam.Api.Tests` | 서버 업무 규칙과 에이전트 처리 테스트 (34건) | 개발 |

### 실행 주소

- 서버 API: `http://localhost:5100` (Swagger: `/swagger`, Development 환경만)
- 본사 화면: `http://localhost:5006`
- 화면이 호출할 API 주소는 `src/EzItam.Web/appsettings.json` 의 `Api:BaseUrl` 에서 설정합니다.
- 에이전트가 접속할 서버 주소는 `Agent:ServerUrl` 에서 설정합니다.

### API 경로 규칙

- 화면용 API: `api/v1/{자원}` (자산, 유지보수, 라이선스, 사원, 부서, 분류, 위치, 공급업체, 감사, 사용자, 대시보드, 에이전트 관리)
- 드롭다운 목록: `api/v1/options/{종류}`
- 로그인: `POST api/v1/auth/login` → 액세스 토큰 발급
- 에이전트 전용: `api/agent/enroll`, `heartbeat`, `inventory`

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


## 설정 항목 요약

| 키 | 설명 | 기본값 |
|---|---|---|
| `ConnectionStrings:Default` | DB 연결 문자열 | localhost / EzItam |
| `Database:Provider` | `SqlServer` 또는 `Sqlite`(데모) | SqlServer |
| `Database:AutoMigrate` | 시작 시 마이그레이션·시드 적용 | true |
| `Seed:AdminUser` / `Seed:AdminPassword` | 최초 관리자 계정 | admin / Admin!2345 |
| `Seed:SampleData` | 샘플 데이터 생성 | false (Development: true) |
## PC 에이전트 (1차 범위)

벤치마크 보고서의 권고에 따라 **Windows 10/11 전용 .NET 8 에이전트**를 추가했습니다. 에이전트가 먼저 서버에 접속하는 **아웃바운드 전용 HTTPS** 구조이며, 서버 응답으로 접속 주기와 인벤토리 전송 여부를 제어합니다.

| 구성 | 위치 | 역할 |
|---|---|---|
| 공용 계약 | `src/EzItam.Contracts` | 등록, heartbeat, 인벤토리 DTO |
| 에이전트 | `src/EzItam.Agent` | Windows 서비스(`EzItamAgent`). WMI·레지스트리로 하드웨어와 설치 SW 수집 |
| 서버 API | `src/EzItam.Web` `AgentApiController` | `api/agent/enroll`, `heartbeat`, `inventory` |
| 관리 화면 | `PC 에이전트` 메뉴 | 승인, 사용 중지, 자산 연결, 설치 SW 조회 |

### 동작 방식

1. 에이전트가 등록 키로 `enroll` 을 호출합니다. 서버는 장치를 **승인 대기**로 저장하고 토큰을 발급합니다. 토큰은 서버에 해시로만 저장됩니다.
2. 관리자가 **승인**하면 다음 heartbeat 부터 인벤토리가 전송됩니다.
3. 인벤토리는 gzip 으로 압축해 전송하며, 설치 SW 목록은 최신 수집 결과로 전체 교체됩니다.
4. 시리얼번호가 같은 자산을 자동으로 찾아 연결합니다. 자산 대장에 값이 비어 있는 항목만 채우고 기존 값은 덮어쓰지 않습니다.
5. 전송이 실패하면 다음 주기에 최신 값을 다시 수집해 보냅니다. 장기간 오프라인이어도 최신 상태만 필요하므로 별도 대기열은 두지 않았습니다.

### 서버 설정

```json
"Agent": {
  "EnrollmentKey": "여기를 반드시 변경하세요"
}
```

등록 키는 설치 파일과 서버가 같은 값을 써야 합니다. 기본값을 그대로 운영하지 마세요.

### 에이전트 설치 (Windows PC)

```powershell
# 1) 게시 (self-contained 단일 파일, .NET 8 런타임 설치 불필요)
dotnet publish src/EzItam.Agent -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish\agent

# 2) publish 폴더의 appsettings.json 에서 ServerUrl 과 EnrollmentKey 를 설정

# 3) 관리자 PowerShell 에서 서비스 등록
sc.exe create EzItamAgent binPath= "C:\Program Files\EzItam\Agent\EzItam.Agent.exe" start= auto
sc.exe failure EzItamAgent reset= 86400 actions= restart/60000/restart/60000/restart/60000
sc.exe start EzItamAgent
```

- 상태 파일은 `%ProgramData%\EzItam\Agent\agent-state.json` 에 저장되며, 토큰은 DPAPI(LocalMachine)로 보호됩니다.
- 하드웨어 일부(BitLocker)는 관리자 권한이 필요하므로 LocalSystem 서비스로 실행해야 합니다.
- 개발 환경에서는 `Agent:UseFakeCollector=true` 로 실제 PC 정보 없이 프로토콜만 확인할 수 있습니다.

### 1차 범위에서 제외한 항목

정규화 카탈로그, 라이선스 권리(ELP) 관리, 소프트웨어 사용량 측정, 원격 명령, 자동 업데이트는 2차 과제입니다.

## 라이선스 / 기타

사내 사용 목적의 프로젝트입니다. 기여·이슈는 GitHub 저장소를 이용해 주세요.

## 에이전트 업데이트 배포 (설치 후 자동)

설치는 1회 수동으로 합니다. 이후 업데이트는 에이전트가 하트비트 응답을 보고 스스로 받습니다.

1. 패키지 생성 및 배치 (관리 PC, PowerShell):
   ```powershell
   .\tools\Publish-AgentUpdate.ps1 -Version 1.0.1 -Ring Pilot    # 먼저 파일럿 링
   .\tools\Publish-AgentUpdate.ps1 -Version 1.0.1 -Ring Stable   # 검증 후 전체 링
   ```
   결과는 `src/EzItam.Api/updates/{Ring}/` 에 zip 과 `manifest.json`(버전, SHA-256)으로 생깁니다. 배포 경로는 `Updates:Root` 로 바꿀 수 있습니다.
2. 장치 링 지정: `POST api/v1/agents/{id}/ring` (Admin, body `{"Ring":"Pilot"}`). 기본값은 Stable 입니다.
3. 에이전트 동작: 승인된 장치만 업데이트를 받습니다. 새 버전을 발견하면 0~30분(`Agent:UpdateJitterMinutes`) 무작위 대기 후 받아서 SHA-256 을 검증하고, 서비스를 멈춘 뒤 교체·재시작합니다. 20초 안에 Running 이 되지 않으면 백업으로 되돌립니다. 같은 버전은 한 번만 시도합니다.
4. 교체 과정은 `%ProgramData%\EzItam\Agent\update\update.log` 에 기록됩니다.
5. `appsettings*.json` 은 패키지에 포함되지 않아 장치의 서버 주소와 키를 덮어쓰지 않습니다.

> 주의: 교체 스크립트(PowerShell)와 서비스 재시작 동작은 Windows 실기에서 아직 검증하지 않았습니다. 첫 배포는 장치 몇 대의 파일럿으로 확인하세요.
