using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using RandomFortress.Common;
using RandomFortress.Common.Util;
using RandomFortress.Game;
using UnityEngine;
using Random = System.Random;

namespace RandomFortress.Manager
{
    enum SceneType { Intro, Lobby, Game };

    public class GameManager : SingletonPun<GameManager>
    {
        [Header("게임상태")] 
        public Scene.Game game;

        // public int currentStage = 0;
        public float gameTime = 0;

        // public Canvas canvasUI;
        // public bool canInteract = false;
        public int TowerCost = 100;
        public bool isSkipStage = false;
        public bool isPlayGame = false;

        [Header("동기화되는정보")] public bool isGameOver = false;
        public bool isFocus = false;
        public TowerBase focusTower = null; // 타워 또는 무언가를 클릭시에 포커스가 갈경우

        [Header("플레이어")] 
        public Dictionary<int, GamePlayer> players = new Dictionary<int, GamePlayer>();
        public GamePlayer otherPlayer;
        public GamePlayer myPlayer;

        public const int GamePlayerCount = 2;
        public bool isWin;

        private Random random;

        //--------------------------------------------------------------------------------------------------------------

        #region Setup

        public override void Reset()
        {
            JTDebug.LogColor("GameManager Reset");
        }

        public override void Terminate()
        {
            JTDebug.LogColor("GameManager Terminate");
            Destroy(Instance);
        }

        public void SetupGame()
        {
            int i = 0;
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

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(focusTower);
                stream.SendNext(isGameOver);
            }
            else
            {
                // Network player, receive data
                this.focusTower = (TowerBase)stream.ReceiveNext();
                this.isGameOver = (bool)stream.ReceiveNext();
            }
        }


        #region GamePlay

        public void StageProcess()
        {
            myPlayer.stageProcess++;
            photonView.RPC("StageProcessRpc", RpcTarget.Others);
        }

        [PunRPC]
        public void StageProcessRpc()
        {
            otherPlayer.stageProcess++;
        }
        
        public int Range(int min, int max)
        {
            return min + (random.Next() % max);
        }

        public void BuildRandomTower()
        {
            myPlayer.BuildRandomTower();
        }

        public void SkipWaitTime()
        {
            isSkipStage = true;
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

        #endregion

        #region Spawn

        public void MonsterSpawner(int actorNumber, int monsterIndex, float buffHP)
        {
            photonView.RPC("MonsterCreate", RpcTarget.AllViaServer,
                actorNumber, monsterIndex, buffHP);
        }
        
        [PunRPC]
        public void MonsterCreate(int actorNumber, int monsterIndex, float buffHP)
        {
            bool isMine = myPlayer.actorNumber == actorNumber;
            GamePlayer player = isMine ? myPlayer : otherPlayer;
            
            Vector3 startPos = player.tilemap.GetCellCenterWorld(player.wayPoints[0]);
            monsterIndex %= 12;

            GameObject monsterGo = ObjectPoolManager.Instance.GetMonster(startPos, monsterIndex);
            MonsterBase monster = monsterGo.GetComponent<MonsterBase>();
            monster.Init(player, monsterIndex, buffHP);
            player.monsterList.Add(monster);
        }

        #endregion
        
        #region Coroutines
        
        private bool _isPaused = false;
        private List<MonoBehaviour> _monoBehaviours = new List<MonoBehaviour>();
        
        public bool IsPaused
        {
            get { return _isPaused; }
            set { _isPaused = value; }
        }

        public void RegisterMonoBehaviour(MonoBehaviour monoBehaviour)
        {
            if (!_monoBehaviours.Contains(monoBehaviour))
            {
                _monoBehaviours.Add(monoBehaviour);
            }
        }

        public void UnregisterMonoBehaviour(MonoBehaviour monoBehaviour)
        {
            if (_monoBehaviours.Contains(monoBehaviour))
            {
                _monoBehaviours.Remove(monoBehaviour);
            }
        }

        public void StopAllCoroutines()
        {
            foreach (var monoBehaviour in _monoBehaviours)
            {
                monoBehaviour.StopAllCoroutines();
            }
        }

        #endregion

    }
}
