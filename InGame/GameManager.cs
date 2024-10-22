using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

using UnityEngine.Rendering;

namespace RandomFortress
{
    /// <summary>
    /// GameManager
    /// </summary>
    public class GameManager : SingletonPun<GameManager>
    {
        #region 변수
        
#if UNITY_EDITOR
        public float CheatDamage = 1f;
#endif
        [Header("게임상태")] 
        public GameType gameType; // 게임타입
        public Game game; // 게임메인
        public GameMode gameMode; // 게임 모드별 맵 및 UI를 가진 최상단
        public float gameTime = 0; // 현재 게임이 진행된 시간
        public bool isPaused = false; // 게임 일시정지 여부
        public float gameSpeed = 1f; // 게임 전체에 사용되는값. 배속에 관여
        public bool canTowerDrag = true; // 스킬 사용시 타워드래그를 막는다
        public TowerBase focusTower = null; // 타워 또는 무언가를 클릭시에 포커스가 갈경우
        public SerializedDictionary<int, TowerUpgrade> towerUpgradeDic = new(); // 현재타워 업그레이드정보
        public bool isWin; // 게임결과

        [Header("동기화되는정보")] 
        public bool isGameOver = false; // 게임 오버시 호출
        public Action OnGameOver;
        public int readyCount = 0; // 어빌리티카드 선택시 호출

        [Header("플레이어")] 
        public SerializedDictionary<int, GamePlayer> players = new();
        public GamePlayer otherPlayer;
        public GamePlayer myPlayer;

        [Header("기타")] 
        public float offset; // 상단 플레이어 위치로 Y값을 조정할때 사용됨. 하드코딩됨

        public Vector3 GetRoadPos(int index) => gameMode.GetRoadPos(index);
        public int GetWayLength => gameMode.GetRoadWayLength();
        public int myActor => myPlayer.ActorNumber;
        public int otherActor => otherPlayer.ActorNumber;
        public bool IsGameClear { get; private set; } // 게임클리어여부. 솔로모드는 50차클리어


        private Dictionary<int, int> playerEndTimes = new Dictionary<int, int>(); // 플레이어 종료시간 체크를 위한코드
        private float saveGameSpeed = 0; // 일시정지 후 해제시 이전의 게임배속값을 저장함
        private int maxPlayer = 0; // 현재 모드의 플레이어 수
        private int gameEndTime = 0; // 게임종료시간 저장
        public int _unitID = 0;
        public string RoomName = "";
        private bool showGameResult = false; // 게임결과창이 여러번 보여지지않게
        
        #endregion
        
        #region Setup

        private void Awake()
        {
            Reset();
        }
        
        private void Reset()
        {
            focusTower = null;
            towerUpgradeDic.Clear();
            players.Clear();
            playerEndTimes.Clear();
        }

        // 게임 시작시 한번만 호출
        public void InitializeGameManager()
        {
            // 타워 업그레이드 정보
            for (int slotIndex = 0; slotIndex < GameConstants.TOWER_DECK_COUNT; ++slotIndex)
            {
                int towerIndex = Account.I.TowerDeck(slotIndex);
                int cardLevel = 1; //Account.Instance.GetCardLevel(towerIndex);
                towerUpgradeDic.Add(towerIndex, new TowerUpgrade(towerIndex, cardLevel));
            }

            // 최대 플레이어 숫자
            maxPlayer = gameType == GameType.Solo ? 1 : 2;

            // 게임모드 설정
            gameMode.gameObject.SetActive(true);
            gameMode.Init();
        }

        // 플레이어 가져오기
        public GamePlayer GetPlayer(int actor)
        {
            if (gameType == GameType.Solo)
                return myPlayer;

            if (!players.ContainsKey(actor))
            {
                Debug.Log("Not Found Player!! " + actor);
                return myPlayer;
            }

            return players[actor];
        }

        // 플레이어 정보 설정
        public void SetupPlayer()
        {
            switch (gameType)
            {
                case GameType.Solo:
                    maxPlayer = 1;
                    myPlayer.SetPlayer(PhotonNetwork.LocalPlayer, gameMode.gameMap[0]);
                    break;
                case GameType.OneOnOne:
                    RoomName = PhotonNetwork.CurrentRoom.Name;
                    maxPlayer = 2;
                    _unitID = PhotonNetwork.IsMasterClient ? 100000000 : 200000000;
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
                    myPlayer.Userid = Account.I.UserId;
                    Debug.Log("플레이어 "+myActor+ ", UserID : " + Account.I.UserId);
                    photonView.RPC(nameof(SetUserIdRpc), RpcTarget.OthersBuffered, Account.I.UserId);
                    break;
                case GameType.BattleRoyal:
                    break;
            }
            

        }

        [PunRPC]
        private void SetUserIdRpc(string userID)
        {
            otherPlayer.Userid = userID;
        }

        #endregion

        #region Photon

        public void SelectAbilityCard(AbilityData data)
        {
            if (gameType == GameType.Solo)
            {
                myPlayer.SetAbility(data);
            }
            else
            {
                // Debug.Log("플레이어 " + myActor + "카드선택 호출");
                photonView.RPC(nameof(ChoiceAbilityRpc), RpcTarget.AllBuffered, myActor, data.index);
            }
        }

        [PunRPC]
        public void ChoiceAbilityRpc(int actor, int index)
        {
            // Debug.Log("플레이어 " + myActor + ", 카드선택 " + index);
            AbilityData abilityData = DataManager.I.abilityDataDic[index];

            GamePlayer player = GetPlayer(actor);
            player.SetAbility(abilityData);
        }

        public void GameReady()
        {
            if (gameType == GameType.Solo)
            {
                GameStart();
            }
            else
            {
                Debug.Log("플레이어 " + myActor + "게임레디 호출 ");
                photonView.RPC(nameof(ReadyRpc), RpcTarget.AllBuffered);
            }
        }

        [PunRPC]
        private void ReadyRpc()
        {
            Debug.Log("플레이어 " + myActor + "게임레디 완료. 현재룸의 인원 : " + PhotonNetwork.CurrentRoom.PlayerCount);
            readyCount++;
            if (readyCount == maxPlayer && PhotonNetwork.IsMasterClient)
            {
                Debug.Log("플레이어 " + myActor + "게임스타트 호출");
                int startTimeMillis = PhotonNetwork.ServerTimestamp + 500;
                photonView.RPC(nameof(GameStartRpc), RpcTarget.AllBuffered, startTimeMillis);
            }
        }

        [PunRPC]
        private void GameStartRpc(int startTimeMillis)
        {
            StartCoroutine(Utils.ExecuteDelayedActionCor(startTimeMillis, GameStart));
        }

        private void GameStart()
        {
            Debug.Log("플레이어 " + myActor + " 시작시간: " + DateTime.Now);
            game.isAllReady = true;
        }

        // 스테이지 클리어시 호출
        public void StageClear()
        {
            if (isGameOver) return;

            if (gameType == GameType.Solo)
            {
                DoStageClear(myActor);
            }
            else
            {
                int startTimeMillis = PhotonNetwork.ServerTimestamp + 500;
                photonView.RPC(nameof(StageClearRpc), RpcTarget.All, myActor, startTimeMillis);
            }
        }

        [PunRPC]
        private void StageClearRpc(int actor, int startTimeMillis)
        {
            StartCoroutine(Utils.ExecuteDelayedActionCor(startTimeMillis, () => { DoStageClear(actor); }));
        }

        private void DoStageClear(int actor)
        {
            Debug.Log("플레이어 " + actor + " 스테이지 클리어시간: " + DateTime.Now);
            // 플레이어 게임 클리어처리
            GamePlayer player = GetPlayer(actor);
            player.StageClear();

            // 해당 플레이어 스테이지 시작
            if (player == myPlayer)
                game.StartStage();

            // 상대가 나보다 빠르면 내 스테이지 스킵
            if (myPlayer.stageProcess < player.stageProcess)
                game.SkipStage();
        }

        private void SetGameOverState()
        {
            isGameOver = true;
            OnGameOver?.Invoke();
            OnGameOver = null;
            PauseGame();
        }

        // 게임종료시 호출
        public void EndGame()
        {
            if (isGameOver) return;
            
            SetGameOverState();
            
            if (gameType == GameType.Solo)
            {
                ShowGameResult();
            }
            else
            {
                gameEndTime = PhotonNetwork.ServerTimestamp;
                playerEndTimes[myActor] = gameEndTime;
                photonView.RPC(nameof(EndGameRpc), RpcTarget.Others, myActor, gameEndTime);
            }
        }

        [PunRPC]
        private void EndGameRpc(int actor, int endTime)
        {
            playerEndTimes[actor] = endTime;

            if (gameEndTime == 0)
            {
                SetGameOverState();
                gameEndTime = endTime + 1000; // 승자이기에 종료시간을 무조껀 상대방보다 길게설정
                playerEndTimes[myActor] = gameEndTime;
            }

            if (playerEndTimes.Count == 2)
            {
                var sortedTimes = playerEndTimes.OrderBy(kvp => kvp.Value).ToList();
                int winningActor = sortedTimes[1].Key;
                photonView.RPC(nameof(GameResultRpc), RpcTarget.All, winningActor);
            }
        }

        [PunRPC]
        private void GameResultRpc(int winningActor)
        {
            isWin = (myActor == winningActor);
            PhotonManager.I.SaveWinner(winningActor);
            ShowGameResult();
        }
        
        private void ShowGameResult()
        {
            if (showGameResult) return;
            showGameResult = true;
            
            Debug.Log("플레이어 " + myActor + " 게임결과팝업 " + DateTime.Now);
            
            foreach (var pair in players)
                pair.Value.UpdateGameResult();

            SoundManager.I.StopSound(SoundType.BGM);
            PopupManager.I.ShowPopup(PopupNames.GameResultPopup);
        }

        // 클리어시 호출됨.
        public void GameClear()
        {
            IsGameClear = true;
            PauseGame();
            
            if (gameType == GameType.Solo)
            {
                isWin = true;
                ShowGameResult();
            }
            else
            {
                PhotonManager.I.SaveWinner(myActor);
                photonView.RPC(nameof(GameClearRpc), RpcTarget.All);
            }
        }

        [PunRPC]
        private void GameClearRpc()
        {
            PauseGame();
            game.StopAllCoroutines();
            
            isWin = PhotonManager.I.IsWinner(myActor);

            ShowGameResult();
        }

        // 몬스터 소환
        public void MonsterSpawner(int actorNumber, int monsterIndex, int hp)
        {
            // Debug.Log("플레이어 " + actorNumber + " 몬스터 " + monsterIndex + " 소환요청시간: " + DateTime.Now);
            if (gameType == GameType.Solo)
            {
                DoMonsterSpawner(actorNumber, monsterIndex, hp, _unitID);
            }
            else
            {
                int startTimeMillis = PhotonNetwork.ServerTimestamp + 100;
                photonView.RPC(nameof(MonsterSpawnerRpc), RpcTarget.All, actorNumber, monsterIndex, hp, _unitID, startTimeMillis);
            }
            ++_unitID;
        }

        [PunRPC]
        private void MonsterSpawnerRpc(int actor, int monsterIndex, int hp, int unitID, int startTimeMillis)
        {
            StartCoroutine(Utils.ExecuteDelayedActionCor(startTimeMillis,
                () => { DoMonsterSpawner(actor, monsterIndex, hp, unitID); }));
        }

        private void DoMonsterSpawner(int actor, int monsterIndex, int hp, int unitID)
        {
            // Debug.Log("플레이어 " + actor + " 몬스터 " + monsterIndex + " 소환시간: " + DateTime.Now);
            GamePlayer player = GetPlayer(actor);

            Vector3 startPos = GetRoadPos(0);
            if (otherPlayer == player)
                startPos.y += offset;

            // 특수 몬스터일경우
            MonsterType type = MonsterType.None;
            if (monsterIndex > GameConstants.MonsterTypeLimit)
            {
                type = (MonsterType)((monsterIndex / 100000) * 100000);
                monsterIndex %= 100000;
            }

            switch (type)
            {
                case MonsterType.Speed: monsterIndex = (int)MonsterIndex.WolfBlue; break;
                case MonsterType.Tank: break;
            }

            GameObject monsterGo = SpawnManager.I.GetMonster(startPos, monsterIndex);
            MonsterBase monster = monsterGo.GetComponent<MonsterBase>();

            player.monsterList.Add(monster);
            monster.Init(player, monsterIndex, hp, type);
            
            player.entityDic.Add(unitID, monster);
            monster._unitID = unitID;
        }

        // 스킵스테이지
        public bool SkipStage()
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

        // 게임배속 변경
        public void ChangeGameSpeed()
        {
            if (gameType == GameType.Solo)
            {
                DoChangeGameSpeed();
            }
            else
            {
                int startTimeMillis = PhotonNetwork.ServerTimestamp + 500;
                photonView.RPC(nameof(ChangeGameSpeedRpc), RpcTarget.All, startTimeMillis);
            }
        }

        [PunRPC]
        public void ChangeGameSpeedRpc(int startTimeMillis)
        {
            StartCoroutine(Utils.ExecuteDelayedActionCor(startTimeMillis, DoChangeGameSpeed));
        }

        private void DoChangeGameSpeed()
        {
            gameSpeed = (int)gameSpeed == 1 ? 2f : 1f;
            GameUIManager.I.UpdateUI();
            Debug.Log("플레이어 " + myActor + ", 배속변경: " + gameSpeed);
        }

        public void SaveWinner() => PhotonManager.I.SaveWinner(myActor);
        
        // 플레이어 중 한명이 나갔을경우
        public void PlayerOut()
        {
            // 게임결과가 나왔을경우
            if (isGameOver) 
                return;
            
            // 백그라운드에 있을때 호출이라면 무시
            if (isInBackground)
                return;
            
            // 승패체크
            isWin = PhotonManager.I.IsWinner(myActor);
            
            isGameOver = true;
            PhotonManager.I.GameOver();

            string debufMessage = isWin ? "승리" : "패배";
            Debug.Log("플레이어 " + myActor + debufMessage + ", 시간: " + DateTime.Now);
            
            PopupManager.I.CloseAllPopup();
            GameUIManager.I.SetDim(false);
            
            ShowGameResult();
        }
        
        public void SetSkillDim(bool show, GamePlayer player)
        {
            if (gameType == GameType.Solo)
                GameUIManager.I.SetSkillDim(show);
            else
                player.SetSkillDim(show);
        }

        #endregion

        #region GamePlay
        
        // 타워 랜덤건설
        public void BuildRandomTower()
        {
            myPlayer.BuildRandomTower();
        }

        // 타워 업그레이드
        public void TowerUpgrade(int towerIndex)
        {
            TowerUpgrade upgrade = towerUpgradeDic[towerIndex];

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

            GameUIManager.I.UpdateUI();
            GameUIManager.I.UpdateUpgradeBtn();
            SoundManager.I.PlayOneShot(SoundKey.tower_upgrade);
        }

        // 현재 선택한 타워
        public void ShowFocusTower(TowerBase tower)
        {
            focusTower = tower;
            GameUIManager.I.ShowAttackRange(tower.transform.position, tower.Info.attackRange);
        }

        // 선택 타워 해제
        public void HideFocusTower()
        {
            if (focusTower != null)
            {
                focusTower.SetFocus(false, false);
                focusTower = null;
                GameUIManager.I.HideAttackRange();
            }
        }
        
        #endregion

        #region 기본기능

        // 일시정지
        public void PauseGame()
        {
            saveGameSpeed = gameSpeed != 0 ? gameSpeed : saveGameSpeed;
            gameSpeed = 0;
            isPaused = true;
            Debug.Log("일시정지 "+saveGameSpeed);
        }

        // 재개
        public void ResumeGame(bool isDefault = false)
        {
            gameSpeed = isDefault ? 1 : saveGameSpeed;
            isPaused = false;
            Debug.Log("게임재개 "+gameSpeed);
        }


        // 앱 종료시
        private void OnApplicationQuit()
        {
            PhotonNetwork.Disconnect();
        }

        #endregion

        
        #region 지연보상

        private int playerWaitTimeMs = 0; // 상대 플레이어를 기다린 시간
        private int backgroundTimeMs = 0; // 내가 백그라운드로 간 시간
        private WaitPlayerPopup waitPopup; // 대기 팝업 참조

        private bool isInBackground = false; // 현재 백그라운드 상태 여부

        private int backgroundEntryTimestamp; // 백그라운드 진입 시간

        private const int MaxBackgroundTimeMS = 15000; // 15초. 최대 허용 백그라운드 시간 (밀리초)
        private const int ReturnDelayMS = 500; // 0.5초. 게임 재개 지연 시간 (밀리초)

        // 앱이 일시 정지되거나 포커스가 변경될 때 호출되는 메서드
        // private void OnApplicationFocus(bool hasFocus)
        // {
        //     Debug.Log("Focus "+hasFocus);
        //     HandleApplicationStateChange(!hasFocus);
        // }

        // 앱이 일시 정지될 때 호출되는 메서드
        private void OnApplicationPause(bool isPause)
        { 
            Debug.Log("Pause "+isPause);
            HandleApplicationStateChange(isPause);
        }
        
#if UNITY_EDITOR
        //TODO: 테스트
        private bool testPaused = false;
        void Update()
        {

            if (Input.GetKeyDown(KeyCode.Space))
            {
                testPaused = !testPaused;
                HandleApplicationStateChange(testPaused);
            }
        }
#endif

        // 앱의 상태 변경을 처리하는 메서드
        private void HandleApplicationStateChange(bool goingToBackground)
        {
            if (isGameOver) return;
            
            if (goingToBackground == isInBackground)
            {
                // 상태가 변경되지 않았으면 아무 것도 하지 않음
                return;
            }

            isInBackground = goingToBackground;

            if (isInBackground)
            {
                GoToBackground();
            }
            else
            {
                ReturnFromBackground();
            }
        }

        // 앱이 백그라운드로 전환될 때 호출되는 메서드
        private void GoToBackground()
        {
            if (gameType == GameType.Solo)
            {
                PauseGame();
            }
            else
            {
                PauseGame();
                backgroundEntryTimestamp = PhotonNetwork.ServerTimestamp;
                photonView.RPC(nameof(PlayerLeftRpc), RpcTarget.Others);
                PhotonNetwork.SendAllOutgoingCommands(); // 즉시전송
                GameUIManager.I.SetDim(true);
                Debug.Log("플레이어 " + myActor + " 백그라운드진입: " + DateTime.Now);
            }
        }

        // 앱이 포그라운드로 복귀할 때 호출되는 메서드
        private void ReturnFromBackground()
        {
            if (gameType == GameType.Solo)
            {
                ResumeGame();
            }
            else
            {
                // 상대 플레이어가 없다면
                if (!IsOpponentConnected())
                {
                    PhotonManager.I.SaveWinner(myActor);
                    PlayerOut();
                }
                else
                {
                    int backgroundDuration = PhotonNetwork.ServerTimestamp - backgroundEntryTimestamp;
                    backgroundTimeMs += backgroundDuration;
            
                    // 플레이어의 장기간 이탈로 패배처리
                    if (backgroundTimeMs >= MaxBackgroundTimeMS)
                    {
                        PhotonManager.I.SaveWinner(otherActor);
                        PlayerOut();
                        Debug.Log("플레이어 " + myActor + " 장기간이탈" + DateTime.Now);
                    }
                    else
                    {
                        // 복귀처리
                        int startTimeMillis = PhotonNetwork.ServerTimestamp + ReturnDelayMS;
                        photonView.RPC(nameof(PlayerReturnedRpc), RpcTarget.All, startTimeMillis);
                    }
                }
            }
        }
        
        // 상대 플레이어가 연결을 해제했는지 체크
        private bool IsOpponentConnected()
        {
            if (PhotonNetwork.CurrentRoom == null)
                return false;

            if (PhotonNetwork.PlayerList.Length <= 1)
                return false;
            
            return true;
        }

        // 플레이어가 떠났을 때 호출되는 RPC 메서드
        [PunRPC]
        private void PlayerLeftRpc()
        {
            MainThreadDispatcher.RunOnMainThread(() =>
            {
                Debug.Log("플레이어 " + myActor + " 대기시작: " + DateTime.Now);
                PauseGame();
                float waitTime = playerWaitTimeMs / 1000f;
                waitPopup = PopupManager.I.ShowPopup(PopupNames.WaitPlayerPopup, waitTime) as WaitPlayerPopup;
            });
        }

        // 플레이어가 복귀했을 때 호출되는 RPC 메서드
        [PunRPC]
        private void PlayerReturnedRpc(int startTimeMillis)
        {
            Debug.Log("플레이어 " + myActor + "게임재개: " + DateTime.Now);
            myPlayer.SynchronizeOnReturn();
            StartCoroutine(Utils.ExecuteDelayedActionCor(startTimeMillis, () =>
            {
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    GameUIManager.I.SetDim(false);
                    // Debug.Log("플레이어 " + myActor + "게임재개 실제적용: " + DateTime.Now);
                    ResumeGame(true);
                    if (waitPopup != null)
                    {
                        playerWaitTimeMs = (int)(waitPopup.CombackPlayer()*1000);
                        waitPopup.ClosePopup();
                        waitPopup = null;
                    }
                });
            }));
        }
        
        #endregion
    }
}