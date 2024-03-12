using RandomFortress.Common;
using RandomFortress.Common.Utils;
using RandomFortress.Constants;
using RandomFortress.Menu;
using RandomFortress.Scene;
using UnityEngine.SceneManagement;

namespace RandomFortress.Manager
{
    /// <summary> 게임 전체에서 사용될 공통기능 매니저클래스 </summary>
    public class MainManager : Singleton<MainManager>
    {
        #region Scene 기능

        public SceneName preScene = SceneName.Bootstrap;
        public SceneName currentScene;
        public GameType gameType = GameType.Solo;


        public Lobby Lobby;
        public PageController PageController;
        public int GamePlayCount = 0;
        
        public override void Reset()
        {
            JTDebug.LogColor("MainManager Reset");
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("MainManager Terminate");
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