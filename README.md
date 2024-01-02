# Random Fortress

2024.1.2(화) 현황

랜덤 포트리스 : 타워 디펜스

Technology Stack

사용 툴
- Unity 2021.3.15f1
- Rider 2023.2.2

사용 Asset
- Spine
- DoTween

사용될 기술
- Addressable Asset System

서버관련
- Unity Game Servise
- Photon Pun2


작업 진행순서
1. 기본 타워디펜스 게임 로직 구현
  - 타워스킬, light2D를 통한 데코, 각종 리소스 확보 밎 적용
  - 포톤을 활용한 1대1 구현
  - 게임모드별 구현 (솔로, 1대1, 8인)
    
2. 보상형&배너 광고 테스트 및 적용 GoogleAds 선 적용.

3. UGS 모든 기능 사용을 위한 R&D 및 적용 (인앱결제, 앱푸시, 데이터분석 등)

4. 리소스 어드레서블 로드를 위한 CDN 작업 (R&D 필요)

5. 세이브데이터를 필요에 맞게 로컬과 서버에 각각. ScriptableObject
  - 계정정보를 저장하기위해 무엇을 사용할지 ( UGS, Photon Clound, PlayPab 중 택1)
  - 소셜계정 로그인 (구글 로그인만 우선적용 예정)
  - 구글 플레이스토어 오픈신청 및 대응, iOS 스토어 오픈신청 및 대응

6. 메일함, 공지사항, 이벤트팝업 등 인게임외는 게임 시작을 위한 최소기능만 붙인후 차차 붙여나가기로
