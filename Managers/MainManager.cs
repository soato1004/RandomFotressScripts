using RandomFortress.Menu;
using UnityEngine.SceneManagement;

namespace RandomFortress
{
    /// <summary> 게임 전체에서 사용될 공통기능 매니저클래스 </summary>
    public class MainManager : Singleton<MainManager>
    {
        #region Scene 기능

        private SceneName preScene = SceneName.Bootstrap;
        private SceneName currentScene;
        public SceneName CurrentScene => currentScene;
        public GameType gameType = GameType.Solo;


        public Lobby Lobby;
        public PageController PageController;
        public bool ShowPlayAd = false; // 매 게임을 진행후 광고재생
        
        public override void Reset()
        {
            JustDebug.LogColor("MainManager Reset");
        }

        public void ChangeScene(SceneName name)
        {
            preScene = currentScene;
            currentScene = name;
            SceneManager.LoadScene(name.ToString());
        }

        #endregion

        public void ChangePlage(PageController.PageType pageType)
        {
            if (PageController !=null) 
                PageController.OnPageButtonClick((int)pageType);
        }

        public void SetAdDebuff(AdDebuffType type, float time)
        {
            PageController.GetBattlePage.SetAdDebuff(type, time);
        }
        
        public void UpdateAdDebuff()
        {
            Lobby.UpdateAdDebuff();
        }
    }
}