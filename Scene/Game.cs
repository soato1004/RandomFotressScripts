using System.Collections;
using RandomFortress.Data;
using UnityEngine;

namespace RandomFortress
{
    // 게임의 전체적인 흐름을 Main문 내에서 구현한다
    public class Game : MonoBehaviour
    {
        [Header("기본")] 
        [SerializeField] private Canvas canvasUI;
        public GameMode[] gameMode;
        public Transform GameModeParent;
        public Transform UIModeParent;

        private bool isDone = false; // 몬스터 생성 완료 체크
        private bool isSkip = false; // 스테이지 스킵시 사용

        protected void Awake()
        {
            Application.targetFrameRate = 60;
            // 화면이 자동으로 꺼지지 않도록 설정
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            // 캔버스와 크기를 맞춘다
            transform.localScale = canvasUI.transform.localScale;
            GameManager.Instance.mainScale = canvasUI.transform.localScale.x;
            GameManager.Instance.offset = 860f * canvasUI.transform.localScale.x;
        }
        
        void Start()
        {
            // 게임 시작시 초기설정
            GameModeSetup();

            // 시간
            StartCoroutine(TimeUpdateCor());

            // 스테이지 시작
            StartCoroutine( StageProcessCor() );
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (GameManager.Instance.isFocus)
                    GameManager.Instance.HideForcusTower();
            }

            if (Input.GetMouseButton(1))
            {
                
            }
        }
        
        void GameModeSetup()
        {
            foreach (var child in GameModeParent.GetChildren())
                child.transform.gameObject.SetActive(false);
            
            
            foreach (var child in UIModeParent.GetChildren())
                child.transform.gameObject.SetActive(false);
            
            GameManager.Instance.gameType = MainManager.Instance.gameType;
            GameManager.Instance.gameMode = gameMode[(int)MainManager.Instance.gameType];
            GameManager.Instance.game = this;
            
            GameManager.Instance.InitializeGameManager();
            GameUIManager.Instance.InitializeGameUI();
        }
        
        
        // 스테이지 진행
        private IEnumerator StageProcessCor()
        {
            // 게임오버시 종료
            if (GameManager.Instance.isGameOver)
                yield break;
            
            GamePlayer myPlayer = GameManager.Instance.myPlayer;
            GamePlayer otherPlayer = GameManager.Instance.otherPlayer;
            int stageProcess = myPlayer.stageProcess;
            isSkip = false;
            
            bool isBossStage = stageProcess % 10 == 0;
            bool isFirstStage = stageProcess % 10 == 1;
            
            // 스테이지 시작
            GameManager.Instance.StageStart();
            
            // 어빌리티 등장스테이지 일 경우
            if (isFirstStage)
            { 
                GameManager.Instance.HideForcusTower();
                SoundManager.Instance.StopBgm();
                
                // 어빌리티 카드 등장. 최대 5초 이내 선택안하면 첫번째 어빌리티 선택됨
                GameManager.Instance.readyCount = 0;
                int loopCount = stageProcess == 1 ? 2 : 1;
                yield return GameUIManager.Instance.ShowAbilityUI(loopCount);
                
                SoundManager.Instance.PlayBgm("bgm_game");
            }
            else if (isBossStage)
            {
                SoundManager.Instance.PlayBgm("bgm_boss");
            }

            // 몬스터 생성 코루틴 시작
            isDone = false;
            StartCoroutine(SpawnMonster(stageProcess));

            // 모든 몬스터 처치 대기
            yield return new WaitUntil(() => isSkip || myPlayer.monsterDic.Count == 0 && isDone);

            // 다음 스테이지 시작
            yield return StageClear();
        }
        
        private IEnumerator SpawnMonster(int stage)
        {
            GamePlayer myPlayer = GameManager.Instance.myPlayer;
            bool isBossStage = stage % 10 == 0;
            bool isFirstStage = stage % 10 == 1;
            StageDataInfo stageData = DataManager.Instance.stageData.Infos[stage-1];
            int hp = stageData.hp;
            float appearDelay = (float)GameConstants.AppearDelay / 100;
            
            if (isBossStage)
            {
                // 보스 등장 UI 출력
                int monsterIndex = stageData.appearMonster + (int)MonsterType.Boss;
                GameManager.Instance.MonsterSpawner(GameManager.Instance.myPlayer.actorNumber, monsterIndex, hp);
                yield return Utils.WaitForSeconds(appearDelay);
            }
            else
            {
                // 몬스터 등장 UI 출력
                GameUIManager.Instance.StartMonsterInit(appearDelay * stageData.monsterCount);
                
                // 몬스터 생성
                for (int i = 0; i < stageData.monsterCount; ++i)
                {
                    int monsterIndex = stageData.appearMonster;
                    
                    if (i % 5 == 0)
                    {
                        // 특수몬스터 1개. 스피드, 탱커, 히든
                        float[] wights = { 10, 10 };
                        int rand = Utils.GetRandomIndexWithWeight(wights);
                        
                        switch (rand)
                        {
                            case 0: monsterIndex += (int)MonsterType.Speed; break; // 스피드 
                            case 1: monsterIndex += (int)MonsterType.Tank; break; // 탱커
                        }
                    }
                    
                    GameManager.Instance.MonsterSpawner(GameManager.Instance.myPlayer.actorNumber, monsterIndex, hp);
                    
                    yield return Utils.WaitForSeconds(appearDelay);
                }
            }

            if (stage == myPlayer.stageProcess)
                isDone = true;
        }

        // 스테이지 스킵시 처리
        public void SkipStage()
        {
            isSkip = true;
            SoundManager.Instance.PlayOneShot("stage_skip");
        }
        
        private IEnumerator StageClear()
        {
            GamePlayer myPlayer = GameManager.Instance.myPlayer;
            GamePlayer otherPlayer = GameManager.Instance.otherPlayer;
            StageDataInfo stageData = DataManager.Instance.stageData.Infos[myPlayer.stageProcess-1];
            int stageProcess = myPlayer.stageProcess;
            bool isBossStage = stageProcess % 10 == 0;
            
            // 스테이지 클리어
            float stageDelay = (float)GameConstants.StageClearDelay / 100;
            float stageReward = stageData.stageReward;

            switch (GameManager.Instance.gameType)
            {
                case GameType.Solo: 
                    // 마지막차 체크
                    if (GameConstants.TotalStages == myPlayer.stageProcess)
                    {
                        // 모든 몬스터 처치 대기
                        while (myPlayer.monsterDic.Count > 0)
                        {
                            yield return null;
                        }
                
                        // 게임클리어
                        yield return GameClear();
                    }
                    else
                    {
                        GameManager.Instance.StageClear();
                        
                        // 다음 스테이지 시작
                        StartCoroutine( StageProcessCor() );
                    }
                    break;
                case GameType.OneOnOne: 
                    // 마지막차 체크
                    if (GameConstants.TotalStages == myPlayer.stageProcess)
                    {
                        // 모든 몬스터 처치 대기
                        while (myPlayer.monsterDic.Count > 0)
                        {
                            yield return null;
                        }
                
                        // 게임클리어
                        yield return GameClear();
                    }
                    else
                    {
                        // 보스 스테이지 일경우
                        if (isBossStage)
                        {
                            // 모든 몬스터 처치 대기
                            while (myPlayer.monsterDic.Count > 0)
                            {
                                yield return null;
                            }
                            while (otherPlayer.monsterDic.Count > 0)
                            {
                                yield return null;
                            }
                            
                            GameManager.Instance.StageClear();
                            
                            // 다음 스테이지 시작
                            StartCoroutine( StageProcessCor() );
                        }
                        else
                        {
                            GameManager.Instance.StageClear();
                            
                            // 다음 스테이지 시작
                            StartCoroutine( StageProcessCor() );
                        }
                    }
                    break;
                case GameType.BattleRoyal: break;
            }
        }
        
        /// 전체 스테이지클리어
        private IEnumerator GameClear()
        {
            GameManager.Instance.EndGame();
            yield return null;
        }
        
        // 게임 시간 표기
        private IEnumerator TimeUpdateCor()
        {
            while (!GameManager.Instance.isGameOver)
            {
                yield return new WaitForSecondsRealtime(1f);
                GameManager.Instance.gameTime += 1f;
                GameUIManager.Instance.UpdateTime();
            }
        }

        // 종료버튼 클릭시
        public void OnExit()
        {
            PhotonManager.Instance.LeaveRoom();
            MainManager.Instance.ChangeScene(SceneName.Lobby);
        }
    }
}
