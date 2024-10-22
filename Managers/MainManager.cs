using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RandomFortress
{
    /// <summary> 게임 전체에서 사용될 공통기능 매니저클래스 </summary>
    public class MainManager : Singleton<MainManager>
    {
        // 게임 버전
        public string GameVersion { get; private set; }
        
        public SceneName preSceneName { get; private set; }
        public SceneName currentSceneName { get; private set; }
        public GameType gameType = GameType.Solo;
        public SceneBase currentScene;
        
        public Lobby lobby => currentScene as Lobby;
        public Game game => currentScene as Game;
        public Match match => currentScene as Match;
        public Bootstrap bootstrap => currentScene as Bootstrap;

        void Awake()
        {
            GameVersion = Application.version;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            currentSceneName = Enum.Parse<SceneName>(scene.name);
            
            switch (currentSceneName)
            {
                case SceneName.Bootstrap: break;
                case SceneName.Lobby: PopupManager.I.CloseAllPopup(); break;
                case SceneName.Match: break;
                case SceneName.Game: if (gameType != GameType.Solo) PhotonManager.I.OnStartGame(); break;
            }

            currentScene.StartScene();
        }
        
        
        // Scene 전환
        public void ChangeScene(SceneName name)
        {
            preSceneName = currentSceneName;
            SceneManager.LoadScene((int)name, LoadSceneMode.Single);
        }

        // 광고버프 적용
        public void SetAdDebuff(AdRewardType type, float waitTime)
        {
            lobby?.SetAdDebuff(type, waitTime);
        }

        // 로비 UI 업데이트
        public void UpdateLobbyUI()
        {
            MainThreadDispatcher.RunOnMainThread(() =>
            {
                if (currentSceneName == SceneName.Lobby)   
                    lobby?.UpdateLobbyUI();
            });
        }
    }
}