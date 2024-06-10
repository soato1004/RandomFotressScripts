using System.Collections;
using GoogleAds;
using UnityEngine;
using RandomFortress.Menu;
using UnityEngine.UI;
using Application = UnityEngine.Device.Application;

namespace RandomFortress
{
    public class Lobby : MonoBehaviour
    {
        public RectTransform bottomUI;
        // public TutorialController tutorialController;
        public Button rewardAdButton; 
        
        [SerializeField] private RankBoard rankBoard;
        [SerializeField] private GameObject dim;

        private Vector3 ori;
        private float banner_height;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            // 화면이 자동으로 꺼지지 않도록 설정
            Screen.sleepTimeout = 60;
            ori = bottomUI.anchoredPosition;
            MainManager.Instance.Lobby = this;
        }

        private void Start()
        {
            // 배경음
            SoundManager.Instance.PlayBgm("bgm_lobby");
            
            // 튜토리얼 
            // if ( Account.Instance.Data.isTutorialLobby )
            // {
            //     StartCoroutine(tutorialController.StartTutorialCor());
            // }

            // 광고제거 패키지를 구입시에 배너 제거
            // if ()
            {
                ShowBanner();
            }
            
            //
            Account.Instance.InitAdDebuff();
        }
        
        public void OnPlayButtonClick(int index)
        {
            MainManager.Instance.gameType = (GameType)index;
            SoundManager.Instance.PlayOneShot("button_click");
            
            if(MainManager.Instance.ShowPlayAd)
                StartCoroutine(ShowInterstitialAdCor());
            else
                StartCoroutine(GameStartCor());
        }

        private IEnumerator GameStartCor()
        {
            // BGM끄기
            SoundManager.Instance.StopBgm();
            
            // 배너광고 닫기
            BannerClose();
            yield return new WaitForSeconds(0.1f);
            
            // 게임 모드별 실행
            switch (MainManager.Instance.gameType)
            {
                case GameType.Solo: 
                    MainManager.Instance.ChangeScene(SceneName.Game);
                    break;
                case GameType.OneOnOne: 
                case GameType.BattleRoyal: 
                    MainManager.Instance.ChangeScene(SceneName.Match);
                    break;
            }
        }

        private void OnEnable()
        {
            UpdateLobbyUI();
        }

        private void OnDisable()
        {
        }

        public void UpdateLobbyUI()
        {
            rankBoard.UpdateRankBoard();
            
            UpdateAdDebuff();
        }
        
        #region AdMob
        
        // 광고 디버프 업데이트
        public void UpdateAdDebuff()
        {
            rewardAdButton.interactable = true;
            rewardAdButton.gameObject.SetActive(true);
            
            foreach (var adDebuff in Account.Instance.Data.adDebuffList)
            {
                switch (adDebuff.type)
                {
                    case AdDebuffType.AbilityCard:
                        rewardAdButton.interactable = false;
                        rewardAdButton.gameObject.SetActive(false);
                        break;
                }
            }
        }
        
        // 배너광고 표시
        private void ShowBanner()
        {
            StartCoroutine(ShowBannerAdCor());
        }

        private IEnumerator ShowBannerAdCor()
        {
            if (GoogleAdMobController.Instance._bannerView == null)
                GoogleAdMobController.Instance.LoadBannerAd();
            
            float timer = 0;
            do
            {
                if (GoogleAdMobController.Instance._bannerView != null)
                {
                    GoogleAdMobController.Instance.ShowBannerAd();
                    banner_height = GoogleAdMobController.Instance._bannerView.GetHeightInPixels();
                    Vector3 pos = bottomUI.anchoredPosition;
                    pos.y += banner_height;
                    bottomUI.anchoredPosition = pos;
                    break;
                }

                yield return new WaitForSecondsRealtime(0.2f);
                
                timer += 0.2f;
            } while (timer < 5f);
        }

        private void BannerClose()
        {
            GoogleAdMobController.Instance.HideBannerAd();
            bottomUI.anchoredPosition = ori;
        }

        // 보상형광고 시청로직
        public void OnRewardAdClick(int index)
        {
            StartCoroutine(ShowRewardAdCor());
        }

        private IEnumerator ShowRewardAdCor()
        {
            rewardAdButton.interactable = false;

            if (!GoogleAdMobController.Instance.CanShowRewardedAd())
            {
                Debug.Log("보상형광고 리로드");
                GoogleAdMobController.Instance.LoadRewardedAd();
                yield return new WaitForSeconds(0.5f);
            }

            bool isShow = false;
            float timer = 0f;
            while (timer < 3f)
            {
                isShow = GoogleAdMobController.Instance.ShowRewardedAd(GiveReward);
                
                if (isShow) break;
                yield return new WaitForSeconds(0.2f);
                timer += 0.2f;
            }

            if (isShow)
            {
                SoundManager.Instance.PauseBgm();
                dim.SetActive(true);
            }
            else
                rewardAdButton.interactable = true;
        }

        private void GiveReward()
        {
            Debug.Log("GiveReward 접근");
            // 광고 시청완료시 보상
            Account.Instance.AddAdDebuff(AdDebuffType.AbilityCard);
            rewardAdButton.interactable = false;
            rewardAdButton.gameObject.SetActive(false);
            SoundManager.Instance.ResumeBgm();
            rewardAdButton.interactable = true;
        }

        // 최초 게임시작을 제외하고 게임시작하기전 광고를 봐야한다
        private IEnumerator ShowInterstitialAdCor()
        {
            MainManager.Instance.ShowPlayAd = false;

            if (!GoogleAdMobController.Instance.CanShowInterstitialAd())
            {
                Debug.Log("전면광고 리로드");
                GoogleAdMobController.Instance.LoadInterstitialAd();
            }

            bool isShow = false;
            
            float timer = 0f;
            while (timer < 3f)
            {
                isShow = GoogleAdMobController.Instance.ShowInterstitialAd();
                if (isShow) break;
                yield return new WaitForSeconds(0.2f);
                timer += 0.2f;
            }
            
            if (GoogleAdMobController.Instance.InterstitialAd != null)
                GoogleAdMobController.Instance.InterstitialAd.OnAdFullScreenContentClosed += HandleOnInterstitialAdClosed;

            if (isShow)
            {
                SoundManager.Instance.PauseBgm();
                dim.SetActive(true);
            }
        }

        private void HandleOnInterstitialAdClosed()
        {
            if (this != null)
            {
                dim.SetActive(false);
                StartCoroutine(GameStartCor());
            }
        }
        
        #endregion
    }
}