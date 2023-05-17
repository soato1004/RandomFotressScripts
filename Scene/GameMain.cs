using System;
using System.Collections;
using RandomFortress.Common;
using RandomFortress.Data;
using RandomFortress.Game;
using RandomFortress.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace RandomFortress.Scene
{
    // 게임의 전체적인 흐름을 Main문 내에서 구현한다
    
    public class GameMain : BaseMain
    {
        [Header("기본")]
        // public HeroMachine hero;
        public Tilemap tilemap;
        public Canvas canvasUI;
        public Transform hpBarParent;
        public Transform bulletParent;
        public Transform monsterParent;
        public Transform towerParent;
        public Transform towerPosParent;
        public Transform effectParent;
        public Transform skillParent;

        private Vector3Int[] wayPoints = new[]
        {
            new Vector3Int(-9, 0),
            new Vector3Int(8, 0),
            new Vector3Int(8, -4),
            new Vector3Int(-8, -4),
            new Vector3Int(-8, -8),
            new Vector3Int(8, -8),
            new Vector3Int(8, -12),
            new Vector3Int(-9, -12),
        };
        
        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        private void Initialize()
        {
            InitializeManager<GameManager>();
            InitializeManager<EffectManager>();
            InitializeManager<GameUIManager>();
            InitializeManager<ObjectPoolManager>();
        }

        // private void OnDestroy()
        // {
        //     TerminateManager<GameManager>();
        //     TerminateManager<EffectManager>();
        //     TerminateManager<GameUIManager>();
        //     TerminateManager<ObjectPullManager>();
        // }

        private void InitializeManager<T>() where T : Singleton<T>
        {
            if (null != GetComponent<T>()) return;
            // ReSharper disable once UnusedVariable
            // var addComponent = gameObject.AddComponent<T>();
            Singleton<T>.Initialize();
            Singleton<T>.Instance.Reset();
        }
        
        // /// <summary> 해당 타입의 매니저를 마무리한다. </summary>
        // private void TerminateManager<T>(GameObject manager = null) where T : Singleton<T>
        // {
        //     Singleton<T>.Instance.Terminate();
        //     if (manager != null)
        //         Destroy(Singleton<T>.Instance);
        // }

        private IEnumerator Start()
        {
            GameManager.Instance.game = this;
            GameManager.Instance.wayPoints = wayPoints;
            GameManager.Instance.SetupGame();
            yield return new WaitForSeconds(0.5f);
            
            GameManager.Instance.isPlayGame = true;
            GameManager.Instance.canInteract = true;
            SoundManager.Instance.PlayBgm("StreetLove");

            // 몬스터 생성 시작
            StartCoroutine( StageProcessCor() );
        }

        private IEnumerator StageProcessCor()
        {
            float buff = 1;
            
            
            float stageDelay = (float)DataManager.Instance.stageData.StartDelayTime / 10;
            int LastStage = DataManager.Instance.stageData.infos.Length;
            while (LastStage > GameManager.Instance.CurrentStage)
            {
                // if (GameManager.Instance.CurrentStage%5 == 4)
                    // GameUIManager.Instance.ChangeBackground();
                
                ObjectPoolManager.Instance.Clear();
                string stageText = "Stage    " + (GameManager.Instance.CurrentStage + 1) + "    Start";
                GameUIManager.Instance.ShowMidText(stageText);
                
                StageInfo stageData = DataManager.Instance.stageData.infos[GameManager.Instance.CurrentStage];
                int count = stageData.appearMonster.Length;
                float appearDelay = (float)stageData.appearDelay / 10;
                float stageReward = stageData.stageReward;

                GameUIManager.Instance.StartMonsterInit(appearDelay * count);
                
                // 몬스터 생성
                for (int i = 0; i < count; ++i)
                {
                    if (GameManager.Instance.isGameOver)
                        yield break;
                    
                    GameObject go = ObjectPoolManager.Instance.GetMonster(ObjectPoolManager.ObjectType.Monster, stageData.appearMonster[i]);
                    go.transform.position = tilemap.GetCellCenterWorld(wayPoints[0]);
                
                    Monster script = go.GetComponent<Monster>();
                    script.Init(stageData.appearMonster[i], stageData.buffHP);
                    GameManager.Instance.monsterList.Add(script);

                    if (i == 0)
                        Debug.Log("체력 : " + script.currentHP);
                    
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
                GameManager.Instance.CurrentStage++;
            }
        }

        private float waitTime = 0f;
        void Update()
        {
            waitTime += Time.deltaTime;
            if (waitTime >= 1f)
            {
                waitTime = 0;
                GameUIManager.Instance.UpdateTime();
            }
        }
    }
}
