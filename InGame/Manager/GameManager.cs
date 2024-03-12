using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using RandomFortress.Common;
using RandomFortress.Game;
using UnityEngine;
using UnityEngine.Events;
using RandomFortress.Common.Utils;
using RandomFortress.Constants;
using RandomFortress.Data;

namespace RandomFortress.Manager
{
    public class GameManager : SingletonPun<GameManager>
    {
        [Header("게임상태")] 
        public Scene.Game game;
        public GameMode gameMode;
        public float gameTime = 0;
        public bool isPaused = false;
        public float timeScale = 1; // 게임 전체에 사용되는값. 배속에 관여

        public event Action<float> OnTimeScaleChanged;

        public bool canTowerDrag = true;
        // public bool isSkipStage = false;
        public bool isPlayGame = false;
        public bool isFocus = false;    // 타워포커스
        public TowerBase focusTower = null; // 타워 또는 무언가를 클릭시에 포커스가 갈경우
        public Dictionary<int, TowerUpgrade> TowerUpgradeDic = new Dictionary<int, TowerUpgrade>(); // 현재타워 업그레이드정보

        [Header("동기화되는정보")] 
        public bool isGameOver = false;

        [Header("플레이어")] 
        // public Dictionary<int, GamePlayer> players = new Dictionary<int, GamePlayer>();
        public GamePlayer otherPlayer;
        public GamePlayer myPlayer;


        public Vector3 mainScale;
        public int gamePlayerCount;
        public bool isWin;

        public UnityEvent onStageClear;
        public UnityEvent onStageStart;

        // public Vector3Int GetWayPoint(int i) => gameMode.GetRoadWayPoint(i);
        public Vector3 GetRoadPos(int index) => gameMode.GetRoadPos(index);
        public int GetWayLength => gameMode.GetRoadWayLength();
        // public GameMap GameMap => gameMode.gameMap;

        //--------------------------------------------------------------------------------------------------------------

        #region Setup

        // 게임 시작시 한번만 호출하는 코드를 이곳에서
        public override void Reset()
        {
            JTDebug.LogColor("GameManager Reset");
        }

        public override void Terminate()
        {
            JTDebug.LogColor("GameManager Terminate");
            Destroy(Instance);
        }

        public void InitializeGameManager()
        {
            // 게임모드
            switch (MainManager.Instance.gameType)
            {
                case GameType.Solo:
                    break;
            }
            
            // 타워 업그레이드 정보
            for (int slotIndex = 0; slotIndex < GameConstants.TOWER_DECK_COUNT; ++slotIndex)
            {
                int towerIndex = Account.Instance.TowerDeck(slotIndex);
                int cardLevel = Account.Instance.GetCardLevel(towerIndex);
                TowerUpgradeDic.Add(towerIndex, new TowerUpgrade(towerIndex, cardLevel));
            }
        }

        #endregion

        #region Photon

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(isGameOver);
            }
            else
            {
                // Network player, receive data
                isGameOver = (bool)stream.ReceiveNext();
            }
        }
        
        public void SetupPlayer()
        {
            switch (MainManager.Instance.gameType)
            {
                case GameType.Solo:
                    myPlayer.SetPlayer();
                    break;
                case GameType.OneOnOne:
                    foreach (Player p in PhotonNetwork.PlayerList)
                    {
                        if (p.IsLocal)
                        {
                            // players[p.ActorNumber] = myPlayer;
                            myPlayer.SetPlayer(p);
                        }
                        else
                        {
                            // players[p.ActorNumber] = otherPlayer;
                            otherPlayer.SetPlayer(p);
                        }
                    }
                    break;
                case GameType.BattleRoyal:
                    break;
            }
        }

        #endregion
        
        #region GamePlay

        public void StageStart()
        {
            onStageStart.Invoke();
        }
        
        public void StageClear()
        {
            onStageClear.Invoke();
            myPlayer.stageProcess++;
            if (MainManager.Instance.gameType != GameType.Solo)
                photonView.RPC("StageProcessRpc", RpcTarget.Others);
        }

        [PunRPC]
        private void StageProcessRpc()
        {
            otherPlayer.stageProcess++;
        }
        
        // 보스 클리어시 배경이 바뀐다
        public void ShowStageClearEffect()
        {
            GameUIManager.Instance.ShowStageClearEffect();
            
            // 타워생성
            if (MainManager.Instance.gameType == GameType.Solo)
                gameMode.ChangeMapStage();
            else
                photonView.RPC("ShowStageClearEffectRPC", RpcTarget.Others, myPlayer.actorNumber);
        }
        
        [PunRPC]
        public void ShowStageClearEffectRPC(int actorNumber)
        {
            JTDebug.Log("ShowStageClearEffectRPC!!");
            gameMode.ChangeMapStage();
        }

        public void BuildRandomTower()
        {
            myPlayer.BuildRandomTower();
        }

        public bool SkipWaitTime()
        {
            // 보스 스테이지는 스킵할수없다
            if (myPlayer.stageProcess % 10 == 0)
            {
                Debug.Log("보스를 잡지않으면 스킵할수없다");
                return false;
            }

            game.SkipStage();
            return true;
        }
        
        public void GameOver()
        {
            if (isGameOver)
                return;
            
            isGameOver = true;
            if (MainManager.Instance.gameType == GameType.Solo)
                GameOverRpc(myPlayer.actorNumber);
            else
                photonView.RPC("GameOverRpc", RpcTarget.AllViaServer, myPlayer.actorNumber);
        }
        
        [PunRPC]
        private void GameOverRpc(int actorNumber)
        {
            isWin = myPlayer.actorNumber == actorNumber; 
            // StopAllCoroutines();
            SoundManager.Instance.StopBgm();
            GameUIManager.Instance.ShowResult();
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
                JTDebug.Log("돈이 부족합니다");
                return;
            }

            if (upgrade.TowerUpgradeLv >= 5)
            {
                JTDebug.Log("최종 업그레이드");
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
            focusTower = null;
            GameUIManager.Instance.HideAttackRange();
        }

        public void PauseGame()
        {
            if (MainManager.Instance.gameType != GameType.Solo)
                return;

            timeScale = 0;
            Time.timeScale = 0;
            isPaused = true;
        }
        
        public void ResumeGame()
        {
            if (MainManager.Instance.gameType != GameType.Solo)
                return;

            timeScale = 1;
            Time.timeScale = 1;
            isPaused = false;
        }
        
        public void SelectAbilityCard(AbilityData abilityData)
        {
            myPlayer.SelectAbilityCard(abilityData);
        }
        
        #endregion

        #region Spawn

        public void MonsterSpawner(int actorNumber, int monsterIndex)
        {
            if (MainManager.Instance.gameType == GameType.Solo)
                MonsterCreate(actorNumber, monsterIndex);
            else
                photonView.RPC("MonsterCreate", RpcTarget.AllViaServer, actorNumber, monsterIndex);
        }
        
        [PunRPC]
        private void MonsterCreate(int actorNumber, int monsterIndex)
        {
            JTDebug.Log("Monster Create " + monsterIndex);
            bool isMine = myPlayer.actorNumber == actorNumber;
            GamePlayer player = isMine ? myPlayer : otherPlayer;

            Vector3 startPos = gameMode.GetRoadPos(0);
            
            StageDataInfo stageData = DataManager.Instance.stageData.Infos[myPlayer.stageProcess-1];

            // 특수 몬스터일경우
            MonsterType type = MonsterType.None;
            if (monsterIndex > GameConstants.MonsterTypeLimit)
            {
                type = (MonsterType)((monsterIndex / 10000)*10000);
                monsterIndex %= 10000;
            }
            
            GameObject monsterGo = SpawnManager.Instance.GetMonster(startPos, monsterIndex);
            MonsterBase monster = monsterGo.GetComponent<MonsterBase>();
            monster.Init(player, monsterIndex, stageData.hp, type);
            player.monsterList.Add(monster);
        }
        
        #endregion

        private void OnDestroy()
        {
            Time.timeScale = 1;
        }
    }
}
