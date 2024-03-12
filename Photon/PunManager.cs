using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using RandomFortress.Common;
using RandomFortress.Common.Utils;
using RandomFortress.Constants;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game.Netcode
{
    public class PunManager : SingletonPun<PunManager>
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
            JTDebug.LogColor("PunManager Reset");
            
            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.LogLevel = PunLogLevel.Informational;
            PhotonNetwork.LocalPlayer.NickName = "Player " + Random.Range(1, 1000);
            PhotonNetwork.ConnectUsingSettings();
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("PunManager Terminate");
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

        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom");
            
            // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                // #Critical
                // Load the Room Level.
                // PhotonNetwork.LoadLevel("Game");
                CheckStartGame();
            }
        }

        #endregion
        
        #region Photon Callbacks

        public override void OnPlayerEnteredRoom(global::Photon.Realtime.Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

                CheckStartGame();
            }
        }

        public override void OnPlayerLeftRoom(global::Photon.Realtime.Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

                CheckStartGame();
            }
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

        void CheckStartGame()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
                return;
            }
            Debug.LogFormat("PUN Current Room Players : {0}", PhotonNetwork.CurrentRoom.PlayerCount);

            // if (!isStart)
            // {
            //     isStart = true;
            //     StartCoroutine(WaitPlayerAndStartGameCor());                
            // }

            // if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            // {
            //     PhotonNetwork.LoadLevel(SceneName.Game.ToString());
            //     // MainManager.Instance.ChangeScene(SceneName.Game);   
            // }
            
            PhotonNetwork.LoadLevel(SceneName.Game.ToString());
        }

        private bool isStart = false;
        
        public const float delayTime = 3f;
        
        public float waitTime = 0f;
        
        
        IEnumerator WaitPlayerAndStartGameCor()
        {
            while (waitTime < delayTime)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }
            
            PhotonNetwork.LoadLevel(SceneName.Game.ToString());
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
            isStart = false;
            waitTime = 0;
            PhotonNetwork.LeaveRoom();
        }

    #endregion
    }
}