using System;
using System.Collections;
using RandomFortress.Data;
using RandomFortress.Game;
using RandomFortress.Game.Netcode;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;

namespace RandomFortress.Scene
{
    // 게임의 전체적인 흐름을 Main문 내에서 구현한다
    
    public class Game : MainBase
    {
        // [Header("포톤")]
        // [SerializeField] private PhotonView photonView;

        [Header("기본")] 
        public Canvas canvasUI;
        // public HeroMachine hero;
        public TextMeshProUGUI FPS;
        public Transform uiEffectParent;
        public Transform bulletParent;
        public Transform monsterParent;
        public Transform effectParent;
        public Transform skillParent;

        // private float fpsUpdateInterval = 1f;  // 프레임을 업데이트할 간격
        private float fpsAccumulator = 0f;     // 누적된 시간
        private int framesAccumulated = 0;     // 누적된 프레임 수
        private float currentFps = 0f;         // 현재 FPS

        void Awake()
        {
            Initialize();
            StartCoroutine(UpdateFPSCor());
        }
        
        private IEnumerator Start()
        {
            transform.localScale = canvasUI.transform.localScale;
            GameManager.Instance.mainScale = canvasUI.transform.localScale;
            GameManager.Instance.game = this;
            GameManager.Instance.SetupGame();
            yield return new WaitForSeconds(0.5f);
            
            GameManager.Instance.isPlayGame = true;
            // GameManager.Instance.canInteract = true;
            AudioManager.Instance.PlayBgm("First Steps");

            // 시간
            StartCoroutine(TimeUpdateCor());

            // 몬스터 생성 시작
            StartCoroutine( StageProcessCor() );
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

        private void Initialize()
        {
            GameManager.Initialize();
            GameManager.Instance.Reset();
            GameUIManager.Initialize();
            GameUIManager.Instance.Reset();
            SpawnManager.Initialize();
            SpawnManager.Instance.Reset();
        }

        private IEnumerator StageProcessCor()
        {
            float stageDelay = (float)DataManager.Instance.stageData.StartDelayTime / 10;
            int lastStage = DataManager.Instance.stageData.infos.Length;

            GamePlayer myPlayer = GameManager.Instance.myPlayer;

            while (lastStage > myPlayer.stageProcess)
            {
                // 스테이지 시작 연출
                string stageText = "Stage    " + myPlayer.stageProcess + "    Start";
                GameUIManager.Instance.ShowMidText(stageText);
                
                // 스테이지 데이터 가져오기
                StageInfo stageData = DataManager.Instance.stageData.infos[myPlayer.stageProcess - 1];
                int count = stageData.appearMonster.Length;
                float appearDelay = (float)stageData.appearDelay / 10;
                float stageReward = stageData.stageReward;

                // 몬스터 등장 UI 출력
                GameUIManager.Instance.StartMonsterInit(appearDelay * count);
                
                // 몬스터 생성
                for (int i = 0; i < count; ++i)
                {
                    GameManager.Instance.MonsterSpawner(GameManager.Instance.myPlayer.actorNumber,
                            stageData.appearMonster[i], stageData.buffHP);

                    yield return new WaitForSeconds(appearDelay);
                }
                
                // 스테이지 딜레이 아이콘표기
                GameUIManager.Instance.StartStageDelay(stageDelay);

                // 스테이지 대기
                float waitTime = 0f;
                while (waitTime < stageDelay)
                {
                    waitTime += Time.deltaTime;
                    if (GameManager.Instance.isSkipStage)
                    {
                        float reward = stageReward * (stageDelay - waitTime);
                        GameManager.Instance.SkipReward((int)reward);
                        break;
                    }
                    yield return null;
                }
                
                // 다음 스테이지
                GameManager.Instance.StageProcess();
            }
        }
        
        private IEnumerator TimeUpdateCor()
        {
            while (!GameManager.Instance.isGameOver)
            {
                yield return new WaitForSeconds(1f);
                GameManager.Instance.gameTime += 1;
                GameUIManager.Instance.UpdateTime();
            }
        }

        public void OnExit()
        {
            PunManager.Instance.LeaveRoom();
            MainManager.Instance.ChangeScene(SceneName.Menu);
        }
    }
}
