# ITAM 표준·평가기준·벤치마크 프레임워크 및 Oracle EBS 11i Fixed Assets 연계 패턴 (2025–2026 기준)

조사일: 2026-10-11. 조사 환경 제약: 본 세션에서는 웹 검색 결과(발췌 인용)만 사용 가능했고 페이지 전문 fetch(WebFetch/curl)는 프록시에 의해 차단되었다. 따라서 ISO 본문, Gartner/Forrester 보고서 본문, ITIL 4 Practice Guide 원문처럼 유료·비공개 자료는 2차 요약(벤더 리프린트, 위키, 커뮤니티)에 의존했으며, 그 사실을 각 항목의 Gaps에 명시했다. 2020년 이전 자료는 "(구자료)"로 표시했다.

---

## KQ1. ISO/IEC 19770 패밀리와 ITIL 4 ITAM 프랙티스가 도구에 요구하는 구체 역량

### Takeaway
ISO/IEC 19770-1:2017은 "도구"가 아니라 "관리체계(ITAM 시스템)" 요구사항 표준이며, 15개 프로세스 영역(Annex A, 규범)과 3개 티어(Annex B, 참고: Trustworthy Data → Life Cycle Integration → Optimization)로 구성된다. 도구 벤치마크에서는 Tier 1 "신뢰할 수 있는 데이터"(발견·인벤토리·정합성)가 1차 관문이고, -2(SWID 태그)·-3(Ent 권리 스키마)은 각각 "설치 소프트웨어 식별 데이터 포맷"과 "라이선스 권리 데이터 모델"을 정의해 도구의 데이터 구조 설계 기준이 된다. ITIL 4 ITAM 프랙티스는 자산 라이프사이클(계획→취득→배치→운영→폐기)과 자산 대장(asset register)을 핵심으로 하고, CMDB(서비스 구성관리)와는 목적이 다르다는 점을 명확히 한다.

### Cited Findings

**ISO/IEC 19770-1:2017 (ITAM 시스템 요구사항)**
- 2017년 12월 발행된 3판으로, 2012년 2판을 대체하며 ISO/IEC 27001·20000-1과 같은 공통 상위구조(Harmonized Structure)를 가진 "인증 가능한 경영시스템 표준"으로 재구성되었다. ISO 55001:2014(자산관리) 요구사항에 추가되는 IT 자산 요구사항을 규정한다 — [ISO 카탈로그 68531](https://www.iso.org/standard/68531.html); [ITAM Review 2018 보도](https://itassetmanagement.net/2018/01/19/revised-iso-sam-itam-standard-published/) (구자료이나 표준 자체는 현행)
- 규범 Annex A에 15개 ITAM 프로세스 영역을 명시하고, 참고 Annex B에 권장 구현 순서로 3개 티어(Tier 1 Trustworthy Data, Tier 2 Life Cycle Integration, Tier 3 Optimization)를 제시한다 — [ISO JTC1/SC7 공식 페이지](https://committee.iso.org/sites/jtc1sc7/home/projects/flagship-standards/isoiec-19770-12017.html); [SmartSuite 요약](https://www.smartsuite.com/frameworks/iso-iec-19770-1-2017-it-asset-management-systems-requirements)
- 티어는 "프로세스의 그룹핑"일 뿐 반드시 순차 구현해야 하는 것은 아니라는 해석이 있다(ISO 페이지의 "recommended implementation order" 표현과 상충) — [Noventiq 블로그](https://knowledgebase.noventiq.co.uk/moving-from-sam-to-itam-repositioning-of-processes-and-tiers-part-3-of-3); 2012년판은 4개 티어를 순차 달성 구조로 두었고 2017년판에서 티어를 부속서로 옮겼다 — [Wikipedia ISO/IEC 19770](https://en.wikipedia.org/wiki/ISO/IEC_19770)
- Tier 1(Trustworthy Data)은 "무엇을 보유하고 있는지 아는 것"으로, 인프라 데이터·계약/라이선스 데이터·정합(reconciliation)을 포함하며 Flexera는 2017년판 Tier 1 하위 역량 영역의 예로 "Change management"를 든다 — [Flexera: 14 key competencies of SAM](https://www.flexera.com/blog/it-asset-management/14-key-competencies-of-sam/)
- 본문 8장 운영 절의 목차 발췌에는 "8.4 Core data management, 8.5 License management, 8.6 Security management, Other processes"가 있다 — [iTeh 샘플 PDF 목차](https://standards.iteh.ai/catalog/standards/iso/a6bad394-a706-4431-baf7-81fc420b6124/iso-iec-19770-1-2017)
- Jamf는 자사 MDM이 19770-1:2017 Tier 1 "Trustworthy Data"를 충족하는 방법을 별도 세션으로 다룰 만큼, Tier 1이 도구 벤더의 주요 마케팅 기준점이다 — [Jamf 자료](https://www.jamf.com/resources/videos/achieving-trustworthy-data-how-to-satisfy-iso-iec-197702017-with-jamf/)

**ISO/IEC 19770-2:2015 (SWID 태그)**
- SWID 태그는 퍼블리셔가 설치물에 메타데이터(퍼블리셔·제품명·버전)를 포함시켜 인벤토리 도구가 읽도록 한 XML 포맷이며, NIST IR 8060이 상호운용 태그 작성 지침을 제공한다 — [NIST IR 8060](https://nvlpubs.nist.gov/nistpubs/ir/2016/NIST.IR.8060.pdf); [NIST 요약](https://www.nist.gov/publications/guidelines-creation-interoperable-software-identification-swid-tags)
- 태그 유형은 corpus(설치 미디어), primary(설치된 제품), patch(패치), supplemental(보조) 4종이며, 필수 요소는 소수(tagId=전역 고유, name, version, tagVersion, 그리고 최소 1개의 tagCreator 역할 Entity)이고 나머지는 선택이다 — [IETF CoSWID 초안(NISTIR 8060 라이프사이클 표 재수록)](https://datatracker.ietf.org/doc/html/draft-ietf-sacm-coswid-00); [DMTF CIM_SoftwareIDTag](https://schemas.dmtf.org/wbem/cim-html/2.55.0+/CIM_SoftwareIDTag.html)
- 실제 채택률은 낮다: Adobe·Microsoft 등 소수 퍼블리셔만, 그것도 신버전에만 태그를 넣기 시작했다 — [Snow Software(현 Flexera) 설명](https://snowsoftware.com/?p=1306) (구자료 가능성, 연도 미표시)

**ISO/IEC 19770-3:2016 (소프트웨어 권리 스키마, Ent)**
- 소프트웨어 사용권(entitlement)을 기술하는 공통 어휘와 전송 포맷(메트릭 포함)을 규정한다. 사용권을 부여하는 라이선스 부분집합만 다루며, 원 계약 조건이 법적으로 우선하고, 발견·관리 프로세스(-1), 식별 태그(-2), 제품 활성화는 범위 밖이다. 2022년 재확인(confirmed)되어 현행이다 — [ISO 52293](https://www.iso.org/standard/52293.html); [IEC webstore](https://webstore.iec.ch/en/publication/24554)

**ISO/IEC 19770-5:2015 (개요 및 용어)**
- 19770 패밀리 개요, ITAM/SAM 소개, SAM 원칙 요약, 패밀리 공통 용어 정의를 담으며 모든 유형의 조직에 적용된다. DIS 개정안이 존재한다 — [IEC webstore 19770-5](https://webstore.iec.ch/en/publication/23051); [SCC 19770-5:2015](https://www.scc.ca/en/standardsdb/iec/27998)

**ITIL 4 IT Asset Management 프랙티스**
- 프랙티스 목적: 모든 IT 자산의 전체 라이프사이클을 계획·관리하여 구매·재사용·폐기 판단과 규제·계약 요구사항 충족을 지원 — [Global Knowledge ITIL 4 Practitioner ITAM 과정 개요](https://www.globalknowledge.com/en-be/courses/itil_and_service_management/service_management_and_itil/itil4p-pic-itam/co)
- 핵심 용어: IT 자산 유형, IT 자산 대장(register), 자산 라이프사이클과 라이프사이클 모델, 검증(verification), 인벤토리, 발견(discovery), IT 자산 감사(audit); 역할: asset owner, custodian, register administrator — 동일 출처
- 2차 해설에서 라이프사이클 단계는 "계획·예산 → 취득(서비스데스크/구매와 단일 창구) → 할당/배치(변경·릴리스 프랙티스로 승인·테스트) → 최적화(서비스 수준/비용) → 폐기(decommission)"로 정리된다 — [InvGate: 5 key guidance points](https://blog.invgate.com/5-key-guidance-points-in-the-itil-4-itam-practice)
- 자산 대장 설계 지침: "모든 것을 기록"하면 모든 필드를 최신으로 유지해야 하므로, ITIL 4 "keep it simple" 원칙에 따라 기본 필드부터 시작해 확장하라 — 동일 InvGate 출처
- ITIL 4 용어로 자산은 "IT 제품·서비스를 뒷받침하는 유용한 모든 구성요소"이며 ITAM은 조달부터 폐기까지 전 단계를 다룬다 — [Virima: CMDB vs ITAM](https://virima.com/blog/cmdb-asset-management-vs-itam-key-differences-explained)

**CMDB vs ITAM**
- CMDB는 관계·의존성(어떤 서비스가 의존하는지, 바꾸면 무엇이 깨지는지), ITAM/자산대장은 개별 항목의 취득일·재무가치·할당·상태를 추적한다 — [Device42: CMDB vs asset management](https://www.device42.com/cmdb-best-practices/cmdb-vs-asset-management/); [Virima](https://virima.com/blog/cmdb-asset-management-vs-itam-key-differences-explained)
- ITAM은 재무·계약 관리로 비즈니스를 지원하고, 자산 대장은 CMDB 구축의 간과되기 쉬운 선행조건이다 — [ITAM Review 2009](https://itassetmanagement.net/2009/12/04/itam-a-prerequisite-for-building-a-cmdb-part-46-–-“why-building-a-cmdb-is-challenging/) (구자료)

### Inferences
- 도구 관점에서 19770-1 Tier 1은 "(a) 하드웨어/소프트웨어 발견·인벤토리 (b) 계약·라이선스 데이터 보관 (c) 둘의 정합(reconciliation) (d) 데이터 변경 통제"로 환산된다. Tier 2는 조달·배치·이동·폐기 이벤트가 자산 레코드에 연결되는 것(ERP/HR/서비스데스크 연계), Tier 3는 비용·계약·재무 최적화 리포트로 환산할 수 있다. 이것이 스코어카드의 1차 축이 되어야 한다.
- 19770-2는 "SWID 태그 읽기"를 도구 기능 요구로 만들지만, 채택률이 낮으므로(Adobe/MS 일부) 벤치마크에서는 "SWID 태그가 있으면 우선 사용, 없으면 Add/Remove Programs(ARP) 레지스트리·MSI 데이터로 폴백"하는 구조가 현실적이다.
- 19770-3 Ent 스키마의 개념(권리 단위, 메트릭, 제품/버전 범위, 유효기간, 유지보수 포함 여부)은 자사 빌드의 라이선스 테이블 설계 체크리스트로 바로 쓸 수 있다.
- ITIL 4 관점의 "완료 정의": 각 자산이 라이프사이클 상태 중 하나에 반드시 속하고, 상태 전이가 이벤트(구매→입고→배치→이동→반납→폐기)와 함께 기록되며, 대장에 owner/custodian이 비어 있지 않아야 한다.

### Gaps
- 19770-1:2017 Annex A의 15개 프로세스 영역 공식 명칭 목록은 공개 소스에서 확보하지 못했다(ISO 본문 유료). 한 커뮤니티 사이트는 "27개 프로세스 영역"이라 적으나 이는 2012년판 또는 다른 집계 방식으로 추정되며 ISO 공식 수치(15)와 상충한다 — [itamstandards.org](https://itamstandards.org/iso-iec-19770-1/).
- 19770-2:2015의 필수 요소 전체 목록(8.5.1절)은 표준 본문 확인이 필요하다. DMTF/IETF 자료는 2차 출처다.
- 19770-8, -11, -22 등 후속 파트의 상세는 조사하지 못했다.
- ITIL 4 ITAM Practice Guide의 공식 PSF(Practice Success Factors)·핵심 메트릭 목록과 자산 대장 최소 속성 목록은 PeopleCert 원문에서만 확인 가능하며 공개 소스에 없었다.

---

## KQ2. Gartner/Forrester/IDC/G2의 ITAM·SAM 도구 평가 기준

### Takeaway
Gartner는 SAM Tools Magic Quadrant를 2020년경 중단하고 Market Guide로 대체했으며(최신판 2026-01-15), MQ는 "SAM Managed Services"(2024·2025)와 "SaaS Management Platforms"에만 남아 있다. 공개된 2차 요약에 따르면 Gartner가 보는 SAM 도구 핵심 기능은 discovery → normalization → reconciliation → optimization → reporting이고, Flexera 요약 기준 10개 활동(플랫폼·권리 발견, 소비·권리 식별, 정규화, 정합, 거버넌스, 최적화, 자산정보 공유 등)이다. Forrester는 2025 Q1 SAM Wave에서 9개 벤더를 14개 기준으로 평가했다. 가중치는 전부 유료 보고서 내부에 있어 확보하지 못했다.

### Cited Findings

**Gartner**
- Gartner Market Guide for Software Asset Management Tools, 2026년 1월 15일 발행, 저자 Yolanda Harris·Angelica Wekwete·Jaswant Kalay; Oomnitza는 Representative Vendor로 등재 — [Oomnitza 리프린트 페이지](https://www.oomnitza.com/resources/gartner-2026-market-guide-for-software-asset-management-tools/)
- 직전 판은 2024년 11월(저자 Jaswant Kalay, Ciaran Hudson, Yolanda Harris), 그 전은 2022년 8월 31일 — [USU 리프린트](https://www.usu.com/en/resources/24/gartner-sam-tools-market-guide); [USU 2022](https://media.usu.com/en-us/resources/22/gartner-2022-market-guide-for-software-asset-management-tools)
- SAM Tools MQ는 2020년에 중단되고 Market Guide로 대체되었다는 커뮤니티 발언(비공식) — [Flexera Community](https://community.flexera.com/t5/Flexera-One-Forum/Gartner-Magic-Quadrant-for-Asset-management-Tools-2022/td-p/236889); 상업 블로그도 "SAM Tools MQ 은퇴, SAM Managed Services MQ는 별도 발행"이라 설명 — [acciyo](https://www.acciyo.com/software-asset-management-tools-gartner/)
- Gartner가 정의하는 SAM 도구 핵심 역량: discovery, normalization, reconciliation, optimization, reporting — [Gartner Peer Insights SAM Tools 시장 정의](https://www.gartner.com/reviews/market/software-asset-management-tools)
- Flexera의 Market Guide 해설: SAM 도구가 갖춰야 할 10개 핵심 활동 = 플랫폼·권리(entitlement) 발견, 소비(consumption)·권리 식별, 소비·권리 데이터 정규화, 정합(reconciliation), 거버넌스, 최적화, 자산 정보 공유 등 — [Flexera 블로그](https://www.flexera.com/blog/it-asset-management/how-to-choose-a-software-asset-management-tool-insights-from-the-gartner-market-guide/)
- Zylo 해설: SAM 도구는 라이선스 권리를 해석하고 소비 데이터 수집을 자동화하며 벤더별 ELP(Effective License Position)를 산출; Gartner는 이를 "소프트웨어·SaaS 지출 최적화 제품"으로 기술 — [Zylo](https://zylo.com/blog/zylo-recognized-in-gartner-market-guide-for-software-asset-management-tools)
- 2026 Market Guide에 대한 ServiceNow 인용: "사일로화된 기능과 품질·완전성·일관성·정확성이 부족한 데이터를 내는 SAM 도구는 비용 거버넌스를 저해하고 AI 분석에 필요한 데이터 집계를 방해한다" — [ServiceNow 리프린트](https://www.servicenow.com/lpayr/gartner-market-guide-software-asset-management-tools.html)
- Gartner Critical Capabilities for SaaS Management Platforms(2024-07-29)의 역량 축: Discovery, Unapproved App Identification, Expense Management, Adoption, Workflow Orchestration + license entitlement optimization, application rationalization, compliance analysis — [Gartner 문서 5621791](https://www.gartner.com/en/documents/5621791)
- Gartner MQ/CC for SAM Managed Services: MQ 2024-08-26, CC 2024-09-03(Leaders: Anglepoint, Crayon, Livingstone, SoftwareOne), CC 2025-10-06 — [Gartner 5725351](https://www.gartner.com/en/documents/5725351); [Gartner 7019798](https://www.gartner.com/en/documents/7019798)
- Gartner는 2026년 "Market Guide for Hardware Asset Management Tools"도 발행(Oomnitza 리프린트 언급) — [Oomnitza 홈](https://oomnitza.com/)

**Forrester / IDC**
- The Forrester Wave: Software Asset Management Solutions, Q1 2025 — 9개 주요 SAM 벤더를 14개 기준으로 평가; 저자 Biswajeet Mahapatra, Frederic Giron, Min Say, Bill Nagel; AI/ML 통합 비전이 있는 벤더 우선 권고(벤더 리프린트 해석) — [USU 리프린트](https://www.usu.com/en/resources/25/the-forrester-wave-software-asset-management-solutions-q1-2025)
- Forrester의 ITAM Wave(2006, 43개 기준), IT Asset Life-Cycle Management Wave(2009, 81개 기준)는 구자료 — [Forrester 2006](https://www.forrester.com/report/the-forrester-wave-it-asset-management-q3-2006/RES38925); [Forrester 2009](https://www.forrester.com/report/the-forrester-wave-it-asset-life-cycle-management-q2-2009/RES44904)

**G2 IT Asset Management 카테고리**
- 정의: ITAM 소프트웨어의 중심 목적은 회사의 디지털·물리·가상 구성요소를 포괄하는 DB를 만들고 유지하는 것; 계약·재무·리스크 관리, 패치·정책 거버넌스, 실시간 모니터링(신규·미승인 장치 탐지) 기능을 흔히 포함 — [G2 카테고리](https://g2.com/categories/it-asset-management?page=2)
- 카테고리 포함 조건 3가지: (1) 워크스테이션·서버·프린터·라우터·IoT 등 물리 항목을 포괄하는 중앙 자산 DB 유지, (2) 각 자산의 라이선스·보증·계약·라이프사이클·사용자 상태 보고, (3) 자동 발견 또는 수동 생성 지원 — 동일 출처
- 일부 ITAM 도구는 SAM 기능도 포함(하드웨어 전용 vs 통합) — 동일 출처

### Inferences
- Gartner 공식 가중치는 입수 불가하므로, 스코어카드 가중치는 (a) Gartner 5대 핵심 기능(discovery/normalization/reconciliation/optimization/reporting), (b) 19770-1 티어, (c) G2 포함 조건을 교차해 자체 설정하는 것이 방어 가능한 방법이다.
- Gartner 2026 메시지("데이터 품질이 거버넌스·AI 분석의 전제")는 자사 빌드 평가에서 "정규화 품질·완전성·일관성" 지표를 기능 유무보다 높게 가중할 근거가 된다.
- SaaS 관리 역량(Unapproved app 탐지, 비용관리, adoption)은 Gartner가 SAM 도구와 분리해 평가하므로, 제조업 온프레미스 중심 자사 빌드에서는 별도 축(낮은 가중)으로 두는 것이 타당하다.

### Gaps
- Gartner Market Guide(2024, 2026)의 원문 권고사항과 Critical Capabilities 가중치, Forrester Wave Q1 2025의 14개 기준 명칭은 유료 보고서 안에 있어 확보하지 못했다.
- IDC MarketScape의 ITAM/SAM 2024–2025 보고서는 검색에서 확인되지 않았다.
- G2의 세부 기능 체크리스트(리뷰 항목 단위)는 카테고리 설명 수준만 확인되었다.

---

## KQ3. ITAM 효과성 벤치마크 KPI와 목표값

### Takeaway
공개 소스에서 "업계 표준" 목표값은 거의 없고, 대부분 벤더 블로그가 제안하는 수치다. 실무에서 반복 등장하는 KPI는 인벤토리 정확도(발견 자산 vs 대장 일치율), 완전성(구매·배치·폐기 자산의 등록 여부), 에이전트 보고율(최근 N일 내 체크인), 라이선스 활용률/ELP, 고아 자산, 리프레시 대상 비율, 감사 변동(spot check 불일치)이며, 수치 목표는 조직 내부 기준선으로 설정하라는 것이 공통 권고다. 참고용 수치로 95% 정확도(2009), 90% 미만 인벤토리율은 사각지대 신호(Proactivanet), 에이전트 7/30/90일 건강도 버킷(Cyrisma)이 있다.

### Cited Findings
- 자산 인벤토리 정확도 = "발견된 자산 중 공식 자산 대장과 일치하는 비율"; 완전성 = "구매·배치·폐기 자산이 실제로 모두 기록되었는지"는 별개 지표; 에이전트 커버리지는 노트북·데스크톱 정확도의 근간이며 에이전트 건강을 매일 점검하고 "실제 보고 중인 장치 비율"을 측정하라 — [Cleverence: IT inventory report](https://www.cleverence.com/articles/for-business/it-inventory-report-4827/)
- 98% 인벤토리율이라는 헤드라인 수치도 잘못된 라이선싱·낮은 활용·미관리 장치를 숨길 수 있다; 각 KPI에 정의·공식·데이터 소스를 붙여 감사에서 방어 가능하게 하라 — 동일 출처
- 기타 KPI: 라이선스 활용률(할당 vs 설치 vs 권리), 리프레시 준비도(정책 연한 초과 자산), 감사 변동(spot check 실패 항목), 고아 자산(소유자 없는 활성 장치) — [ITU Online: Key metrics](https://www.ituonline.com/blogs/key-metrics-for-measuring-it-asset-management-success/)
- "인벤토리 대비 설치 자산 비율이 90% 미만이면 사각지대 신호"(벤더 의견) — [Proactivanet 10 KPIs](https://www.proactivanet.com/en/blog/proactivanet-en/10-essential-it-asset-management-kpis-that-every-cio-should-measure/)
- "95% 정확도는 건전한 ITAM 실무와 도구로 달성 가능" — [EDN 2009](https://www.edn.com/?p=4019160) (구자료)
- ITAM Review: 총 자산 수는 가장 기본적인 모니터링 지표; 교육 설문에서 47.2%만 정확도를 측정한다고 답함; 정확도 산정의 업계 표준 방식은 불확실 — [ITAM Review 2016](https://marketplace.itassetmanagement.net/2016/04/19/itam-metric) (구자료); Microsoft SAM Services Baseline Guide는 "400 같은 딱 떨어지는 배포 수"를 ELP 정확도 적신호로 본다 — [ITAM Review: Basic ITAM metrics](https://marketplace.itassetmanagement.net/?p=21763)
- 에이전트 상태 모델(최근 체크인 경과 기준): Healthy ≤7일, At Risk 7–30일, Warning 30–90일, Likely Decommissioned >90일 — [Cyrisma Asset Inventory](https://support.cyrisma.com/articles/6406022526-asset-inventory)
- 에이전트 건강은 heartbeat 주기, 마지막 수집 타임스탬프, 보고 SLA 임계치로 정의 — [Trio Agent Health](https://help.trio.so/en/articles/13234640-agent-health); 유효 보호 엔드포인트 = min(Installed, Reporting, Healthy, Compliant) 모델 — [codingace 계산기](https://codingace.net/cbs/endpoint_agent_coverage.html) (비공식)
- 벤더 인용 업계 수치: 주당 2%의 에이전트가 실패, 어느 시점에든 42%의 엔드포인트가 어떤 형태로든 미보호 — [Oomnitza](https://www.oomnitza.com/coverage-gap-analysis/) (벤더 주장)
- NIST CSF 2.0·CIS Controls v8.1은 자산 인벤토리를 요구하지만 운영 임계치(universal thresholds)를 정하지 않는다 — [Level.io](https://level.io/blog/what-makes-a-healthy-endpoint)
- Flexera 2025 State of ITAM(2025-06-18, 506명): 기술 스택 전체에 대한 완전한 가시성 보유 비율 43%(전년 47%에서 하락); ITAM–클라우드 팀 협업 44%, FinOps 38%; 45%가 최근 3년간 감사 비용 100만 달러 초과; 55%의 ITAM 팀이 SaaS 사용 추적 담당; 데스크톱 지출은 여전히 최대 낭비 영역 — [Flexera 보도자료](https://www.flexera.com/about-us/press-center/it-teams-losing-visibility-according-to-flexera-2025-state-of-itam-report); Channel Insider 기사는 수치 방향을 반대로 적어 상충 — [Channel Insider](https://www.channelinsider.com/news-and-trends/flexera-2025-itam-visibility-finops/)
- Flexera 2024 State of ITAM(2024-06-12, 503명): 53%가 완전한 가시성 확보·유지에 어려움; 22%가 3년간 감사 비용 500만 달러 초과(2023년 15%); 데스크톱·데이터센터·SaaS·클라우드 지출의 약 1/3이 저활용·낭비(SaaS 32%) — [Flexera 2024 보도자료](https://flexera.com/about-us/press-center/flexera-2024-state-of-itam-report-finds-software-audit-costs-continue-to-rise); [Raconteur](https://www.raconteur.net/technology/cut-waste-and-boost-efficiency-with-it-asset-visibility); CIO Influence는 낭비를 20–30%로 요약 — [CIO Influence](https://cioinfluence.com/it-and-devops/flexera-2024-state-of-itam-report-finds-that-it-teams-face-increasing-audit-fines/)

### Inferences
- 자사 빌드의 "done" 수치 목표는 공개 벤치마크가 아니라 내부 기준선으로 정의하되, 다음 정도가 2차 자료와 정합한다: 에이전트 보고율(7일 내 체크인) ≥95%, 30일 내 ≥98%(Cyrisma 버킷 응용); 대장 대비 발견 자산 일치율 ≥95%(EDN 2009 수치); 90% 미만은 즉시 조치 트리거(Proactivanet).
- "데스크톱 지출이 최대 낭비 영역"(Flexera 2025)이므로 PC 에이전트 기반 빌드는 미사용 소프트웨어 회수(reclaim) 지표를 핵심 KPI로 삼을 가치가 있다.
- 감사 방어 KPI는 "ELP를 벤더별로 산출 가능한가"와 "라운드 넘버가 아닌 실측 기반인가"(MS SAM Baseline 적신호)로 환산할 수 있다.

### Gaps
- 인벤토리 정확도·에이전트 커버리지·라이선스 준수율의 권위 있는 업계 벤치마크 수치(Gartner/Forrester/IAITAM 발행)는 찾지 못했다.
- "신규 장치 탐지 소요시간(time-to-detect)"에 대한 공개 목표값은 검색되지 않았다.
- Flexera 보고서 전문은 입수하지 못해 보도자료·2차 기사 수치에 의존했다.

---

## KQ4. 소프트웨어 정규화/인식 카탈로그와 공개 대안

### Takeaway
상용 ITAM의 핵심 해자는 정규화 카탈로그다. Flexera Technopedia는 8.5만+ 제조사, 160만 소프트웨어 릴리스, 230만 하드웨어 모델(벤더 주장)을 보유하고 ServiceNow SAM Content Service는 주 단위로 정규화 규칙을 갱신한다. 인하우스 빌드가 어려운 이유는 ARP(Add/Remove Programs) 원시 데이터의 퍼블리셔·제품명·버전 변형이 매우 많고(사례: Adobe 47개 변형, 3,200→847 퍼블리셔), 카탈로그가 지속 유지보수를 요구하기 때문이다. 공개 대안(SWID, purl, winget 매니페스트, osquery programs)은 식별자·스키마는 제공하지만 "완성된 매핑 카탈로그"는 아니다.

### Cited Findings
- Technopedia 규모: 85,000+ 제조사, 160만 소프트웨어 릴리스, 230만 하드웨어 모델(ServiceNow Store 내 Flexera 앱 설명, 벤더 주장) — [ServiceNow Store](https://tpp.servicenow.com/store/app/2eabafaa1b246a50a85b16db234bcbfd)
- Flexera는 Technopedia에 제품·릴리스·버전·에디션을 지속 추가하며(구 릴리스 포함) 라이프사이클 날짜는 사후 채워질 수 있음 — [Flexera Community](https://community.flexera.com/t5/Data-Platform-Forum/Data-Platform-Information/m-p/283776); 카탈로그에 없는 소프트웨어는 Private Catalog 항목을 Technopedia 항목에 매핑해 정규화 — [Flexera Docs](https://docs.flexera.com/data-platform/user-console-guide/private-catalog-basics/creating-normalize-mapping-for-private-catalog-software/)
- BDNA Normalize는 Technopedia Catalog를 사용하며 BDNA는 현재 Flexera 소유 — [Flexera BDNA 5.5 문서](https://docs.flexera.com/dataplatform55/dp55webhelp/Content/helplibrary/Configuring__Synchronization.htm); [Gartner 리뷰](https://www.gartner.com/reviews/product/bdna-normalize)
- Flexera 자사 비교 페이지: Lansweeper 정규화는 "Partial", Flexera 인식률 98%+ (경쟁사 마케팅) — [Flexera vs Lansweeper](https://flexera.com/products/compare/flexera-vs-lansweeper)
- Lansweeper Sites는 머신러닝으로 소프트웨어명·퍼블리셔·버전을 정규화(Classic 미지원) — [Lansweeper Docs](https://docs.lansweeper.com/docs/software-data-normalization); 정규화 엔진을 OEM 임베드 상품으로도 제공 — [Lansweeper Embedded](https://embedtech.lansweeper.com/products/software-normalization/)
- ServiceNow SAM Content Service: 정규화는 활성(active) 퍼블리셔·제품·규칙에만 적용, 콘텐츠 버전 비활성화 가능, 설치 데이터 기반 주간 업데이트, 대시보드는 "완전 정규화된 설치 비율"을 보고 — [ServiceNow Docs: Content Service](https://www.servicenow.com/docs/r/pyt4lF0aAin_R6wVyV414Q/BgAHUWtzbvOa6OvfZAaFFQ); [Normalization dashboard](https://www.servicenow.com/docs/r/pyt4lF0aAin_R6wVyV414Q/6YUbt_Fd~gduwpXua3UVzw)
- 실무 사례(벤더 블로그): 동일 벤더에 47개 퍼블리셔명 변형; 6주 프로젝트로 원시 3,200개 퍼블리셔를 847개 정규 퍼블리셔로 축소, Adobe 47개 항목→1개 퍼블리셔·23개 제품군 — [Virima](https://virima.com/blog/software-publisher-normalization-itam)
- osquery `programs` 테이블: Windows 설치 소프트웨어 표준 조회 수단(name, version, install_location, publisher 등), `registry` 로직을 일부 재사용해 구현; ARP 마법사에 등록된 프로그램만 포함하므로 누락 가능(WindowsApps/MSIX 미반영 보고) — [Kolide](https://www-assets.kolide.com/features/device-inventory/properties/windows-programs); [osquery chat](https://chat.osquery.io/t/16141224/hello-we-are-using-the-programs-to-fetch-all-apps-installed-); [TryHackMe 예시(7-Zip 21.07 / Igor Pavlov)](https://www.jalblas.com/blog/tryhackme-osquery-the-basics-soc-level-1/)
- winget 매니페스트: PackageIdentifier는 `Publisher.Package` 형태(대소문자 구분); PackageVersion은 ARP 표시값과 일치시키는 것이 모범; Publisher/PackageName이 ARP와 다르면 `AppsAndFeaturesEntries`(DisplayName, DisplayVersion, ProductCode, UpgradeCode, InstallerType, PackageFamilyName)로 상관관계 명시; MSI ProductCode 포함 권장 — [Microsoft winget-pkgs Authoring](https://github.com/microsoft/winget-pkgs/blob/master/doc/Authoring.md); [MS Learn manifest](https://learn.microsoft.com/windows/package-manager/package/manifest); [SchemaStore installer 1.0.0](https://www.schemastore.org/winget-pkgs-installer-1.0.0.json)
- purl(Package URL): 2017년 nexB의 Philippe Ombredanne이 만든 외부 식별자; Windows 전용 타입 논의에서 "SWID를 purl과 함께 쓰자"는 의견과 "winget 매니페스트는 purl로 완전 표현 불가"라는 의견이 있음; chocolatey는 자체 타입 보유 — [purl-spec issue 255](https://github.com/package-url/purl-spec/issues/255); [SWHID와 purl 비교(2026-03)](https://toscalix.com/2026/03/31/swhid-and-purl/)

### Inferences
- 인하우스 정규화의 현실적 설계: (1) 1차 키 = ARP DisplayName+Publisher+DisplayVersion+ProductCode/UpgradeCode, (2) SWID 태그가 있으면 tagId로 우선 매핑, (3) winget 커뮤니티 매니페스트(공개 GitHub 저장소)의 `AppsAndFeaturesEntries`를 ARP→PackageIdentifier 역매핑 테이블로 활용, (4) 매핑 실패분은 "미정규화 큐"로 분류해 사람이 승인. 이 구조면 "완전 정규화된 설치 비율"(ServiceNow 대시보드 지표)을 자사 KPI로 그대로 채택할 수 있다.
- winget 저장소는 글로벌 오픈소스/상용 앱 중심이라 한국 제조업 특유의 CAD/CAM, MES 클라이언트, 그룹웨어 에이전트, 보안 에이전트는 커버하지 못할 가능성이 높다. 이 영역은 자체 "사설 카탈로그"(Flexera Private Catalog 개념)로 유지해야 한다.
- 상용 대비 벤치마크 항목: 카탈로그 규모 자체보다 "자사 환경에서의 정규화율(%)"과 "미정규화 항목 처리 리드타임"을 비교 지표로 두는 것이 공정하다.

### Gaps
- ServiceNow 콘텐츠 라이브러리의 퍼블리셔/제품 수, Lansweeper 카탈로그의 출처(자체 vs 제3자)는 확인되지 않았다.
- Flexera 인식률 98%+는 경쟁사 비교 페이지의 주장이며 독립 검증 자료가 없다.
- Virima 블로그가 인용한 "Flexera 2024 State of ITAM의 미정규화 통계"는 원문에서 확인되지 않았다.

---

## KQ5. Oracle EBS 11i Fixed Assets·HR 연계: 테이블, 패턴, 지원 종료 영향

### Takeaway
EBS 11i(11.5.10.2)는 2013-12-01부터 Sustaining Support 상태로 신규 패치·보안 업데이트·신규 제품 인증이 없다. FA 핵심 테이블은 FA_ADDITIONS_B(자산 식별: ASSET_NUMBER, TAG_NUMBER, SERIAL_NUMBER, INVENTORIAL), FA_BOOKS(장부별 원가/감가), FA_DISTRIBUTION_HISTORY(사원·위치·계정 배분, 버전형), FA_LOCATIONS(위치 플렉스필드), FA_MASS_ADDITIONS(AP 송장 유래 스테이징)이며, 실물 재고 대조는 FA_INV_INTERFACE(물리 인벤토리 스테이징)와 "자산번호→태그번호→일련번호" 순 매칭 로직을 쓴다. 통합 패턴은 (1) 읽기 전용 DB 뷰/DB Link, (2) FA_MASS_ADDITIONS 인터페이스 테이블 적재 후 Post, (3) FA_ADDITION_PUB.DO_ADDITION API, (4) Web ADI Mass Additions 템플릿이며, 11i의 iRep은 irep.oracle.com 호스팅 형태였다. 최신 ODP.NET Core(23ai)는 DB 19c 이상만 지원하므로 11i의 10g/11g DB에 직접 붙이려면 구버전 드라이버 또는 중간 계층이 필요하다.

### Cited Findings

**지원 상태**
- EBS 11.5.10.2는 2013년 12월 1일부터 Sustaining Support; Severity 1 한정 예외는 2015-12-31 종료, 2016-01-01부터 표준 Sustaining Support로 복귀 — [Oracle EBS Tech 블로그](https://blogs.oracle.com/ebstech/reminder-ebs-11i-reverts-to-sustaining-support-on-january-1-2016); [Oracle FAQ](https://blogs.oracle.com/ebstech/frequently-asked-questions-about-latest-ebs-support-changes)
- Sustaining Support에서 제외되는 것: 신규 업데이트·수정·보안 경보·데이터 수정·CPU(Critical Patch Update), 신규 세무·법규 업데이트, 신규 제3자 제품 및 신규 Oracle 제품과의 인증; 남는 것: 기존 패치·문서 다운로드, 진단 지원; Oracle 권고는 12.2 업그레이드 — 동일 출처; [Oracle CPU 예외 안내](https://blogs.oracle.com/ebstech/critical-patch-updates-during-ebs-11i-exception-to-sustaining-support-period)
- 11i와 DB 인증: DB 11.2.0.4는 11.5.10.2 + ATG PF.H RUP 6 이상에서 Solaris SPARC(10, 11), AIX(5.3, 6.1)만 인증, Solaris x86-64는 DB 티어 전용 split-tier; 상호운용 노트는 MOS 881505.1 — [Oracle EBS Tech 블로그](https://blogs.oracle.com/ebstech/11204-database-certified-with-e-business-suite-on-11-new-platforms); DB 10gR2 10.2.0.5도 11i에 인증 — [MOSC](https://community.oracle.com/mosc/discussion/2744508/ebs-performance-tip-of-the-week-oracle-database-10gr2-10-2-0-5-is-now-certified-with-oracle-e-busin)

**FA 테이블·개념** (주의: Oracle 공식 데이터사전 페이지는 Fusion Cloud 기준이며 EBS와 유사하나 동일하지 않음)
- FA_ADDITIONS_B: 자산 식별용 기술 정보, PK ASSET_ID, ASSET_NUMBER는 사용자 정의 식별자, INVENTORIAL 플래그(물리 인벤토리 포함 여부) — [Oracle Fusion 데이터사전 FA_ADDITIONS_B](https://docs.oracle.com/en/cloud/saas/financials/26a/oedmf/faadditionsb-29577.html)
- FA_DISTRIBUTION_HISTORY: 자산별 사원(employee)·위치·회계 플렉스필드 배분 정보, PK DISTRIBUTION_ID+BOOK_TYPE_CODE, FK ASSET_ID→FA_ADDITIONS_B, LOCATION_ID→FA_LOCATIONS; 이전(transfer) 시 기존 행을 닫고 새 행을 만드는 버전형 구조; 물리 인벤토리가 "fully matched"면 inventory date가 이 테이블에 스탬프 — [Oracle Fusion FA_DISTRIBUTION_HISTORY](https://docs.oracle.com/en/cloud/saas/financials/25d/oedmf/fadistributionhistory-10637.html); [Oracle: Physical Inventory Posting Statuses](https://docs.oracle.com/en/cloud/saas/financials/26a/faalm/how-physical-inventory-posting-statuses-are-set.html)
- FA_LOCATIONS: 유효한 위치 플렉스필드 세그먼트 조합(SEGMENT1…), PK LOCATION_ID — [Oracle Fusion FA_LOCATIONS](https://docs.oracle.com/en/cloud/saas/financials/24a/oedmf/falocations-11515.html)
- FA_INV_INTERFACE(물리 인벤토리 스테이징): TAG_NUMBER(사용자 정의 고유 추적번호), SERIAL_NUMBER, ASSIGNED_TO(사원 ID), 위치 필드, DISTRIBUTION_ID — [Oracle Fusion FA_INV_INTERFACE](https://docs.oracle.com/en/cloud/saas/financials/25c/oedmf/fainventoryint-29860.html)
- 물리 인벤토리 비교 상태: "Difference"(위치·수량·사원 불일치), "Asset not in book"(자산번호·태그번호·일련번호로 고유 매칭 실패); 11i 문서는 자산번호 → 태그번호 → 일련번호 순으로 매칭 — [Oracle 11i Physical Inventory](https://docs.oracle.com/cd/A60725_05/html/comnls/us/fa/physin03.htm)
- EBS 실무 쿼리: `fa_additions`(뷰)에서 `tag_number`, `serial_number` 사용, `fa_distribution_history.location_id = fa_locations.location_id`, `fa_distribution_history.asset_id = fa_additions.asset_id` 조인 — [Doyensys 블로그](https://doyensys.com/blogs/query-to-new-assets-addition-in-oracle-apps/) (비공식)
- FA_MASS_ADDITIONS: SQL*Plus 등으로 적재 후 "Mass Additions Post"로 자산 생성; DB 정의상 필수 컬럼은 없으나 Post 프로그램의 컬럼 규칙을 따라야 하며 NULL로 표시된 컬럼은 Post 프로그램이 사용하므로 적재 금지 — [Oracle 11i: Importing Your Asset Information](https://docs.oracle.com/cd/A60725_05/html/comnls/us/fa/maoi05.htm)
- AP→FA: "Create Mass Additions"는 Payables 송장 분배 라인을 FA 인터페이스 테이블로 옮기며, 자산 계정 또는 "Track as Asset" 플래그가 켜진 비용 계정 라인이 대상; 송장 승인·GL 전기·처리일 이전 조건; 11i 조건은 assets_addition_flag='U', asset_tracking_flag='Y', GL 전기 — [Oracle 11i: Create Mass Additions from Payables](https://docs.oracle.com/cd/A60725_05/html/comnls/us/fa/massad04.htm); [Mass Additions 슬라이드](https://www.slideshare.net/slideshow/mass-additions/140857792)
- FA 인터페이스 테이블 목록(비공식): FA_PRODUCTION_INTERFACE, FA_INV_INTERFACE, FA_MASS_ADDITIONS — [oracleapps88](https://oracleapps88.blogspot.com/2012/11/oracle-module-wise-tables.html?m=1)

**통합 API·도구**
- Additions API: `FA_ADDITION_PUB.DO_ADDITION`; 표준 인터페이스에 맞추기 어려운 커스텀 연동용; BONUS_RULE/CEILING_NAME/GROUP_ASSET_ID를 null로 하려면 FND_API.G_MISS_CHAR/G_MISS_NUM 필요 — [Oracle R12.2 Assets Additions API](https://docs.oracle.com/cd/E26401_01/doc.122/e48755/T293142T471916.htm) (R12 문서, 11i는 별도 확인); 11i에서 루프 호출 시 두 번째부터 "asset already exists in this book" 오류 보고 — [MOSC 스레드](https://community.oracle.com/mosc/discussion/3968557/fa-additions-pub-do-addition-in-a-loop)
- Web ADI 경로는 Mass Additions 템플릿으로 FA_MASS_ADDITIONS에 적재(R12에서 컬럼 확장) — [R12 FA 변경 슬라이드](https://www.slideshare.net/slideshow/oracle-r12-fixed-assets-changes-from-11i/2202848) (비공식)
- Oracle Integration Repository(iRep)는 R12부터 제품 내장, 11.5.10은 irep.oracle.com 호스팅으로 제공; Oracle Adapter는 11.5.10/R12에서 iRep을 통합 콘텐츠의 source of truth로 사용, 11.5.10 이전은 DB 직접 접속; 11i 최소 요건 OWF.G Rollup 7, ATG_PF.H.Delta.6(RUP6) — [Oracle Adapter 문서](https://docs.oracle.com/cd/E29542_01/integration.1111/e10537/T430238T430242.htm)

**HR 테이블**
- PER_ALL_ASSIGNMENTS_F: 직무·관리자·부서(ORGANIZATION_ID)·급여·비용계정 등 배치 정보, PRIMARY_FLAG로 주 배치 식별; PER_ALL_PEOPLE_F.PERSON_ID ↔ PER_ALL_ASSIGNMENTS_F.PERSON_ID 조인; PER_PERSON_TYPES/PER_PERSON_TYPE_USAGES로 사원/계약직 구분; 두 테이블 모두 date-effective(EFFECTIVE_START_DATE/END_DATE)라 "현재/특정일/이력" 슬라이스를 정해야 함; 퇴직자는 SYSDATE 필터로는 누락 — [Cleverence HCM tables](https://www.cleverence.com/amp/articles/oracle-documentation/hcm-tables-and-views-4829/); [Oracle Forums](https://forums.oracle.com/ords/apexds/post/ex-employees-in-per-all-assignments-8929)
- MANAGER_FLAG가 전원 NULL인 사례가 있어 보고 라인은 SUPERVISOR_ID가 더 유용 — [MOSC](https://community.oracle.com/mosc/discussion/3582452/manager-flag-is-null)
- HR_ALL_ORGANIZATION_UNITS = 조직 단위 정의(2007년 HRMS 테이블 목록) — [Scribd Oracle HRMS Tables](https://www.scribd.com/document/349669403/Oracle-HRMS-Tables) (구자료, 비공식)

**.NET/JDBC 드라이버 호환**
- ODP.NET Core 23ai: Oracle DB 19c 이상 필요, 64-bit .NET만 지원 — [Oracle 23 System Requirements](https://docs.oracle.com/en/database/oracle/oracle-database/23/odpnt/InstallSystemRequirements.html); ODP.NET Core 19.3.2 문서는 DB 11g Release 2 이상 — [Oracle 19.3.2 System Requirements](https://docs.oracle.com/en/database/oracle/oracle-data-access-components/19.3.2/odpnt/InstallSystemRequirements.html); NuGet Oracle.ManagedDataAccess.Core 2.19.x 계열이 19c 세대 — [NuGet 2.19.300](https://feed.nuget.org/packages/Oracle.ManagedDataAccess.Core/2.19.300)
- JDBC 19.x 드라이버는 19c 문서 기준 DB 11.2.0.4 인증, 26ai 문서에서는 19.x 드라이버 인증 대상이 26/21/19만 — [Oracle 19c JDBC Getting Started](https://docs.oracle.com/en/database/oracle/oracle-database/19/jjdbc/JDBC-getting-started.html); [Oracle 26 JDBC](https://docs.oracle.com/en/database/oracle/oracle-database/26/jjdbc/JDBC-getting-started.html); 클라이언트/서버 상호운용 매트릭스는 MOS 207303.1, JDBC는 401934.1 — [Red Hat 참고](https://access.redhat.com/node/897363)

### Inferences
- 자사 ITAM(.NET 서버)과 EBS 11i의 안전한 연계 패턴: (A) EBS DB에 읽기 전용 계정 + 커스텀 뷰(FA_ADDITIONS_B/FA_BOOKS/FA_DISTRIBUTION_HISTORY/FA_LOCATIONS/PER_* 조인) → ITAM 서버가 주기 동기화(pull). 쓰기는 FA_MASS_ADDITIONS 적재 또는 FA_INV_INTERFACE(물리 인벤토리 결과) 적재 후 EBS 표준 프로그램으로 Post/Compare. 직접 FA_* 기본 테이블 쓰기는 금지. (B) 드라이버: EBS DB가 10.2/11.2라면 ODP.NET Core 2.19.x(11gR2+)로 가능하나 10g는 공식 지원 범위 밖이므로, 중간에 지원되는 Oracle DB(19c 이상)에 DB Link로 뷰를 노출하거나 EBS 측에서 파일/REST 중계 계층을 두는 것이 안전하다. (C) 11i는 iRep/SOA Gateway가 내장되지 않으므로 REST/SOAP 표준 API 기반 연동은 기대할 수 없다.
- 고정자산↔IT자산 대조 키는 EBS의 물리 인벤토리 로직과 동일하게 "ASSET_NUMBER → TAG_NUMBER → SERIAL_NUMBER" 순으로 설계하면 FA_INV_INTERFACE 양식으로 결과를 되돌려 줄 수 있다. 따라서 PC 에이전트는 BIOS 일련번호(SMBIOS)와 자산 태그(라벨/바코드 또는 BIOS Asset Tag)를 반드시 수집해야 한다.
- 11i는 자본화 단위(한 송장 라인 = 여러 PC 묶음) 때문에 FA 자산 1건 ≠ IT 자산 1대일 수 있다. 벤치마크 항목으로 "FA 자산 1:N IT 자산 매핑 지원"을 둬야 한다.
- Sustaining Support의 실무 함의: EBS 측 변경(새 뷰·트리거·패치) 자체가 위험이므로 ITAM 측에서 변환·검증을 수행하고 EBS에는 "표준 인터페이스 테이블 적재"만 남기는 것이 리스크 최소화다.

### Gaps
- Oracle eTRM 11i의 FA_ADDITIONS_B 컬럼(TAG_NUMBER, SERIAL_NUMBER, MANUFACTURER_NAME, MODEL_NUMBER 등) 공식 정의는 접근하지 못했고 Fusion 데이터사전과 커뮤니티 쿼리로 대체했다. 실제 11i 인스턴스에서 `DESC FA_ADDITIONS_B`로 확인 필요.
- 11i Web ADI의 FA 통합자(integrator) 공식 문서, 11i용 FA_ADDITION_PUB 공식 문서는 찾지 못했다(R12 문서 대체).
- HR_ALL_ORGANIZATION_UNITS를 조인한 검증된 11i 부서 조회 쿼리는 확보하지 못했다(Fusion 예시만 존재).
- ODP.NET의 DB 10g 지원 여부는 MOS 726240.1 확인이 필요하며 공개 문서에서 확인되지 않았다.
- Oracle Lifetime Support Policy 최신 문서(2025–2026)를 직접 확인하지 못했으나, 11i가 Sustaining Support라는 사실은 2013년 이후 변동이 없고 Sustaining은 기간 제한이 없다.

---

## KQ6. 한국 제조기업 ITAM의 규제·컴플라이언스 드라이버

### Takeaway
민간 제조업에 직접 적용되는 드라이버는 (1) 개인정보보호법 §29 및 「개인정보의 안전성 확보조치 기준」(개인정보보호위원회 고시 제2023-6호, 2023-09-22 시행; 2025년 추가 개정으로 위험분석 기반 망차단·접근권한·접속기록 조항 강화, 일부 1년 유예)의 내부관리계획·접근권한·접근통제·악성프로그램 방지 요구, (2) ISMS-P 인증기준 1.2.1(정보자산 식별: 자산명·용도·위치·책임자·관리자·관리부서 목록, 보안등급, 정기 현행화)과 2.1.3(정보자산 관리: 등급별 취급절차, 책임자·관리자 지정, 인사이동·자산 변경 시 반영), (3) 저작권법 양벌규정(§141)에 따른 소프트웨어 불법복제 책임과 SPC/BSA 단속 대응이다. 국가정보보안기본지침은 국가·공공기관 대상이므로 민간 제조업에는 직접 적용되지 않는다.

### Cited Findings

**개인정보의 안전성 확보조치 기준**
- 현행 고시: 개인정보보호위원회고시 제2023-6호(2023-09-22 일부개정·시행), 법 §29·영 §30 근거로 분실·도난·유출 방지를 위한 기술적·관리적·물리적 안전조치의 최소 기준 — [wikidocs 고시 사본](https://wikidocs.net/264634) (비공식 사본, 원문 대조 필요); 정보통신서비스 제공자용 고시가 폐지·일원화된 배경 — [CIO Korea](https://www.cio.com/article/3523249/)
- 영 §30: 내부 관리계획의 수립·시행 및 점검 요구(2023-09-12 개정) — [wikidocs](https://wikidocs.net/264634)
- 제6조(접근통제) 골격: IP 기반 접근 제한, 외부 접속 시 인증서·보안토큰·OTP 등 안전한 인증수단, 일정 시간 미사용 시 자동 차단 — [IT위키 제6조](https://itwiki.kr/w/%EA%B0%9C%EC%9D%B8%EC%A0%95%EB%B3%B4%EC%9D%98_%EC%95%88%EC%A0%84%EC%84%B1_%ED%99%95%EB%B3%B4%EC%A1%B0%EC%B9%98_%EA%B8%B0%EC%A4%80_%EC%A0%9C6%EC%A1%B0)
- 2025년 개정(개인정보위 발표, 2025-10 보도): 제2조(정의)·제6조의2(인터넷망 차단조치 등)는 즉시 시행, 내부관리계획·접근권한 관리·접근통제·접속기록 보관·점검 조항은 1년 유예; 대규모 처리자의 망차단을 위험분석 기반으로 재설계, 개별 기기 위험성 분석 후 차단 대상 제외 가능; 개인정보위는 「개인정보의 안전성 확보조치 안내서」를 연내 발간 예정 — [Byline Network 2025-10-31](https://byline.network/2025/10/31-281/); [nepla 해설](https://www.nepla.ai/wiki/it-정보-방송통신/개인정보-위치정보-신용정보/망분리에서-위험기반-보호-로-2025년-개정-개인정보-안전성확보조치-고시의-의미-gy3807x469rx)
- 개정 방향: 개인정보처리시스템 접근권한 관리와 접근·통제 범위·대상 강화, 취약점 점검·보완조치 의무화 신설(제5조, 제6조) — [Byline Network](https://byline.network/?p=3103) (개정안 단계 보도일 수 있어 최종 조문 대조 필요)

**ISMS-P 인증기준**
- 1.2.1 정보자산 식별 — 주요 확인사항: 자산 분류기준 수립 및 관리체계 범위 내 자산을 빠짐없이 목록화(자산명, 용도, 위치, 책임자 및 관리자, 관리부서 등), 법적 요구·업무 영향 기반 중요도 평가 및 보안등급 부여, 정기 자산 현황 조사로 목록 최신화 — [IT위키 1.2.1](https://itwiki.kr/w/ISMS-P_%EC%9D%B8%EC%A6%9D_%EA%B8%B0%EC%A4%80_1.2.1.%EC%A0%95%EB%B3%B4%EC%9E%90%EC%82%B0_%EC%8B%9D%EB%B3%84); [CELA 1.2.1](https://www.cela.kr/ISMS_1_2)
- 2.1.3 정보자산 관리 — 인증기준: "정보자산의 용도와 중요도에 따른 취급 절차 및 보호대책을 수립·이행하고, 자산별 책임소재를 명확히 정의하여 관리"; 확인사항: 보안등급별 취급절차(생성·도입, 저장, 이용, 파기)와 보호대책 정의·이행, 식별된 자산별 책임자·관리자 지정; 세부: 자산번호·바코드·워터마크로 등급 표시, 퇴직·전보 등 인사이동 또는 자산 도입·변경·폐기 시 자산목록의 책임자·담당자 갱신 — [IT위키 2.1.3](https://itwiki.kr/w/ISMS-P_%EC%9D%B8%EC%A6%9D_%EA%B8%B0%EC%A4%80_2.1.3.%EC%A0%95%EB%B3%B4%EC%9E%90%EC%82%B0_%EA%B4%80%EB%A6%AC)
- 공식 해설은 KISA 인증기준 안내서(2022.4) 참조 — [KISA ISMS-P 자료실 PDF](https://isms.kisa.or.kr/data/content/CT00000013/resources/include/down/operating_01.pdf)

**소프트웨어 저작권(SPC/BSA)**
- 저작권법 양벌규정(§141)에 따라 대표가 불법복제 방지를 위한 충분한 주의·감독을 입증하지 못하면 형사·민사 책임; 대비책은 정식 라이선스 확보, 주기적 교육, 사내 소프트웨어 자산 실태 점검 — [nepla 법률해설](https://www.nepla.ai/wiki/지식재산/저작권/불법-소프트웨어-단속-공문-내용증명-을-받았다면-형사고소-및-손해배상-초기대응-전략-0xr84d6g28q1)
- SPC(한국소프트웨어저작권협회)는 저작권사 위임으로 법무법인과 함께 수사 요청; "스마트 체크" 무상 자가점검 서비스 제공(2019) — [보안뉴스](https://m.boannews.com/html/detail.html?idx=79682) (구자료); [iNews24](https://www.inews24.com/view/933708)
- 문체부 특별사법경찰이 연 2,500여 기업 방문 컨설팅·무료 점검도구 제공(구자료) — [문체부 보도자료](https://www.mcst.go.kr/site/s_notice/press/pressView.jsp?pSeq=15040)
- BSA 기준 한국 SW 불법복제율 40%, 손실액 약 8,900억 원(2011년 보고서, 구자료) — [kidd.co.kr](https://kidd.co.kr/news/163122)
- 기관용 SAM 지침 예시: 인하대 "소프트웨어 자산관리지침"(PDF), 에너지경제연구원 SAM 가이드라인(식별·재고관리·기록 검증·라이선스 준수 단계 구성) — [인하대 PDF](https://newdept.inha.ac.kr/sites/cic/download/소프트웨어%20자산관리지침.pdf); [에너지경제연구원](https://www.keei.re.kr/library/10110/contents/5970849)

**국가정보보안기본지침**
- 국가정보원 관리, 국가·공공기관 대상; 2021-11-01 개정본부터 일반 공개; 제3조 적용범위는 "각급기관의 정보보안업무"이며 행정부 외 국가기관은 관계 법률 위임 범위 내 — [IT위키 국가정보보안기본지침](https://itwiki.kr/w/%EA%B5%AD%EA%B0%80_%EC%A0%95%EB%B3%B4%EB%B3%B4%EC%95%88_%EA%B8%B0%EB%B3%B8%EC%A7%80%EC%B9%A8); [제3조](https://itwiki.kr/w/%EA%B5%AD%EA%B0%80_%EC%A0%95%EB%B3%B4%EB%B3%B4%EC%95%88_%EA%B8%B0%EB%B3%B8%EC%A7%80%EC%B9%A8_%EC%A0%9C3%EC%A1%B0)

### Inferences
- ITAM 자산 대장의 "한국 컴플라이언스 최소 속성"은 ISMS-P 1.2.1/2.1.3에서 직접 도출된다: 자산번호, 자산명, 유형, 용도, 위치, 책임자, 관리자(담당자), 관리부서, 보안등급(중요도), 도입일/변경일/폐기일, 상태. HR 동기화(퇴직·전보 반영)가 2.1.3 세부 요구와 직결되므로 EBS HR(PER_ALL_ASSIGNMENTS_F) 연계는 규제 대응 기능으로 분류된다.
- 안전성 확보조치 기준 대응 관점에서 ITAM은 "개인정보처리시스템 접근권한 관리의 대상 기기 식별" 및 "악성프로그램 방지(백신 설치·업데이트 상태)"의 증적 소스가 된다. PC 에이전트가 백신/EDR 설치·엔진 버전·OS 패치 상태를 수집하면 제9조(악성프로그램 등 방지) 증적으로 활용 가능하다(제9조 2025년 개정 내용은 미확인).
- SPC/BSA 대응은 "벤더별 ELP(설치 vs 권리) 즉시 산출"과 "PC별 설치 SW 증적(스냅샷·시점)" 기능으로 환산되며, 정규화 품질이 낮으면 ELP가 과다·과소 집계되어 감사 방어가 약해진다.
- 공공기관이 아닌 민간 제조업이므로 국가정보보안기본지침은 적용 제외로 두되, 방산·국가핵심기술 보유 기업이라면 산업기술보호법·방산 보안 요구가 별도로 붙을 수 있다(본 조사 범위 밖).

### Gaps
- 안전성 확보조치 기준의 제5조(접근권한), 제9조(악성프로그램) 현행 조문 전문과 2025년 개정 최종 문구는 국가법령정보센터 원문 확인이 필요하다(비공식 사본만 확인).
- KISA ISMS-P 인증기준 안내서 원문(결함사례 포함)은 접근하지 못했다.
- 2024–2025년 BSA 한국 불법복제율·기업 감사·손해배상 판례 최신 자료는 검색되지 않았다.
- 정보통신망법상 정보보호 조치 기준의 자산관리 조문은 확인하지 못했다.

---

## KQ7. Build vs Buy: 인하우스 ITAM의 타당성·공수·숨은 비용

### Takeaway
공개 자료는 대부분 벤더 측 주장이지만 일관된 메시지는 "인하우스는 카탈로그 정규화·다중 소스 정합·에이전트 OS 호환 유지보수가 반복 비용이며, 초기 추정의 약 2배로 불어나기 쉽다"는 것이다. Redress Compliance(SAM 자문사)는 빌드 12–18개월/1년차 50만–150만 달러, 구매 3–6개월/20만–50만 달러를 예시로 들고, Exclaimer 설문은 IT팀의 63%가 월 최대 50시간을 내부 도구 유지에 쓰고 66%가 연 2만–10만 달러 추가 비용이 든다고 한다. 인하우스가 타당한 경우는 요구가 단순(하드웨어 대장+PC 인벤토리)하고 ERP/HR 등 사내 시스템 연계가 상용 커넥터로는 어려우며(EBS 11i처럼), 정규화 범위를 사내 표준 SW로 한정할 수 있을 때다.

### Cited Findings
- 빌드 vs 구매 예시표: 빌드는 가치 실현까지 12–18개월, 1년차 50만–150만 달러(인력·도구); 구매는 3–6개월, 20만–50만 달러(라이선스+구축) — [Redress Compliance SAM Tools Guide 2026](https://redresscompliance.com/software-asset-management-tools-guide-2026) (자문사 예시, 독립 검증 없음)
- Exclaimer 설문(IT·보안 리더 2,000+): 63%의 IT팀이 내부 도구 유지에 월 최대 50시간 투입, 66%가 연 2만–10만 달러 추가 필요, 내부 IT 프로젝트의 46%가 예산의 약 2배 소요, 핵심 엔지니어 이탈 시 문서화 안 된 시스템이 취약 — [Exclaimer](https://exclaimer.com/resources/videos/hidden-cost-of-in-house-tools/) (이메일 플랫폼 벤더, 구매 측 이해관계)
- 스프레드시트/SharePoint 시작은 싸 보이나 MDM·IdP·구매·서비스데스크·실사 결과와의 정합을 사람이 해야 하므로 변화량이 커지면 한계 — [AssetSonar](https://ezo.io/assetsonar/blog/build-vs-buy-itam-building-in-house/) (벤더)
- 영구·구독·OEM 번들 라이선스가 섞인 환경의 정합은 수작업으로 따라갈 수 없어 비용이 가장 크게 증가 — [NinjaOne](https://www.ninjaone.com/blog/itam-software-cost/) (벤더)
- DIY vs 관리형 서비스 선택 기준 논의 — [SHI 블로그](https://blog.shi.com/business-of-it/choosing-an-itam-solution-diy-vs-managed-service/); [ITAM Review: Outsource or in-house](https://marketplace.itassetmanagement.net/?p=23262)
- 정규화 카탈로그는 "설치 데이터 기반으로 주간 갱신"(ServiceNow)되고 "지속 추가"(Flexera)되는 살아있는 데이터셋 — [ServiceNow Docs](https://www.servicenow.com/docs/r/pyt4lF0aAin_R6wVyV414Q/BgAHUWtzbvOa6OvfZAaFFQ); [Flexera Community](https://community.flexera.com/t5/Data-Platform-Forum/Data-Platform-Information/m-p/283776)
- SWID 태그 채택이 낮아 인벤토리 도구는 여전히 레지스트리/ARP 휴리스틱에 의존 — [Snow Software](https://snowsoftware.com/?p=1306); osquery `programs`도 ARP 등록 앱만 반환하고 MSIX/WindowsApps는 누락 — [Kolide](https://www-assets.kolide.com/features/device-inventory/properties/windows-programs)

### Inferences
- 자사 빌드의 숨은 비용 항목(벤치마크 TCO 산정 시 반드시 포함): (1) 정규화 카탈로그 큐레이션(신규 미정규화 항목 처리 인력·주기), (2) 에이전트의 Windows 버전/서명/EDR 호환 회귀 테스트, (3) EBS 11i DB 드라이버 호환·EBS 업그레이드 시 뷰 재작성, (4) 라이선스 모델 변경(구독·사용자 기반) 반영, (5) 문서화·인수인계.
- 인하우스가 유리한 조건이 이 회사에 맞는지 판단할 기준: PC 중심(서버/클라우드 라이선스 최적화 요구 낮음), 사내 표준 SW 수백 종 수준으로 정규화 범위 제한 가능, EBS 11i 고정자산·HR 연계가 상용 커넥터로 불가(11i는 iRep 비내장·Sustaining Support)해 어차피 커스텀 필요. 반대로 Oracle/Microsoft/Adobe/SAP 등 대형 벤더 감사 방어용 ELP가 핵심이면 상용 SAM 카탈로그 없이는 어렵다.
- 상용 비교 시 "가치 실현 기간(3–6개월 vs 12–18개월)"과 "연간 유지 인력(FTE)"을 스코어카드의 비기능 축에 넣어야 공정하다.

### Gaps
- 독립 기관(Gartner/Forrester/IAITAM)의 인하우스 ITAM 공수 추정치·성공률은 찾지 못했다.
- "에이전트 OS 버전 호환 유지보수" 비용에 대한 정량 자료는 없었다.

---

## KQ8. 벤치마크 스코어카드 초안(상용 ITAM vs 자사 빌드) 및 "Done" 정의

### Takeaway
위 조사 결과를 교차해, 19770-1 티어 × Gartner 5대 핵심 기능 × G2 포함 조건 × 한국 규제 요구 × EBS 11i 연계를 축으로 한 10개 영역·총 100점 스코어카드를 제안한다. 가중치는 공개 자료에 근거한 자체 설정이며(Gartner/Forrester 가중치 비공개), 제조업·PC 중심·EBS 11i 환경을 반영해 "신뢰 데이터(발견·정규화·정합)"와 "ERP/HR 연계"에 높은 비중을 두었다.

### Cited Findings
- 근거 소스는 KQ1–KQ7에 인용된 것과 동일: 19770-1 3개 티어 — [ISO SC7](https://committee.iso.org/sites/jtc1sc7/home/projects/flagship-standards/isoiec-19770-12017.html); Gartner 5대 기능 — [Gartner Peer Insights](https://www.gartner.com/reviews/market/software-asset-management-tools); G2 포함 조건 — [G2](https://g2.com/categories/it-asset-management?page=2); ISMS-P 1.2.1/2.1.3 — [IT위키](https://itwiki.kr/w/ISMS-P_%EC%9D%B8%EC%A6%9D_%EA%B8%B0%EC%A4%80_2.1.3.%EC%A0%95%EB%B3%B4%EC%9E%90%EC%82%B0_%EA%B4%80%EB%A6%AC); EBS 물리 인벤토리 매칭 — [Oracle 11i](https://docs.oracle.com/cd/A60725_05/html/comnls/us/fa/physin03.htm); 에이전트 건강 버킷 — [Cyrisma](https://support.cyrisma.com/articles/6406022526-asset-inventory); ServiceNow "완전 정규화 설치 비율" — [ServiceNow Docs](https://www.servicenow.com/docs/r/pyt4lF0aAin_R6wVyV414Q/6YUbt_Fd~gduwpXua3UVzw)

### Inferences

**A. 스코어카드 초안 (총 100점; 각 항목 0–5점 평가 후 가중 환산)**

| # | 영역 (참조 모델) | 가중 | 세부 기준(채점 항목) |
|---|---|---|---|
| 1 | 발견·인벤토리 — HW (19770-1 Tier 1, Gartner Discovery, G2 조건 1·3) | 12 | PC 에이전트(Win 10/11, 서버 OS) 수집 항목: SMBIOS 일련번호·BIOS Asset Tag·모델·CPU/RAM/디스크·MAC·OS 버전·패치 수준·로그인 사용자; 에이전트리스 네트워크 발견(프린터·NW 장비·IoT); 에이전트 건강 상태(7/30/90일 버킷) 대시보드; 신규/미승인 장치 탐지 알림 |
| 2 | 발견·인벤토리 — SW (Tier 1, Gartner Discovery) | 10 | ARP/레지스트리·MSI ProductCode·MSIX/WindowsApps·포터블 실행파일 탐지; SWID 태그 읽기; 사용량(실행 빈도·최종 실행일) 측정; 설치 스냅샷 이력(감사 시점 증적) |
| 3 | 정규화·카탈로그 (Gartner Normalization) | 12 | 퍼블리셔/제품/버전/에디션 정규화율(%); 사설 카탈로그(사내 CAD/MES/보안 에이전트) 관리 UI; 미정규화 큐·승인 워크플로; 외부 참조(winget 매니페스트·SWID·purl) 활용; 카탈로그 갱신 주기 |
| 4 | 자산 대장·라이프사이클 (ITIL 4, ISMS-P 1.2.1/2.1.3, G2 조건 2) | 12 | 상태 모델(계획→구매→입고→배치→이동→반납→수리→폐기)과 전이 이력; 필수 속성(자산번호·유형·용도·위치·책임자·관리자·관리부서·보안등급·도입/폐기일); 보증·계약 연결; owner/custodian 공란 0%; 바코드/QR 라벨·실사(physical inventory) 모듈 |
| 5 | 라이선스 권리·정합·ELP (19770-3, Gartner Reconciliation) | 10 | 권리 레코드(제품·버전 범위·메트릭·수량·유효기간·유지보수) 등록; 설치 vs 권리 ELP 벤더별 산출; 다운그레이드/업그레이드 권리·OEM 번들 처리; 감사 패키지 내보내기 |
| 6 | 최적화·회수 (Tier 3, Gartner Optimization) | 6 | 미사용 SW 회수(reclaim) 워크플로; 리프레시 대상(연한 초과) 리스트; 중복·금지 SW 리포트; (SaaS/클라우드는 낮은 비중) |
| 7 | ERP/HR 연계 — Oracle EBS 11i (Tier 2 Life Cycle Integration) | 14 | FA 읽기 동기화(FA_ADDITIONS_B·FA_BOOKS·FA_DISTRIBUTION_HISTORY·FA_LOCATIONS); 고정자산↔IT자산 매칭(ASSET_NUMBER→TAG_NUMBER→SERIAL_NUMBER, 1:N 지원); 불일치 리포트(위치·사원·수량) 및 FA_INV_INTERFACE 양식 출력; FA_MASS_ADDITIONS 적재 또는 Web ADI 템플릿 생성; HR 동기화(PER_ALL_PEOPLE_F/PER_ALL_ASSIGNMENTS_F/HR_ALL_ORGANIZATION_UNITS, date-effective·퇴직자 처리); 10g/11g DB 드라이버 호환 및 EBS 무변경 원칙 |
| 8 | 보고·대시보드·KPI (Gartner Reporting) | 8 | 인벤토리 정확도·완전성·에이전트 보고율·정규화율·ELP·고아 자산·실사 변동 KPI를 정의·공식·데이터소스와 함께 제공; 감사용 시점 리포트 |
| 9 | 보안·컴플라이언스 증적 (안전성 확보조치 기준, ISMS-P, SPC/BSA) | 8 | 백신/EDR 설치·엔진 버전·OS 패치 상태 수집; 자산별 책임자 변경 이력(인사이동 반영); 접근권한·역할(RBAC) 및 감사 로그; 불법 SW 탐지 리포트; 개인정보처리시스템 접근 단말 식별 |
| 10 | 비기능·TCO (Build vs Buy) | 8 | 에이전트 배포·자동 업데이트·OS 호환 회귀 테스트; 성능(수천 대 규모 수집 주기); 가용성·백업; 문서화·운영 인력(FTE); 가치 실현 기간; 3년 TCO |

채점 규칙 제안: 각 항목 0(없음)–5(상용 선두 수준)로 평가, 가중 점수 = 평균 점수/5 × 가중. 상용 제품은 공개 문서·데모 기준, 자사 빌드는 실제 구현 기준으로 동일 심사자가 채점.

**B. 자사 ITAM(PC 에이전트 + 서버 + 웹 FE)의 "Done" 정의(안)**

1. 커버리지: 대상 PC·서버의 ≥95%에 에이전트 설치, 7일 내 체크인 ≥95%, 30일 내 ≥98%; 미보고 장치는 자동으로 "At Risk/Warning" 상태 전이.
2. 데이터 신뢰: 자산 대장 vs 발견 데이터 일치율 ≥95%; 필수 속성(ISMS-P 1.2.1 기준 자산명·용도·위치·책임자·관리자·관리부서·보안등급) 공란 0%; 완전 정규화 설치 비율 측정 가능하고 미정규화 큐의 처리 리드타임 목표 존재.
3. 라이프사이클: 모든 자산이 정의된 상태 중 하나에 있고, 상태 전이는 이벤트·사용자·시각과 함께 기록; 인사이동(퇴직·전보) 발생 시 HR 동기화로 책임자 변경이 N일 내 반영.
4. ERP 연계: 고정자산(FA) 레코드와 IT 자산의 매칭률 보고(자산번호→태그→일련번호), 불일치 리포트가 FA_INV_INTERFACE 양식으로 출력; AP 송장 유래 신규 고정자산이 ITAM에 "입고 대기" 자산으로 자동 생성; EBS 11i 측 변경 없음(읽기 전용 뷰 + 표준 인터페이스 테이블만 사용).
5. 라이선스: 사내 상위 N개 퍼블리셔(최소 Microsoft·Adobe·Autodesk 등 감사 빈도 높은 벤더)에 대해 ELP를 1시간 이내 산출; 설치 스냅샷을 시점별로 보존.
6. 규제 증적: 백신/EDR·OS 패치 상태 리포트, 자산별 책임자 이력, RBAC·감사 로그, 불법/미승인 SW 리포트가 즉시 출력.
7. 운영: 에이전트 자동 업데이트, OS 메이저 업데이트 후 회귀 테스트 절차, 운영 문서, 백업/복구 절차 존재.

### Gaps
- 가중치는 공개 분석 자료를 교차한 자체 설정이며, Gartner/Forrester 공식 가중치와 비교 검증할 수 없었다.
- "Done" 수치 목표(95%/98%)는 권위 있는 벤치마크가 아닌 2차 자료(EDN 2009, Cyrisma 버킷, Proactivanet 90% 경고선)에서 유추한 값이므로 내부 합의로 확정해야 한다.
