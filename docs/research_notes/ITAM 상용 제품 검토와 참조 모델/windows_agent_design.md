# Windows PC 인벤토리/자산관리 에이전트 설계 노트 (C#/.NET, 2025–2026 기준)

> 조사 범위: Windows 10/11 중심, 필요 시 Windows 7/8.1/Server 2012 레거시 고려. 서버는 ASP.NET Core + MS SQL Server 가정.
> 조사 환경 제약: 이 세션에서는 learn.microsoft.com, docs.lansweeper.com, docs.tacticalrmm.com, osquery.io 등으로의 직접 페이지 fetch가 egress 정책으로 차단되어, Microsoft Learn 내용은 GitHub 미러(dotnet/docs, dotnet/core)와 검색 결과 스니펫으로, 벤더 문서는 검색 스니펫으로 확인했다. 스니펫 기반 인용은 "검색 스니펫 확인"으로 표기했으며, 원문 확인을 권장한다.

---

## 1. 런타임 선택: .NET Framework 4.8 vs .NET 8 (self-contained/framework-dependent), Worker Service, 트레이 앱

### Takeaway
Windows 7/8.1 지원이 요구되면 .NET 8은 공식적으로 불가능하므로 .NET Framework 4.8(Win7 SP1 지원)이 유일한 단일 코드베이스 선택지다. 레거시가 불필요하면 .NET 8 LTS + `Microsoft.Extensions.Hosting.WindowsServices` + self-contained single-file(트리밍 끔, ReadyToRun 켬)이 Microsoft 공식 권장 구성이다. 상용 제품(Lansweeper LsAgent)은 Win7까지 커버하기 위해 .NET Framework 4.8을 전제로 한다.

### Cited Findings
- .NET 8의 Windows 지원 OS: Windows 11(23H2 이상), Windows 10(1607 Enterprise, 1809 Enterprise, 21H2 등), Windows Server 2012/2012 R2(ESU 필요)/2016/2019/2022/23H2/2025. **Windows 7, 8.1은 목록에 없음.** x86/x64/Arm64 지원 — [dotnet/core supported-os.md (.NET 8)](https://github.com/dotnet/core/blob/main/release-notes/8.0/supported-os.md)
- .NET Framework 4.8은 Windows 7 SP1, Windows Server 2012를 지원 OS로 명시(단, Windows 7은 Microsoft 지원 종료 상태임을 문서가 표기) — [.NET Framework system requirements](https://learn.microsoft.com/en-us/dotnet/framework/get-started/system-requirements) (검색 스니펫 확인); [.NET Framework 4.8 offline installer KB4503548](https://support.microsoft.com/en-us/kb/4503548)
- Lansweeper LsAgent는 Windows 7 SP1(32/64bit), 8.1, 10 1607+, 11, Server 2008 R2~2022를 지원하되 **.NET Framework 4.8 이상 필수**이며, 설치기가 필요 시 microsoft.com에 접속해 .NET 4.8을 자동 설치(재부팅 필요 가능). 이 요구사항은 LsAgent 8.x(Lansweeper 8.4.100)부터 — [LsAgent installation requirements](https://docs.lansweeper.com/classic/docs/lsagent-installation-requirements) (검색 스니펫 확인); [Lansweeper community](https://community.lansweeper.com/t5/general-discussions/lansweeper-agent-automatic-update/m-p/34582)
- Microsoft Worker Service → Windows Service 공식 가이드: NuGet `Microsoft.Extensions.Hosting.WindowsServices`, `Host.CreateApplicationBuilder` 후 `builder.Services.AddWindowsService(o => o.ServiceName = "...")`; `BackgroundService.ExecuteAsync` 패턴; 설치는 `sc.exe create "<name>" binpath= "C:\...\App.exe"`, 복구는 `sc.exe failure`/`sc.exe qfailure`; `--contentRoot` 인자로 콘텐츠 루트 변경 가능 — [Create a Windows Service using BackgroundService (dotnet/docs 미러)](https://github.com/dotnet/docs/blob/main/docs/core/extensions/windows-service.md)
- 동일 문서의 publish 권장: `OutputType=exe`, `PublishSingleFile=true`(Release), `RuntimeIdentifier=win-x64`, `PlatformTarget=x64`, **self-contained + ReadyToRun 켬 + 트리밍 끔**. 대안으로 framework-dependent DLL을 `dotnet.exe <dll>`로 binpath 지정 가능 — [같은 문서](https://github.com/dotnet/docs/blob/main/docs/core/extensions/windows-service.md)
- 같은 문서의 주의사항: .NET 6+에서 `BackgroundService` 미처리 예외의 기본 동작은 `StopHost`(이전 버전은 `Ignore`로 좀비 프로세스 발생). 서비스가 재시작되길 원하면 `Environment.Exit(비0)`으로 종료해야 SCM 복구가 작동. PublishSingleFile 앱은 서비스 프로세스 하에서 디버깅 불가. 이벤트 로그는 `appsettings.json`의 `Logging:EventLog` (`SourceName`, `LogName`)로 설정, 기본 심각도는 Warning — [같은 문서](https://github.com/dotnet/docs/blob/main/docs/core/extensions/windows-service.md)
- Single-file self-contained는 런타임과 프레임워크 라이브러리를 포함하므로 파일이 큼; `dotnet publish -r win-x64 -p:PublishSingleFile=true`; 트리밍은 `-p:PublishTrimmed=true`로 크기 감소하지만 트리밍 비호환 기능이 꺼지고 경고가 발생 — [Single-file deployment overview](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview); [Trim self-contained deployments](https://learn.microsoft.com/en-us/dotnet/core/deploying/trim-self-contained) (검색 스니펫 확인)
- 트레이 앱–서비스 IPC: .NET 8의 `System.IO.Pipes`에 `NamedPipeServerStreamAcl.Create(...)`가 있어 `PipeSecurity`/`PipeAccessRule`로 파이프 ACL을 지정할 수 있다 — [NamedPipeServerStreamAcl.Create (.NET 8)](https://learn.microsoft.com/en-us/dotnet/api/system.io.pipes.namedpipeserverstreamacl.create?view=net-8.0) (검색 스니펫 확인). 서비스가 자기 ID로만 ACL을 걸면 비관리자 트레이 클라이언트가 `UnauthorizedAccessException`을 겪는 사례가 보고됨 — [Habr Q&A](https://qna.habr.com/q/313631)
- 상용 참고: Tactical RMM 에이전트는 Go로 작성, NinjaOne Windows 에이전트는 ARM 미지원(x86/x64만) — [Tactical RMM docs](https://docs.tacticalrmm.com/) ; [NinjaOne Windows agent requirements](https://www.ninjaone.com/docs/endpoint-management/ninjaone-endpoint-management-microsoft-windows-agent-system-requirements-and-os-support) (검색 스니펫 확인)

### Inferences
- **결정 트리**: (a) Win7/8.1/2008R2 지원 필요 → .NET Framework 4.8 단일 코드베이스(WinForms 트레이 + `ServiceBase` 서비스), WMI는 `System.Management` 사용. (b) Win10 1607+ 전용 → .NET 8 Worker Service self-contained single-file(약 60–80MB급으로 예상되나 이 세션에서 수치 출처를 찾지 못함, Gaps 참조). (c) 혼합 fleet → 두 빌드를 유지하거나, 공통 수집 로직을 `netstandard2.0` 라이브러리로 분리하고 호스트만 두 개(4.8 서비스 / .NET 8 Worker)로 두는 방법이 유지보수 부담이 가장 적다.
- Self-contained를 택하면 OS에 설치된 .NET 런타임 버전 분포를 신경 쓸 필요가 없고, 런타임 보안 패치는 에이전트 자체 업데이트로 밀어내야 한다(반대로 framework-dependent는 런타임 패치가 Windows Update/별도 배포로 분리됨).
- 서비스 프로세스(Session 0)에서는 UI를 띄울 수 없으므로 트레이 앱은 사용자 세션에서 별도 실행(HKLM Run 키 또는 로그온 작업 스케줄러)하고, 명명된 파이프 ACL에 `BUILTIN\Users`(또는 `Authenticated Users`)에 대해 ReadWrite를 부여해야 한다. 파이프로 넘기는 명령은 "인벤토리 지금 실행", "상태 조회" 정도로 제한하고, 쓰기성 명령은 서버 서명 명령으로만 허용하는 것이 안전하다.
- `PublishTrimmed`는 WMI(`System.Management`는 리플렉션/COM 의존), WinForms, `Microsoft.Management.Infrastructure`와 궁합이 나쁘므로 Microsoft 권장대로 서비스 바이너리에서는 끄는 것이 맞다.

### Gaps
- .NET 8 self-contained single-file(trim 미적용) Windows 서비스의 실제 파일 크기(MB)를 공식 문서에서 확인하지 못함. 팀이 hello-world Worker를 publish해 실측 권장.
- NativeAOT로 Windows 서비스를 빌드하는 사례/제약(System.Management 비호환 여부)은 이번 조사에서 확인하지 않음.

---

## 2. 데이터 수집 기법: WMI/CIM 클래스, 레지스트리 경로, API

### Takeaway
하드웨어는 `root\cimv2`의 `Win32_*` 클래스와 `root\wmi`의 `WmiMonitorID`로 대부분 수집 가능하며, 소프트웨어는 `Win32_Product` 대신 Uninstall 레지스트리(64bit + WOW6432Node + HKCU)를 읽는 것이 사실상 표준이다. 보안 상태(AV, BitLocker, 라이선스)는 별도 네임스페이스/메서드 호출이 필요하고, 로그온 사용자·유휴 시간은 Session 0 서비스에서는 WMI/`GetLastInputInfo`가 불완전하므로 WTS API 또는 사용자 세션 헬퍼가 필요하다. .NET 8에서는 Microsoft가 `Microsoft.Management.Infrastructure`(MI)를 권장하지만 `System.Management`도 NuGet으로 계속 제공된다.

### Cited Findings

**WMI 라이브러리 선택 (.NET 8)**
- Microsoft WMI SDK: "System.Management 클래스는 여전히 사용 가능하지만, Microsoft.Management.Infrastructure 클래스가 일반적으로 더 효율적이고 확장성이 좋으므로 새 코드에는 MI 클래스 사용을 권장". 기본 사용법은 `CimSession.Create(null)` → `QueryInstances("root\\cimv2", "WQL", "SELECT ...")` → `CimInstance.CimInstanceProperties[...]` — [Creating WMI Clients](https://learn.microsoft.com/en-us/windows/win32/wmisdk/creating-wmi-clients) (검색 스니펫 확인)
- `System.Management` NuGet은 .NET 8/9/10용으로 계속 배포(현재 10.0.x), **Windows 전용**(타 플랫폼 미지원). 주요 타입: `ManagementObjectSearcher`, `SelectQuery`, `ManagementObject`, `ManagementClass` — [System.Management 10.0.12 on NuGet](https://nuget.org/packages/System.Management/10.0.12)
- `Microsoft.Management.Infrastructure` NuGet을 .NET Framework 4.7.2 프로젝트에 설치하다 실패한 사례가 Microsoft Q&A에 보고됨(.NET Core 대상은 아님) — [Microsoft Q&A 441227](https://learn.microsoft.com/en-us/answers/questions/441227/how-do-i-use-microsoft-management-infrastructure-t)

**하드웨어 클래스 (상용/OSS 에이전트가 실제로 쓰는 목록)**
- Tactical RMM Go 에이전트(`wmi_windows.go`)가 조회하는 클래스: `Win32_USBController`, `Win32_Processor`, `Win32_DesktopMonitor`, `Win32_NetworkAdapter`, `Win32_DiskDrive`, `Win32_ComputerSystemProduct`, `Win32_BIOS`, `Win32_ComputerSystem`, `Win32_NetworkAdapterConfiguration`, `Win32_PhysicalMemory`, `Win32_OperatingSystem`, `Win32_BaseBoard`, `Win32_VideoController`. 쿼리에 타임아웃을 두지 않으며, 클래스 실패는 debug 로그만 남기고 계속 진행(부분 결과 허용) — [rmmagent wmi_windows.go](https://raw.githubusercontent.com/amidaware/rmmagent/master/agent/wmi_windows.go)
- Tactical RMM의 `agent-agentinfo` 체크인은 로그온 사용자명, hostname, OS 문자열, 플랫폼, 총 RAM, 부팅 시각, 재부팅 필요 여부, 아키텍처를 보냄; `agent-disks`는 고정 디스크(Win32 `GetDriveTypeW`==3) 별 용량/사용률; `agent-publicip`는 공인 IP — [rmmagent checkin.go](https://github.com/amidaware/rmmagent/blob/master/agent/checkin.go); [rmmagent agent_windows.go](https://github.com/amidaware/rmmagent/blob/master/agent/agent_windows.go)
- osquery Windows 전용 테이블 스펙(체크리스트로 유용): `programs`, `patches`, `windows_update_history`, `windows_security_products`, `windows_security_center`, `bitlocker_info`, `tpm_info`, `deviceguard_status`, `chassis_info`, `disk_info`, `logical_drives`, `video_info`, `drivers`, `services`, `scheduled_tasks`, `logon_sessions`, `ntdomains`, `shared_resources`, `windows_optional_features`, `windows_firewall_rules`, `chocolatey_packages`, `authenticode`, `certificates`(공통), `windows_eventlog`, `wmi_bios_info`, `connectivity`, `kva_speculative_info`, `windows_crashes` 등 56개 — [osquery/specs/windows](https://github.com/osquery/osquery/tree/master/specs/windows)
- `Win32_ComputerSystemProduct.UUID`는 SMBIOS Type 1 UUID 필드에서 오며, `IdentifyingNumber`가 제품 일련번호 — [Win32_ComputerSystemProduct](https://learn.microsoft.com/en-au/windows/win32/cimwin32prov/win32-computersystemproduct) (검색 스니펫 확인)

**모니터 (EDID)**
- `root\wmi`의 `WmiMonitorID`는 VESA E-EDID의 Vendor/Product Identification 블록을 표현. `UserFriendlyName`, `SerialNumberID`, `ManufacturerName`은 **`uint16[]` 배열**이므로 문자열로 디코딩(ASCII 변환 후 0 패딩 제거)해야 함 — [WmiMonitorID class](https://learn.microsoft.com/en-us/windows/win32/wmicoreprov/wmimonitorid) (검색 스니펫 확인)
- 흔한 실수: PowerShell에서 `-notmatch 0`으로 0을 제거하면 배열 요소 단위로 작동해 일련번호 중간의 '0' 문자가 사라짐 → 끝의 0 패딩만 잘라야 함 — [powershell.org forum](https://forums.powershell.org/t/get-serial-from-wmimonitorid-wrong-format-what-am-i-missing/14867)

**소프트웨어 인벤토리**
- `Win32_Product`는 조회 시 MSI 공급자가 **모든 MSI 패키지에 대해 reconfiguration(일관성 검사)을 수행**해 느리고 부작용이 있음(KB974524). Microsoft는 대신 `HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall` 및 64bit에서 `Wow6432Node` 경로 모두를 읽으라고 권고. `Win32Reg_AddRemovePrograms`는 레거시이며 클라이언트에 없는 경우 많음 — [How to NOT Use Win32_Product in Group Policy Filtering (Microsoft AskDS)](https://learn.microsoft.com/en-us/archive/blogs/askds/how-to-not-use-win32_product-in-group-policy-filtering)
- `Win32_Product`는 MSI 설치 소프트웨어만 나열하므로 레지스트리 방식과 개수가 다름; 한 사례에서 레지스트리 항목이 Win32_Product의 2배 이상 — [Kansa PR #126](https://github.com/davehull/Kansa/pull/126); [Working with software installations (Microsoft)](https://learn.microsoft.com/en-us/powershell/scripting/samples/working-with-software-installations?view=powershell-7.3)
- Tactical RMM은 소프트웨어 목록을 `go-win64api`의 `InstalledSoftwareList()`에 위임하고 `Name/Version/Publisher/InstallDate/EstimatedSize(KB→bytes)/InstallSource/InstallLocation/UninstallString`를 사용; 필터링 없이 전부 전송 — [rmmagent software_windows_amd64.go](https://raw.githubusercontent.com/amidaware/rmmagent/master/agent/software_windows_amd64.go)
- `Get-AppxPackage -AllUsers`는 모든 사용자 계정의 Store/AppX 패키지를 나열하며 관리자 권한 필요 — [Get-AppxPackage](https://learn.microsoft.com/powershell/module/appx/get-appxpackage) (검색 스니펫 확인)

**Windows 업데이트/패치**
- WUA API `IUpdateSearcher::QueryHistory(startIndex, count)`는 업데이트 이벤트 이력을 내림차순으로 동기 반환; `GetTotalHistoryCount`로 총 개수 확인; count≤0 또는 startIndex<0이면 `WU_E_INVALIDINDEX` — [IUpdateSearcher::QueryHistory](https://learn.microsoft.com/en-us/windows/win32/api/wuapi/nf-wuapi-iupdatesearcher-queryhistory) (검색 스니펫 확인)
- `Win32_QuickFixEngineering.InstallDate`가 채워지지 않는다는 보고가 있으며, 우회로 WUA COM 객체 사용을 제안(커뮤니티 보고, 공식 문서 아님) — [generation-nt forum](https://www.generation-nt.com/reponses/wmi-et-vbs-entraide-164978.html)
- Tactical RMM 에이전트에는 별도 `wua_windows.go`, `patches_windows.go`가 있어 WUA 기반으로 패치를 다룸(파일명 기준) — [rmmagent/agent 디렉터리](https://github.com/amidaware/rmmagent/tree/master/agent)

**안티바이러스 상태**
- `root\SecurityCenter2`의 `AntiVirusProduct.productState`는 비트필드이며 커뮤니티 해석이 둘로 갈림: (a) hex 변환 후 가운데 두 자리 '10'=enabled, 마지막 두 자리 '00'=up-to-date — [jdhitsolutions](https://jdhitsolutions.com/blog/powershell/5187/get-antivirus-product-status-with-powershell/); (b) `0x1000` 비트=실시간 보호 켜짐, `0x10` 비트=정의 오래됨 — [NSClient++ CheckSecurity](https://nsclient.org/docs/reference/check/CheckSecurity/)
- Cloudflare 문서: "Microsoft는 SecurityCenter2 네임스페이스의 productState 디코딩을 지원하지 않는다" — [Cloudflare WARP antivirus posture check](https://developers.cloudflare.com/cloudflare-one/reusable-components/posture-checks/client-checks/antivirus/)
- 예: Windows Defender의 ProductState 393472(=0x60100)가 "disabled, up to date"로 디코딩된 사례 — [Microsoft Tech Community tag securitycenter2](https://techcommunity.microsoft.com/tag/securitycenter2)

**BitLocker**
- `root\CIMV2\Security\MicrosoftVolumeEncryption`의 `Win32_EncryptableVolume`; `GetConversionStatus()`의 `ConversionStatus`: 0 완전 복호화, 1 완전 암호화, 2 암호화 진행 중, 3 복호화 진행 중, 4 암호화 일시중지, 5 복호화 일시중지; `EncryptionPercentage` 0/100만으로 판단하지 말고 `ConversionStatus`를 쓰라고 명시. `GetProtectionStatus()`는 별개(0=Off, 1=On) — [Win32_EncryptableVolume](https://learn.microsoft.com/en-ie/Windows/Win32/secprov/win32-encryptablevolume); [GetConversionStatus](https://learn.microsoft.com/en-ie/Windows/Win32/secprov/getconversionstatus-win32-encryptablevolume) (검색 스니펫 확인)
- Surface 등 신규 기기는 "암호화되어 있으나 보호가 꺼진" 상태가 흔함(ProtectionStatus=0, ConversionStatus=1) — [Lansweeper community](https://community.lansweeper.com/t5/archive/bitlocker-conversion-status-volumestatus/m-p/47820)
- PowerShell `Get-WmiObject`로 메서드 호출 시 out 파라미터 없이 ReturnValue만 받는 문제가 보고됨(C#에서는 `InvokeMethod`의 out 파라미터 처리 필요) — [AutoIt forum](https://autoitscript.com/forum/topic/195324-solved-wmi-retrieve-out-params-from-method)

**라이선스/정품 인증**
- `SoftwareLicensingProduct`에서 `PartialProductKey`가 있는 행을 필터링해 `LicenseStatus`/`Description` 읽기; Windows만 보려면 ApplicationID로 필터 — [sharepointdiary](https://www.sharepointdiary.com/?p=65290)
- `LicenseStatus` 값: 0 Unlicensed, 1 Licensed, 2 OOB Grace, 3 OOT Grace, 4 Non-Genuine Grace, 5 Notification, 6 Extended Grace — [SolarWinds Thwack](https://thwack.solarwinds.com/discussion/54636/sam-windows-activations)
- Office는 설치 방식에 따라 `SoftwareLicensingProduct`(Name에 "office" 포함)에 나오거나, 별도 `OfficeSoftwareProtectionProduct` 클래스를 써야 함(BigFix 분석 사례); `ospp.vbs /dstatus`가 공통 fallback — [allthings.how](https://allthings.how/how-to-check-license-type-and-activation-status-of-microsoft-office/); [BigFix analysis](https://bigfix.me/analysis/details/2994711)

**로그온 사용자 / 유휴 / 잠금**
- `Win32_ComputerSystem.UserName`은 콘솔 로그온 사용자는 반환하지만 **RDP 사용자에 대해 null** 반환; 우회로 `query user` 사용 — [Microsoft Q&A 349657](https://learn.microsoft.com/en-us/answers/questions/349657/get-wmiobject-win32-computersystem-username-return)
- `WTSQueryUserToken`은 호출자가 LocalSystem + `SE_TCB_NAME` 권한이어야 하며, 서비스는 Session 0에서 실행됨; 다른 사용자 세션 조회는 Query Information 권한 필요 — [WTSQueryUserToken](https://learn.microsoft.com/en-us/windows/win32/api/wtsapi32/nf-wtsapi32-wtsqueryusertoken); [WTS_INFO_CLASS](https://learn.microsoft.com/en-us/windows/win32/api/wtsapi32/ne-wtsapi32-wts_info_class) (검색 스니펫 확인)
- `Win32_LoggedOnUser`는 콘솔/RDP 세션을 각각 별도 항목으로 나열(중복 제거 필요) — [SharpWMI docs](https://docs.specterops.io/ghostpack-docs/SharpWMI-mdx/actions/loggedon)
- `GetLastInputInfo`는 **호출한 세션의 입력 정보만** 제공(시스템 전체 아님) → Session 0 서비스에서 호출하면 사용자 유휴 시간을 알 수 없음; 반환 tick은 단조 증가가 보장되지 않음 — [GetLastInputInfo](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getlastinputinfo) (검색 스니펫 확인)
- Tactical RMM의 `LoggedOnUser()`는 psutil 스크립트(5초 타임아웃) → 실패 시 `wapf.ListLoggedInUsers()` fallback, 도메인 접두사 제거, 없으면 "None" — [rmmagent agent_windows.go](https://github.com/amidaware/rmmagent/blob/master/agent/agent_windows.go)

**USB 기기 이력**
- USB 관련 아티팩트 대부분은 레지스트리에 있음: `HKLM\SYSTEM\CurrentControlSet\Enum\USBSTOR`(장치 일련번호), `Enum\USB`(VID/PID), `MountedDevices`(드라이브 문자), `EMDMgmt`(볼륨명), 최초 연결 시각은 USBSTOR 키 타임스탬프 및 `C:\Windows\INF\setupapi.dev.log`(Win7부터 INF 폴더) — [Magnet Forensics USB artifact profile](https://www.magnetforensics.com/blog/artifact-profile-usb-devices/); [Forensic Focus forum](https://www.forensicfocus.com/forums/general/usbstor-registry-entries-windows-7)

### Inferences
- **권장 수집 매트릭스(설계안)** — 아래는 위 출처를 조합한 설계 제안이며, 괄호 안은 근거 출처 섹션:

| 항목 | 1차 소스 | 비고 |
|---|---|---|
| 시스템/제조사/모델 | `Win32_ComputerSystem`(Manufacturer, Model, TotalPhysicalMemory, Domain, PartOfDomain), `Win32_ComputerSystemProduct`(UUID, IdentifyingNumber, Vendor, Name) | UUID placeholder 처리는 §3 |
| BIOS/펌웨어 | `Win32_BIOS`(SerialNumber, SMBIOSBIOSVersion, ReleaseDate), `Win32_BaseBoard` | |
| CPU | `Win32_Processor`(Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, ProcessorId) | |
| RAM | `Win32_PhysicalMemory`(Capacity, Speed, Manufacturer, PartNumber, SerialNumber, BankLabel) | 슬롯 단위 |
| 디스크 | `Win32_DiskDrive`(Model, SerialNumber, Size, InterfaceType, MediaType), `Win32_LogicalDisk`(DriveType=3) | SSD/NVMe 여부는 `root\microsoft\windows\storage`의 `MSFT_PhysicalDisk.MediaType`(Win8+) |
| GPU | `Win32_VideoController`(Name, AdapterRAM, DriverVersion) | AdapterRAM은 32bit uint로 4GB 초과 GPU에서 부정확(일반적 알려진 문제; 출처 미확인) |
| NIC | `Win32_NetworkAdapterConfiguration`(IPEnabled=TRUE: MACAddress, IPAddress, DHCPEnabled, DNSDomain), `Win32_NetworkAdapter`(PhysicalAdapter) | 가상 어댑터 제외 필요 |
| 모니터 | `root\wmi` `WmiMonitorID` + `WmiMonitorBasicDisplayParams` | uint16[] 디코딩 |
| 프린터 | `Win32_Printer`(Name, DriverName, PortName, Network, Default) | |
| 배터리 | `Win32_Battery`, `root\wmi` `BatteryStatus`/`BatteryFullChargedCapacity` | 노트북만 |
| OS | `Win32_OperatingSystem`(Caption, Version, BuildNumber, OSArchitecture, InstallDate, LastBootUpTime, SerialNumber) + `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion`(DisplayVersion, UBR) | 업타임 = now − LastBootUpTime |
| 설치 SW | `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*`, `HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*`, 각 사용자 하이브 `HKU\<SID>\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*` | `SystemComponent=1`, `DisplayName` 없음, `ParentKeyName` 있는(업데이트) 항목 제외는 일반 관행 |
| Store/AppX | PowerShell `Get-AppxPackage -AllUsers` 호출 또는 WinRT `Windows.Management.Deployment.PackageManager.FindPackages()` | .NET 8은 `net8.0-windows10.0.17763.0` TFM으로 WinRT 투영 사용 가능(일반 지식, 미검증) |
| 패치 | WUA `IUpdateSearcher.QueryHistory` + `Win32_QuickFixEngineering` 보조 | QFE는 InstallDate 누락 보고 |
| AV | `root\SecurityCenter2` `AntiVirusProduct`(displayName, productState, pathToSignedProductExe) | productState 해석 불안정 → 원시값 저장 + 서버에서 해석 |
| BitLocker | `Win32_EncryptableVolume` + `GetProtectionStatus`/`GetConversionStatus` | 관리자/SYSTEM 필요 |
| 라이선스 | `SoftwareLicensingProduct`(PartialProductKey 있음), `OfficeSoftwareProtectionProduct` | |
| 로그온 사용자 | `WTSEnumerateSessions` → `WTSQuerySessionInformation(WTSUserName, WTSDomainName, WTSConnectState)` | WMI UserName은 RDP null |
| 유휴/잠금 | 사용자 세션에서 실행되는 트레이 헬퍼가 `GetLastInputInfo` + `WTSRegisterSessionNotification`(WTS_SESSION_LOCK/UNLOCK)으로 측정 후 파이프로 서비스에 보고 | Session 0에서는 불가 |
| USB 이력 | `Enum\USBSTOR`, `Enum\USB` 레지스트리 | 개인정보 영향 검토 필요(§5) |

- `productState`는 원시 정수 그대로 서버에 보내고, 서버에서 해석 테이블을 버전 관리하는 것이 안전하다(해석 규칙이 바뀌어도 에이전트 재배포 불필요).
- 소프트웨어 사용 측정(metering)은 ETW(`Microsoft-Windows-Kernel-Process`)나 WMI `__InstanceCreationEvent`/`Win32_ProcessStartTrace` 구독으로 가능하지만 이번 조사에서 1차 출처를 확보하지 못함(Gaps). 개인정보 최소수집 관점에서 "실행 파일명+누적 실행시간" 수준으로 제한하는 것이 적절하다.
- 짧은 C# 예(MI, 설계 제안):

```csharp
using Microsoft.Management.Infrastructure;
using var session = CimSession.Create(null);
foreach (var inst in session.QueryInstances(@"root\cimv2", "WQL",
    "SELECT Manufacturer, Model, TotalPhysicalMemory, Domain FROM Win32_ComputerSystem"))
{
    var model = inst.CimInstanceProperties["Model"]?.Value as string;
}
// EDID 디코딩: WmiMonitorID.SerialNumberID (ushort[])
static string Edid(ushort[] a) => a == null ? "" :
    new string(a.TakeWhile(c => c != 0).Select(c => (char)c).ToArray()).Trim();
```

```csharp
// Uninstall 키 열거 (64bit + WOW6432Node)
using Microsoft.Win32;
var roots = new[] {
  RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64),
  RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32) };
const string P = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
foreach (var root in roots) using (var k = root.OpenSubKey(P))
  foreach (var n in k?.GetSubKeyNames() ?? Array.Empty<string>())
    using (var s = k.OpenSubKey(n)) {
      if (s?.GetValue("DisplayName") is not string name) continue;
      if ((int?)s.GetValue("SystemComponent") == 1) continue;
      // DisplayVersion, Publisher, InstallDate(yyyyMMdd), UninstallString, EstimatedSize(KB), InstallLocation
    }
```

### Gaps
- `Win32_VideoController.AdapterRAM` 4GB 상한, `MSFT_PhysicalDisk` 네임스페이스, `WTSRegisterSessionNotification` 등은 일반적으로 알려진 사항이나 이번 조사에서 1차 출처를 확인하지 못함.
- 소프트웨어 사용 측정(ETW/프로세스 추적)의 구현 사례·오버헤드 수치 출처를 확보하지 못함.
- `PackageManager.FindPackages`를 SYSTEM 서비스에서 호출할 때의 제약(사용자별 패키지 열거 권한)을 확인하지 못함.
- `Win32_QuickFixEngineering`이 어떤 업데이트를 누락하는지에 대한 공식 문서를 찾지 못함.

---

## 3. 안정적 디바이스 식별 (Device Identity)

### Takeaway
단일 식별자는 모두 실패 사례가 있다(SMBIOS UUID는 클론 VM/OEM placeholder, MachineGuid는 sysprep generalize 시 재생성 추정, MAC은 어댑터 교체). 상용 도구(AlloyScan)는 UUID blocklist를 두고, MeshAgent는 MAC 변경 시 NodeID를 바꾸며, GLPI는 hostname+타임스탬프를 쓴다. 에이전트가 최초 등록 시 서버가 발급한 랜덤 `DeviceId`를 로컬에 저장하고, 하드웨어 지문(UUID, BIOS serial, baseboard serial, MAC 집합, MachineGuid)은 "중복/재설치 판정용 보조 신호"로 보내는 구조가 가장 견고하다.

### Cited Findings
- `Win32_ComputerSystemProduct.UUID`는 SMBIOS Type 1 UUID(펌웨어 제공); Wine의 Windows 7 bottle에서는 `00000000-0000-0000-0000-000000000000` 반환 사례, 그 값을 고유 PC ID로 쓰는 앱이 있어 취약 — [Win32_ComputerSystemProduct](https://learn.microsoft.com/en-au/windows/win32/cimwin32prov/win32-computersystemproduct); [WineHQ bug mail](https://mail.winehq.org/mailman3/hyperkitty/list/wine-bugs@winehq.org/message/F3X5VDUU5SLPMT43A7UYQ5TSWGLMFAYO)
- AlloyScan은 BIOS UUID를 Windows 기기의 안정적 식별 신호로 쓰되, "일부 하드웨어 벤더와 가상화 플랫폼은 중복 또는 placeholder UUID를 반환"하여 서로 다른 기기가 같은 자산으로 보이고 감사 이력이 덮어써질 수 있으므로 **UUID blocklist**(all-zero, all-F 등)를 운영 — [AlloyScan UUID blocklist](https://docs.alloysoftware.com/alloyscan/help/web/uuid-blocklist.htm)
- VM 클론 시 BIOS UUID 중복: VMware는 "I copied it" 선택 시 새 식별자 생성, "I moved it"은 유지; VirtualBox full clone(새 MAC)은 새 BIOS UUID; **Hyper-V의 "create a new unique ID"는 VM GUID만 바꾸고 게스트의 BIOS UUID는 바꾸지 않음** — [BlackFog KB](https://www.blackfog.com/knowledge-base/multiple-devices-identical-vmware/); [VMware community](https://communities.vmware.com/t5/vSphere-Management-SDK/Dealing-with-duplicate-UUID-bios/m-p/2434524)
- 클론 VM의 동일 UUID는 모니터링 도구에서 별개 호스트를 하나로 취급하는 "topology churn"을 유발 — [Quest KB 4257931](https://support.quest.com/kb/4257931/cloned-virtual-machine-with-same-uuid-can-cause-a-foglight-topology-churn)
- `HKLM\SOFTWARE\Microsoft\Cryptography\MachineGuid`: Microsoft Q&A 답변은 "설치마다 MachineGuid가 바뀐다면 sysprep generalize가 실행된 것"이라며 generalize가 이 GUID를 포함한 시스템 고유 데이터를 지운다고 설명(공식 문서가 아닌 Q&A 답변). 검증은 `HKLM\SYSTEM\Setup`의 `CleanupState`(2)/`GeneralizationState`(7)와 Panther `setupact.log` — [Microsoft Q&A 1489139](https://learn.microsoft.com/en-us/answers/questions/1489139/identifying-unique-windows-installation)
- generalize로도 지워지지 않는 것이 있음: Intune MDM 등록/Entra join 인증서와 ID가 살아남아 클론 기기에서 중복 ID 발생 — [Sysprep on MDM-enrolled device (Microsoft)](https://learn.microsoft.com/en-us/troubleshoot/mem/intune/device-enrollment/troubleshoot-sysprep-windows-10-device-enrolled-mdm)
- MeshAgent: `skipmaccheck` 옵션이 있으며, 이는 로컬 MAC 주소 변경 시 NodeID가 바뀌는 동작을 끄는 옵션(즉 기본적으로 MAC 변경이 NodeID에 영향); 별도의 "agent identity certificate"가 존재 — [MeshAgent readme](https://github.com/Ylianst/MeshAgent/blob/master/readme.md)
- GLPI agent 1.7.1 Windows의 기본 deviceid는 `shortname-YYYY-MM-DD-hh-mm-ss` 패턴(사용자 보고) — [GLPI forum](https://forum.glpi-project.org/viewtopic.php?id=289270); JSON 프로토콜에서 `deviceid`는 필수 필드 — [GLPI JSON protocol: inventory](https://glpi-json-protocol.readthedocs.io/en/latest/inventory.html)
- Tactical RMM은 `HKLM\SOFTWARE\TacticalRMM`에 `AgentID`, `AgentPK`, `Token`, `BaseURL`, `ApiURL`, `Cert`, `Proxy`, `Nats*` 등을 저장하고 제거 시 키 삭제 — [rmmagent agent_windows.go](https://github.com/amidaware/rmmagent/blob/master/agent/agent_windows.go)

### Inferences
- **설계안**: (1) 최초 enrollment 시 서버가 `DeviceId`(GUID v4 또는 ULID)를 발급 → 에이전트가 `HKLM\SOFTWARE\<Vendor>\Agent\DeviceId` + ProgramData 파일(이중 저장)에 보관. (2) 매 체크인에 하드웨어 지문 `{smbiosUuid, biosSerial, baseboardSerial, machineGuid, macs[], hostname, osInstallDate, windowsProductId}`를 동봉. (3) 서버는 "같은 DeviceId + 전혀 다른 지문" → 이미지 복제 의심(새 DeviceId 재발급 명령), "다른 DeviceId + 같은 지문(blocklist 제외)" → 재설치/재이미징으로 보고 자산 병합 후보로 큐잉. (4) placeholder blocklist: 전부 0, 전부 F, "To be filled by O.E.M.", "System Serial Number", "Default string", "INVALID" 등(벤더 문자열은 AlloyScan처럼 운영 중 추가 가능해야 함 — 구체 문자열 목록의 1차 출처는 미확보).
- 클론된 VM 템플릿에 에이전트가 미리 설치돼 있으면 DeviceId까지 복제된다 → 설치기에 `/generalize`에 해당하는 "ID 초기화" 모드(또는 첫 부팅 시 hostname/MAC/UUID가 등록 당시와 모두 다르면 자동 재등록) 필요.

### Gaps
- MachineGuid가 sysprep generalize로 재생성된다는 **Microsoft 공식 문서**를 찾지 못함(커뮤니티 답변만 확인).
- OEM placeholder 일련번호 문자열의 정식 목록(예: "To be filled by O.E.M.")을 1차 출처로 확인하지 못함.

---

## 4. 에이전트–서버 프로토콜 설계

### Takeaway
공개 참조 모델들은 공통적으로 (a) 443/HTTPS 단일 포트, (b) 토큰/인증서 기반 에이전트 식별, (c) 서버 지시 재시도(expiration) 및 랜덤 지터, (d) 체크인을 여러 소형 메시지로 분할하는 설계를 따른다. 5k–20k 에이전트 규모에서는 SignalR 상시 연결보다 "짧은 heartbeat + 서버 명령 큐 롱폴링/풀링" 또는 NATS/WebSocket 같은 전용 메시징이 운영 부담이 낮다.

### Cited Findings
- Tactical RMM: v0.14.0(2022-07-07) 이전에는 에이전트가 공개 포트 4222로 NATS 접속; 이후 **NATS over WebSocket on 443**으로 전환해 노출 포트를 443 TCP 하나로 줄임. TLS 스택 문제가 있는 에이전트는 레지스트리 값으로 4222 TCP fallback 가능. NATS는 서버 내부 서비스 간 통신용으로 4222에서 계속 수신, 로컬 9235(websocket) 추가 — [Tactical RMM settings override](https://docs.tacticalrmm.com/functions/settings_override/); [tacticalrmm releases](https://github.com/amidaware/tacticalrmm/releases?page=4)
- Tactical RMM 체크인은 모드별 소형 메시지(`agent-hello`: id+version, `agent-winsvc`, `agent-agentinfo`, `agent-wmi`, `agent-disks`, `agent-publicip`)로 분할되어 msgpack 인코딩 후 publish되며, 각 메시지 사이에 100–400ms 랜덤 sleep을 둠. 소프트웨어 목록은 별도 REST `POST /api/v3/software/`로 전송 — [rmmagent checkin.go](https://github.com/amidaware/rmmagent/blob/master/agent/checkin.go); [rmmagent agent_windows.go](https://github.com/amidaware/rmmagent/blob/master/agent/agent_windows.go)
- MeshAgent는 `wss://host:443/agent.ashx`로 WebSocket Secure 접속, 유휴 시 Ping/Pong keepalive, 리버스 프록시 뒤에서 클라이언트 인증서(mTLS) 옵션 지원 — [MeshAgent readme](https://github.com/Ylianst/MeshAgent/blob/master/readme.md)
- GLPI JSON 프로토콜: 인벤토리 제출은 `{action, deviceid, content, itemtype(기본 Computer)}`; 서버 응답에 `status`와 `expiration`이 있으며, 서버 과부하 오류 시 에이전트는 인벤토리를 보관했다가 `expiration`에 지정된 시점에 재시도해야 함 — [GLPI JSON protocol: inventory](https://glpi-json-protocol.readthedocs.io/en/latest/inventory.html)
- Lansweeper LsAgent는 릴레이 서버로 HTTPS(TLS 1.2) 전송, 릴레이는 데이터를 암호화 저장 후 스캔 서버가 가져가면 삭제; 스캔 서버는 `lsagentrelay.lansweeper.com:443` 아웃바운드 필요 — [LsAgent docs](https://docs.lansweeper.com/classic/docs/introduction-to-lsagent-for-windows-linux-and-mac) (검색 스니펫 확인)
- SignalR 스케일아웃: 백플레인은 모든 메시지를 모든 노드로 전달하므로 단일 노드 대비 최대 처리량이 낮아짐; 롱폴링 클라이언트도 커서로 스트림 위치를 추적해 다른 서버에 재접속 시 이어받음; Azure SignalR Service가 관리형 대안 — [Scaleout in SignalR (Microsoft)](https://learn.microsoft.com/en-us/aspnet/signalr/overview/performance/scaleout-in-signalr) (검색 스니펫 확인)
- SignalR은 WebSocket 우선, SSE/롱폴링 fallback; 롱폴링은 오버헤드·지연이 큰 최후 수단 — [Red Gate: SignalR intro](https://www.red-gate.com/simple-talk/dotnet/asp.net/an-introduction-to-real-time-communication-with-signalr)

### Inferences
- **권장 프로토콜(설계안)**:
  1. **Enrollment**: `POST /api/v1/enroll` with `{enrollmentKey(일회성/만료형, MSI 속성으로 주입), fingerprint, hostname, agentVersion}` → 서버가 `{deviceId, deviceToken(장기, 회전 가능)}` 반환. 토큰은 DPAPI로 보호해 저장(§5). mTLS를 쓰려면 enrollment 응답에서 서버가 발급한 클라이언트 인증서를 `LocalMachine\My`에 설치(관리형 PKI 없이도 서버 자체 CA로 가능)하되, 프록시/TLS 검사 장비가 있는 고객망에서 mTLS가 깨지는 경우가 많으므로 토큰을 기본, mTLS를 옵션으로.
  2. **Heartbeat**: `POST /api/v1/devices/{id}/heartbeat` 1–5분 주기(랜덤 지터 ±20%), 바디는 `{agentVersion, uptime, loggedOnUser, lastInventoryHash, pendingQueueLen}`; 응답으로 `{nextHeartbeatSec, commands[], configVersion}` → 서버 주도 스케줄링과 명령 큐를 한 번의 왕복으로 처리(Tactical RMM의 `agent-hello` + GLPI의 `expiration` 패턴 결합).
  3. **Inventory**: 섹션별 분할 전송(`hardware`, `software`, `patches`, `security`, `users`)으로 각 섹션에 SHA-256 해시를 붙여 서버가 보유한 해시와 같으면 바디 생략(delta). 완전 인벤토리는 하루 1회 강제. gzip/br 압축(ASP.NET Core `RequestDecompression` 미들웨어). 단일 요청 상한은 Kestrel 기본 30MB보다 낮게(예: 4MB) 두고 소프트웨어 목록은 페이징.
  4. **Idempotency**: 각 제출에 `Idempotency-Key`(ULID) 헤더; 서버는 `(deviceId, key)` 유니크 인덱스로 중복 삽입 방지. 에이전트 시계가 틀릴 수 있으므로 서버 수신 시각을 정본으로 하고 에이전트 시각은 `clientTime`으로만 기록(clock skew는 응답 `Date` 헤더와 비교해 로그).
  5. **Offline queue**: `%ProgramData%\<Vendor>\Agent\queue\*.json.gz` 파일 큐, 상한(예: 50MB/200건) 초과 시 오래된 인벤토리부터 폐기(최신 상태가 중요). 재시도는 지수 백오프(30s→…→1h, 지터) + 서버 `Retry-After` 존중.
  6. **명령**: 서버→에이전트 명령은 `{commandId, type, args, issuedAt, expiresAt, signature}`로, 허용 타입 화이트리스트(`RunInventory`, `UpdateAgent`, `SetConfig`, `CollectLogs`)만 구현. 결과는 `POST /commands/{id}/result`.
  7. **규모**: 10k 에이전트 × 2분 heartbeat ≈ 83 req/s로 단일 ASP.NET Core 인스턴스로 충분한 수준; 인벤토리 폭주 방지를 위해 서버가 `nextInventoryAt`를 분산 배정. SignalR은 "즉시 명령"(원격 실행)이 필수일 때만 추가.
- 상용 제품 다수가 443 단일 포트 + 아웃바운드 전용 연결을 쓰는 이유는 고객 방화벽 변경 없이 배포하기 위함 → 서버가 에이전트로 인바운드 접속하는 설계는 피한다.

### Gaps
- 5k–20k 에이전트 규모의 실제 서버 부하 수치나 벤더의 권장 heartbeat 간격(분 단위)을 공식 문서에서 확인하지 못함(Tactical RMM의 기본 check-in 간격도 코드 조각에서는 확인 불가).
- 델타 인벤토리의 구체 구현(해시 단위)을 공개한 상용 제품 문서를 찾지 못함.

---

## 5. 보안/하드닝 (코드 서명, 실행 계정, 안티탬퍼, 토큰 저장, 한국 컴플라이언스)

### Takeaway
2024년 이후 EV 인증서도 SmartScreen 즉시 신뢰를 주지 않으므로 서명은 "무결성/출처 증명" 목적이며 평판은 다운로드 이력으로 쌓인다(Azure Artifact Signing[구 Trusted Signing]이 권장 경로). 토큰 저장은 DPAPI `LocalMachine` 범위가 동일 PC의 모든 프로세스에서 복호화 가능하므로 엔트로피+ACL(SYSTEM/Administrators만)로 보강해야 한다. 한국 법제는 PIPA 제3조 최소수집과 ISMS-P 3.1.2가 적용되며 PC 모니터링 전용 가이드라인은 확인되지 않았다.

### Cited Findings
- Microsoft 코드 서명 옵션: Azure Trusted Signing은 **Azure Artifact Signing**으로 개명, 비-Store 배포에 권장(월 약 $10, 하드웨어 토큰 불필요), 조직은 미국/캐나다/EU/영국만 가능(개인은 미국/캐나다). **Artifact Signing은 즉시 SmartScreen 신뢰를 제공하지 않으며**, 2024년부터 EV는 OV와 동일하게 취급 — [Code signing options (Microsoft)](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/code-signing-options); [SmartScreen reputation (Microsoft)](https://learn.microsoft.com/et-ee/windows/apps/package-and-deploy/smartscreen-reputation) (검색 스니펫 확인). 2025년 Microsoft Q&A 답변과 Velopack 문서는 여전히 EV/Trusted Signing이 SmartScreen을 우회한다고 하나 현행 Microsoft 문서와 상충 — [Microsoft Q&A 1989290](https://learn.microsoft.com/en-us/answers/a/1989290); [Velopack signing](https://docs.velopack.io/packaging/signing)
- DPAPI `DataProtectionScope.LocalMachine`: "컴퓨터에서 실행되는 **어떤 프로세스든** 데이터를 unprotect할 수 있음", 신뢰되지 않는 사용자가 접근할 수 없는 서버 앱용 — [DataProtectionScope enum](https://msdn2.microsoft.com/en-us/library/32y3d3x0); `DpapiDataProtector`: LocalMachine 범위에서 purpose(엔트로피)를 신중히 지정하지 않으면 purpose를 아는 다른 앱이 복호화 가능 — [DpapiDataProtector (.NET 8)](https://learn.microsoft.com/ko-kr/dotnet/api/system.security.cryptography.dpapidataprotector?view=net-8.0) (검색 스니펫 확인)
- `WTSQueryUserToken` 등 사용자 세션 조작 API는 LocalSystem + SE_TCB 권한 필요 — [WTSQueryUserToken](https://learn.microsoft.com/en-us/windows/win32/api/wtsapi32/nf-wtsapi32-wtsqueryusertoken) (검색 스니펫 확인)
- MSI `ARPNOREMOVE`: 제어판 "프로그램 제거" 기능을 비활성화하며 Uninstall 키에 `UninstallString`이 기록되지 않음; 관리자 커스터마이징 변환(transform)용으로 제시 — [ARPNOREMOVE property](https://learn.microsoft.com/en-us/windows/win32/msi/arpnoremove) (검색 스니펫 확인)
- 서비스 복구: WiX Util `ServiceConfig`의 `FirstFailureActionType`(필수)/`SecondFailureActionType`/`ThirdFailureActionType` ∈ {none, reboot, restart, runCommand}, `ResetPeriodInDays`, `RestartServiceDelayInSeconds`; 서비스가 설치 전 존재하지 않으면 설치 실패 — [WiX util:ServiceConfig](https://wixtoolset.org/docs/schema/util/serviceconfig)
- Tactical RMM: 에이전트 설정·토큰을 `HKLM\SOFTWARE\TacticalRMM`에 평문 레지스트리 값(`Token`, `Cert`)으로 저장 — [rmmagent agent_windows.go](https://github.com/amidaware/rmmagent/blob/master/agent/agent_windows.go)
- 한국: 개인정보 보호법 제3조 — 처리 목적을 명확히 하고 목적에 필요한 범위에서 최소한의 개인정보만 적법·정당하게 수집, 사생활 침해 최소화 방식으로 처리, 익명/가명 처리 우선 — [개인정보 보호법 제3조 (IT위키)](https://itwiki.kr/w/%EA%B0%9C%EC%9D%B8%EC%A0%95%EB%B3%B4_%EB%B3%B4%ED%98%B8%EB%B2%95_%EC%A0%9C3%EC%A1%B0); ISMS-P 인증기준 3.1.2 개인정보 수집 제한 — [ISMS-P 3.1.2 (IT위키)](https://itwiki.kr/w/ISMS-P_%EC%9D%B8%EC%A6%9D_%EA%B8%B0%EC%A4%80_3.1.2.%EA%B0%9C%EC%9D%B8%EC%A0%95%EB%B3%B4_%EC%88%98%EC%A7%91_%EC%A0%9C%ED%95%9C)
- 개인정보위·고용노동부 「개인정보 보호 가이드라인(인사·노무편)」 2023.1 개정: 고용 단계별 최소수집, 근태 확인 목적 CCTV 설치 시 근로자 의견 청취; 근로자 감시 등 목적 외 이용은 법 위반 소지 — [바이라인네트워크 2023-01](https://byline.network/2023/01/0131_02/); [보안뉴스](https://m.boannews.com/html/detail.html?idx=118661)
- 국내 다수 기업이 "생산성 관리" 명목으로 직원 PC 활동을 실시간 추적하는 프로그램을 도입해 사생활 침해 비판이 제기됨(언론 보도) — [쿠키뉴스](https://www.kukinews.com/article/view/kuk202510150228)

### Inferences
- **실행 계정**: WMI 전 클래스(특히 `Win32_EncryptableVolume`, `SecurityCenter2`), WTS 세션 조회, HKU 하이브 열람, 자동 업데이트 설치에는 사실상 LocalSystem이 필요하다. LocalSystem으로 두되 (a) 에이전트가 받는 명령 타입을 화이트리스트로 제한, (b) 임의 스크립트 실행 기능은 기본 비활성(필요 시 서버 서명 + 감사 로그), (c) 파이프/파일 ACL로 비관리자 사용자의 설정 변경을 차단한다.
- **토큰 저장**: `ProtectedData.Protect(token, entropy, DataProtectionScope.LocalMachine)` + 엔트로피는 바이너리 내 상수가 아닌 설치 시 생성해 `HKLM\...\Agent`(SYSTEM/Administrators Full, Users 없음) ACL 하에 저장. 이렇게 해도 로컬 관리자는 복호화 가능하므로, 서버 측에서 토큰을 `deviceId`에 바인딩하고 하드웨어 지문 불일치 시 거부(토큰 탈취 후 다른 PC에서 재사용 방지)하는 것이 실질적 방어선이다. Tactical RMM처럼 평문 레지스트리 저장은 피한다.
- **안티탬퍼**: `ARPNOREMOVE`는 제어판 경로만 막고 `msiexec /x`는 막지 못하므로, "제거 비밀번호"는 MSI의 `InstallExecuteSequence`에 제거 시 조건부 custom action(`REMOVE="ALL" AND NOT UNINSTALL_TOKEN="<서버발급>"` → 실패)을 넣어 구현한다(이 패턴의 1차 출처는 미확보). 서비스 복구는 WiX `util:ServiceConfig`로 1/2/3차 모두 `restart`, `ResetPeriodInDays=1`. 서비스 SDDL로 `Users`의 `SERVICE_STOP` 권한 제거도 고려(출처 미확보).
- **한국 컴플라이언스 적용**: 로그온 사용자 ID, USB 이력, 소프트웨어 사용시간, 브라우저/문서 접근 이력은 "근로자 개인정보"이므로 (1) 수집 항목을 자산관리 목적으로 한정한 "수집 항목표"를 문서화·공지, (2) USB 이력·사용 측정은 기본 OFF(옵트인 정책), (3) 문서명/URL/키 입력 같은 콘텐츠 수준 데이터는 수집 금지, (4) 보존 기간 설정 및 자동 파기, (5) 수집 항목 변경 시 근로자 고지. 이는 PIPA 제3조·ISMS-P 3.1.2의 일반 원칙에서 도출한 설계 지침이며 PC 에이전트 전용 공식 가이드라인은 확인되지 않았다.

### Gaps
- 개인정보위/KISA의 "업무용 PC 모니터링/자산관리 에이전트" 전용 가이드라인 또는 수집 항목 예시를 찾지 못함(인사·노무편 원문 확인 필요).
- MSI 제거 비밀번호 custom action, 서비스 SDDL 하드닝의 1차 출처를 확보하지 못함.
- Azure Artifact Signing이 한국 조직에 제공되는지(현행 문서는 미국/캐나다/EU/영국만 명시) 확인 필요 → 한국 법인은 OV 인증서 + 타임스탬프가 현실적 대안일 수 있음.

---

## 6. 패키징·배포·자동 업데이트

### Takeaway
공개/상용 에이전트는 예외 없이 MSI(또는 Inno Setup) + `/quiet` + 공개 속성(서버 URL, 등록 키)으로 무인 설치를 지원한다. MSIX는 서비스 포함 패키지에 제약(패키지 외부 서비스 의존 불가, 관리자 설치 필요)이 있어 에이전트에는 비권장이다. 자동 업데이트는 서명 검증된 패키지를 다운로드해 분리 프로세스로 설치하고(자기 자신을 덮어쓰지 않음), 실패 시 롤백·재시도를 둔다.

### Cited Findings
- GLPI Agent MSI: 기본은 GUI, 무인 설치는 `msiexec /i GLPI-Agent-1.x-x64.msi /quiet SERVER=<URL>`; 옵션명은 **대소문자 구분**, `=` 앞뒤 공백 불가, 공백 있는 값은 따옴표, **PowerShell을 명령 해석기로 쓰지 말 것**(환경 컨텍스트 변경 문제); 기타 `ADD_FIREWALL_EXCEPTION=1`, `LOCAL`, `LAZY`, `LISTEN`, `ADDLOCAL=feat_DEPLOY,feat_COLLECT` — [GLPI Agent Windows installer docs 1.9](https://glpi-agent.readthedocs.io/en/1.9/installation/windows-command-line.html) (검색 스니펫 확인). 커뮤니티 예: `RUNNOW=1`, 자체서명 HTTPS에 `NO_SSL_CHECK=1` — [NethServer community](https://community.nethserver.org/t/glpi-agent-installation-on-a-windows-10-client/20609)
- Lansweeper: Windows/Linux/Mac용 LsAgent 무인 설치 문서 존재; 설치기가 .NET 4.8 자동 설치 시도 — [Silently installing LsAgent](https://docs.lansweeper.com/classic/docs/silently-installing-lsagent-on-a-windows-linux-or-mac-computer); [LsAgent requirements](https://docs.lansweeper.com/classic/docs/lsagent-installation-requirements) (검색 스니펫 확인)
- Tactical RMM 자동 업데이트: 1–15초 랜덤 대기 → 멈춘 업데이트 프로세스 kill → `WinTmpDir`에 15분 타임아웃으로 다운로드 → **Inno Setup** 설치기를 `cmd.exe /C ... /VERYSILENT /LOG=...` 분리(detached) 프로세스로 실행(msiexec 아님) — [rmmagent agent_windows.go](https://github.com/amidaware/rmmagent/blob/master/agent/agent_windows.go)
- MeshAgent 업데이트 제어 옵션: `disableUpdate`(자기 업데이트 차단), `forceUpdate`, `fakeUpdate`(동일 버전 재설치), `logUpdate`, `noUpdateCoreModule`(서버가 푸시하는 JS 코어 수락 거부) — [MeshAgent readme](https://github.com/Ylianst/MeshAgent/blob/master/readme.md)
- WiX: `ServiceInstall`로 서비스 시작 유형/계정 설정, 같은 컴포넌트에서 `KeyPath="yes"`인 파일이 서비스 실행 파일; 커널/시스템 드라이버 서비스는 Windows Installer 미지원 — [rareese.com WiX service post](https://rareese.com/blog/installing-and-starting-windows-services-with-wix); Util `ServiceConfig`로 실패 복구 정책 — [WiX util:ServiceConfig](https://wixtoolset.org/docs/schema/util/serviceconfig)
- MSI 무인 설치의 공개 속성은 **전부 대문자** 이름이어야 명령줄로 전달 가능; Windows Installer 3.0부터 `/quiet` 사용 가능 — [Flexera silent installs](https://resources.flexera.com/web/pdf/archive/silent_installs.pdf); [Advanced Installer WiX silent](https://cdn.advancedinstaller.com/versus/wix-toolset/msi-package-silent-install-uninstall-wix.html)
- MSIX 서비스 제약: 서비스 의존성은 패키지 내부 서비스로만 해결 가능(패키지 간 의존 불가), 서비스 포함 MSIX 설치는 관리자 권한 필요, MSIX Packaging Tool에서 서비스 실행 경로 편집 불가 — [Advanced Installer MSIX migration limitations](https://www.advancedinstaller.com/msix-migration-limitations-and-solutions.html); [PowerShell MSIX module limitations](https://www.powershellgallery.com/packages/msix/0.75.0/Content/MSIX.Limitations.ps1)
- Windows 서비스 수동 설치/복구: `sc.exe create ... binpath=`, `sc.exe failure`, `sc.exe qfailure` — [dotnet/docs windows-service.md](https://github.com/dotnet/docs/blob/main/docs/core/extensions/windows-service.md)

### Inferences
- **패키지**: WiX v5 MSI, `MajorUpgrade` 요소로 in-place 업그레이드, 공개 속성 `SERVERURL`, `ENROLLKEY`, `PROXY`, `TENANTID`(모두 대문자)를 `RegistryValue`로 `HKLM\SOFTWARE\<Vendor>\Agent`에 기록. GPO(컴퓨터 구성 소프트웨어 설치 또는 시작 스크립트 `msiexec /i agent.msi /qn SERVERURL=... ENROLLKEY=...`), Intune Win32 앱(.intunewin), SCCM 애플리케이션 모두 같은 명령줄로 통일. `.NET 8` self-contained이면 사전 요구사항이 없어 배포 실패율이 낮다(.NET Framework 4.8 선택 시 Lansweeper처럼 부트스트래퍼로 4.8 설치 필요).
- **자동 업데이트(설계안)**: 서버 heartbeat 응답에 `{version, url, sha256, signature}` → 에이전트가 임시 폴더에 다운로드 → SHA-256 + Authenticode 서명자 검증(`X509Certificate.CreateFromSignedFile` 또는 `WinVerifyTrust`) → `msiexec /i new.msi /qn /l*v log` 를 분리 프로세스로 실행(Tactical RMM 패턴) → MSI `MajorUpgrade`가 구 서비스 중지/교체/시작 수행 → 신버전 첫 heartbeat에서 버전 보고로 완료 확인 → 일정 시간 내 heartbeat가 없으면 서버가 "업데이트 실패" 플래그. 롤백은 이전 MSI를 로컬 캐시에 보관해 신버전이 N회 연속 크래시(SCM 복구 횟수 또는 자체 기록)하면 재설치. 단계적 배포(ring: 1% → 10% → 100%)는 서버 측 그룹 지정으로.
- 별도 "Updater" 서비스를 두면 메인 에이전트가 손상돼도 복구 가능하지만 서비스 수·공격 표면이 늘어난다. MSI `MajorUpgrade` + SCM 복구 조합으로 충분한 경우가 많다.
- 제거 보호는 §5 참조; GPO/Intune로 배포한 경우 "할당 해제 시 자동 제거"를 켜 두면 운영자가 의도치 않게 전체 제거할 수 있으므로 주의.

### Gaps
- WiX v5의 `ServiceInstall`/`ServiceConfig` 원문 페이지(docs.firegiant.com)를 fetch하지 못해 v4/v5 간 속성 변경 여부는 확인 못함(검색 결과는 v3/v4와 동일한 정의를 보여줌).
- 상용 에이전트의 롤백 정책(실패 감지 기준, ring 비율) 공식 문서를 찾지 못함.

---

## 7. 관측성 (로그, 헬스 heartbeat, 버전 보고, 크래시 리포팅)

### Takeaway
Microsoft 호스팅 모델은 Windows Event Log 로깅을 기본 제공하며, 참조 에이전트들은 heartbeat에 버전·OS·부팅 시각·재부팅 필요 여부를 실어 서버에서 fleet 건강 상태를 집계한다. 크래시는 SCM 복구 횟수 + 로컬 롤링 파일 로그 + 서버로의 "마지막 오류 요약" 전송으로 커버한다.

### Cited Findings
- `Microsoft.Extensions.Hosting.WindowsServices` 호스트는 `Logging:EventLog`(`SourceName`, `LogName`, `LogLevel`)로 Windows 이벤트 로그에 기록, 기본 Warning 이상 — [dotnet/docs windows-service.md](https://github.com/dotnet/docs/blob/main/docs/core/extensions/windows-service.md)
- 서비스 재시작을 원하면 `Environment.Exit(비0)`로 종료해야 SCM 실패 복구가 작동(정상 `StopHost`는 "성공 종료"로 간주되어 재시작되지 않음) — [같은 문서](https://github.com/dotnet/docs/blob/main/docs/core/extensions/windows-service.md)
- Tactical RMM `agent-hello`는 `{agentId, version}`만 전송; `agent-agentinfo`에 boot time, reboot-required 포함 — [rmmagent checkin.go](https://github.com/amidaware/rmmagent/blob/master/agent/checkin.go)
- Tactical RMM은 업데이트 설치 로그를 `/LOG=` 파일로 남기고, `WinTmpDir` 레지스트리 값으로 임시 경로를 설정 — [rmmagent agent_windows.go](https://github.com/amidaware/rmmagent/blob/master/agent/agent_windows.go)
- MeshAgent는 `logUpdate` 옵션으로 업데이트 상태를 로깅하고 JS 디버거 포트를 제공 — [MeshAgent readme](https://github.com/Ylianst/MeshAgent/blob/master/readme.md)
- osquery의 `windows_crashes` 테이블(WER 크래시 덤프 메타데이터)과 `windows_eventlog` 테이블이 존재 — [osquery/specs/windows](https://github.com/osquery/osquery/tree/master/specs/windows)

### Inferences
- **로깅**: `ILogger` → (1) Event Log(Warning+), (2) `%ProgramData%\<Vendor>\Agent\logs\agent-yyyyMMdd.log` 롤링(Serilog/NLog, 7일×10MB 상한), (3) heartbeat에 `lastError{code, message, at}` 요약만 포함. 서버 명령 `CollectLogs`로 필요 시 로그 zip 업로드.
- **헬스 지표(heartbeat)**: `agentVersion`, `runtimeVersion`, `osBuild`, `lastBootUpTime`, `rebootPending`, `lastInventoryAt/Result`, `queueDepth`, `wmiErrors[]`(클래스별 실패 — Tactical RMM처럼 부분 실패를 허용하되 보고), `cpuMsLastRun`, `peakWorkingSetMB`.
- **크래시 리포팅**: `AppDomain.UnhandledException`/`TaskScheduler.UnobservedTaskException` 핸들러에서 스택을 파일에 기록 후 `Environment.Exit(1)`; SCM 복구가 재시작; 다음 기동 시 "crash marker" 파일을 읽어 서버에 보고. WER 덤프는 `HKLM\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps\<agent.exe>` 설정으로 로컬 수집 가능(일반 지식, 출처 미확인).

### Gaps
- 상용 에이전트의 로그 보존 정책·크래시 리포팅 구현에 대한 공개 문서를 찾지 못함.

---

## 8. 성능 footprint 목표 (상용 에이전트 인용치)

### Takeaway
벤더 문서는 "에이전트 런타임 CPU%/RAM MB"를 거의 공개하지 않고 **호스트 최소 사양**(1GHz CPU, 512MB–2GB RAM, 수백 MB–3GB 디스크)만 제시한다. 따라서 목표치는 자체 측정 기준으로 정해야 하며, 공개 소스 에이전트(Tactical RMM)는 WMI 쿼리에 타임아웃을 두지 않고 체크인 메시지 간 100–400ms 지터를 두는 정도의 완화만 한다.

### Cited Findings
- Lansweeper IT Agent(Discovery agent) 요구사항: 최소 x64 1GHz 싱글코어, 2GB RAM, 500MB 디스크; 권장 x64 2GHz 듀얼코어, 8GB RAM, 1GB 디스크; 네트워크 100Mbps 최소/1Gbps 권장 — [Lansweeper discovery agent requirements](https://docs.lansweeper.com/docs/discovery-agent-requirements) (검색 스니펫 확인)
- Lansweeper 스캔 서버 측: LsAgent 스캔 처리를 위해 최소 1GB 또는 10% 여유 메모리, 4GB 여유 권장 — [Lansweeper blog](https://www.lansweeper.com/blog/pro-tips/schedule-lsagent-to-run-at-user-logon/)
- NinjaOne Windows 에이전트: 워크스테이션 2GB RAM, 서버 4GB RAM, 1GHz 이상 x86/x64(듀얼코어 권장), 약 2GB 디스크(통합 기능에 따라 추가), ARM 미지원 — [NinjaOne Windows agent requirements](https://www.ninjaone.com/docs/endpoint-management/ninjaone-endpoint-management-microsoft-windows-agent-system-requirements-and-os-support) (검색 스니펫 확인)
- ManageEngine Endpoint Central 에이전트: Intel Pentium 1.0GHz, 512MB RAM, 약 3GB 디스크; Windows 7 에이전트 지원 유지(서버는 Win7 미지원, 배포 서버만 가능) — [ManageEngine system requirements](https://www.manageengine.com/products/desktop-central/system-requirements.html) (검색 스니펫 확인)
- Tactical RMM 체크인: 메시지 간 `randRange(100, 400)` ms sleep; WMI 쿼리 타임아웃 없음 — [rmmagent checkin.go](https://github.com/amidaware/rmmagent/blob/master/agent/checkin.go); [rmmagent wmi_windows.go](https://raw.githubusercontent.com/amidaware/rmmagent/master/agent/wmi_windows.go)
- `Win32_Product` 조회는 전체 MSI 재구성을 유발해 느림(수집 주기에 포함하면 안 됨) — [Microsoft AskDS](https://learn.microsoft.com/en-us/archive/blogs/askds/how-to-not-use-win32_product-in-group-policy-filtering)

### Inferences
- **자체 목표치 제안(측정 기준으로 사용)**: 유휴 시 CPU ≈ 0%, Working Set ≤ 40–60MB(.NET 8 self-contained 서비스), 전체 인벤토리 1회 실행 시 CPU 5–15% 1코어로 10–30초 이내, 인벤토리 payload gzip 후 ≤ 200–500KB(소프트웨어 300–500개 기준), heartbeat ≤ 2KB. 이 수치는 벤더 출처가 아니라 설계 목표이며 파일럿에서 검증해야 한다.
- 수집 완화 기법: 인벤토리 작업을 `ProcessPriorityClass.BelowNormal` + 스레드 우선순위 하향, 무거운 클래스(소프트웨어, 패치 이력, AppX)는 하루 1회, 경량 항목(디스크 사용량, 로그온 사용자)은 heartbeat에, WMI 쿼리마다 `CancellationToken`/타임아웃(예: 30초) 적용, 로그온 직후 5–10분은 스캔 지연(부팅 폭주 방지), 네트워크는 서버가 분산 배정한 시각에만 전송.

### Gaps
- 벤더(Lansweeper, NinjaOne, ManageEngine) 모두 에이전트 자체의 런타임 CPU%/RAM MB를 문서에 명시하지 않음(호스트 최소 사양만 제공). 실측 비교 자료(블로그/벤치마크)도 이번 조사에서 찾지 못함.
- 인벤토리 1회당 네트워크 사용량(KB) 공식 수치 없음.
