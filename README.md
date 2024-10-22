# Random Fortress

랜덤 포트리스 : 타워 디펜스

2024.10.22(화) 현황
- 공개 테스트버전 진행중 오픈베타 예정

Technology Stack

개발 툴
- Unity 6000.0.23f1, Rider 2024.2.6

사용 Asset
- Spine, DoTween, Google Sheets To Unity

적용 및 구현(예정포함) 기술목록
- Addressable Asset System. 현재 로컬 빌트인으로 되어있고, 리모트로 CDN서버에 붙어서 다운로드받는부분 구현예정
- Photon Pun2. 멀티플레이 진행시 사용되며, 1대1 게임진행 개발구현됨
- Firebase를 사용한 Google 로그인 및 계정생성, 계정데이터 관리 진행 (realtime database, functions 사용중)

작업사항
1. 기본 타워디펜스 게임 로직 구현
  - 타워디펜스 게임 기본 로직 구현
  - Photon Pun2을 활용한 1대1 구현

2. 광고 적용
  - GoogleAdmob 으로 배너, 보상형광고, 전면광고를 적용

3.현재 데이터는 로컬및 서버에서 관리
  - 게임의 모든데이터는 ScriptableObject로 저장되어있다
  - 계정 정보는 firebase로 관리되고 있다
  - 덜 중요한 정보는 PlayerPerf를 사용하여 관리


작업 진행예정
1. iOS 동시출시를 위한 테스트작업 진행중

다음 업데이트 예정사항 (오픈베타 기준스펙)
- 메일함, 공지사항, 이벤트팝업, 옵션, 랭킹 게임내 필수적인 기본기능을 구현


출시스펙
- 타워마다 스킬을 보유하고있고, 메인타워 선택시에 해당 스킬을 사용할수있게 각타워의 특성에 맞쳐 스킬구현
- 가챠 도입

