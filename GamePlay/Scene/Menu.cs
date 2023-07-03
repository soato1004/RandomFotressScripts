using GoogleAds;
using RandomFortress.Common.Util;
using RandomFortress.Game.Photon;
using RandomFortress.Manager;
using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace RandomFortress.Scene
{
    public class Menu : MainBase
    {
        public RectTransform bottomUI;
        public Launcher launcher;

        private float banner_height;

        private void Start()
        {
            AudioManager.Instance.PlayBgm("Casual Menu GUITAR LOOP");
        }
        
        public void OnPlayButtonClick()
        {
            AudioManager.Instance.PlayOneShot("ui_menu_button_click_01");
            MainManager.Instance.ChangeScene(SceneName.Match);
        }

        public void OnExitButtonClick()
        {
            Application.Quit();
        }

        #region Banner Ad

        // private void OnEnable()
        // {
        //     if (GoogleAdMobController.Instance.Banner == null)
        //     {
        //         GoogleAdMobController.Instance.RequestBannerAd();
        //         GoogleAdMobController.Instance.OnAdLoadedEvent.AddListener(BannerOpen);
        //         GoogleAdMobController.Instance.OnAdClosedEvent.AddListener(BannerClose);
        //         GoogleAdMobController.Instance.Banner.Show();
        //     }
        // }
        //
        // private void OnDisable()
        // {
        //     if (GoogleAdMobController.Instance.Banner == null)
        //     {
        //         GoogleAdMobController.Instance.Banner.Hide();
        //         GoogleAdMobController.Instance.OnAdLoadedEvent.RemoveListener(BannerOpen);
        //         GoogleAdMobController.Instance.OnAdClosedEvent.RemoveListener(BannerClose);
        //     }
        // }
        //
        // private void BannerOpen()
        // {
        //     banner_height = GoogleAdMobController.Instance.Banner.GetHeightInPixels();
        //     Vector3 p = bottomUI.anchoredPosition;
        //     p.y += banner_height*2;
        //     bottomUI.anchoredPosition = p;
        //     JTDebug.LogColor("Banner Pos Move!!!!");
        // }
        //
        // private void BannerClose()
        // {
        //     Vector3 p = bottomUI.anchoredPosition;
        //     p.y -= banner_height*2;
        //     bottomUI.anchoredPosition = p;
        //     JTDebug.LogColor("Banner Pos Reset!!!!");
        // }

        #endregion
    }
}