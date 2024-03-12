using System.Collections;
using RandomFortress.Common.Utils;
using RandomFortress.Data;
using RandomFortress.Game;
using RandomFortress.Game.Netcode;
using RandomFortress.Manager;
using RandomFortress.Constants;
using TMPro;
using UnityEngine;

namespace RandomFortress.Scene
{
    // 게임의 전체적인 흐름을 Main문 내에서 구현한다
    
    public class Game : MonoBehaviour
    {
        [Header("기본")] 
        public Canvas canvasUI;
        // public HeroMachine hero;
        public TextMeshProUGUI FPS;
        public Transform uiEffectParent;
        // public GameObject[] MapPrefab;
        public GameMode[] gameMode;

        // private float fpsUpdateInterval = 1f;  // 프레임을 업데이트할 간격
        private float fpsAccumulator = 0f;     // 누적된 시간
        private int framesAccumulated = 0;     // 누적된 프레임 수
        private float currentFps = 0f;         // 현재 FPS
        private bool isSkip = false;

        protected void Awake()
        {
            ManagerInitialize();
        }
        
        private void ManagerInitialize()
        {
            GameManager.Initialize();
            GameUIManager.Initialize();
            SpawnManager.Initialize();
            ObjectPoolManager.Initialize();
        }
        
        protected IEnumerator Start()
        {
            yield return null;
            
            // 캔버스와 크기를 맞춘다
            transform.localScale = canvasUI.transform.localScale;
            GameManager.Instance.mainScale = canvasUI.transform.localScale;
            
            // 게임 시작시 초기설정
            GameModeSetup();
            
            // 게임 플레이카운트 증가
            MainManager.Instance.GamePlayCount++;

            yield return null;
            
            GameManager.Instance.isPlayGame = true;

            // 시간
            StartCoroutine(TimeUpdateCor());

            // 스테이지 시작
            StartCoroutine( StageProcessCor() );
            
            // FPS 표시
            StartCoroutine(UpdateFPSCor());
        }
        
        void GameModeSetup()
        {
            // GameObject prefab = gameModePrefabs[(int)MainManager.Instance.gameType];
            // GameObject goInst = Instantiate(prefab, transform);
            
            switch (MainManager.Instance.gameType)
            {
                case GameType.Solo: GameManager.Instance.gameMode = gameMode[0]; break;
                case GameType.OneOnOne: GameManager.Instance.gameMode = gameMode[1]; break;
                case GameType.BattleRoyal: GameManager.Instance.gameMode = gameMode[2]; break;
                default: Debug.Log("Not Found GameType");
                    break;
            }

            GameManager.Instance.game = this;
            GameManager.Instance.gameMode.gameObject.SetActive(true);
            GameManager.Instance.gameMode.Init();
            
            GameManager.Instance.InitializeGameManager();
            GameUIManager.Instance.InitializeGameUI();
        }

        void Update()
        {
            fpsAccumulator += Time.deltaTime;  // 시간 누적
            framesAccumulated++;  // 프레임 수 누적
        }
        
        private IEnumerator UpdateFPSCor()
        {
            while (!GameManager.Instance.isGameOver)
            {
                currentFps = framesAccumulated / fpsAccumulator;  // 현재 FPS 계산

                // 텍스트 업데이트
                FPS.text = string.Format("FPS: {0}", Mathf.RoundToInt(currentFps));

                // 누적된 값 초기화
                fpsAccumulator = 0f;
                framesAccumulated = 0;
                yield return new WaitForSeconds(1f);
            }
        }



        // 스테이지 진행
        private IEnumerator StageProcessCor()
        {
            // 게임오버시 종료
            if (GameManager.Instance.isGameOver)
                yield break;
            
            GamePlayer myPlayer = GameManager.Instance.myPlayer;
            int stageProcess = myPlayer.stageProcess;
            isSkip = false;
            
            bool isBossStage = stageProcess % 10 == 0;
            bool isFirstStage = stageProcess % 10 == 1;
            
            // 스테이지 시작
            GameManager.Instance.StageStart();
                
            StageDataInfo stageData = DataManager.Instance.stageData.Infos[stageProcess-1];
                
            // 스테이지 시작 연출
            GameManager.Instance.ShowStageClearEffect();
            
            // 어빌리티 등장스테이지 일 경우
            if (isFirstStage)
            { 
                SoundManager.Instance.StopBgm();
                
                // 어빌리티 카드 등장
                yield return GameUIManager.Instance.ShowAbilityUI();

                if (stageProcess == 1)
                {
                    // 어빌리티 카드 등장
                    yield return GameUIManager.Instance.ShowAbilityUI();
                    
                    // 어빌리티 카드 등장
                    yield return GameUIManager.Instance.ShowAbilityUI();
                }
                
                SoundManager.Instance.PlayBgm("bgm_game");
            }
            else if (isBossStage)
            {
                SoundManager.Instance.PlayBgm("bgm_boss");
            }

            // 몬스터 생성
            StartCoroutine(SpawnMonster());
            
            // 모든 몬스터 처치 대기
            while (true)
            {
                if (isSkip)
                    break;

                if (myPlayer.monsterList.Count == 0)
                    break;
                
                yield return null;
            }
            
            //  다음 스테이지 시작
            yield return StageClear();
        }
        
        private IEnumerator SpawnMonster()
        {
            GamePlayer myPlayer = GameManager.Instance.myPlayer;
            int stageProcess = myPlayer.stageProcess;
            bool isBossStage = stageProcess % 10 == 0;
            bool isFirstStage = stageProcess % 10 == 1;
            StageDataInfo stageData = DataManager.Instance.stageData.Infos[stageProcess-1];
            
            // TODO: 보스생성부
            if (isBossStage)
            {
                // 보스 등장 UI 출력
                stageData.appearMonster += (int)MonsterType.Boss;
                GameManager.Instance.MonsterSpawner(GameManager.Instance.myPlayer.actorNumber, stageData.appearMonster);
            }
            else
            {
                float appearDelay = (float)GameConstants.AppearDelay / 100;
                
                // 몬스터 등장 UI 출력
                GameUIManager.Instance.StartMonsterInit(appearDelay * stageData.monsterCount);
                
                // 몬스터 생성
                for (int i = 0; i < stageData.monsterCount; ++i)
                {
                    int monsterIndex = stageData.appearMonster;
                    MonsterType type = MonsterType.None;
                    
                    if (i % 5 == 0)
                    {
                        // 특수몬스터 1개. 스피드, 탱커, 히든
                        float[] wights = { 10, 10 };
                        int rand = RandomUtil.GetRandomIndexWithWeight(wights);
                        
                        switch (rand)
                        {
                            case 0: monsterIndex += (int)MonsterType.Speed; break; // 스피드 
                            case 1: monsterIndex += (int)MonsterType.Tank; break; // 탱커
                        }
                    }

                    GameManager.Instance.MonsterSpawner(GameManager.Instance.myPlayer.actorNumber, monsterIndex);
                    
                    yield return JTUtil.WaitForSeconds(appearDelay);
                }
            }
        }

        // 스테이지 스킵시 처리
        public void SkipStage()
        {
            isSkip = true;
            SoundManager.Instance.PlayOneShot("stage_skip");
            // StartCoroutine(StageClear());
        }
        
        private IEnumerator StageClear()
        {
            GamePlayer myPlayer = GameManager.Instance.myPlayer;
            StageDataInfo stageData = DataManager.Instance.stageData.Infos[myPlayer.stageProcess-1];
            
            // 스테이지 클리어
            float stageDelay = (float)GameConstants.StageClearDelay / 100;
            float stageReward = stageData.stageReward;
                
            GameManager.Instance.StageClear();
                    
            // 마지막차 체크
            if (GameConstants.TotalStages == myPlayer.stageProcess)
            {
                // 모든 몬스터 처치 대기
                while (myPlayer.monsterList.Count > 0)
                {
                    yield return null;
                }
                
                // 게임클리어
                yield return GameClear();
            }
            else
            {
                // 다음 스테이지 시작
                StartCoroutine( StageProcessCor() );
            }
        }
        
        /// 전체 스테이지클리어
        private IEnumerator GameClear()
        {
            GameManager.Instance.GameOver();
            yield return null;
        }
        
        // 게임 시간 표기
        private IEnumerator TimeUpdateCor()
        {
            while (!GameManager.Instance.isGameOver)
            {
                yield return new WaitForSeconds(1f / GameManager.Instance.timeScale);
                GameManager.Instance.gameTime += 1;
                GameUIManager.Instance.UpdateTime();
            }
        }

        // 종료버튼 클릭시
        public void OnExit()
        {
            PunManager.Instance.LeaveRoom();
            MainManager.Instance.ChangeScene(SceneName.Lobby);
        }
    }
}
