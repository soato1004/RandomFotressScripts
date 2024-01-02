using System.Collections.Generic;
using GamePlay.GameMode;
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
    enum SceneType { Intro, Lobby, Game };

    public class TowerUpgrade
    {
        public int towerIndex = -1;
        public int towerUpgradeLV = 1;
        public int upgradeCost = 100;

        public TowerUpgrade(int towerIndex, int towerUpgradeLv, int upgradeCost)
        {
            this.towerIndex = towerIndex;
            this.towerUpgradeLV = towerUpgradeLv;
            this.upgradeCost = upgradeCost;
        }

        public void Upgrade()
        {
            ++towerUpgradeLV;
            upgradeCost = towerUpgradeLV * 100;
        }
    }
    
    public class GameManager : SingletonPun<GameManager>
    {
        [Header("게임상태")] 
        public Scene.Game game;
        public GameMode gameMode;
        public float gameTime = 0;
        public float timeScale = 1; // 게임 전체에 사용되는값. 배속에 관여
        // public bool isSkipStage = false;
        public bool isPlayGame = false;
        public bool isFocus = false;
        public TowerBase focusTower = null; // 타워 또는 무언가를 클릭시에 포커스가 갈경우
        public Dictionary<int, TowerUpgrade> towerUpgradeDic = new Dictionary<int, TowerUpgrade>();

        [Header("동기화되는정보")] 
        public bool isGameOver = false;

        [Header("플레이어")] 
        public Dictionary<int, GamePlayer> players = new Dictionary<int, GamePlayer>();
        public GamePlayer otherPlayer;
        public GamePlayer myPlayer;


        public Vector3 mainScale;
        public int GamePlayerCount;
        public bool isWin;

        public UnityEvent onStageClear;
        public UnityEvent onStageStart;

        public Vector3Int GetWayPoint(int i) => gameMode.GetRoadWayPoint(i);
        public int GetWayLength => gameMode.GetRoadWayLength();

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
            // 타워 업그레이드 정보
            for (int slotIndex = 0; slotIndex < GameConstants.SELECT_TOWER_COUNT; ++slotIndex)
            {
                int towerIndex = MainManager.Instance.account.TowerDeck(slotIndex);
                towerUpgradeDic.Add(towerIndex, new TowerUpgrade(towerIndex, 1, 100));
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
                this.isGameOver = (bool)stream.ReceiveNext();
            }
        }
        
        public void SetupPlayer()
        {
            // 플레이어 정보 초기화
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.IsLocal)
                {
                    players[p.ActorNumber] = myPlayer;
                    myPlayer.SetPlayer(p);
                }
                else
                {
                    players[p.ActorNumber] = otherPlayer;
                    otherPlayer.SetPlayer(p);
                }
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
            photonView.RPC("StageProcessRpc", RpcTarget.Others);
        }

        [PunRPC]
        public void StageProcessRpc()
        {
            otherPlayer.stageProcess++;
        }

        public void BuildRandomTower()
        {
            myPlayer.BuildRandomTower();
        }

        public void SkipWaitTime()
        {
            // isSkipStage = true;
            game.SkipStage();
        }
        
        public void GameOver()
        {
            if (isGameOver)
                return;
            
            isGameOver = true;
            photonView.RPC("GameOverRpc", RpcTarget.AllViaServer, myPlayer.actorNumber);
        }
        
        [PunRPC]
        public void GameOverRpc(int actorNumber)
        {
            isWin = myPlayer.actorNumber == actorNumber ? true : false; 
            StopAllCoroutines();
            AudioManager.Instance.StopBgm();
            GameUIManager.Instance.ShowResult();
        }

        public void SkipReward(int reward)
        {
            myPlayer.gold += reward;
        }

        public void TowerUpgrade(int towerIndex)
        {
            TowerUpgrade upgrade = towerUpgradeDic[towerIndex];
            
            // TowerInfo info = myPlayer.towerInfoDic[towerIndex];
            // int cost = info.tier * 100;

            if (myPlayer.gold < upgrade.upgradeCost)
            {
                JTDebug.Log("돈이 부족합니다");
                return;
            }

            if (upgrade.towerUpgradeLV > 5)
            {
                JTDebug.Log("최종 업그레이드");
                return;
            }
            
            myPlayer.gold -= upgrade.upgradeCost;
            
            upgrade.Upgrade();
            
            GameUIManager.Instance.UpdateInfo();
            GameUIManager.Instance.UpdateUpgradeBtn();
            
            AudioManager.Instance.PlayOneShot("DM-CGS-01");
        }

        #endregion

        #region Spawn

        public void MonsterSpawner(int actorNumber, int monsterIndex, int hp)
        {
            photonView.RPC("MonsterCreate", RpcTarget.AllViaServer,
                actorNumber, monsterIndex, hp);
        }
        
        [PunRPC]
        public void MonsterCreate(int actorNumber, int monsterIndex, int hp)
        {
            JTDebug.Log("Monster Create " + monsterIndex);
            bool isMine = myPlayer.actorNumber == actorNumber;
            GamePlayer player = isMine ? myPlayer : otherPlayer;
            
            Vector3 startPos = player.roadTilemap.GetCellCenterWorld(gameMode.GetRoadWayPoint(0));
            monsterIndex %= 12;

            GameObject monsterGo = SpawnManager.Instance.GetMonster(startPos, monsterIndex);
            MonsterBase monster = monsterGo.GetComponent<MonsterBase>();
            monster.Init(player, monsterIndex, hp);
            player.monsterList.Add(monster);
        }

        #endregion
    }
}
