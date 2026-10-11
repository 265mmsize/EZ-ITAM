# 국내 상용 IT 자산관리(ITAM) / PC 자산관리 제품 조사 노트 (2025–2026 기준)

> 조사 방법 메모: 본 세션의 샌드박스에서는 벤더 사이트·PDF 직접 열람(WebFetch/curl)이 네트워크 정책(프록시 403)으로 차단되어, 모든 사실은 웹 검색 결과(검색 엔진이 반환한 페이지 발췌)에 근거한다. 따라서 브로셔 원문의 세부 수집 항목·서버 사양은 "검색 발췌에서 확인된 범위"로 한정되며, 확인 불가 항목은 각 질문의 **Gaps**에 명시했다. 2010년대 자료는 "구자료"로 표시했다.

---

## 핵심 질문 1. 제품별 프로필 — 벤더, 대상 시장, 에이전트 수집 범위, 서버 스택, 콘솔, 가격, 고객 사례

### Takeaway
국내 시장에서 "PC 자산관리"는 대부분 ① 통합 PC 관리(자산+패치+원격제어+PC-OFF) 제품군(엠엘소프트 Tgate DMS, 닥터소프트 NetClient, 닉스테크 SafePC), ② DLP/PC보안에 자산관리를 끼워 넣은 제품(지란지교 오피스키퍼, 지니언스 NAC), ③ SW 저작권 준수(SAM) 특화 제품(SPC 스위퍼, 오딧맨, SAMQ, 아니어스 iMON, 엠엘소프트 TCO!sam), ④ 신생 SaaS형 ITAM(셀파스 심플리)으로 나뉜다. 공개된 PC당 단가는 극히 드물며(SAMQ 클라우드 월 2,000원/유저, 오피스키퍼 클라우드 연 11만원대/유저 수준의 리셀러 가격 정도), 대부분 견적 문의 방식이다.

### Cited Findings

**(A) 엠엘소프트(MLsoft) — Tgate DMS / TCO!sam (통합 PC 관리 + SAM, 공공 강세)**
- Tgate DMS는 "사내 모든 PC의 하드웨어와 소프트웨어 정보를 종합적으로 수집 및 관리해 주는 데스크톱 통합관리 전문 솔루션"으로 소개된다 — [엠엘소프트 Tgate DMS 제품 페이지](https://www.mlsoft.com/product-tgatedms.php)
- Tgate DMS 브로슈어에는 "조달청 등록, 특허등록, 행망용 소프트웨어" 표시가 있고, 기능으로 "모든 상용 소프트웨어 현황 수집 및 라이선스 인가/회수 관리", "약 1만개의 국내외 상용 소프트웨어 및 폰트 자체 DB 제공", "장애 발생 PC에 대한 실시간 원격제어(다중 PC 동시 제어, LAN/WAN 무관, 사용자 허락 옵션, 원격제어 로그)"가 나열된다 — [Tgate DMS 브로슈어(티씨오서비스 배포)](https://tcosvc.co.kr/pdf/tgatedms-brochure.pdf)
- 티씨오서비스 제품 페이지: "PC의 하드웨어 사양과 소프트웨어 설치 현황을 자동으로 수집하여 자산을 체계적으로 관리", "모든 사용자 대상 PC에 설치된 소프트웨어와 보안 업데이트 내용을 중앙에서 확인" — [티씨오서비스 Tgate DMS](https://tcosvc.co.kr/product-tgatedms)
- 엠엘소프트는 "1995년부터 현재까지 2000여개의 공공기관 및 기업에 NAC 시스템, IP 관리 시스템, PC 자산관리 시스템 등을 제공"했다고 밝힘(업력 32년차) — [사람인 기업정보](https://m.saramin.co.kr/job-search/company-info-view?csn=WlRXMkZXRFR5VjB1SldTRlRTcUsvZz09)
- 2023년 회사소개서에는 교육부, 국토교통부, 관세청, 통계청 등 공공기관 납품처가 나열되어 있고, 자산관리 솔루션이 'TCO!stream' 명칭으로도 표기되어 Tgate/TCO! 명칭이 혼재한다 — [엠엘소프트 회사소개 2023(PDF)](https://ksaem.or.kr/wp-content/uploads/2023/11/%EC%A3%BC%EC%97%A0%EC%97%98%EC%86%8C%ED%94%84%ED%8A%B8_%ED%9A%8C%EC%82%AC%EC%86%8C%EA%B0%9C_2023.pdf)
- 2015년 사명 변경, 2015년 11월 Tgate V2.5 CC인증 획득 기록(구자료; NAC 계열 인증으로 보이며 DMS 단독 CC 여부는 미확인) — [시사뉴스 2023 히든기업 특집](https://www.sisa-news.com/mobile/article.html?no=230533)
- TCO!sam(상용 소프트웨어 관리 시스템)은 "사내 서버 및 PC에 설치되며 외부 접속이 불가능하고 모든 데이터는 고객사에서 관리"하도록 설계되었고 라이선스 증서 첨부 기능을 언급 — [엠엘소프트 TCO!sam](https://www.mlsoft.com/product-tcosam.php)
- 유통 채널: 티씨오솔루션/티씨오서비스/미디어랜드 등이 Tgate 계열(NAC, IPMS, DMS, PMS)을 함께 공급 — [티씨오솔루션](https://sites.google.com/tcosol.com/tcosolution/%ED%99%88/tgate-nac-%EC%86%8C%EA%B0%9C); [미디어랜드 Tgate DMS](https://www.medialand.net/product-tgatedms.php)

**(B) 닥터소프트(DoctorSoft) — NetClient(넷클라이언트) 6 (통합 PC 관리, 금융·제조 강세, 공공 조달 등록)**
- "단일 에이전트를 이용한 통합 PC 관리 솔루션"으로, 모듈은 DMS(자산관리), PMS(패치관리), PC-OFF(근로시간관리), HSM(보안관리) — [닥터소프트 NetClient](https://doctorsoft.co.kr/product/netclient.jsp)
- SW자산관리 모듈: "조직 내 설치된 소프트웨어 현황과 구매 라이선스 현황을 파악한 뒤 미인가 소프트웨어 사용을 차단하고, 사용량 모니터링(사용횟수/기간 등)으로 라이선스를 할당·회수" — [쉐어잇 NetClient 제품 소개](https://www.sharedit.co.kr/products/546); [다온데이타 NETCLIENT](https://daondata.co.kr/product/fo/productview/14)
- 하드웨어 자산: "장비 1대가 하나의 자산으로 관리될 수 있는 고유키 관리 기술"로 자산 중복 방지; 플랫폼 기능으로 "Windows/Mac OS 관리, 장치 관리, 원격제어, 고정IP"; "자산관리, 원격제어 분야 제품 품질인증 및 특허" 보유 주장; "서버 1대당 PC 지원수 최대 2만 대" — [닥터소프트 NetClient 소개](https://www.doctorsoft.co.kr/netclient/netclient1.jsp); [쉐어잇](https://www.sharedit.co.kr/products/546)
- 서버 스택(유통사 페이지, 구버전 가능성): "SERVER OS: CentOS 7.4(리눅스), Windows Server 2008 이상 / DATABASE: MariaDB / CLIENT OS: Windows 7, Mac 10.7 이상" — [다온데이타 NETCLIENT](https://daondata.co.kr/product/fo/productview/14); [알티메이트 Netclient](http://m.rtmate.co.kr/page/page11)
- 자산 사용 통제: 미등록 장비 통제, PC 강제 종료·화면 잠금 언급 — [쉐어잇 NetClient](https://www.sharedit.co.kr/products/546)
- 회사: 2000년 설립, "금융권 고객과 건설·조선 등 제조 분야 위주의 1,000여 고객사"; 연혁에 2013년 LGU+ 자산관리 솔루션 구축·공급 계약, 삼성생명 자산관리 시스템 구축 계약 — [잡코리아 닥터소프트](https://www.jobkorea.co.kr/Recruit/Co_Read/C/dr2000so); [닥터소프트 NetClient2](https://www.doctorsoft.co.kr/product/netclient2.jsp)
- 공공: 가온아이가 넷클라이언트 공공분야 영업을 수행했고 "조달청 디지털서비스몰에서 '넷클라이언트 6 DMS'로 검색하면 구매 가능"(약 4년 전 기사, 단가는 미기재) — [아이티데일리](https://www.itdaily.kr/news/articleView.html?idxno=209432)
- Dr.NetClient_DMS는 "조직 내 설치 SW 현황 파악 및 불법 SW 설치 여부 중앙 점검" 구조 — [쉐어잇 Dr.NetClient_DMS](https://www.sharedit.co.kr/products/507)
- 라이선스 전문 제품 SLIM: "Stand-Alone/Node-Lock 타입 SW는 Agent와 통신해 사용량 정보 전송, 플로팅 라이선스는 라이선스 서버 연동으로 수집, 사용량 기반 회수" — [닥터소프트 SLIM](https://doctorsoft.co.kr/product/slim2.jsp)

**(C) 지란지교소프트 — 오피스키퍼(OfficeKeeper) (중소기업 DLP + IT 자산관리 포함, 클라우드/설치형)**
- 오피스키퍼는 "정보유출방지, 민감정보관리, 웹·소프트웨어 차단, 출력물 보안, 문서백업, PC 취약점 점검, IT 자산관리 총 7가지 기능을 통합 에이전트로 제공" — [바이라인네트워크 2024.04](https://byline.network/2024/04/9-201/); [아이뉴스24](https://www.inews24.com/view/972500)
- 자산관리(Asset) 기능: "소프트웨어 라이선스 관리, PC 시스템 현황, PC 전력소비 관리, 네트워크 사용현황 파악"; "SW의 설치 갯수 추적뿐 아니라 실제 활용 시간까지 파악하여 추가 구매·삭제 판단 기준 제공" — [뉴스와이어(지란지교소프트 보도자료)](https://www.newswire.co.kr/newsRead.php?no=514448)
- 제공 형태: "사용한 만큼 과금되는 '클라우드형'과 미니서버 어플라이언스를 설치하는 '설치형'"(2023) — [바이라인네트워크 2024.03](https://byline.network/2024/03/240326_003)
- 가격(리셀러 공개가): 컴퓨존 "오피스키퍼 클라우드형 기업용/라이선스/1년/1유저" 110,500~115,000원; 세큐어몬스터 렌탈형 클라우드 Lite 45,000 / Basic 60,000(1PC, 년과금 표기) — [컴퓨존](https://www.compuzone.co.kr/product/product_detail.htm?ProductNo=875024); [세큐어몬스터](https://www.securemonster.com/goods/goods_view.php?goodsNo=1000000302). 공식 가격은 견적 문의 — [쉐어잇 오피스키퍼](https://www.sharedit.co.kr/products/399)
- 설치형 '미니서버'는 출시 당시 "기존 구축형 대비 약 70% 낮은 가격"(약 10년 전 구자료) — [데일리시큐](https://www.dailysecu.com/news/articleView.html?idxno=11663)
- 대상: "중소기업 최적화 정보유출방지 솔루션"으로 포지셔닝 — [한경 2018](https://magazine.hankyung.com/business/article/201809114046b)
- 시장 활동: 2025년 11월 22번째 오피스키퍼 보안세미나 400명 참가; 2024년 '클라우드 서비스 보급·확산 사업' 공급기업 선정; 2025 ISEC 참가 — [바이라인 2025.11](https://byline.network/2025/11/28-550/); [바이라인 2024.03](https://byline.network/2024/03/240326_003); [보안뉴스 ISEC 2025](https://m.boannews.com/html/detail.html?idx=138165)

**(D) 지니언스(Genians) — Genian NAC (NAC + 단말 가시성/자산관리, 공공·대기업)**
- 2019 데이터시트: "자산관리(DMS) 모듈 — DPI를 이용한 IP 유/무선 단말 탐지 및 Agent 기반 H/W 및 S/W 상세 정보 수집·관리"; 플랫폼 EOL·CVE 정보 제공 — [Genian NAC Datasheet 2019(구자료)](https://wp.genians.co.kr/wp-content/uploads/2019/07/Genian_NAC_Datasheet.pdf)
- 2024 브로셔: "Agent 설치 없이 OS 종류, 모델명, 버전, 제조사 등 정보 제공", 에이전트는 "자산 관리와 장치 사용 통제" 담당, "Agent 설치에 따른 비용 부담 없음(필요에 따라 사용/미사용)", 단말을 "시스템정보, 에이전트 상태, 소프트웨어 정보, 구입가격 등 조건으로 동적 분류", "에이전트 미설치 단말 설치/필수 SW 미설치 단말 차단" 정책 — [Genian NAC 브로셔 2024.07](https://genians.co.kr/hubfs/NAC%20Brochure_24.07.30.pdf?hsLang=ko)
- NAC v5.0 자료: 필수 소프트웨어 설치 현황 파악, 미설치 단말 설치 유도, 규정 위반 소프트웨어 제거 — [Genian NAC v5.0 introduction](https://wp.genians.co.kr/resources/Genian_NAC_v5.0_introduction.pdf); 2026년 세일즈킷 버전 존재 — [NAC v5.0 2026](https://www.genians.co.kr/hubfs/2026%EB%85%84%EB%8F%84%20Sales%20Kit/Genian%20NAC/NAC_v5.0_2026.pdf?hsLang=ko)
- 가격: 사업보고서상 "단가는 최종사용자(네트워크 환경), 판매 형태(패키지/라이선스), 판매경로, 제품 버전 등에 따라 상이"; 조달청 물품계약(NAC) 2017.11.22~2021.11.21; "지니안 NAC는 나라장터 종합쇼핑몰 등록 전체 NAC 제품의 2018·2019년 매출 중 약 70% 점유" — [지니언스 사업보고서 2020.12](https://wp.genians.co.kr/wp-content/uploads/%EC%82%AC%EC%97%85%EB%B3%B4%EA%B3%A0%EC%84%9C2020.12.pdf)
- EDR 계열 Genian Insights E의 나라장터 등록가: "연간라이선스 클라이언트 기준 36,300"(2019~2021 계약, 통화단위 미표기, NAC 아님) — [지니언스 PR룸](https://www.genians.co.kr/pr-room/company/genian-insights-e-%EC%A1%B0%EB%8B%AC%EC%B2%AD-%EB%82%98%EB%9D%BC%EC%9E%A5%ED%84%B0-%EB%93%B1%EB%A1%9D)
- 온프레미스 체험 라이선스: 30일, 최대 300대 — [My Genians](https://my.genians.co.kr/buy-license/)
- 클라우드 NAC: IT/OT 자산 실시간 탐지·분류, 취약점 정보 제공 — [전자신문 2023.07](https://m.etnews.com/20230704000020)

**(E) 안랩 — AhnLab ESA(EPP Security Assessment, "내PC지키미") (공공·대학 PC 취약점 점검; 자산관리는 부가)**
- "업무용 PC의 보안 상태를 점검하고 자동조치를 통해 엔드포인트 보안 수준을 강화하는 취약 시스템 점검 및 조치 솔루션", 브로셔상 "국내 최대 수준 70여 개 점검 항목" — [AhnLab ESA 브로셔](https://download.ahnlab.com/kr/site/brochure/Brochure_AhnLab_ESA.pdf)
- 대학 배포 사례: 매월 셋째 수요일 "사이버보안진단의 날" 자동 점검, 기준 점수 80점 미만 팝업, 점검 보고서 관리자 전송; PMS·백신관리와 함께 배포되고 "점검 정책 배포 및 결과의 중앙 관리" — [성신여대 안내](https://www.sungshin.ac.kr/bbs/main_kor/3182/110681/download.do); [전북대 ESA 매뉴얼](https://its.jbnu.ac.kr/sites/its/file/AhnLab%20ESA%20manual.pdf); [서울여대 물품사양서 v2.1](https://www.swwu.ac.kr/download/1468887620676/index.html)
- 안랩 EPP 구성(2026.05): EPPM, EPM, EPrM, EDC, V3 제품군; 다수 PC 중앙관리는 AhnLab EMS — [바이라인 2026.05](https://byline.network/2026/05/4-434/); [AhnLab EMS 브로셔](https://download.ahnlab.com/kr/site/brochure/Brochure_EMS.pdf)
- 공공용 표준제안서(비츠코리아 배포) 존재 — [AhnLab 내PC지키미 공공 표준제안서](https://www.bitzkorea.com/marketing/news/product/ahnlab/%5B%ED%91%9C%EC%A4%80%EC%A0%9C%EC%95%88%EC%84%9C%5DAhnLab%20%EB%82%B4PC%EC%A7%80%ED%82%A4%EB%AF%B8_%EA%B3%B5%EA%B3%B5.pdf)

**(F) 이스트시큐리티 — 알약 내PC지키미(MPI) (공공·기업 PC 취약점 관리; 자산은 ASM 연동)**
- "관리자가 기업 내 PC 보안 상태를 점검하고, 점검 결과에 따라 취약점을 스스로 조치할 수 있도록 지원하는 취약점 관리 솔루션"; 공공기관용 매뉴얼상 14개 기본 점검 항목 + 상세 점검 항목; ISMS, 사이버보안진단의 날, 주요정보통신기반시설 PC 점검 항목 지원 — [이스트시큐리티 제품 페이지](https://www.estsecurity.com/product/mypc); [강릉원주대 매뉴얼](https://cc.gwnu.ac.kr/sites/cc/files/PC-protecter-manual.pdf)
- 커스텀 점검 항목: 프로세스명, 서비스명, 레지스트리 값/경로, 파일 기준으로 관리자가 정의 — [알약 블로그 #5084](https://blog.alyac.co.kr/5084)
- 제안서(2023.07): 관리 서버로 점검 결과·로그 자동 전송, 기간/부서/에이전트별 통계 보고서, "별도 서버 구축 없이 라이선스만으로 사용" 옵션, "ASM(통합관리솔루션) 연동으로 백신·자산·취약점을 함께 관리", 알약 PMS 연동 시 Windows/주요 SW 패치 현황 실시간 확인 — [알약 MPI 제안서 2023.07](https://cdn1.estsecurity.com/es_channel/23fw_partner/proposal/alyac%20proposal/alyac%20mpi%20proposal/alyac_mpi_proposal_230720.pdf)
- 개방형 OS 버전(alyac-mpi-openos) 존재 — [이스트시큐리티 openOS](https://www.estsecurity.com/enterprise/product/alyac-mpi-openos)
- 조달: "알약 공공기관용 제품은 조달청 종합쇼핑몰에서 '알약' 검색으로 구매 가능" — [이스트시큐리티 FAQ](https://www.estsecurity.com/enterprise/support/faq/view/42)

**(G) 닉스테크 — SafePC Enterprise (통합 PC보안에 자산관리 포함; 공공·금융, 구자료 위주)**
- 근로복지공단 3,300대 PC 도입, "해킹툴 탐지, 방화벽, 파일 암복호화 외에 중앙에서의 소프트웨어와 하드웨어 등의 자원을 관리하는 기능" 추가; 이후 DLP·매체제어·NAC 기능 추가로 통합 PC보안 제품화; "공공·금융분야 300여 곳 레퍼런스"(2000년대 후반~2016 구자료) — [디지털타임스](https://www.dt.co.kr/article/10137636); [전자신문 2009](https://m.etnews.com/200904170185); [바이라인 2016](https://byline.network/2016/04/1-127/)

**(H) 한국소프트웨어저작권협회(SPC) 계열 — 스위퍼(SWeeper) / SAM Tool / SMART Check / 오딧맨(Audit-MAN) (SW 저작권 준수 특화)**
- SPC SAM Tool: "SAM DB가 탑재된 SAM Tool은 실시간 소프트웨어 사용 현황 점검, 불법 소프트웨어 설치 방지 기능을 갖춘 솔루션", "에이전트, 서버, 콘솔 등을 자동으로 업그레이드" — [SPC SAM Tool 페이지](https://www.spc.or.kr/ko/business/sam/sw_sub313)
- 스위퍼: "SAMDB를 활용하여 자동화된 SW사용 현황 분석… 기업 및 기관에 맞는 SW자산관리를 제공하는 상용 솔루션", "PC에 설치된 SW 식별 및 라이선스 유형 정보, 저작권사 최신 라이선스 실시간 자동 업데이트" — [스위퍼 공식](https://www.sweeper.or.kr/); [소프트2000 SWeeper](https://www.soft2000.com/solution/asset-management-solution/sweeper)
- 2026년 2월 SPC 총회 'SPC 3.0' 선언: 주력 솔루션 스위퍼에 AI 챗봇, 리스크 관리, 정책 추천 모듈 추가 계획 — [디지털데일리 2026.02](https://www.ddaily.co.kr/page/view/2026022715265337414)
- SMART Check: PC 설치 SW·폰트 관리와 악성코드 방지 도구로 "기업, 공공/교육기관, 개인 무료", 단 SAMDB 미등록 SW는 검색 불가 — [smartcheckplus.or.kr](https://www.smartcheckplus.or.kr/)
- 오딧맨(Audit-MAN): SPC가 개발·관리에 참여한 기업전용 SW자산관리+백신 관리 솔루션(2020년 무료 체험 이벤트; 정가 미공개) — [소프트2000 오딧맨 이벤트](https://www.soft2000.com/14065)
- 과거 SPC는 SW저작권관리툴을 1,000개 중소기업에 무상 배포(2014 구자료) — [전자신문 2014](https://www.etnews.com/20140424000073)

**(I) 기타 SAM/자산관리 전문 중소 벤더**
- SAMQ(샘큐, 줌인테크): 클라우드형 "유저당 월 2,000원"; 구축형은 컨설팅 후 별도 견적; iOS 구축용 앱 존재 — [SAMQ 공식](https://pro.samq.co.kr/); [쉐어잇 SAMQ](https://www.sharedit.co.kr/products/643); [앱스토어](https://apps.apple.com/np/app/samq-%EA%B5%AC%EC%B6%95%EC%9A%A9/id6741567442)
- 아니어스시스템즈 iMON: "자동 불법 설치 차단, 미사용 라이선스 회수·재할당" — [아니어스시스템즈](http://anyussystems.com/property_management/)
- 올라잇IT: IT 자산관리/SW 배포 — [올라잇IT](https://www.alright-it.co.kr/it-sw)
- 넷릭스(Netrix) SW 자산관리 솔루션 제안서 존재 — [netrix.co.kr 제안서](http://netrix.co.kr/data/netrix_proposals.pdf)
- NHITAM(아이젝스 판매): "ITIL SAM ISO/IEC 19770-1 요건" 충족 표방 — [아이티솔루션 NHITAM](https://itsolution.co.kr/shop/item.php?it_id=1657521334)
- Micro Focus Asset Manager(세인이엔에스 판매): 포트폴리오·계약·재무·구매·SW 관리 모듈 — [세인이엔에스](http://www.seinens.co.kr/sub/products/itam.asp)

**(J) 신생 SaaS형 ITAM — 셀파스 심플리(SMPLY)**
- 2023.06 설립, 2024.04 정식 출시; 2024년 말 기준 250개 이상 기업 고객, 약 8,000개 SaaS 관리, 기업당 평균 32개 SaaS, 연동 법인카드 5,000장 이상, 관리 IT자산 비용 350억 원; 출시 8개월 만에 MRR 5배 — [바이라인 2025.02](https://byline.network/2025/02/24-390/); [더페어](https://www.thefairnews.co.kr/news/articleView.html?idxno=41958); [플래텀](https://platum.kr/archives/253137)
- 에이전트: "설치된 앱, 버전, 브라우저 확장 프로그램 추적, 선택적으로 앱별 사용 시간 측정"; 미사용 라이선스·중복 구독·퇴사자 계정 오프보딩 누락 탐지 — [smply.one/agent](https://www.smply.one/en/agent); [SMPLY](https://www.smply.one/)
- 2024.09 '심플리 디바이스' 출시, 2024.12 조달청 벤처창업기업제품 선정 — [ZDNet 2024.09](https://zdnet.co.kr/view/?no=20240930164305); [ZDNet 2024.12](https://zdnet.co.kr/view/?no=20241211191031)
- 요금제 페이지 존재(수치는 검색 발췌에서 미확인) — [smply.one/pricing](https://www.smply.one/en/pricing)
- (유사 명칭) '셀리즈'가 ITAM 기능(PC·노트북 자동 등록, SW 사용 시간·빈도 분석)을 확장했다는 보도 — [플래텀](https://platum.kr/archives/252140)

**(K) 해외 제품의 국내 채널**
- ManageEngine Endpoint Central: 자산관리 기능 포함, 클라우드/온프레미스, 해외 리뷰 사이트 기준 유료 연 USD 795부터 — [Endpoint Central 기능](https://manageengine.com/products/desktop-central/help/configuring_desktop_central/desktop-central-features.html); [GetApp](https://www.getapp.co.uk/software/103857/manageengine-endpoint-central). **한국 총판 정보는 검색으로 확인되지 않음.**
- Lansweeper: 자산 수 기준 구독(Free/Starter/Pro/Enterprise), 리뷰 사이트 기준 Starter 2,000자산 연 $2,868, Pro 연 $5,268; 패치·EPP·ITSM 기능 미제공 — [Lansweeper 공식](https://www.lansweeper.com/ai-info/); [TrustRadius](https://tr-web.dev.trustradius.com/products/lansweeper). **국내 리셀러 미확인.**
- 서비스나우 기반 사례: 고려아연이 엑시스아이티를 통해 ServiceNow ITOM 체계로 IT자산관리 구축 — [고려아연 보도](https://www.koreazinc.co.kr/%EA%B3%A0%EB%A0%A4%EC%95%84%EC%97%B0-it%EC%9E%90%EC%82%B0%EA%B4%80%EB%A6%AC-%EC%86%94%EB%A3%A8%EC%85%98-%EB%8F%84%EC%9E%85%EC%9E%90%EB%8F%99%ED%99%94-%EA%B8%B0%EB%B0%98-%EC%9A%B4/)

**(L) 확인 실패/무관으로 판단한 후보**
- 에스지앤(SGN)·SGA솔루션즈: "PC 자산관리" 제품이 검색되지 않음(동명 해외 기업만 검색됨) — [검색 결과 예: SGA퓨처스](https://thevc.kr/sgafts)
- 소만사 Privacy-i: 2008년 출시 보도상 "PC 내 고객정보(주민번호·전화번호·카드번호) 패턴 검색→암호화/이동/삭제" 개인정보 탐지 솔루션이며, PC 자산관리·USB 제어 기능은 검색으로 확인되지 않음 — [보안뉴스 2008](https://m.boannews.com/html/detail.html?idx=12227)
- 메타넷 ITAM, 에스피알 ITAM, 나무기술, 인포플라 ITMS, 티맥스 자산관리, 소프트캠프, 와이즈넛, 삼성SDS/LG CNS 상용 ITAM: 관련 제품 정보가 검색되지 않음(삼성SDS는 Brity Works 협업/RPA 제품만 확인) — [삼성SDS Brity Works](https://www.samsungsds.com/kr/news/200914-brityworks.html)

### Inferences
- 국내 제품군의 공통 모듈 구조는 "DMS(자산) + PMS(패치) + 원격제어 + PC-OFF/보안점검"이며, 자산관리 단독 제품보다 통합 PC 관리 플랫폼의 한 모듈로 팔리는 것이 지배적이다(NetClient, Tgate 계열, SafePC, 오피스키퍼 모두 동일 패턴).
- 서버 스택은 Linux(CentOS) 또는 Windows Server + MariaDB(NetClient 사례)처럼 비교적 가벼운 오픈소스 DB를 쓰는 경우가 확인되며, MS SQL Server를 전제로 한 제품은 검색에서 확인되지 않았다(미확인일 뿐 부재 단정 불가).
- 공개 가격은 클라우드형/소규모용에서만 노출된다(SAMQ 월 2,000원/유저, 오피스키퍼 클라우드 연 11만원대/유저). 구축형은 전부 견적이며, 조달 단가는 본 조사에서 확인하지 못했다.

### Gaps
- 나라장터 종합쇼핑몰(디지털서비스몰)의 Tgate DMS·NetClient 6 DMS·Genian NAC·알약 내PC지키미·AhnLab ESA 실제 PC당 등록 단가·유지보수율: 검색 결과에 수치가 없었고 쇼핑몰 직접 조회가 차단됨.
- Tgate DMS·오피스키퍼·Genian NAC·ESA·알약 MPI의 서버 OS/DB 요구사항: 브로셔 원문 열람 불가로 미확인.
- 소만사·SGN·메타넷·SPR·나무기술·인포플라의 ITAM 제품 존재 여부: 미확인(제품이 없거나 검색 인덱싱이 안 된 것으로 추정).
- ManageEngine/Lansweeper 한국 총판: 미확인.

---

## 핵심 질문 2. 국내 특수 요구사항 — 개인정보보호법/ISMS-P 자산목록, SW 저작권 감사(SPC/BSA) 대응, CC/GS 인증·보안적합성, 나라장터, ERP/인사 연동, 한글 UI

### Takeaway
국내 제품이 실제로 마케팅하는 한국형 요구사항은 (1) SPC/BSA 단속·감사 대응용 상용 SW DB 기반 라이선스 식별(약 1만 개 SW·폰트 DB, SAMDB), (2) 공공 조달(나라장터/디지털서비스몰 등록, GS인증, 행망용 SW), (3) 공공 점검 프레임(사이버보안진단의 날, ISMS, 주요정보통신기반시설 PC 점검)이다. ISMS-P 자체는 자산 목록의 책임자/관리자 지정과 최신성 유지를 요구하며, 자산관리시스템 활용을 언급한다. ERP(SAP/Oracle EBS) 인사 연동을 자산관리 제품이 명시한 자료는 찾지 못했다.

### Cited Findings
- ISMS-P 2.1.3 정보자산 관리: "정보자산의 용도와 중요도에 따른 취급 절차 및 보호대책을 수립·이행하고, 자산별 책임소재를 명확히 정의"; "정보자산별로 책임자 및 관리자 지정하고 자산목록에 기록", 인사이동·자산 도입/변경/폐기 시 목록 반영 — [IT위키 ISMS-P 2.1.3](https://itwiki.kr/w/ISMS-P_%EC%9D%B8%EC%A6%9D_%EA%B8%B0%EC%A4%80_2.1.3.%EC%A0%95%EB%B3%B4%EC%9E%90%EC%82%B0_%EA%B4%80%EB%A6%AC)
- ISMS-P 1.2.1 정보자산 식별: 자산명·용도·위치·책임자/관리자·관리부서 기재, "정기적으로 정보자산 현황을 조사하여 목록을 최신으로 유지", "정보자산의 효율적 관리를 위하여 자산관리시스템 활용" 언급 — [IT위키 ISMS-P 1.2.1](https://itwiki.kr/w/ISMS-P_%EC%9D%B8%EC%A6%9D_%EA%B8%B0%EC%A4%80_1.2.1.%EC%A0%95%EB%B3%B4%EC%9E%90%EC%82%B0_%EC%8B%9D%EB%B3%84)
- SW 단속 절차: 검찰·경찰·문체부 저작권 특별단속반이 "SPC와 BSA 또는 저작권자로부터 불법복제 단속 요청을 접수받아" 단속하며, 정품 증빙이 없으면 불법복제로 간주(구자료 블로그) — [컴프로 블로그](https://blog.daum.net/ayukawah/20); SPC는 "사법기관의 SW 불법복제 단속 시 기술 지원" — [여성경제신문](https://www.womaneconomy.co.kr/news/articleView.html?idxno=225949)
- 점검 프로그램은 "컴퓨터에 설치된 소프트웨어 목록을 한 번에 확인", 불법 판명 시 정품 CD·구매 영수증·라이선스 계약서로 입증 필요 — [한국교육신문](https://hangyo.com/news/article.html?no=3111)
- SPC는 "원스텝 솔루션 및 컨설팅 서비스(ⓒ-SAM)", SW 관리 솔루션, 라이선스 검증, 감사(Audit) 대응 서비스를 제공 — [SPC SAM 서비스](https://www.spc.or.kr/ko/business/sam/sw_sub311)
- SPC 주관 자격 C-SAM: 2급 국가공인, 1급 민간자격; 업무 범위는 "기획·정책수립, 수요예측, 구매, 운영, 감사, 사후관리, 교육"; 이스트소프트는 채용 우대 — [리디북스 C-SAM 교재](https://ridibooks.com/books/754018702); [디지털타임스](https://www.dt.co.kr/article/10157840)
- BSA: 국내 PC 설치 SW의 32%가 비정품(2016 대비 3%p 감소), 기업에 SAM 프로그램 구축 권고(구자료) — [BSA 코리아](https://www.bsa.org/kr/nyuseu-mich-ibenteu/eonlon-bodo/cio-bulbeob-sopeuteuweeoga-deiteo-haeking-cholaehandago-balghyeo)
- 공공 인증 체계: CC 필수 24종 중 스팸차단·패치관리·망간자료전송은 CC 또는 GS 중 택일 가능(2018); 백신·DDoS·소스코드 분석도구는 KISA 성능평가로 대체; 보안기능시험결과서(보안적합성 대체) 유효기간 1년 — [바이라인 2018](https://byline.network/2018/03/1-1047/); [바이라인 2016](https://byline.network/2016/12/1-469/)
- "공공시장 진입의 첫 관문 GS인증" — 알약 내PC지키미 전 버전 포함 대부분 제품 GS인증 보유 보도 — [보안뉴스](http://www.boannews.com/media/view.asp?idx=88589); 이스트시큐리티 공식 페이지에는 알약 4.0 CC인증만 게시 — [이스트시큐리티 회사](https://www.estsecurity.com/public/company)
- Tgate DMS: "조달청 등록, 행망용 소프트웨어" — [Tgate DMS 브로슈어](https://tcosvc.co.kr/pdf/tgatedms-brochure.pdf); NetClient 6 DMS: 조달청 디지털서비스몰 등록 — [아이티데일리](https://www.itdaily.kr/news/articleView.html?idxno=209432); 심플리: 조달청 벤처창업기업제품 — [ZDNet](https://zdnet.co.kr/view/?no=20241211191031)
- 공공 PC 점검 프레임: 알약 MPI는 "ISMS 인증 및 사이버 보안 진단의 날 기본 점검, 주요정보통신기반시설 PC 점검" 지원 — [이스트시큐리티 mypc](https://www.estsecurity.com/product/mypc); 공공기관 전체에 '내PC지키미' 도입 전제의 학술연구 존재 — [순천향대 논문](https://scholarworks.bwise.kr/sch/handle/2021.sw.sch/7981?mode=full)
- 조직/인사 연동 관행: 그룹웨어 측에서 "ERP(더존, 영림원, SAP, 뉴젠 등) 인사 데이터로 조직도·사용자 정보 동기화" — [다우오피스 ERP 연동 가이드](https://daouoffice.com/blog/erp-groupware-integration-guide); AD 조직도 연동은 "csvde로 CSV 추출 후 가져오기" 또는 솔루션 기본 AD 연동 기능 활용(커뮤니티 답변) — [쉐어잇 Q&A](https://www.sharedit.co.kr/qnaboards/25817)
- 대학 SW 자산관리 지침 예: "개인 PC에도 적법한 SW만 설치, 불법 SW 설치 PC 반입 금지, 책임은 사용자 본인" — [인하대 지침](https://newdept.inha.ac.kr/sites/cic/download/소프트웨어%20자산관리지침.pdf)

### Inferences
- 국내 제조기업이 가장 실질적으로 마주하는 규제 압력은 ISMS-P보다 SW 저작권 감사(SPC/BSA)다. 따라서 참조 모델에서 "설치 SW 식별 → 상용 SW 카탈로그(라이선스 유형) 매칭 → 구매 라이선스 대비 초과 설치 산출 → 증빙(증서/영수증) 첨부" 흐름은 필수 기능으로 봐야 한다(TCO!sam의 증서 첨부, 스위퍼의 SAMDB, Tgate의 1만 개 SW DB가 그 근거).
- ISMS-P 관점에서는 자산별 책임자/관리자(사용자·부서) 필드와 변경 이력(도입·이동·폐기)이 필수이며, 이는 인사(HR) 마스터와의 동기화 요구로 이어진다. 제품들은 보통 AD/그룹웨어 조직도를 소스로 쓰므로, Oracle EBS 11i HR 데이터를 AD나 중간 테이블로 동기화하는 방식이 현실적이다(직접 EBS 연동 사례는 미확인).
- PC 자산관리 단독 제품은 CC 필수 대상군으로 확인되지 않았고, 공공 진입 시 GS인증이 사실상 관문이다. 제조기업 사내 구축에는 인증이 필요 없다.

### Gaps
- 2025년 기준 국정원 "CC 필수/보안기능시험결과서 대상" 목록에 PC 자산관리·PC 보안관리 제품군이 포함되는지: 최신 자료 미확인(2016~2018 기사만 확인).
- 각 제품의 SAP/Oracle EBS 인사 연동 커넥터 명세: 미확인. 한글 UI는 국산 제품의 기본이나 명시 자료를 인용하지 못함.
- 2025년 SPC/BSA 단속 실적·불법복제율 최신 수치: 미확인.

---

## 핵심 질문 3. "순수 ITAM" vs "PC 보안관리에 자산관리 포함" 구분 및 시장 인기 신호

### Takeaway
순수 ITAM(자산 생애주기·계약·재무 중심)에 가까운 국산 제품은 SaaS형 심플리와 SAM 특화 제품(스위퍼·SAMQ·TCO!sam) 정도이며, 전통 강자(Tgate DMS, NetClient, SafePC)는 "통합 PC 관리/보안"이고, 오피스키퍼·Genian NAC·내PC지키미류는 보안 제품에 자산 가시성이 부가된 형태다. 인기 신호는 고객 수·조달 점유 보도에서 얻을 수 있으나 ITAM 단독 시장점유율 통계는 없다.

### Cited Findings
- **통합 PC 관리(자산 포함)**: NetClient — DMS/PMS/PC-OFF/HSM 모듈, 1,000여 고객사(금융·제조) — [닥터소프트](https://doctorsoft.co.kr/product/netclient.jsp); [잡코리아](https://www.jobkorea.co.kr/Recruit/Co_Read/C/dr2000so). Tgate DMS — 2,000여 공공·기업 납품(NAC/IPM/DMS 합산) — [사람인](https://m.saramin.co.kr/job-search/company-info-view?csn=WlRXMkZXRFR5VjB1SldTRlRTcUsvZz09). SafePC — 공공·금융 300여 레퍼런스(구자료) — [전자신문 2009](https://m.etnews.com/200904170185)
- **보안 제품 + 자산 가시성**: 오피스키퍼(DLP 7기능 중 1개가 IT 자산관리) — [바이라인 2024.04](https://byline.network/2024/04/9-201/); Genian NAC(NAC 조달 매출 약 70% 점유 2018–2019, DMS 모듈) — [지니언스 사업보고서](https://wp.genians.co.kr/wp-content/uploads/%EC%82%AC%EC%97%85%EB%B3%B4%EA%B3%A0%EC%84%9C2020.12.pdf); 내PC지키미(ESA/알약 MPI)는 취약점 점검 중심, 자산은 ASM 연동 — [알약 MPI 제안서](https://cdn1.estsecurity.com/es_channel/23fw_partner/proposal/alyac%20proposal/alyac%20mpi%20proposal/alyac_mpi_proposal_230720.pdf)
- **SAM 특화/순수 ITAM**: 스위퍼(SPC 주력, 2026 AI 모듈 확장) — [디지털데일리 2026.02](https://www.ddaily.co.kr/page/view/2026022715265337414); SAMQ(월 2,000원/유저 클라우드) — [SAMQ](https://pro.samq.co.kr/); 심플리(250+ 고객, MRR 5배) — [바이라인 2025.02](https://byline.network/2025/02/24-390/); NHITAM(ISO/IEC 19770-1 표방) — [아이티솔루션](https://itsolution.co.kr/shop/item.php?it_id=1657521334)
- **대기업형 ITAM**: 고려아연 ServiceNow ITOM 기반 자산관리 구축 — [고려아연](https://www.koreazinc.co.kr/%EA%B3%A0%EB%A0%A4%EC%95%84%EC%97%B0-it%EC%9E%90%EC%82%B0%EA%B4%80%EB%A6%AC-%EC%86%94%EB%A3%A8%EC%85%98-%EB%8F%84%EC%9E%85%EC%9E%90%EB%8F%99%ED%99%94-%EA%B8%B0%EB%B0%98-%EC%9A%B4/); 2007년 대한생명의 CA Unicenter APM 도입이 국내 첫 ITAM 구축 사례로 보도(구자료) — [보안뉴스](https://m.boannews.com/html/detail.html?idx=7109)
- 공공 PC 자산관리 조달 규모 참고: 용인외대부고 "2026년 PC 및 자산관리 프로그램 렌탈 용역" 추정가 409,090,909원(대수 미기재) — [산군 입찰정보](https://www.sankun.com/bid-info/WctCKddI0qEcC6e5s9l41dhBf5CNSVnWgXRFrlB3QhY)
- 시장 전망(글로벌): ITAM SW 시장 2029년까지 CAGR 8.4% — [GII Korea](https://www.giikorea.co.kr/report/tbrc1717048-information-technology-it-asset-management.html)

### Inferences
- 제조기업(Windows PC 중심, Oracle EBS)이 참조할 모델은 "통합 PC 관리형(DMS 모듈)"이 가장 적합하다. 순수 SaaS ITAM(심플리)은 SaaS 구독 비용 관리에 치우쳐 제조 현장 PC HW/SW 인벤토리 요구와 거리가 있고, NAC/DLP형은 네트워크 통제·유출 방지가 본업이라 자산 모듈이 부가적이다.
- 인기 신호 기준으로는 NetClient(금융·제조 1,000여 고객)와 Tgate DMS(공공 2,000여 납품)가 국내 PC 자산관리의 사실상 레퍼런스이며, 둘 다 "단일 에이전트 + 모듈" 구조를 쓴다.

### Gaps
- 국내 PC 자산관리/ITAM 세그먼트의 벤더별 시장점유율 통계: 공개 자료 없음.
- NetClient/Tgate의 2024–2026 신규 수주 보도: 검색에서 확인되지 않음(회사 소개·채용 페이지 수치만 존재).

---

## 핵심 질문 4. 국내 제품의 전형적 에이전트 설계 — Windows 서비스+트레이, 통신 주기, 정책 푸시, 무결성/삭제 방지

### Takeaway
공개 문서로 설계가 가장 잘 드러나는 것은 Genian NAC 에이전트다: Windows 서비스(GnAgent.exe, 표시명 "Genian Agent") + 트레이, 설치는 GPO(MSI)/CWP/수동, 삭제는 노드 정책의 Delete 옵션·관리자 인증코드·사용자 삭제 중 관리자가 선택, 종료방지·삭제방지 자체보호 제공. 다른 제품(NetClient, Tgate, 오피스키퍼)은 "단일 에이전트"와 자산 고유키 등만 확인되며 통신 주기·푸시 방식은 공개 발췌에서 확인되지 않았다.

### Cited Findings
- Genian NAC Windows 에이전트 설치 방식: "GPO, CWP(Captive Web Portal)를 통해 설치하거나 메일, USB 등을 이용하여 수동 설치"; AD GPO 배포용 MSI 패키지 제공; 기본 설치 경로 `%ProgramFiles%\Geni\Genian` — [Genians Docs: Windows Agent 설치](https://docs.genians.com/release/ko/install/windows-agent.html); [에이전트 설치](https://docs.genians.com/release/ko/install/installing-agent.html)
- 자체 보호: 관리자 가이드상 "에이전트는 관리자의 설정에 따라 종료방지 및 삭제방지와 같은 자체 보안 기능을 제공" — [Genian NAC 5.0.88 관리자 가이드(2026.07)](https://docs.genians.com/release/ko/genian-nac-admin-guide.pdf)
- 삭제 방식: "노드 정책의 에이전트정책(Delete), 에이전트 설정의 인증코드 사용, 인증코드 미사용(사용자 직접 삭제)"; 삭제방식이 '지원 안함'이면 트레이 메뉴에서 삭제 불가 — [에이전트 삭제](https://docs.genians.com/release/ko/install/deleting-agent.html); [에이전트 기본 구성](https://docs.genians.com/nac/5.0/rc/ko/endpoints/agent-default.html)
- 서비스 등록(비공식 3rd-party): GnAgent.exe가 부팅 시 자동실행되는 Windows 서비스(표시이름 Genian Agent) — [windowexe.com](http://www.windowexe.com/bbs/board.php?q=gnagent-exe-c-program-files-geni-genian-gnagent-exe)
- Genian NAC 에이전트 역할: 정책 기반 "에이전트 미설치 단말 설치 유도, 필수 SW 미설치 단말 차단", "자산 관리와 장치 사용 통제" — [Genian NAC 브로셔 2024](https://genians.co.kr/hubfs/NAC%20Brochure_24.07.30.pdf?hsLang=ko); 에이전트 장 별도 문서 — [NAC 에이전트 chapter-5](https://www.genians.co.kr/hubfs/Genians_July2022/PDF/chapter-5.pdf?hsLang=ko)
- NetClient: "단일 에이전트", 자산 "고유키 관리 기술", 서버 1대당 최대 2만 대, 클라이언트 Windows/Mac — [닥터소프트](https://www.doctorsoft.co.kr/netclient/netclient1.jsp); [다온데이타](https://daondata.co.kr/product/fo/productview/14)
- 오피스키퍼: 7개 기능을 "통합 에이전트"로 제공 — [바이라인 2024.04](https://byline.network/2024/04/9-201/)
- 알약 MPI: 점검 결과·로그 "관리 서버로의 자동 업데이트 전송", 서버 없이 라이선스만으로 운영 가능한 옵션, 점검 항목을 프로세스/서비스/레지스트리/파일 기준으로 정의 — [알약 MPI 제안서](https://cdn1.estsecurity.com/es_channel/23fw_partner/proposal/alyac%20proposal/alyac%20mpi%20proposal/alyac_mpi_proposal_230720.pdf); [알약 블로그](https://blog.alyac.co.kr/5084)
- AhnLab ESA: 월 1회(셋째 수요일) 자동 점검 스케줄 + 수동 실행, 기준 점수 미달 시 팝업, 보고서 관리자 전송 — [성신여대 안내](https://www.sungshin.ac.kr/bbs/main_kor/3182/110681/download.do)
- SPC SAM Tool: "에이전트, 서버, 콘솔 등을 자동으로 업그레이드" — [SPC SAM Tool](https://www.spc.or.kr/ko/business/sam/sw_sub313)
- 심플리 에이전트: 설치 앱·버전·브라우저 확장 추적, 선택적 앱별 사용시간 측정 — [smply.one/agent](https://www.smply.one/en/agent)
- TCO!sam: 사내 서버·PC 설치형, 외부 접속 불가 설계 — [TCO!sam](https://www.mlsoft.com/product-tcosam.php)

### Inferences
- 참조 설계로 추출 가능한 공통 패턴: (1) 단일 Windows 서비스(SYSTEM 권한) + 사용자 세션 트레이 프로세스, (2) MSI 패키지로 GPO 배포, (3) 서버 정책(노드/그룹별)으로 삭제·종료 허용 여부와 인증코드 제어, (4) 에이전트·서버·콘솔 자동 업그레이드 채널, (5) 점검/인벤토리의 스케줄(월 1회 전사 점검 + 수동) 및 결과 자동 업로드, (6) 자산 고유키(HW 지문)로 재설치·OS 재설치 시 중복 등록 방지.
- 폴링 주기나 서버→에이전트 푸시(롱폴링/소켓) 방식은 어느 벤더도 공개 발췌에 수치를 내지 않았으므로, 자체 설계 시 "주기적 하트비트 + 정책 버전 비교 후 다운로드" 같은 보수적 방식이 무난하다(추론).

### Gaps
- 에이전트-서버 통신 주기, 프로토콜(HTTPS/REST vs 전용 포트), 서버 푸시 방식: 전 제품 미확인.
- 에이전트 무결성 검증(코드 서명, 파일 변조 감지) 구현 세부: Genian "삭제방지/종료방지" 외 미확인.

---

## 핵심 질문 5. 공개된 참조 아키텍처(에이전트-서버-콘솔) 및 백서

### Takeaway
형식을 갖춘 공개 아키텍처 문서는 지니언스(docs.genians.com 관리자 가이드·배포 모델 문서, 브로셔)와 닥터소프트/티씨오서비스(제품 페이지·브로슈어), 이스트시큐리티(제안서 PDF), 안랩(브로셔·표준제안서)이 있으며, 그 외는 유통사 소개 페이지 수준이다. SPC의 SAM 가이드라인도 프로세스 참조로 쓸 수 있다.

### Cited Findings
- Genian NAC 관리자 가이드 PDF(Release 5.0.88, 2026.07) 및 배포 고려사항 문서(정책서버/센서 배치 모델) — [관리자 가이드](https://docs.genians.com/release/ko/genian-nac-admin-guide.pdf); [Deployment Considerations](https://docs.genians.com/release/en/deploying/deployment-models.html); 설치 문서 — [Genian NAC 설치](https://docs.genians.com/release/ko/install.html); Docker Hub 이미지 공개 — [hub.docker.com/r/genians/genian-nac](https://hub.docker.com/r/genians/genian-nac)
- Genian NAC 브로셔·소개서·MSP 솔루션브리프 — [v5.0 Brochure](https://wp.genians.co.kr/resources/Genian_NAC_v5.0_Brochure.pdf); [SolutionBrief MSP](https://www.genians.co.kr/hubfs/product/NAC/Genian_NAC_SolutionBrief_MSP.pdf?hsLang=ko); [Cloud NAC 2024](https://www.genians.co.kr/hubfs/Genian_Cloud_NAC_v5.0_2024_v1%20(1).pdf?hsLang=ko); 관리자 교육 영상 — [YouTube](https://www.youtube.com/watch?v=RIg5D6ECH5A)
- Tgate DMS 브로슈어(기능 목록·운영환경 탭은 사이트에서만 열람) — [tgatedms-brochure.pdf](https://tcosvc.co.kr/pdf/tgatedms-brochure.pdf)
- NetClient 제품 페이지(모듈 구조, 서버 1대당 2만 대) 및 유통사 사양표(OS/DB) — [닥터소프트](https://www.doctorsoft.co.kr/netclient/netclient1.jsp); [다온데이타](https://daondata.co.kr/product/fo/productview/14); 유튜브 채널 — [닥터소프트 YouTube](https://www.youtube.com/@%EB%8B%A5%ED%84%B0%EC%86%8C%ED%94%84%ED%8A%B8)
- 알약 내PC지키미 제안서(2023.07) 및 활용백서 블로그 — [제안서 PDF](https://cdn1.estsecurity.com/es_channel/23fw_partner/proposal/alyac%20proposal/alyac%20mpi%20proposal/alyac_mpi_proposal_230720.pdf); [활용백서](https://blog.alyac.co.kr/5264)
- AhnLab ESA 브로셔·EMS 브로셔·공공 표준제안서·물품사양서 — [ESA](https://download.ahnlab.com/kr/site/brochure/Brochure_AhnLab_ESA.pdf); [EMS](https://download.ahnlab.com/kr/site/brochure/Brochure_EMS.pdf); [표준제안서](https://www.bitzkorea.com/marketing/news/product/ahnlab/%5B%ED%91%9C%EC%A4%80%EC%A0%9C%EC%95%88%EC%84%9C%5DAhnLab%20%EB%82%B4PC%EC%A7%80%ED%82%A4%EB%AF%B8_%EA%B3%B5%EA%B3%B5.pdf); [물품사양서 v2.1](https://www.swwu.ac.kr/download/1468887620676/index.html)
- SPC 소프트웨어 자산관리 가이드라인(재고 식별, 통제, 라이선스 준수 검증 프로세스) — [에너지경제연구원 전자도서관 소장본](https://www.keei.re.kr/library/10110/contents/5970849); SPC SAM 서비스 페이지 — [SPC](https://www.spc.or.kr/ko/business/sam/sw_sub311)
- Genian GPI(내PC지키미류) 브로셔: 부서/개인별 보안 점수·추이 그래프 — [Genian GPI Brochure](https://wp.genians.co.kr/resources/Genian_GPI_Brochure.pdf)
- 국내 ITAM 학술 자료: ITAM을 "자산 이력, 비용, 계약, 구매·리스 데이터 관리"로 정의하고 목적으로 비용관리·라이선스/HW 재활용·내부통제·외부감사 대응 제시 — [KoreaScience 2022](https://koreascience.kr/article/CFKO202223650378347.pdf)
- 해외 참조: NIST SP 1800-5 ITAM 가이드(자산 가시성→보안 대응 강화, 라이선스 사용/구매 비교) — [NIST SP 1800-5](https://csrc.nist.gov/News/2018/NIST-Releases-Special-Publication-1800-5)

### Inferences
- 사내 구축 참조 모델로는 (a) 아키텍처·정책 모델은 Genian NAC 문서(정책서버–에이전트–웹 콘솔, 노드 그룹 기반 정책, MSI/GPO 배포, 삭제방지), (b) 자산 데이터 모델·모듈 구성은 NetClient/Tgate DMS(DMS·PMS·원격제어·PC-OFF, 자산 고유키, 상용 SW DB 기반 라이선스 인가/회수), (c) 컴플라이언스 점검 모델은 알약 MPI/ESA(점검 항목을 프로세스·서비스·레지스트리·파일로 정의, 월 단위 전사 점검, 부서별 점수)가 조합 가능하다.

### Gaps
- 벤더가 공개한 "아키텍처 백서" 형태의 문서(DB 스키마, API 명세)는 Genian 관리자 가이드 외에 확인되지 않았다.
- Tgate DMS 운영환경(서버 OS/DB) 탭과 오피스키퍼 미니서버 사양은 사이트 직접 열람이 필요하다.

---

## 보론. SAM(소프트웨어 자산관리) 컨설팅 시장 — SPC 인증·감사 대응

### Takeaway
SPC가 SAM 컨설팅·툴(스위퍼/SAM Tool/SMART Check)·자격(C-SAM)·단속 기술지원을 한 손에 쥔 구조이며, "솔루션 제품 인증" 제도는 확인되지 않았다(사람 자격 C-SAM만 확인). BSA는 조사·권고 역할이다.

### Cited Findings
- SPC: ⓒ-SAM 원스텝 솔루션·컨설팅, 라이선스 검증, Audit 대응 서비스 — [SPC](https://www.spc.or.kr/ko/business/sam/sw_sub311); SPC 3.0 선언(2026.02)으로 AI 지식재산 전문단체 지향, 스위퍼 고도화 — [디지털데일리](https://www.ddaily.co.kr/page/view/2026022715265337414)
- C-SAM 자격(2급 국가공인/1급 민간) — [리디북스](https://ridibooks.com/books/754018702)
- 스위퍼 홈페이지 무료 온라인 SW저작권 교육(2020) — [이데일리](https://edaily.co.kr/News/Read?mediaCodeNo=257&newsId=03926166625965064); [전자신문](https://www.etnews.com/20201112000249)
- 영세기업 대상 '스위퍼 클라우드' 1년 무상 지원(과거) — [SPC](https://www.spc.or.kr/ko/business/sam/sw_sub311)
- 오라클 GLAS VSAM(벤더 측 SAM 프로그램) 한국어 안내 존재 — [Oracle VSAM](https://www.oracle.com/kr/corporate/global-licensing-advisory-services/vsam-program/)

### Inferences
- Oracle EBS 사용 기업은 오라클 라이선스 감사(GLAS)와 PC SW(MS Office, AutoCAD 등) 감사가 별개 트랙이므로, 자체 ITAM은 PC SW 인벤토리와 구매 라이선스 대조에 집중하고 서버/DB 라이선스는 별도 관리하는 것이 합리적이다(추론).

### Gaps
- SPC의 SAM "솔루션 인증" 제도 존재 여부: 미확인(검색상 없음).
- 스위퍼/오딧맨 정가 및 PC당 단가: 미확인.
