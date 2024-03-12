# Random Fortress

2024.3.12(화) 현황
- 공개 테스트버전 신청 검토중


랜덤 포트리스 : 타워 디펜스

Technology Stack

개발 툴
- Unity 2021.3.19f1, Rider 2023.2.2

사용 Asset
- Spine, DoTween, Google Sheets To Unity

적용 및 구현 예정 기술목록
- Addressable Asset System. 현재 로컬 빌트인으로 되어있고, 리모트로 CDN서버에 붙어서 다운로드받는부분 구현예정
- Photon Pun2. 멀티플레이 진행시 사용되며, 1대1 게임진행 개발구현됨

작업사항
1. 기본 타워디펜스 게임 로직 구현
  - 타워디펜스 게임 기본 로직 구현
  - Photon Pun2을 활용한 1대1 구현
  - 게임모드별 구현 (솔로, 1대1, 8인)

2. 광고 적용
  - GoogleAdmob 으로 배너, 보상형광고, 전면광고를 적용

3.현재 데이터는 로컬에서 저장 및 관리되고, 차후 서버데이터를 받는것으로 교체
  - 게임의 모든데이터는 ScriptableObject로 저장되어있다
  - GoogleSheet에 게임내 사용되는 데이터를 넣고, 에디터상에서 데이터를 Pull 하고 ScriptableObject로 만들어서 저장한다

인게임 이외에는 최소기능만 부착되어있고, 나머지는 차차 구현예정


작업 진행예정
1. NaverGamePot으로 계정정보 관리 구현. (로그인, 계정생성, 푸쉬 등 전반적인 부분을 해당SDK로 구현예정)


업데이트 예정사항
- 메일함, 공지사항, 이벤트팝업, 옵션창등 게임내 필수적인 기본기능을 구현
- 타워마다 스킬을 보유하고있고, 메인타워 선택시에 해당 스킬을 사용할수있게 각타워의 특성에 맞쳐 스킬구현

