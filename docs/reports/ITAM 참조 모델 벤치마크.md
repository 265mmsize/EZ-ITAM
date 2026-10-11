# GLPI 계열을 기준선으로 삼아 100점 벤치마크를 세운다

제조사 인하우스 ITAM의 참조 모델로는 **GLPI 11 인벤토리 아키텍처**(GLPI Agent의 JSON 프로토콜, 서버 측 룰 엔진, 에이전트 레코드와 자산 레코드의 분리)를 1차 기준으로 삼는 것을 권한다. 이 아키텍처는 프로토콜 스키마, 서버 코드, 데이터 모델이 공개 소스로 확인되는 유일한 후보군이어서 설계 기준으로 검증할 수 있다. 상용 제품 중 Lansweeper, ManageEngine, Flexera/Snow, ServiceNow는 기능과 과금 정보가 공개되어 있으나 프로토콜과 데이터 모델의 1차 소스가 없어 설계 기준보다는 기능 비교 기준선으로 쓰는 것이 맞다. 다만 GLPI 계열과 Wazuh, OCS는 GPL 계열이므로 **코드가 아니라 스키마, 프로토콜, 설계만 참조**해야 하며, 이 판단은 법무 검토가 필요하다. 벤더 목록 가격과 국내 벤더의 배포 사양은 대부분 검증되지 않았고, 스코어카드의 상용 점수도 공개 문서와 데모 기준으로만 채점해야 한다. 벤치마크는 **9개 영역 100점 스코어카드**로 구성했으며, 이전 초안의 EBS 11i 연계 14점은 발견, 정규화, 라이선스, 자산 대장, 보안 증적 영역에 재배분했다. 가장 큰 비중은 발견(SW·HW)과 정규화, 자산 대장(각 13~14점)에 두었는데, 이는 SPC/BSA 감사 대응과 ISMS-P 자산 목록 요건에서 직접 도출된 결과다.

## 참조 모델 선정: GLPI 11 아키텍처를 중심축으로 삼는다

PC 에이전트 티어에서는 GLPI Agent의 프로토콜 설계를 기준으로 삼는다. GLPI Agent는 `action`, `deviceid`, `partial`, `expiration` 필드를 가진 JSON 메시지를 쓰고, 응답의 `expiration` 값으로 다음 접촉 시각을 정한다. 네트워크 오류가 나면 **60초부터 두 배씩 늘려 최대 3600초까지** 백오프하고 ([Target.pm](https://github.com/glpi-project/glpi-agent/blob/develop/lib/GLPI/Agent/Target.pm)), full 인벤토리 체크섬이 **14회 연속** 바뀌지 않으면 전송을 미루는 `full-inventory-postpone` 설정을 둔다 ([Task/Inventory.pm](https://github.com/glpi-project/glpi-agent/blob/develop/lib/GLPI/Agent/Task/Inventory.pm)). 인벤토리 본문은 `inventory.schema.json`으로 정의되며 섹션 약 50개의 필드 이름을 제공하므로, 자체 에이전트의 필드 어휘를 정할 때 그대로 참고할 수 있다 ([inventory.schema.json](https://github.com/glpi-project/inventory_format/blob/master/inventory.schema.json)). 다만 GLPI Agent는 Perl과 `Win32::OLE` 기반이고 OLE 워커 프로세스 분리와 메모리 측정 코드가 있을 만큼 리소스 관리가 과제였다는 점에서, C# 구현에서는 이 부분을 반면교사로 삼아야 한다.

Windows 수집 의미론은 GLPI 단독보다 **osquery와 Fleet의 Windows 테이블**을 보조 참조로 쓰는 편이 정확하다. osquery `programs` 테이블은 Uninstall 레지스트리를 순회하고 `HKLM\SOFTWARE\Classes\Installer\UpgradeCodes`에서 MSI UpgradeCode를 복원하며, MSIX는 패키지 이름과 퍼블리셔 해시로 식별한다 ([programs.cpp](https://github.com/osquery/osquery/blob/master/osquery/tables/system/windows/programs.cpp)). 이 식별자 체계는 정규화 키를 만드는 데 직접 쓸 수 있다. 한편 Microsoft는 `Win32_Product` 조회가 모든 MSI를 재구성해 느리고 부작용이 있다고 명시하므로 ([AskDS](https://learn.microsoft.com/en-us/archive/blogs/askds/how-to-not-use-win32_product-in-group-policy-filtering)), 설치 소프트웨어 수집은 Uninstall 레지스트리 두 경로와 사용자 하이브, AppX 조합으로 설계해야 한다. 수집 주기의 랜덤 지연은 Snow의 scheduled randomization과 ConfigMgr의 최대 240분 지연 사례를 함께 참고할 수 있다 ([Flexera Snow scanning](https://docs.flexera.com/snow-inventory-agents/snow-inventory-agent-for-windows/the-scanning-process/)).

서버 티어에서는 **GLPI 11의 `RuleImportAsset`과 에이전트 레코드 분리**를 가장 중요한 참조로 본다. GLPI는 `glpi_agents`(deviceid, last_contact, version, tag)를 자산 레코드와 분리해 두어 재설치나 호스트명 변경이 있어도 자산은 유지되도록 했다 ([Agent.php](https://github.com/glpi-project/glpi/blob/main/src/Agent.php)). 자산 매칭은 serial, uuid, mac, ip, model 등 기준의 룰 엔진으로 **Link if possible** 또는 **Refuse import**를 고르며, 거부된 장비는 `glpi_refusedequipments`에 기록된다 ([RuleImportAsset.php](https://github.com/glpi-project/glpi/blob/main/src/RuleImportAsset.php)). 여기에 OCS의 `AUTO_DUPLICATE_LVL` 비트마스크(hostname, serial, MAC, model, UUID, asset tag 조합)와 블랙리스트 테이블을 더하면 중복 판정 매트릭스가 완성된다 ([OCS Duplicate.pm](https://github.com/OCSInventory-NG/OCSInventory-Server/blob/master/Apache/Ocsinventory/Server/Duplicate.pm)). 반면 OCS는 섹션마다 DELETE 후 INSERT를 수행해 대규모에서 DB 부하가 컸으므로, 행 단위 diff를 처음부터 설계해야 한다. GLPI 11 네이티브 인벤토리도 전환 직후 `wakeupagents` 락 실패가 포럼에 보고되어 있으므로 ([GLPI 포럼](https://forum.glpi-project.org/viewtopic.php?id=284322)), 안정성은 검증 대상이다.

FE 티어는 근거가 가장 약하다. 노트에 수집된 어떤 제품의 FE 화면도 1차 문서로 확인되지 않았다. 따라서 FE의 정보 구조는 노트에 나온 메뉴 경로를 기준으로 잡는다. GLPI의 관리 메뉴(Agent 목록, Lockedfield, RefusedEquipment)는 "에이전트 데이터와 수동 데이터의 조정"을 위한 최소 구성이고, Atera의 기기 상세 General 탭(마지막 로그인 사용자, 마지막 재부팅, last-seen)은 기기 상세 화면의 기준이다 ([Atera support](https://support.atera.com/hc/en-us/articles/9903603088284-The-Devices-page)). 수명주기 화면은 Ralph의 `TransitionField` 상태 전이 모델을 참고하되, ManageEngine의 드래그앤드롭 캔버스는 벤더 주장이므로 검증 전까지는 기능 목록에만 둔다.

국내 제품은 아키텍처 기준이 아니라 **기능 체크리스트** 보조 참조로 쓴다. 닥터소프트 NetClient는 단일 에이전트, 자산 고유키 관리, DMS·PMS·PC-OFF 모듈 구조를 가지며 ([NetClient 제품 페이지](https://www.doctorsoft.co.kr/netclient/netclient1.jsp)), 엠엘소프트 Tgate DMS는 "약 1만개의 국내외 상용 소프트웨어 및 폰트 자체 DB"와 라이선스 인가·회수 기능을 주장한다 ([Tgate DMS 브로슈어](https://tcosvc.co.kr/pdf/tgatedms-brochure.pdf)). 지니언스 Genian NAC는 Windows 에이전트에 종료방지와 삭제방지 같은 자체 보호 기능을 두고 삭제 방식을 노드 정책으로 제어하며 ([Genian 에이전트 삭제](https://docs.genians.com/release/ko/install/deleting-agent.html)), 이 자체 보호 모델은 PC 에이전트의 무결성 요건을 정할 때 참고가 된다. 다만 이 제품들은 NAC나 PC 통합 관리가 본업이라 ITAM 데이터 모델이 공개되어 있지 않다.

나머지 후보를 기준으로 쓰지 않는 이유는 분명하다. Intune은 Microsoft가 정의한 속성 카탈로그만 수집하고 임의 WMI 클래스나 MIF 수집을 지원하지 않으며 ([MS Learn properties catalog](https://learn.microsoft.com/en-us/intune/intune-service/configuration/properties-catalog)), Tactical RMM은 자체 라이선스로 SaaS 제공을 제한하고 소프트웨어 목록을 필터 없이 보내며 ([TRMM LICENSE](https://github.com/amidaware/tacticalrmm/blob/develop/LICENSE.md)), Fleet는 ITAM의 구매와 상태 수명주기가 없다고 노트에 정리되어 있다. Snipe-IT, Ralph, iTop은 에이전트가 없어 기술 인벤토리를 외부에서 가져와야 한다.

## 후보 비교: 공개 1차 소스가 있는 제품이 설계 기준이 된다

아래 표는 노트에 근거가 있는 후보만 비교한다. "미확인"은 노트에서 근거를 찾지 못했다는 뜻이며 기능이 없다는 뜻이 아니다. 마지막 열은 본 설계에서 각 후보가 맡는 역할이다.

| 후보 | 에이전트·통신·인증 | Windows 수집 범위 | 소프트웨어 정규화 | 라이선스·권리 | 자산 생애주기 | 라이선스 | 본 설계에서의 역할 |
|---|---|---|---|---|---|---|---|
| GLPI 11 + GLPI Agent | JSON over HTTP, zlib 압축, `GLPI-Agent-ID` UUID 헤더, `expiration` 기반 스케줄 | 인벤토리 스키마 섹션 약 50개, 설치 SW 다경로, 모니터, 배터리, 드라이브 암호화 필드 | 소프트웨어 사전 룰, 제조사 키 정규화 | `softwarelicenses`의 수량과 할당 비교, `allow_overquota` | 에이전트와 자산 분리, lockedfields, logs, refused equipment, 계약 | GPL 계열(서버 GPLv3는 미확인) | **1차 참조**: 프로토콜, 스키마, 서버 룰 엔진 |
| OCS Inventory NG | XML over HTTP(S), PROLOG 후 SEND/STOP 응답 | HARDWARE, BIOS, SOFTWARE 등 섹션별 CHECKSUM delta | `dico_soft`, 이름·퍼블리셔·버전 테이블 | 미확인 | 장비별 FREQUENCY, accountinfo TAG | GPLv2(Windows 에이전트 LICENSE 경로는 404) | 중복 판정 매트릭스 참조. XML 프로토콜은 채택하지 않음 |
| Fleet + osquery | TLS JSON, `enroll_secret`에서 `node_key` 발급, config etag | `programs`, `patches`, `bitlocker_info`, `chassis_info` 등 | `software`, `software_titles`, `host_software` 3계층 | 미확인 | 미확인(ITAM 수명주기 없음) | osquery Apache-2.0 또는 GPL-2.0 선택, Fleet MIT(`ee/` 제외) | Windows 수집 의미론과 enroll 인증 참조 |
| Wazuh syscollector | 에이전트↔매니저 채널, 기본 1시간 주기 | 하드웨어(SMBIOS RSMB 직접 파싱), 패키지(HKU, AppX), 핫픽스 4개 레지스트리 경로 | WCS 표준화 | 없음 | 없음 | GPLv2 | SMBIOS 파싱과 핫픽스 경로 참조 |
| Tactical RMM | REST + NATS over WebSocket 443 단일 포트, 체크인 메시지 분할 | WMI 13개 클래스, 고정 디스크, 로그온 사용자 | 없음(소프트웨어 목록 무필터) | 없음 | 에이전트/사이트/정책 | 자체 라이선스(SaaS 제한) | 전송 패턴만 참조, 코드 재사용 제외 |
| Lansweeper (상용) | LsAgent .NET 4.8, 9524 직결 또는 443 릴레이 | 에이전트리스와 동일 HW/SW, 모니터, 보증 조회 | Sites에서 ML 정규화(Classic 미지원) | 미확인 | 미확인 | 상용 | 기능 기준선, 최소 스캔 간격 1시간 |
| ManageEngine AssetExplorer / Endpoint Central (상용) | AE 에이전트 TCP 9000, EC는 HTTPS 8383과 약 90분 정책 폴링 | 구매 대비 설치 라이선스, 미승인 설치 플래그 | 미확인 | 구매 대비 설치 대조, 자동 할당(벤더 주장) | CMDB, PO, 계약, 수명주기 캔버스(벤더 주장) | 상용 | 자산 대장과 라이선스 기능 기준선 |
| Flexera One / Snow (상용) | 암호화 파일 업로드, beacon failover, heartbeat 최소 10분 | 소프트웨어, 사용량, 사용자, 하드웨어 | Technopedia(벤더 주장 8.5만 제조사) | SAM 권리 기반 | 미확인 | 상용 | 통신 보안과 스케줄링 모델 참조 |
| ServiceNow ITAM / ACC (상용) | ACC가 MID Server websocket 8800에 wss 접속, TLS 1.3 | ACC-V 9개 모듈, 한국어 OS 값 파싱 실패 경고 | Normalization Library, 옵트인 Content Service | Subscription Unit 과금 | CMDB 중심 | 상용 | 정규화 피드백 루프 참조. 비용과 구축 부담이 커서 기준으로는 제외 |
| Microsoft Intune / ConfigMgr | Intune Device Inventory Agent 24시간 주기, ConfigMgr 기본 7일 | Microsoft 정의 속성 카탈로그만 수집 | 없음 | ConfigMgr 소프트웨어 미터링(보존 90일) | 없음 | 상용 | Windows 수집 소스의 검증 기준선 |
| Genian NAC (국내) | `GnAgent.exe` Windows 서비스, MSI와 GPO 배포 | 장치 HW/SW 정보, 사용 통제 | 미확인 | 미확인 | 미확인 | 상용 | 에이전트 자체 보호와 삭제 정책 참조 |
| NetClient, Tgate DMS (국내) | 단일 에이전트(NetClient), Tgate는 운영환경 미확인 | DMS, 자산 고유키 | Tgate는 자체 DB 약 1만 개 주장 | NetClient SLIM의 사용량 기반 회수 | 자산 고유키 중복 방지 | 상용 | 기능 체크리스트. 단가와 서버 사양은 미확인 |

이 표에서 읽히는 핵심은 프로토콜과 데이터 모델이 공개된 후보가 GLPI, OCS, osquery/Fleet, Wazuh뿐이라는 점이다. 상용 후보는 Windows 수집 범위와 정규화 능력을 내세우지만 그 내부 구조는 검증할 수 없다. 따라서 상용 제품은 스코어카드의 **상용 선두 수준(5점)을 정의하는 기능 기준선**으로만 쓰고, 설계 결정은 공개 소스가 있는 후보에서 가져온다.

## 100점 스코어카드: EBS 연계 14점을 데이터 신뢰 영역으로 재배분

스코어카드는 9개 영역으로 구성하며 각 항목은 0점(없음)부터 5점(상용 선두 수준)까지 채점한다. 영역 점수는 `평균 점수 / 5 × 가중치`로 환산하고, 상용 제품은 공개 문서와 데모 기준으로, 자사 빌드는 실제 구현 기준으로 같은 심사자가 채점해야 한다. 이전 초안의 10개 영역 중 "ERP/HR 연계 — Oracle EBS 11i" 14점 항목은 제외했다. 재배분은 HW 발견 +2, SW 발견 +3, 정규화 +2, 자산 대장 +2, 라이선스 +2, 보안 증적 +2, 비기능 +1로 했다. 이 배분은 두 가지 근거를 따른다. 첫째, 노트들이 공통으로 지적하듯 국내 제조기업이 가장 실질적으로 마주하는 압력은 ISMS-P보다 SW 저작권 감사이므로 SW 발견과 라이선스 영역을 키웠다. 둘째, 정규화가 상용 제품의 핵심 차별점이라는 노트 결론에 따라 정규화 비중을 높였다. 기존 HR 동기화 요구는 EBS가 아니라 ISMS-P 2.1.3의 책임자 갱신 요건에서 나온 것이므로 자산 대장 영역에 남기되 소스는 AD나 CSV 같은 중립 소스로 한정했다.

| # | 영역 (참조 근거) | 가중치 (이전→변경) | 채점 세부 기준 | 측정 가능한 합격 기준 | 근거 등급 |
|---|---|---|---|---|---|
| A | HW 발견·인벤토리 (GLPI 스키마, Wazuh SMBIOS, Win32 클래스) | 12→**14** | SMBIOS serial, BIOS asset tag, UUID와 placeholder 블랙리스트, CPU·RAM·디스크, MAC 집합, OS build, 모니터 EDID, BitLocker 상태(`ConversionStatus`), 노트북 배터리 | 필수 항목 수집 성공률 ≥98%(파일럿 PC), 블랙리스트 적용 후 중복 자산 0건, 재부팅 후 자산 ID 유지 100% | 설계 제안 |
| B | SW 발견 (osquery `programs`, GLPI Softwares.pm, AskDS) | 10→**13** | Uninstall 레지스트리 3경로와 HKU, AppX(`Get-AppxPackage -AllUsers`), MSI ProductCode와 UpgradeCode, 포터블 실행파일 스캔, SWID 태그 읽기(있을 때), `SystemComponent=1` 제외, 설치 스냅샷 | ARP 기준 설치 SW 샘플 검수에서 누락 0건, AppX 포함, 임의 시점 스냅샷 조회 가능 | 1차 소스 기반 |
| C | 정규화·카탈로그 (ServiceNow 정규화, Flexera Private Catalog, winget 매니페스트) | 12→**14** | publisher·product·version 분리, 룰 사전(첫 매칭 중단), 재처리(replay), 미정규화 큐와 승인 워크플로, winget `AppsAndFeaturesEntries` 역매핑, 사설 카탈로그 관리 | 정규화율 측정식 고정(설치 건수 기준), 파일럿 정규화율 목표 ≥90%, 미정규화 큐 처리 ≤5영업일 | 목표치는 설계 제안, 측정식은 노트 근거 |
| D | 자산 대장·라이프사이클 (ITIL 4, ISMS-P 1.2.1·2.1.3, Ralph) | 12→**14** | 상태 모델과 전이 이벤트, ISMS-P 1.2.1 필수 속성(자산명, 용도, 위치, 책임자, 관리자, 관리부서, 보안등급), 보증과 계약 연결, 바코드 라벨, AD나 CSV 기반 책임자 갱신 | 필수 속성 공란 0%, 모든 상태 전이의 사용자·시각 기록 100%, 인사이동 반영 ≤5영업일 | ISMS-P 요건은 노트 근거, 수치는 설계 제안 |
| E | 라이선스 권리·ELP (ISO/IEC 19770-3, GLPI softwarelicenses) | 10→**12** | 권리 레코드(제품, 버전 범위, 메트릭, 수량, 유효기간, 유지보수), 벤더별 ELP 산출, 다운·업그레이드 권리, 감사 패키지 내보내기 | 주요 감사 빈도 높은 퍼블리셔의 ELP를 1시간 내 산출, 설치 스냅샷 시점별 보존 | 설계 제안 |
| F | 최적화·회수 (Flexera 10개 활동, Gartner Optimization) | 6→**6** | 미사용 SW 회수 워크플로, 연한 초과 기기 리프레시 목록, 금지 SW 리포트 | 미사용 판정 기준(N일 무실행) 정의 후 리포트 제공 | 설계 제안 |
| G | 보고·KPI (Cyrisma 버킷, Proactivanet 90% 기준) | 8→**8** | 정확도, 완전성, 보고율, 정규화율, ELP, 고아 자산 KPI의 정의·공식·데이터 소스 명시, 감사용 시점 리포트, 저장된 뷰 | KPI 6종 공식 문서화 100%, 리포트 생성 ≤30초(파일럿) | 2차 자료 기반 |
| H | 보안·컴플라이언스 증적 (안전성 확보조치 기준, ISMS-P, 인사·노무편 가이드) | 8→**10** | AV·EDR 상태(`productState` 원시값 저장 후 서버 해석), OS 패치 상태(WUA), BitLocker, RBAC와 감사 로그, USB 이력과 사용시간 측정은 기본 OFF, 콘텐츠 수집 금지 | 민감 항목 기본 OFF 100%, 감사 로그 append-only 검증 통과, 역할별 접근 테스트 통과 | 규제 요건은 노트 근거, 수치는 설계 제안 |
| I | 비기능·운영·TCO (TRMM 지터, Snow 랜덤화, 자체 footprint 목표) | 8→**9** | MSI 무인 설치(GPO, Intune Win32), 자동 업데이트의 서명 검증과 롤백, 오프라인 큐, 성능 목표, 한국어 Windows 로케일 수집 테스트, 3년 TCO, 운영 FTE | 유휴 CPU ≈0%, 서비스 Working Set ≤60MB, 인벤토리 payload gzip 후 ≤500KB, 한국어 로케일 수집 실패 0건 | 설계 목표(벤더 수치 없음) |

**합계는 100점이다.** 14점 재배분 후 9개 영역의 가중치는 14, 13, 14, 14, 12, 6, 8, 10, 9이다. 가중치는 공개 자료를 교차해 설정한 자체 값이며, Gartner와 Forrester의 공식 가중치는 비공개여서 비교 검증할 수 없다.

이 스코어카드에서 "Done" 정의는 세 가지 조건을 동시에 만족해야 한다. 첫째, PC 커버리지가 대상의 95% 이상이고 7일 내 체크인이 95% 이상, 30일 내 체크인이 98% 이상이어야 한다. 노트의 Cyrisma 상태 버킷(7일, 30일, 90일)을 응용한 값이다. 둘째, 자산 대장과 발견 데이터의 일치율이 95% 이상이어야 한다. 이 수치는 **2009년 EDN 자료**에 근거하므로 구자료다. 90% 미만을 경고선으로 두는 Proactivanet 의견과 함께 쓴다. 셋째, 라이선스 ELP를 주요 퍼블리셔별로 1시간 안에 산출하고 설치 스냅샷을 시점별로 보존해야 한다. 이전 초안의 "ERP 연계" 완료 조건은 삭제했다.

## 에이전트-서버 프로토콜과 데이터 모델

프로토콜은 **443 단일 포트, 에이전트 발신 아웃바운드 전용**을 원칙으로 한다. 상용 제품 다수가 이 구조를 쓰는 이유는 고객사 방화벽을 바꾸지 않고 배포하기 위해서이며, 서버가 에이전트로 인바운드 접속하는 설계는 피해야 한다. 등록 단계는 osquery처럼 일회성 enroll 키로 시작해 서버가 `deviceId`와 장기 토큰을 발급하는 방식을 쓴다. 토큰은 DPAPI로 보호하되, `DataProtectionScope.LocalMachine`은 기기의 어떤 프로세스든 복호화할 수 있다는 점을 고려해 엔트로피와 ACL을 함께 두고 서버에서 토큰을 `deviceId`와 하드웨어 지문에 묶어야 한다 ([DataProtectionScope](https://msdn2.microsoft.com/en-us/library/32y3d3x0)). 아래 표의 엔드포인트 이름은 설계 제안이며 노트에서 가져온 것이 아니다.

| 단계 | 설계 엔드포인트(제안) | 본문과 동작 | 근거 |
|---|---|---|---|
| 등록 | `POST /api/v1/enroll` | enroll 키, 지문, hostname, 에이전트 버전 → `deviceId`, 장기 토큰 | osquery enroll, Fleet `EnrollOsquery` |
| 접촉(heartbeat) | `POST /api/v1/agent/contact` | `action: contact` 형식. 응답에 `expiration`, 명령 큐, 설정 버전 포함. 기본 5분에 ±20% 지터(설계 제안) | GLPI Contact.pm, Snow 랜덤화, TRMM 지터 |
| 인벤토리 | `POST /api/v1/agent/inventory` | 섹션별 SHA-256 해시를 붙이고, 서버 보유 해시와 같으면 본문을 생략(delta). 완전 인벤토리는 하루 1회 | GLPI `partial`, OCS CHECKSUM |
| 결과 보고 | `POST /api/v1/commands/{id}/result` | 명령 실행 결과. 명령 타입은 `RunInventory`, `UpdateAgent`, `SetConfig`, `CollectLogs`로 화이트리스트 제한 | TRMM 명령 모델, 설계 제안 |
| 재시도 | 요청 헤더 `Idempotency-Key`(ULID) | 서버는 `(deviceId, key)` 유니크 인덱스로 중복 삽입 방지. 서버 수신 시각을 정본으로 쓰고 에이전트 시각은 `clientTime`에만 기록 | 설계 제안 |
| 오프라인 | `%ProgramData%\<Vendor>\Agent\queue\` | 압축 파일 큐(상한 초과 시 오래된 인벤토리부터 폐기), 지수 백오프와 `Retry-After` 존중 | GLPI 백오프, 설계 제안 |

데이터 모델은 노트에 나온 다섯 계층으로 설계한다. 발견 원시 데이터, 정규화 카탈로그, 자산과 CI, 권리(entitlement), 수명주기 상태 머신이다. 여기에 에이전트 레코드, 룰, 잠금, 거부 장비, 감사 로그를 더한다. 핵심은 **에이전트 레코드와 자산 레코드를 분리**하는 것이다. 에이전트 레코드는 `deviceId`, 설치 인스턴스 ID, 버전, 마지막 접촉, 토큰 해시를 가지고, 자산 레코드는 serial, UUID, 자산 태그, 상태, 책임자, 보안등급을 가진다. 두 레코드를 연결하는 것은 룰 엔진이며, 재설치 때 에이전트 레코드만 바뀌고 자산은 유지되어야 한다.

| 엔터티 | 역할 | 참조 모델 |
|---|---|---|
| `agent` | deviceId, 설치 인스턴스 ID, 버전, last_contact, 토큰 해시, 자산 연결 | GLPI `glpi_agents`, osquery node_key |
| `asset` | serial, UUID, asset tag, 상태, 책임자, 관리자, 관리부서, 보안등급 | GLPI `glpi_computers`, OCS `hardware` |
| `device_fingerprint_history` | 지문 변화 이력(클론 감지, 재이미징 판정 근거) | 노트 3절 설계안 |
| `inventory_snapshot` | 섹션별 해시, 수신 시각, Idempotency-Key | OCS CHECKSUM, GLPI partial |
| `sw_raw` → `sw_title` / `sw_version` | 원시 설치 항목(출처: ARP, MSI, AppX, 포터블)과 정규화 결과 | GLPI softwares, Fleet `software_titles` |
| `install_link` | 자산과 SW 버전 연결, first_seen, last_seen, 사용자 | GLPI `glpi_items_softwareversions`, Fleet `host_software` |
| `match_rule` | 기준(serial, uuid, mac, hostname)과 동작(Link, Import, Refuse) | GLPI `RuleImportAsset` |
| `refused_device`, `blacklist` | 거부 장비와 공통 시리얼, MAC, UUID 블랙리스트 | GLPI `glpi_refusedequipments`, OCS `blacklist_*` |
| `locked_field` | 자산 단위 필드 잠금(수동 값 보호) | GLPI `glpi_lockedfields` |
| `entitlement` | 제품, 버전 범위, 메트릭, 수량, 유효기간, 유지보수 | ISO/IEC 19770-3 개념, GLPI `softwarelicenses` |
| `lifecycle_event` | 자산, 이전 상태, 다음 상태, 행위자, 시각, 사유 | ITIL 4, Ralph `TransitionField` |
| `audit_log` | 변경 전후 값, 사용자, 시각(append-only) | GLPI `glpi_logs` |

MS SQL Server 구현에서는 두 가지를 먼저 결정해야 한다. 하나는 섹션별 DELETE 후 INSERT를 피하고 행 단위 diff로 쓰는 것이며, 다른 하나는 인벤토리 수신을 큐로 분리해 DB 락 경합을 격리하는 것이다. 두 가지 모두 OCS와 GLPI에서 운영 불만이 가장 많이 나온 지점이다.

## 한국 컴플라이언스 드라이버가 정하는 최소 요건

국내 제조기업에 직접 걸리는 드라이버는 세 가지다. 첫째는 **개인정보의 안전성 확보조치 기준**이다. 현행 고시는 개인정보보호위원회 고시 제2023-6호이며 2023년 9월 22일 시행되었고, 2025년 개정에서 제2조와 제6조의2(인터넷망 차단조치)는 즉시 시행, 내부관리계획과 접근권한 관리, 접근통제, 접속기록 보관, 점검 조항은 1년 유예되었다고 보도되었다 ([Byline Network 2025-10-31](https://byline.network/2025/10/31-281/)). 다만 노트에서 확인한 고시 문구는 비공식 사본이므로 조문은 국가법령정보센터 원문으로 대조해야 한다. 이 고시에서 ITAM이 맡을 역할은 접근권한 대상 기기 식별과 백신 설치·업데이트 상태의 증적 제공이다.

둘째는 **ISMS-P 자산 관리 기준**이다. 인증기준 1.2.1은 자산명, 용도, 위치, 책임자와 관리자, 관리부서를 목록에 기재하고 정기적으로 현황을 조사해 최신으로 유지하라고 요구하며 ([IT위키 1.2.1](https://itwiki.kr/w/ISMS-P_%EC%9D%B8%EC%A6%9D_%EA%B8%B0%EC%A4%80_1.2.1.%EC%A0%95%EB%B3%B4%EC%9E%90%EC%82%B0_%EC%8B%9D%EB%B3%84)). 인증기준 2.1.3은 자산별 책임소재를 명확히 정의하고 인사이동, 자산 도입, 변경, 폐기 시 목록을 갱신하라고 요구한다 ([IT위키 2.1.3](https://itwiki.kr/w/ISMS-P_%EC%9D%B8%EC%A6%9D_%EA%B8%B0%EC%A4%80_2.1.3.%EC%A0%95%EB%B3%B4%EC%9E%90%EC%82%B0_%EA%B4%80%EB%A6%AC)). 이 두 요건이 자산 대장의 필수 속성과 상태 전이 이벤트를 직접 결정한다. ISMS-P 인증을 받은 회사라면 이 요건이 필수이고, 인증 대상이 아니더라도 같은 항목을 내부 기준으로 삼는 것이 감사 방어에 유리하다.

셋째는 **근로자 개인정보와 모니터링 제한**이다. 로그온 사용자, USB 이력, 소프트웨어 사용시간은 근로자 개인정보에 해당할 수 있다. 개인정보위와 고용노동부의 인사·노무편 가이드(2023년 1월 개정)는 고용 단계별 최소수집을 요구하고, 근로자 감시 등 목적 외 이용은 법 위반 소지가 있다고 본다 ([바이라인네트워크 2023-01](https://byline.network/2023/01/0131_02/)). 노트의 설계 지침은 USB 이력과 사용 측정을 기본 OFF로 두고, 문서명이나 URL, 키 입력 같은 콘텐츠 수준 데이터는 수집하지 않으며, 수집 항목 변경 시 근로자에게 고지하는 것이다. 이 지침은 PIPA 제3조와 ISMS-P 3.1.2의 일반 원칙에서 도출한 것이며, PC 에이전트 전용 공식 가이드라인은 찾지 못했다.

소프트웨어 저작권은 세 번째 드라이버이자 가장 실질적인 압력이다. 저작권법 양벌규정(제141조)에 따라 대표가 불법복제 방지를 위한 충분한 주의와 감독을 입증하지 못하면 형사와 민사 책임을 질 수 있다 ([nepla 법률해설](https://www.nepla.ai/wiki/지식재산/저작권/불법-소프트웨어-단속-공문-내용증명-을-받았다면-형사고소-및-손해배상-초기대응-전략-0xr84d6g28q1)). SPC는 사법기관의 단속 시 기술 지원을 하고 ⓒ-SAM 원스텝 솔루션과 Audit 대응 서비스를 제공한다 ([SPC SAM 서비스](https://www.spc.or.kr/ko/business/sam/sw_sub311)). 따라서 참조 모델은 설치 SW 식별, 상용 SW 카탈로그 매칭, 구매 권리 대비 초과 설치 산출, 증빙(증서와 영수증) 첨부 흐름을 지원해야 한다. 국내 SAM DB를 가진 스위퍼의 존재는 이 기능이 국내 시장에서 실제로 팔린다는 신호다. 다만 단속 실적과 판례의 최신 수치는 노트에서 확인하지 못했다.

| 요건 | 근거 | 참조 모델이 충족해야 할 기능 | 검증 상태 |
|---|---|---|---|
| 자산 목록 필수 속성과 정기 현행화 | ISMS-P 1.2.1 | 자산 대장 필수 속성, 필수값 검증, 정기 재수집 | 요건은 IT위키 요약 기반, 원문 미확인 |
| 인사이동과 자산 변경 반영 | ISMS-P 2.1.3 | 상태 전이 이벤트, 책임자 이력, AD나 CSV 기반 갱신 | 요건은 IT위키 요약 기반 |
| 접근권한, 접근통제, 악성프로그램 방지 | 안전성 확보조치 기준(고시 2023-6) | 백신과 패치 상태 수집, RBAC, 접속기록 | 비공식 사본 기반, 2025 개정 조문 미확인 |
| 최소수집과 근로자 모니터링 제한 | 개인정보 보호법 제3조, ISMS-P 3.1.2, 인사·노무편 가이드 | USB와 사용시간 기본 OFF, 콘텐츠 수집 금지, 보존기간과 자동 파기 | 일반 원칙에서 도출한 설계 지침 |
| 설치 SW 식별과 권리 대조 | 저작권법 제141조, SPC와 BSA 단속 | SAM 카탈로그 매칭, ELP 산출, 증빙 첨부 | 구자료 블로그 근거 일부 포함 |
| 감사 시점 증적 | 감사 대응 일반 관행 | 설치 스냅샷 보존, append-only 감사 로그 | 노트 근거는 SPC 점검 프로그램 설명 수준 |

국가정보보안기본지침은 국가와 공공기관 대상이므로 민간 제조기업에는 적용되지 않는다. GS인증과 CC인증도 공공 조달 진입 시의 관문이며, 사내 구축에는 인증이 필요 없다. 방산이나 국가핵심기술 보유 기업이라면 산업기술보호법 요구가 별도로 붙을 수 있으나 이번 조사 범위 밖이다.

## 공백과 수작업 검증 항목

조사의 가장 큰 공백은 **국내 벤더의 가격과 배포 사양**이다. 나라장터 종합쇼핑몰의 PC당 단가, 유지보수율, Tgate와 오피스키퍼의 서버 OS와 DB 요구사항은 검색 결과에서 확인하지 못했고 쇼핑몰 직접 조회도 차단되었다. 오피스키퍼 클라우드 가격(연 11만원대/유저)은 리셀러 페이지 값이므로 공식 가격이 아니다. 노트에 있는 NetClient의 서버 사양(CentOS 7.4, MariaDB)은 유통사 구버전 페이지이므로 현행 값으로 쓸 수 없다. 해외 상용 제품의 가격은 대부분 제3자 추정이며, 한국 원화 기준이나 총판 마진 정보는 없다. 따라서 TCO 비교는 최소 세 곳의 견적 요청으로 검증해야 한다.

기술 쪽 공백도 중요하다. 노트의 많은 수치는 벤더 주장이거나 커뮤니티 답변이다. 그중 가장 설계에 영향을 주는 항목은 다음과 같다. `MachineGuid`가 sysprep generalize로 재생성된다는 주장은 Microsoft Q&A 답변에만 근거하므로 테스트 이미지에서 실측해야 한다. .NET 8 self-contained single-file 서비스의 파일 크기는 노트에서 확인하지 못했으므로 hello-world 빌드로 실측해야 한다. `Win32_VideoController.AdapterRAM`의 4GB 상한과 Azure Artifact Signing의 한국 조직 제공 여부도 확인이 필요하다. Azure Artifact Signing은 현행 문서에서 미국, 캐나다, EU, 영국 조직만 명시되어 있어서 한국 법인에는 OV 인증서가 현실적인 대안일 수 있다. 또한 ETW나 prefetch 기반 소프트웨어 사용량 측정은 구현 사례와 오버헤드 수치가 없어 별도 설계가 필요하다.

법적 공백은 세 가지다. GLPI 서버의 GPLv3 라이선스는 노트가 직접 확인하지 않은 상태이고, iTop의 라이선스 파일도 확인하지 못했으며, OCS Windows 에이전트의 LICENSE 파일은 404였다. Tactical RMM은 SaaS 제공을 제한하는 자체 라이선스라서 사내 사용이 허용되는지 해석이 필요하다. GPL 계열 코드를 C# 제품에 옮기는 것은 노트가 피하라고 권고한 사항이며, 실제 배포 의무 발생 여부는 법무 검토가 필요하다.

표준과 시장 데이터의 공백도 있다. ISO/IEC 19770-1:2017 Annex A의 프로세스 영역 수는 공식 수치 15와 커뮤니티 수치 27이 상충하므로 표준 본문으로 확인해야 한다. Gartner 가중치와 Forrester 14개 기준은 유료 보고서 안에 있어 확보하지 못했다. 상용 제품의 SSO 프로토콜, AD 동기화 방식, RBAC 모델도 공개 발췌에서 찾지 못했다. 한국어 Windows에서 ServiceNow ACC가 비영어 값을 파싱하지 못해 discovery가 실패할 수 있다는 경고가 있으므로 자체 에이전트는 로케일 독립 수집(WMI 값 그대로, 날짜와 숫자 포맷 고정)을 하고 파일럿에서 한국어 로케일 테스트를 필수로 넣어야 한다.

FE는 근거가 가장 얇다. 노트 어디에도 특정 제품의 FE 화면을 1차 자료로 확인한 항목은 없으므로 FE 채점은 데모 캡처 기준으로 다시 해야 한다. 오라클 EBS 11i 관련 테이블과 연계 패턴은 이번 보고서에서 범위 밖이므로 검증 대상에서도 제외한다.

## Conclusion

이 조사가 주는 가장 중요한 결론은 상용 제품의 기능 목록보다 **데이터 품질과 식별 체계가 설계의 중심**이라는 점이다. 노트에 나온 모든 운영 불만, 즉 중복 장비, 설정 탐색 어려움, 대규모 DB 병목, 에이전트 신뢰성 문제는 식별자 설계, 룰 우선순위, 행 단위 diff, 큐 분리에서 시작된다. 따라서 자체 빌드의 초기 공수는 UI보다 정규화 카탈로그와 중복 판정 매트릭스에 집중해야 한다. EBS 연계 14점을 재배분한 결과도 같은 방향을 가리킨다. 데이터 신뢰 영역이 전체의 절반 가까이를 차지하게 되었다.

다음 단계는 두 가지다. 첫째, 대상 PC 100대 규모의 파일럿에서 Done 기준의 커버리지, 일치율, 정규화율, 한국어 로케일 수집 성공률을 실측하고 목표 수치를 내부 합의로 확정한다. 노트의 95%와 98%는 2차 자료에서 가져온 값이므로 실측 결과로 바꿔야 한다. 둘째, 국내 벤더 세 곳과 상용 벤더 두 곳에서 데모와 견적을 받아 스코어카드의 상용 점수를 실제 데모 기준으로 채점한다. 이 두 가지가 끝나기 전까지는 벤치마크 점수가 설계 제안 수준에 머문다.

<!-- 주의: 본 보고서의 상용 제품 수치와 국내 벤더 정보는 검색 발췌 기반이며 대부분 원문 대조가 필요하다. -->
