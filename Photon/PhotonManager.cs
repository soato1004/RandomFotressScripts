using Photon.Pun;
using Photon.Realtime;




using UnityEngine;

namespace RandomFortress
{
    public class PhotonManager : SingletonPun<PhotonManager>
    {
        #region Private Serializable Fields

        /// <summary>
        /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
        /// </summary>
        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        [SerializeField]
        private byte maxPlayersPerRoom = 2;
        
        #endregion

        #region Private Fields

        /// <summary>
        /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
        /// </summary>
        string gameVersion = "1";
        
        /// <summary>
        /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
        /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
        /// Typically this is used for the OnConnectedToMaster() callback.
        /// </summary>
        bool isConnecting;

        #endregion
        
        public override void Reset()
        {
            JustDebug.LogColor("PunManager Reset");

            PhotonNetwork.GameVersion = this.gameVersion;
            PhotonNetwork.SendRate = 60;
            PhotonNetwork.SerializationRate = 60;
            
            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.LogLevel = PunLogLevel.Informational;
            PhotonNetwork.LocalPlayer.NickName = "Player " + Random.Range(1, 1000);
            PhotonNetwork.ConnectUsingSettings();
        }

        #region MonoBehaviour CallBacks

        #endregion
        
        #region MonoBehaviourPunCallbacks Callbacks

        public override void OnConnectedToMaster()
        {
            Debug.Log("OnConnectedToMaster");
            
            // we don't want to do anything if we are not attempting to join a room.
            // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
            // we don't want to do anything.
            if (isConnecting)
            {
                // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("OnDisconnected : {0}", cause);
        }
        
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("OnJoinRandomFailed");

            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
        }

        #endregion
        
        #region Photon Callbacks

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", newPlayer.NickName); // 로그 기록

            bool isGameInProgress = MainManager.Instance.CurrentScene == SceneName.Game;
            
            // 새로 들어온 플레이어가 게임에 재참여하는 경우
            if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom && isGameInProgress)
            {
                Debug.Log("Player reconnected: Reinitializing game state...");
                // 게임 상태 복원 로직, 예를 들어 점수나 게임 진행 상태 등
                ReinitializeGameStateFor(newPlayer);
            }
            else
            {
                // 플레이어가 새로 참여한 경우 게임 시작 조건 검사
                CheckStartGame();
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0} has left the room", otherPlayer.NickName);

            // 플레이어가 게임을 떠난 경우, 게임 일시 중지 및 기타 플레이어에게 알림
            if (PhotonNetwork.CurrentRoom.PlayerCount < maxPlayersPerRoom)
            {
                Debug.Log("Player disconnected: Pausing game...");
                // 게임 일시 중지 로직
                PauseGame();
            }
        }

        private void ReinitializeGameStateFor(Player player)
        {
            // 특정 플레이어에게 게임의 현재 상태를 전송하거나 재설정하는 로직 구현
            // 이 메소드는 게임의 현재 상태를 새 플레이어나 재연결된 플레이어에게 동기화합니다.
            GameManager.Instance.ComeBackPlayer();
        }

        private void PauseGame()
        {
            if (MainManager.Instance.CurrentScene == SceneName.Game)
            {
                // 게임 일시 중지 로직 구현
                // 모든 클라이언트에서 게임이 일시 중지되었다는 UI를 표시하거나, 게임 타이머를 멈추는 등의 조치를 취할 수 있습니다.
                GameManager.Instance.PlayerOut();
            }
        }
        
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // 앱이 백그라운드로 전환됐을 때
                Debug.Log("Application is paused. Pausing game...");
                PauseGame();
            }
            else
            {
                if (MainManager.Instance.CurrentScene == SceneName.Game)
                {
                    GameManager.Instance.ComeBackPlayer();
                }
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // 앱이 포커스를 잃었을 때
                Debug.Log("Application lost focus. Pausing game...");
                PauseGame();
            }
            else
            {
                if (MainManager.Instance.CurrentScene == SceneName.Game)
                {
                    GameManager.Instance.ComeBackPlayer();
                }
            }
        }

        // void CheckAndPauseGame()
        // {
        //     if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount < maxPlayersPerRoom)
        //     {
        //         PauseGame();
        //     }
        // }

        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom");

            // 방에 들어왔을 때 게임 시작 조건을 검사
            CheckStartGame();
        }

        public override void OnMasterClientSwitched(global::Photon.Realtime.Player newMasterClient)
        {
            Debug.LogFormat("OnMasterClientSwitched. New master client is {0}", newMasterClient.NickName);

            // 마스터 클라이언트가 변경되었을 때 게임 시작 조건을 검사
            CheckStartGame();
        }
        
        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
            if (MainManager.Instance != null)
                MainManager.Instance.ChangeScene(SceneName.Lobby);
        }

        #endregion
        
        #region Private Methods
        
        private void CheckStartGame()
        {
            // 모든 플레이어가 준비되었는지 확인하고 게임을 시작하는 로직 구현
            // 예를 들어, 방에 필요한 최소 인원이 충족되었는지 등을 확인
            if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
            {
                // 게임 시작 로직
                PhotonNetwork.LoadLevel(SceneName.Game.ToString());
            }
            // else
            // {
            //     Debug.Log("Waiting for more players...");
            // }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the connection process.
        /// - If already connected, we attempt joining a random room
        /// - if not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect()
        {
            // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
            isConnecting = true;
            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.IsConnected)
            {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
                PhotonNetwork.JoinRandomOrCreateRoom();
            }
            else
            {
                // #Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        
        public void LeaveRoom()
        {
            isConnecting = false;
            PhotonNetwork.LeaveRoom();
        }

    #endregion
    }
}