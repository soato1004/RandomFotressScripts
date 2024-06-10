using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using RandomFortress.Data;
using UnityEngine.Rendering;

namespace RandomFortress
{
    public class GameManager : SingletonPun<GameManager>
    {
        public float CheatDamage = 1;
        
        [Header("게임상태")] 
        public GameType gameType;
        public Game game;
        public GameMode gameMode;
        public float gameTime = 0;
        public bool isPaused = false;
        [SerializeField] private float timeScale = 1f; // 게임 전체에 사용되는값. 배속에 관여
        public float TimeScale => timeScale;
        public event Action<float> OnTimeScaleChanged;

        public bool canTowerDrag = true;
        public bool isFocus = false;    // 타워포커스
        public TowerBase focusTower = null; // 타워 또는 무언가를 클릭시에 포커스가 갈경우
        public SerializedDictionary<int, TowerUpgrade> TowerUpgradeDic = new SerializedDictionary<int, TowerUpgrade>(); // 현재타워 업그레이드정보

        [Header("동기화되는정보")] 
        public bool isGameOver = false;
        public int readyCount = 0;

        [Header("플레이어")] 
        public SerializedDictionary<int, GamePlayer> players = new SerializedDictionary<int, GamePlayer>();
        public GamePlayer otherPlayer;
        public GamePlayer myPlayer;

        public float mainScale; // 크기가 조정된 캔버스크기에 맞쳐서 스케일값 조정
        public float offset; // 상단 플레이어 위치로 Y값을 조정할때 사용됨. 하드코딩됨
        
        public int gamePlayerCount;
        public bool isWin;
        
        public Vector3 GetRoadPos(int index) => gameMode.GetRoadPos(index) * mainScale;
        public int GetWayLength => gameMode.GetRoadWayLength();
        // public GameMap GameMap => gameMode.gameMap;

        private float saveTimeScale = 0;
        private int nextUnitID = 0;

        //--------------------------------------------------------------------------------------------------------------

        #region Setup

        // 게임 시작시 한번만 호출하는 코드를 이곳에서
        public override void Reset()
        {
            JustDebug.LogColor("GameManager Reset");
        }

        // 게임 시작시 한번만 호출
        public void InitializeGameManager()
        {
            // 게임모드
            switch (gameType)
            {
                case GameType.Solo:
                    break;
            }
            
            // 타워 업그레이드 정보
            for (int slotIndex = 0; slotIndex < GameConstants.TOWER_DECK_COUNT; ++slotIndex)
            {
                int towerIndex = Account.Instance.TowerDeck(slotIndex);
                int cardLevel = 1; //Account.Instance.GetCardLevel(towerIndex);
                TowerUpgradeDic.Add(towerIndex, new TowerUpgrade(towerIndex, cardLevel));
            }
            
            //
            timeScale = 1;
            
            // 게임 진행시 전면광고 스택추가
            MainManager.Instance.ShowPlayAd = true;
            
            gameMode.gameObject.SetActive(true);
            gameMode.Init();
        }

        public void ChangeTimeScale(float scale)
        {
            timeScale = scale;
            OnTimeScaleChanged?.Invoke(scale);
        }
        
        private GamePlayer GetPlayer(int actor)
        {
            if (gameType == GameType.Solo)
                return myPlayer;
            
            if (!players.ContainsKey(actor))
            {
                Debug.Log("Not Found Player!! "+ actor);
                return myPlayer;
            }
            
            return players[actor];
        }
        
        #endregion

        #region Photon
        
        // 플레이어가 준비 상태를 변경할 때 호출되는 메서드
        public void ChangeReadyState(bool isReady)
        {
            if (isReady)
            {
                photonView.RPC("IncrementReadyCount", RpcTarget.AllBuffered);
            }
            else
            {
                photonView.RPC("DecrementReadyCount", RpcTarget.AllBuffered);
            }
        }
        
        [PunRPC]
        void DecrementReadyCount()
        {
            readyCount--;
        }
        
        [PunRPC]
        void IncrementReadyCount()
        {
            readyCount++;
            if (readyCount == PhotonNetwork.PlayerList.Length)
            {
                PrepareGameStart();
            }
        }
        
        // 게임 시작을 위한 준비가 모두 되었을 때 호출될 메서드
        public void PrepareGameStart()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // 현재 시각에서 3초 후의 시각을 게임 시작 시각으로 설정
                double startDelay = 0.2;
                double startTime = PhotonNetwork.Time + startDelay;

                // 모든 클라이언트에게 게임 시작 시각 전송
                photonView.RPC("SetGameStartTime", RpcTarget.AllBuffered, startTime);
            }
        }
        
        [PunRPC]
        void SetGameStartTime(double time)
        {
            readyCount = 0;
            GameUIManager.Instance.AbilityReady(time);
        }
        
        public void SetupPlayer()
        {
            switch (gameType)
            {
                case GameType.Solo:
                    myPlayer.SetPlayer(gameMode.gameMap[0]);
                    break;
                case GameType.OneOnOne:
                    foreach (Player p in PhotonNetwork.PlayerList)
                    {
                        if (p.IsLocal)
                        {
                            myPlayer.SetPlayer(p, gameMode.gameMap[0]);
                            players.Add(p.ActorNumber, myPlayer);
                        }
                        else
                        {
                            otherPlayer.SetPlayer(p, gameMode.gameMap[1]);
                            players.Add(p.ActorNumber, otherPlayer);
                        }
                    }
                    break;
                case GameType.BattleRoyal:
                    break;
            }
        }

        public void StageClear()
        {
            myPlayer.StageClear();
            if (gameType != GameType.Solo)
                photonView.RPC("StageClearRpc", RpcTarget.Others, myPlayer.actorNumber);
        }

        [PunRPC]
        private void StageClearRpc(int actor)
        {
            GamePlayer player = GetPlayer(actor);
            player.StageClear();

            if (myPlayer.stageProcess < player.stageProcess)
                game.SkipStage();
        }
        
        public void EndGame()
        {
            if (isGameOver) return;
            isGameOver = true;
            
            if (gameType == GameType.Solo)
                EndGameRpc(myPlayer.actorNumber);
            else
                photonView.RPC("EndGameRpc", RpcTarget.AllViaServer, myPlayer.actorNumber);
        }
        
        
        [PunRPC]
        private void EndGameRpc(int actor)
        {
            isWin = myPlayer.actorNumber != actor;

            foreach (var pair in players)
                pair.Value.UpdateGameResult();
            
            SoundManager.Instance.StopBgm();
            GameUIManager.Instance.ShowResult();
        }
        
        public void MonsterSpawner(int actorNumber, int monsterIndex, int hp)
        {
            if (gameType == GameType.Solo)
                MonsterSpawnerRpc(actorNumber, monsterIndex, hp, nextUnitID++);
            else
                photonView.RPC("MonsterSpawnerRpc", RpcTarget.AllViaServer, actorNumber, monsterIndex, hp, nextUnitID++);
        }
        
        [PunRPC]
        private void MonsterSpawnerRpc(int actor, int monsterIndex, int hp, int id)
        {
            GamePlayer player = GetPlayer(actor);

            // Debug.Log("Monster Create " + actor + ", id : " + id);
            
            Vector3 startPos = GetRoadPos(0);
            if (otherPlayer == player)
                startPos.y += offset;
        
            // 특수 몬스터일경우
            MonsterType type = MonsterType.None;
            if (monsterIndex > GameConstants.MonsterTypeLimit)
            {
                type = (MonsterType)((monsterIndex / 10000)*10000);
                monsterIndex %= 10000;
            }
        
            switch (type)
            {
                case MonsterType.Speed: monsterIndex = (int)MonsterIndex.WolfBlue; break;
                case MonsterType.Tank: break;
            }
            
            GameObject monsterGo = SpawnManager.Instance.GetMonster(startPos, monsterIndex);
            MonsterBase monster = monsterGo.GetComponent<MonsterBase>();
            
            player.monsterOrder.Add(monster);
            player.monsterDic.Add(id, monster);
            monster.Init(player, monsterIndex, hp, id, type);
        }
        
        public bool SkipStage()
        {
            // 보스 스테이지는 스킵할수없다
            if (myPlayer.stageProcess % 10 == 0)
            {
                Debug.Log("보스를 잡지않으면 스킵할수없다");
                return false;
            }
            
            game.SkipStage();
            
            // if (gameType == GameType.Solo)
            //     game.SkipStage();
            // else
            //     photonView.RPC("SkipStageRpc", RpcTarget.AllViaServer);
            
            return true;
        }

        // [PunRPC]
        // public void SkipStageRpc()
        // {
        //     game.SkipStage();
        // }
        
        public void SelectAbilityCard(AbilityData data)
        {
            myPlayer.SetAbility(data);
            if (gameType != GameType.Solo)
                photonView.RPC("SelectAbilityRpc", RpcTarget.Others, data.index);
        }
        
        [PunRPC]
        public void SelectAbilityRpc(int index)
        {
            AbilityData abilityData = DataManager.Instance.abilityDataDic[index];
            otherPlayer.SetAbility(abilityData);
        }

        #endregion
        
        #region GamePlay

        public void StageStart()
        {
            GameUIManager.Instance.StageStart();
        }
        
        public void ChangeGameSpeed()
        {
            timeScale = (int)timeScale == 1 ? 2f : 1f;
        }
        
        // 보스 클리어시 배경이 바뀐다
        public void ShowStageClearEffect()
        {
            GameUIManager.Instance.StageStart();
        }

        public void BuildRandomTower()
        {
            myPlayer.BuildRandomTower();
        }

        public void SkipReward(int reward)
        {
            myPlayer.gold += reward;
        }

        public void TowerUpgrade(int towerIndex)
        {
            TowerUpgrade upgrade = TowerUpgradeDic[towerIndex];

            if (myPlayer.gold < upgrade.UpgradeCost)
            {
                Debug.Log("돈이 부족합니다");
                return;
            }

            if (upgrade.TowerUpgradeLv >= 5)
            {
                Debug.Log("최종 업그레이드");
                return;
            }
            
            myPlayer.gold -= upgrade.UpgradeCost;
            
            upgrade.Upgrade();
            
            GameUIManager.Instance.UpdateInfo();
            GameUIManager.Instance.UpdateUpgradeBtn();
            
            // TODO: 타워 업그레이드 적용부분
            SoundManager.Instance.PlayOneShot("tower_upgrade");
        }

        public void ShowForcusTower(TowerBase tower)
        {
            isFocus = true;
            focusTower = tower;
            GameUIManager.Instance.ShowAttackRange(tower.transform.position, tower.Info.attackRange);
        }
        
        public void HideForcusTower()
        {
            isFocus = false;
            if (focusTower != null)
                focusTower.SetFocus(false, false);
            focusTower = null;
            GameUIManager.Instance.HideAttackRange();
        }

        public void PlayerOut()
        {
            PauseGame();
            GameUIManager.Instance.SetPlayerOutPopup(true);
        }

        public void ComeBackPlayer()
        {
            ResumeGame();
            GameUIManager.Instance.SetPlayerOutPopup(false);
        }

        public void PauseGame()
        {
            if (gameType != GameType.Solo)
                return;

            saveTimeScale = timeScale;
            
            timeScale = 0;
            Time.timeScale = 0;
            isPaused = true;
        }
        
        public void ResumeGame()
        {
            if (gameType != GameType.Solo)
                return;

            timeScale = saveTimeScale;
            Time.timeScale = 1;
            isPaused = false;
        }
        

        
        #endregion
        
        private void OnDestroy()
        {
            Time.timeScale = 1;
        }
    }
}
