using System;
using System.Collections;
using GamePlay.GameMode;
using RandomFortress.Common.Extensions;
using RandomFortress.Data;
using RandomFortress.Game;
using RandomFortress.Game.Netcode;
using RandomFortress.Manager;
using RandomFortress.Constants;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace RandomFortress.Scene
{
    // 게임의 전체적인 흐름을 Main문 내에서 구현한다
    
    public class Game : MainBase
    {
        [Header("기본")] 
        public Canvas canvasUI;
        // public HeroMachine hero;
        public TextMeshProUGUI FPS;
        public Transform uiEffectParent;
        // public GameObject[] MapPrefab;
        public GameMode gameMode;

        // private float fpsUpdateInterval = 1f;  // 프레임을 업데이트할 간격
        private float fpsAccumulator = 0f;     // 누적된 시간
        private int framesAccumulated = 0;     // 누적된 프레임 수
        private float currentFps = 0f;         // 현재 FPS

        protected void Awake()
        {
            ManagerInitialize();
        }
        
        private void ManagerInitialize()
        {
            GameManager.Initialize();
            GameManager.Instance.Reset();
            GameUIManager.Initialize();
            GameUIManager.Instance.Reset();
            SpawnManager.Initialize();
            SpawnManager.Instance.Reset();
        }
        
        protected IEnumerator Start()
        {
            yield return null;
            
            // 캔버스와 크기를 맞춘다
            transform.localScale = canvasUI.transform.localScale;
            GameManager.Instance.mainScale = canvasUI.transform.localScale;
            
            // 게임 시작시 초기설정
            GameModeSetup();

            yield return null;
            
            GameManager.Instance.isPlayGame = true;
            AudioManager.Instance.PlayBgm("First Steps");

            // 시간
            StartCoroutine(TimeUpdateCor());

            // 몬스터 생성 시작
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
                case GameType.Solo: gameMode = GetComponentInChildren<SoloMode>(); break;
                case GameType.OneOnOne: gameMode = GetComponentInChildren<OneOnOneMode>(); break;
                case GameType.BattleRoyal: gameMode = GetComponentInChildren<BattleRoyalMode>(); break;
                default: Debug.Log("Not Found GameType");
                    break;
            }

            GameManager.Instance.game = this;
            GameManager.Instance.gameMode = gameMode;
            gameMode.Init();
            
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
            GamePlayer myPlayer = GameManager.Instance.myPlayer;
            int stageProcess = myPlayer.stageProcess-1;
            
            // 스테이지 시작
            GameManager.Instance.StageStart();
                
            StageData stageData = DataManager.Instance.stageDatas[stageProcess];
                
            // 스테이지 시작 연출
            string stageText = "Stage    " + myPlayer.stageProcess + "    Start";
            GameUIManager.Instance.ShowMidText(stageText);
                
            // 스테이지 데이터 가져오기
            int count = stageData.appearMonster.Length;
            float appearDelay = (float)stageData.appearDelay / 10 / GameManager.Instance.timeScale;
            // float stageReward = stageData.stageReward;

            // 몬스터 등장 UI 출력
            GameUIManager.Instance.StartMonsterInit(appearDelay * count);
                
            // 몬스터 생성
            for (int i = 0; i < count; ++i)
            {
                GameManager.Instance.MonsterSpawner(GameManager.Instance.myPlayer.actorNumber,
                    stageData.appearMonster[i], stageData.hp);

                yield return new WaitForSeconds(appearDelay);
            }

            // 모든 몬스터 처치 대기
            while (myPlayer.monsterList.Count > 0)
            {
                yield return null;
            }

            //  다음 스테이지 시작
            if (stageProcess == myPlayer.stageProcess - 1)
                yield return StageClear();
            
        }

        // 스테이지 스킵시 처리
        public void SkipStage()
        {
            StartCoroutine(StageClear());
        }
        
        private IEnumerator StageClear()
        {
            GamePlayer myPlayer = GameManager.Instance.myPlayer;
            int stageProcess = myPlayer.stageProcess-1;
            StageData stageData = DataManager.Instance.stageDatas[stageProcess];
            
            // 스테이지 클리어
            float stageDelay = stageData.startDelayTime / 10 / GameManager.Instance.timeScale;
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
                // // 스테이지 대기 + 딜레이 아이콘 표기
                // GameUIManager.Instance.StartStageDelay(stageDelay);
                //     
                // float waitTime = 0f;
                // while (waitTime < stageDelay)
                // {
                //     waitTime += Time.deltaTime;
                //     if (GameManager.Instance.isSkipStage)
                //     {
                //         float reward = stageReward * (stageDelay - waitTime);
                //         GameManager.Instance.SkipReward((int)reward);
                //         break;
                //     }
                //
                //     yield return null;
                // }
                
                // 다음 스테이지 시작
                StartCoroutine( StageProcessCor() );
            }
        }
        
        /// 전체 스테이지클리어
        private IEnumerator GameClear()
        {
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
