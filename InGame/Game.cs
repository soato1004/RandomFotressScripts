using System;
using System.Collections;
using System.Linq;
using Photon.Pun;

using UnityEngine;
using UnityEngine.Events;

namespace RandomFortress
{
    /// 게임의 전체적인 흐름을 관리하는 클래스
    public class Game : SceneBase
    {
        [Header("기본")] 
        [SerializeField] private Canvas canvasUI;
        [SerializeField] private GameMode[] gameModes; // 변수명 변경: gameMode -> gameModes
        [SerializeField] private Transform gameModeParent; // 변수명 변경: GameModeParent -> gameModeParent
        [SerializeField] private Transform uiModeParent; // 변수명 변경: UIModeParent -> uiModeParent
        [SerializeField] private PhotonView photonView;
        
        public bool isAllReady = false;
        
        private bool isMonsterSpawnComplete = false; // 변수명 변경: isDone -> isMonsterSpawnComplete
        private bool isStageSkipped = false; // 변수명 변경: isSkip -> isStageSkipped
        private TowerBase targetTower;
        private const float TIME_UPDATE_INTERVAL = 1f;
        private GameType gameType = GameType.Solo;

        private GamePlayer myPlayer => GameManager.I.myPlayer;
        private GamePlayer otherPlayer => GameManager.I.otherPlayer;

        /// 초기 설정을 수행합니다.
        protected override void Awake()
        {
            base.Awake();
            photonView = GetComponent<PhotonView>();
        }

        /// 게임 모드를 설정합니다.
        private void GameModeSetup()
        {
            //TODO: 해상도대응 하드코딩
            Camera.main.orthographicSize = 1170f / canvasUI.transform.localScale.x;
            GameManager.I.offset = 860f;
            EntityBase.offSet = 860f;
            
            SetActivateAllChildren(gameModeParent, false);
            SetActivateAllChildren(uiModeParent, false);

            gameType = MainManager.I.gameType;
            GameManager.I.gameType = MainManager.I.gameType;
            GameManager.I.gameMode = gameModes[(int)MainManager.I.gameType];
            GameManager.I.game = this;
            GameManager.I.InitializeGameManager();
            GameUIManager.I.InitializeGameUI();
            _ = SpawnManager.I;
            ObjectPoolManager.I.Init();
        }
        
        public override void StartScene()
        {
            GameModeSetup();
            StartCoroutine(TimeUpdateCor());
            StartStage();
        }
        
        public void StartStage()
        {
            StartCoroutine(StageProcessCor());
        }

        #region 입력

        private TowerBase GetClickedTower()
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(worldPoint, Vector2.zero, Mathf.Infinity);

            return hits
                .Where(hit => hit.collider.gameObject != gameObject)
                .Select(hit => hit.collider.GetComponent<TowerBase>())
                .FirstOrDefault(tower => tower != null);
        }

        private void OnMouseDown()
        {
            targetTower = GetClickedTower();
        
            if (targetTower == null)
            {
                GameManager.I.HideFocusTower();
            }
            else
            {
                targetTower.MouseDown();
            }
        }

        private void OnMouseDrag()
        { 
            targetTower?.MouseDrag();
        }

        private void OnMouseUp()
        {
            targetTower?.MouseUp();
            targetTower = null;
        }

        #endregion

        /// 주어진 Transform의 모든 자식 오브젝트를 활성 또는 비활성화합니다.
        private void SetActivateAllChildren(Transform parent, bool active)
        {
            foreach (Transform child in parent)
            {
                child.gameObject.SetActive(active);
            }
        }

        /// 스테이지 진행을 처리하는 코루틴입니다.
        private IEnumerator StageProcessCor()
        {
            int stageProcess = myPlayer.stageProcess;
            isStageSkipped = false;
                
            Debug.Log("현재 스테이지 : "+stageProcess);

            bool isBossStage = stageProcess % 10 == 0;
            bool isFirstStage = stageProcess % 10 == 1;

            GameUIManager.I.StageStart();

            if (isFirstStage)
            {
                yield return HandleFirstStage(stageProcess);
            }
            else if (isBossStage)
            {
                SoundManager.I.PlaySound(SoundKey.bgm_boss, SoundType.BGM, 0.3f);
            }
            
            yield return SpawnAndWaitForMonsters(stageProcess);

            yield return StageClear();
        }
        
        /// 첫 번째 스테이지를 처리합니다.
        private IEnumerator HandleFirstStage(int stageProcess)
        {
            GameManager.I.HideFocusTower();
            SoundManager.I.StopSound(SoundType.BGM);
            
            GameManager.I.PauseGame();

            isAllReady = false;
            GameManager.I.readyCount = 0;
            int loopCount = stageProcess == 1 ? 2 : 1;

            // 어빌리티 카드 선택
            AbilityPopup popup = PopupManager.I.ShowPopup(PopupNames.AbilityPopup, loopCount) as AbilityPopup;
            yield return popup.ShowAbilityCard(loopCount);
            
            if (gameType != GameType.Solo)
                yield return new WaitUntil(() => isAllReady );
            
            popup.ClosePopup();
            GameManager.I.ResumeGame();
            
            // 게임 시작
            SoundManager.I.PlaySound(SoundKey.bgm_game, SoundType.BGM, 0.3f);
        }
        
        /// 몬스터를 생성하고 모든 몬스터가 처치될 때까지 기다립니다.
        private IEnumerator SpawnAndWaitForMonsters(int stageProcess)
        {
            isMonsterSpawnComplete = false;

            StageDataInfo stageData = DataManager.I.stageData.Infos[stageProcess - 1];
            int hp = stageData.hp;
            float appearDelay = (float)GameConstants.AppearDelayMs / 1000;

            if (stageProcess % 10 == 0)
            {
                StartCoroutine( SpawnBossMonster(stageData, hp, appearDelay));
            }
            else
            {
                StartCoroutine(SpawnNormalMonsters(stageData, hp, appearDelay, stageProcess));
            }
            
            yield return new WaitUntil(() => isStageSkipped || (myPlayer.monsterList.Count == 0 && isMonsterSpawnComplete));
        }

        /// 보스 몬스터를 생성합니다.
        private IEnumerator SpawnBossMonster(StageDataInfo stageData, int hp, float appearDelay)
        {
            int monsterIndex = stageData.appearMonster + (int)MonsterType.Boss;
            GameManager.I.MonsterSpawner(myPlayer.ActorNumber, monsterIndex, hp);
            yield return Utils.WaitForSeconds(appearDelay);
            
            isMonsterSpawnComplete = true;
        }


        /// 일반 몬스터들을 생성합니다.
        private IEnumerator SpawnNormalMonsters(StageDataInfo stageData, int hp, float appearDelay, int stageProcess)
        {
            GameUIManager.I.StartMonsterInit(appearDelay * stageData.monsterCount);
            
            for (int i = 0; i < stageData.monsterCount; ++i)
            {
                if (GameManager.I.isGameOver)
                    yield break;

                int monsterIndex = DetermineMonsterType(stageData, i);
                GameManager.I.MonsterSpawner(myPlayer.ActorNumber, monsterIndex, hp);

                yield return Utils.WaitForSeconds(appearDelay);
            }
            
            if (stageProcess == myPlayer.stageProcess)
                isMonsterSpawnComplete = true;
        }


        /// 몬스터 타입을 결정합니다.
        private int DetermineMonsterType(StageDataInfo stageData, int index)
        {
            int monsterIndex = stageData.appearMonster;

            if (index % 5 == 0)
            {
                float[] weights = { 10, 10 };
                int rand = Utils.GetRandomIndexWithWeight(weights);

                switch (rand)
                {
                    case 0:
                        monsterIndex += (int)MonsterType.Speed;
                        break;
                    case 1:
                        monsterIndex += (int)MonsterType.Tank;
                        break;
                }
            }

            return monsterIndex;
        }


        /// 스테이지를 스킵합니다.
        public void SkipStage() => isStageSkipped = true;


        /// 스테이지 클리어 처리를 수행합니다.
        private IEnumerator StageClear()
        {
            int stageProcess = myPlayer.stageProcess;
            bool isBossStage = stageProcess % 10 == 0;

            if (GameConstants.TotalStages == stageProcess)
            {
                yield return WaitForAllMonstersClear(myPlayer);
                
                // 게임 클리어
                GameManager.I.GameClear();
                StopAllCoroutines();
            }
            else
            {
                if (isBossStage)
                {
                    yield return WaitForAllMonstersClear(myPlayer);
                    if (gameType != GameType.Solo)
                        yield return WaitForAllMonstersClear(otherPlayer);
                }

                GameManager.I.StageClear();
            }
        }

        /// 모든 몬스터가 처치될 때까지 기다립니다.
        private IEnumerator WaitForAllMonstersClear(GamePlayer player)
        {
            while (player.monsterList.Count > 0)
                yield return null;
        }

        /// 게임 시간을 업데이트하는 코루틴입니다.
        private IEnumerator TimeUpdateCor()
        {
            float timer = 0;
            while (true)
            {
                timer += Time.deltaTime * GameManager.I.gameSpeed;
                if (timer >= 1)
                {
                    timer = 0;
                    if (GameManager.I.isGameOver)
                        yield break;

                    GameManager.I.gameTime += TIME_UPDATE_INTERVAL;
                    GameUIManager.I.UpdateTime();
                }

                yield return null;
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}