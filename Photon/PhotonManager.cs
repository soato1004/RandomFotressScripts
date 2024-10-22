using System;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

namespace RandomFortress
{
    public class PhotonManager : SingletonPun<PhotonManager>
    {
        public bool IsConnected => PhotonNetwork.IsConnected;
        public bool InRoom => PhotonNetwork.InRoom;
        public string CurrentRoomName => PhotonNetwork.CurrentRoom?.Name ?? "Not in a room";
        public int PlayersInRoom => PhotonNetwork.CountOfPlayersInRooms;
        public int PlayersOnMaster => PhotonNetwork.CountOfPlayersOnMaster;
        public string NickName => PhotonNetwork.NickName;
        public int myActor => PhotonNetwork.LocalPlayer.ActorNumber;

        [SerializeField] private byte maxPlayersPerRoom = 2;

        // [SerializeField] private float eloMatchingTime = 5f; // ELO 매칭 시간 (초)
        // [SerializeField] private float roomJoinTimeout = 2f; // 방 입장 대기 시간 (초)
        [SerializeField] private float initialEloRange = 100f;
        [SerializeField] private float eloRangeIncrement = 30f;


        private bool isConnectedToMaster;
        private bool isInLobby;
        private int playerELO;
        private bool isJoinRoomTry = false; // 방 참여시도시에
        private int winner = -1; // 승리자의 actor를 로컬에서도 저장한다

        private List<RoomInfo> availableRooms = new List<RoomInfo>();

        // 네트워크 상태 관리
        public enum NetworkState
        {
            Disconnected,
            Connecting,
            Connected,
            JoiningLobby,
            InLobby,
            Matchmaking,
            InRoom,
            StartGame,
            InGame
        }

        public NetworkState currentNetworkState = NetworkState.Disconnected;
        
        private Coroutine matchmakingCoroutine;

        private void Awake()
        {
            // 포톤 설정 세팅
            ApplyDefaultSettings();
        }


        // 기본 설정
        private void ApplyDefaultSettings()
        {
            // 주석 처리된 설정들 활성화 (필요에 따라 조정)
            PhotonNetwork.SendRate = 60; // 클라이언트가 초당 서버에 전송하는 메시지 횟수 (초당 60회 전송)
            PhotonNetwork.SerializationRate = 60; // 초당 메시지를 직렬화(serialize)하는 횟수, 업데이트 빈도를 의미 (초당 60회 직렬화)
            PhotonNetwork.MaxResendsBeforeDisconnect = 5; // 서버로부터 응답을 받지 못한 경우, 연결을 끊기 전에 재전송할 최대 시도 횟수
            PhotonNetwork.QuickResends = 3; // 데이터 패킷이 손실되었을 때 재전송하는 빠른 재시도 횟수

            PhotonNetwork.GameVersion = Application.version; // 현재 게임의 버전을 설정, 같은 버전의 클라이언트끼리만 연결
            PhotonNetwork.AutomaticallySyncScene = true; // 마스터 클라이언트가 장면을 변경할 때 모든 클라이언트에 자동으로 동기화
            PhotonNetwork.LogLevel = PunLogLevel.Informational; // 로그 수준을 Informational로 설정, 중요한 정보 및 경고를 출력
            
            // 추가 설정
            PhotonNetwork.EnableCloseConnection = true; // 클라이언트가 서버와의 연결을 수동으로 종료할 수 있도록 허용
            PhotonNetwork.KeepAliveInBackground = 60; // 앱이 백그라운드로 전환된 후에도 60초 동안 네트워크 연결을 유지
            
            // 콘솔에 로그 출력 활성화
            // Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.Full);
            // PhotonNetwork.NetworkingClient.LoadBalancingPeer.DebugOut = DebugLevel.ALL;

            Debug.Log("Photon Network settings applied successfully.");
        }

        // 플레이어 커스텀 프로퍼티 설정
        private void SetPlayerCustomProperties()
        {
            playerELO = Account.I.Data.eloRating;
            var properties = new Hashtable { { "ELO", playerELO } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            PhotonNetwork.LocalPlayer.NickName = Account.I.Data.nickname; // 로컬 플레이어의 닉네임을 설정
        }

        // 매칭 시작
        public void StartMatching()
        {
            switch (currentNetworkState)
            {
                case NetworkState.Disconnected:
                    // 네트워크가 연결되어 있지 않은 경우 연결 시도
                    PhotonNetwork.ConnectUsingSettings();
                    break;
                case NetworkState.Connected:
                    // 마스터 서버에 연결된 경우 로비에 참여
                    PhotonNetwork.JoinLobby();
                    break;
                case NetworkState.InLobby:
                    // 로비에 있는 경우 매칭 프로세스 시작
                    StartMatchmakingProcess();
                    break;
                default:
                    Debug.LogWarning($"Cannot start matching in current state: {PhotonNetwork.NetworkClientState}");
                    break;
            }
        }
        
        // 마스터 서버 연결 완료 콜백
        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Master");
            currentNetworkState = NetworkState.Connected;
            isJoinRoomTry = false;
            SetPlayerCustomProperties();
            PhotonNetwork.JoinLobby();
        }

        // 로비 입장 완료 콜백
        public override void OnJoinedLobby()
        {
            Debug.Log("Joined Lobby");
            currentNetworkState = NetworkState.InLobby;
            isJoinRoomTry = false;
        }
        
        // 룸 목록 업데이트 콜백
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            Debug.Log("룸리스트 업데이트");
            
            availableRooms = roomList;

            // 룸 리스트가 업데이트되면 매치메이킹 시작
            if (currentNetworkState == NetworkState.InLobby || currentNetworkState == NetworkState.Matchmaking)
            {
                StartMatchmakingProcess();
            }
        }
        
        // 매칭 프로세스 시작
        private void StartMatchmakingProcess()
        {
            if (isJoinRoomTry)
                return;
            
            if (matchmakingCoroutine != null)
                StopCoroutine(matchmakingCoroutine);
            
            matchmakingCoroutine = StartCoroutine(MatchmakingCoroutine());
        }

        private IEnumerator MatchmakingCoroutine()
        {
            currentNetworkState = NetworkState.Matchmaking;
            isJoinRoomTry = false;
            
            // 방이 한개도없고, 방에 입장한 상태가 아니라면 바로 생성
            if (availableRooms.Count == 0 && !PhotonNetwork.InRoom)
            {
                CreateRoom();
                yield break;
            }
                
            // 참여 가능한 방에 입장 시도
            yield return StartCoroutine(TryJoinRoom());

            // 입장 실패시 방 생성
            CreateRoom();
        }

        private IEnumerator TryJoinRoom(int loopCount = 3)
        {
            for (int i = 0; i < loopCount; i++)
            {
                RoomInfo matchedRoom = FindMatchingRoom(i >= 2);
                if (matchedRoom != null)
                {
                    Debug.Log("방 입장 시작");
                    isJoinRoomTry = true;
                    PhotonNetwork.JoinRoom(matchedRoom.Name);
                    if (matchmakingCoroutine != null)
                        StopCoroutine(matchmakingCoroutine);
                }

                yield return new WaitForSeconds(0.8f);
            }
        }

        // 방 생성
        private void CreateRoom()
        {
            if (PhotonNetwork.InRoom || isJoinRoomTry)
            {
                return;
            }
            
            // 현재 날짜와 시간을 가져옵니다.
            DateTime currentTime = DateTime.Now;
            string formattedTime = currentTime.ToString("yyyy-MM-dd-HH:mm");
            
            Debug.Log("방 생성 시작");
            string roomName = $"Room_{PhotonNetwork.NickName}_{formattedTime}";
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = maxPlayersPerRoom,
                CustomRoomProperties = new Hashtable { { "ELO", playerELO }},
                CustomRoomPropertiesForLobby = new string[] { "ELO" }
            };

            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }
        
        // 매칭되는 플레이어 찾기
        private RoomInfo FindMatchingRoom(bool ignoreElo)
        {
            float currentEloRange = initialEloRange + eloRangeIncrement * 2;
        
            foreach (var room in availableRooms)
            {
                // IsOpen과 IsVisible 상태를 다시 확인
                if (!room.IsOpen || !room.IsVisible)
                {
                    continue; // 방이 닫혔거나 보이지 않게 되었다면 다음 방으로 넘어갑니다.
                }
        
                // 플레이어 수도 다시 확인 (다른 플레이어가 이미 들어갔을 수 있으므로)
                if (room.PlayerCount != 1)
                {
                    continue; // 플레이어 수가 변경되었다면 다음 방으로 넘어갑니다.
                }
        
                if (room.CustomProperties.TryGetValue("ELO", out object eloObject) && eloObject is int roomELO)
                {
                    if (ignoreElo || Mathf.Abs(playerELO - roomELO) <= currentEloRange)
                    {
                        return room;
                    }
                }
            }
            return null;
        }
        
        // 플레이어가 매칭을위하여 방을 떠날때
        private IEnumerator WaitLeaveRoom(float waitTime = 4f)
        {
            yield return new WaitForSeconds(waitTime);

            if (currentNetworkState == NetworkState.InGame ||
                currentNetworkState == NetworkState.StartGame)
                yield break;

            currentNetworkState = NetworkState.InLobby;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LeaveRoom();
        }

        // 로컬 플레이어 방 입장 완료
        public override void OnJoinedRoom()
        {
            Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
            isJoinRoomTry = false;
            currentNetworkState = NetworkState.InRoom;
            if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom)
            {
                StartGame();
            }
            else
            {
                StartCoroutine(WaitLeaveRoom(Random.Range(4f, 6f)));
            }
        }

        // 상대 플레이어 입장 콜백
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"Player Entered: {newPlayer.NickName}");

            if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom &&
                currentNetworkState == NetworkState.InRoom)
            {
                StartGame();
            }
            else
            {
                StartCoroutine(WaitLeaveRoom());
            }
        }
        
        // 로비입장 또는 로비일 경우 방목록 갱신요청
        private void JoinLobbyOrRoomListUpdate()
        {
            if (!PhotonNetwork.InLobby)
                PhotonNetwork.JoinLobby();
            else
            {
                currentNetworkState = NetworkState.InLobby;
                StartMatching();
            }
        }
        
        // 방 생성 실패 콜백
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogWarning($"Room creation failed: {message}. Attempting to join the room.");
            JoinLobbyOrRoomListUpdate();
        }

        // 방 참여 실패 콜백
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogWarning($"Failed to join room: {message}. Restarting matchmaking process.");
            isJoinRoomTry = false;
            JoinLobbyOrRoomListUpdate();
        }

        // 로컬 플레이어 퇴장 콜백
        public override void OnLeftRoom()
        {
            Debug.Log("Left Room ");
        }

        // 상대 플레이어 퇴장 콜백
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"Player Left: {otherPlayer.NickName}");
            if (MainManager.I.currentSceneName == SceneName.Game)
            {
                Debug.Log($"OnPlayerLeftRoom 플레이어 승리");
                // 상대방이 나갔으므로 내가 이겼다
                SaveWinner(myActor);
                GameManager.I.PlayerOut();
            }
            else
            {
                Debug.Log($"OnPlayerLeftRoom LeaveRoom");
                currentNetworkState = NetworkState.InLobby;
                PhotonNetwork.LeaveRoom();
            }
        }

        // 로컬 플레이어 연결 해제
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"OnDisconnected: {cause}");
            StopAllCoroutines();
        }

        // 게임 시작
        public void StartGame()
        {
            Debug.Log("StartGame " + NickName);
            StopAllCoroutines();
            currentNetworkState = NetworkState.StartGame;
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.LoadLevel("Game");
            }
        }

        // 게임씬 진입시 호출됨
        public void OnStartGame()
        {
            StopAllCoroutines();
            currentNetworkState = NetworkState.InGame;
            _ = Account.I.ConsumeStaminaOnGameStart();
        }

        // 연결 해제
        public void GameOver()
        {
            StopAllCoroutines();
            currentNetworkState = NetworkState.Disconnected;
            PhotonNetwork.Disconnect();
        }

        // 승리자 저장. 내가 승리했을경우는 즉시 로컬에 반영된다.
        public void SaveWinner(int actor)
        {
            // 로컬에 저장된 값이 없다면 이값을 지정
            if (winner == -1)
                winner = actor;
            
            if (PhotonNetwork.CurrentRoom == null)
            {
                Debug.Log("게임룸에 입장해있지 않습니다");
                return;
            }
            
            // 현재 방의 Custom Properties 가져오기
            Hashtable currentProperties = PhotonNetwork.CurrentRoom.CustomProperties;

            // "winner" 키가 존재하는지 확인
            if (!currentProperties.ContainsKey("winner") || currentProperties["winner"] == null)
            {
                // "winner" 키가 없으면 승리자 설정
                Hashtable winnerProperty = new Hashtable();
                winnerProperty.Add("winner", actor);
                
                Hashtable expectedProperties = new Hashtable();
                expectedProperties.Add("winner", null); // "winner" 값이 없는 경우에만 설정
                
                PhotonNetwork.CurrentRoom.SetCustomProperties(winnerProperty, expectedProperties);
                Debug.Log("승리자 "+actor);
            }
            else
            {
                Debug.Log("이미 승패가 결정됨");
            }
        }

        public bool IsWinner(int actor)
        {
            if (PhotonNetwork.CurrentRoom == null)
            {
                Debug.Log("게임룸에 입장해있지 않습니다");
                return false;
            }
            
            // 상대방이 설정한 승패를 받아옴
            Hashtable currentProperties = PhotonNetwork.CurrentRoom.CustomProperties;

            // 현재 승리자 actor와 주어진 actor 비교
            if (currentProperties.ContainsKey("winner"))
            {
                object winnerActor = currentProperties["winner"];
                if (winnerActor != null)
                {
                    return (int)winnerActor == actor;
                }
            }
            
            // 로컬에 저장된 값이 있다면 이값과 비교
            if (winner != -1)
            {
                return winner == actor;
            }

            Debug.Log($"Actor {actor} is not the winner.");
            return false;
        }
        
        private void DelayCall(float waitTime, Action action) => StartCoroutine(DelayCallCor(waitTime, action));

        private IEnumerator DelayCallCor(float waitTime, Action action)
        {
            yield return new WaitForSeconds(waitTime);
            action?.Invoke();
        }
        
        // 애플리케이션 종료 시 처리
        private void OnApplicationQuit()
        {
            PhotonNetwork.Disconnect();
        }
    }
}