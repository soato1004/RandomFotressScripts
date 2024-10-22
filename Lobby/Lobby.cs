using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GoogleAds;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RandomFortress
{
    public class Lobby : SceneBase
    {
        [SerializeField] private RectTransform bottomUI;
        // public TutorialController tutorialController;
        [SerializeField] private PageController pageController;
        [SerializeField] private TopUI topUI;
        [SerializeField] private RankBoard rankBoard;
        [SerializeField] private Image dim;

        private Vector3 bottomUIanchoredPosOri;
        private BasePage CurrentPage => pageController.GetBattlePage;

        protected override void Awake()
        {
            base.Awake();
            bottomUIanchoredPosOri = bottomUI.anchoredPosition;
        }

        public override void StartScene()
        {
            // 광고제거 구입여부 확인
            if (!Account.I.Data.hasSuperPass)
            {
                // 배너 생성
                InitializeBannerAd();
                
                // 어빌리티 보상 디버프
                Account.I.InitAdDebuff();
            }
            
            // 배경음
            SoundManager.I.PlayBgm(SoundKey.bgm_lobby);
            
            //TODO: 튜토리얼 
            // if ( Account.Instance.Data.isTutorialLobby )
            // {
            //     StartCoroutine(tutorialController.StartTutorialCor());
            // }
            
            // 업데이트 로비 UI
            UpdateLobbyUI();
        }
        
        // 로비 UI 업데이트
        public void UpdateLobbyUI()
        {
            CurrentPage.UpdateUI();
            rankBoard.UpdateRankBoard();
            topUI.UpdateUI();
        }
        
        // 광고버프 적용
        public void SetAdDebuff(AdRewardType type, float waitTime)
        {
            pageController.GetBattlePage.SetAdDebuff(type, waitTime);
        }
        
        // 게임 시작버튼 클릭
        public void OnPlayButtonClick(int index)
        {
            SoundManager.I.PlayOneShot(SoundKey.button_click);

            int needStamina = (GameType)index == GameType.Solo ? 1 : 2;
            
            if (Account.I.Data.stamina >= needStamina)
            {
                MainManager.I.gameType = (GameType)index;
                Debug.Log("게임모드 : "+(GameType)index);
                GameStartCor();
            }
            else
            {
                UnityAction action = OnStaminaRewardAdClick;
                PopupManager.I.ShowPopup(PopupNames.SteminaPopup, action);
            }
        }

        // 게임 시작 로직
        private void GameStartCor()
        {
            dim.gameObject.SetActive(true);
            dim.DOFade(0.6f, 0.2f);
            
            // BGM끄기
            SoundManager.I.StopSound(SoundType.BGM);
            
            // 배너광고 끄기
            BannerHide();
            
            // 게임 모드별 실행
            switch (MainManager.I.gameType)
            {
                case GameType.Solo: 
                    // 스테미너 사용
                    _ = Account.I.ConsumeStaminaOnGameStart();
                    MainManager.I.ChangeScene(SceneName.Game); 
                    break;
                case GameType.OneOnOne: 
                case GameType.BattleRoyal: 
                    MainManager.I.ChangeScene(SceneName.Match); 
                    break;
            }
        }

        // 슈퍼패스 구입직후 보여지는 팝업
        public void ShowSuperPassEffect()
        {
            Utils.SetDim(dim, true);
            BannerHide();
            UpdateLobbyUI();
            UnityAction action = OnSuperPassEffButtonClick;
            SoundManager.I.StopSound(SoundType.BGM);
            SoundManager.I.PlayOneShot(SoundKey.superpass_buy);
            PopupManager.I.ShowPopup(PopupNames.SuperPassEffectPopup, action);
        }
        
        public void OnSuperPassEffButtonClick()
        {
            Utils.SetDim(dim, false);
            SoundManager.I.PlaySound(SoundKey.bgm_lobby, SoundType.BGM, 0.3f);
            PopupManager.I.CloseAllPopup();
        }
        
        #region AdMob 구글 광고 관련
        
        // 배너 광고 초기화
        private void InitializeBannerAd()
        {
            if (GoogleAdManager.I.bannerView == null)
            {
                GoogleAdManager.I.LoadBannerAd(AdjustBottomUIForBanner);
            }
            else
            {
                GoogleAdManager.I.ShowBannerAd();
                AdjustBottomUIForBanner();
            }
        }

        private void BannerHide()
        {
            GoogleAdManager.I.HideBannerAd();
            MainThreadDispatcher.RunOnMainThread(() =>
            {
                bottomUI.anchoredPosition = bottomUIanchoredPosOri;
            });
        }
        
        // 하단 UI 조정
        private void AdjustBottomUIForBanner()
        {
            MainThreadDispatcher.RunOnMainThread(() =>
            {
                float bannerHeight = GoogleAdManager.I.bannerView.GetHeightInPixels();
                Vector3 pos = bottomUI.anchoredPosition;
                pos.y += bannerHeight;
                bottomUI.anchoredPosition = pos;
            });
        }
        
        private GoogleAdManager.AdType currentAdType = GoogleAdManager.AdType.None;

        private async void ShowRewardAd(GoogleAdManager.AdType adType)
        {
            currentAdType = adType;
            GoogleAdManager.I.LoadRewardedAd(adType, OnAdClosed);
            await ShowRewardAdAsync();
        }

        private async System.Threading.Tasks.Task ShowRewardAdAsync()
        {
            const float timeout = 2f;
            const float retryInterval = 0.2f;
            float timer = 0f;

            while (timer < timeout)
            {
                if (GoogleAdManager.I.ShowRewardedAd(OnRewardEarned)) 
                {
                    await PrepareAdEnvironment();
                    return;
                }

                await System.Threading.Tasks.Task.Delay((int)(retryInterval * 1000));
                timer += retryInterval;
            }

            Debug.LogWarning("Failed to show rewarded ad within the timeout period.");
        }

        private async System.Threading.Tasks.Task PrepareAdEnvironment()
        {
            SoundManager.I.PauseSound();
            dim.gameObject.SetActive(true);
            await dim.DOFade(0.6f, 0.2f).AsyncWaitForCompletion();
        }

        private void OnRewardEarned(Reward reward)
        {
            GiveReward(reward);
        }

        private async void GiveReward(Reward reward)
        {
            try
            {
                await Account.I.RecordAdView((int)currentAdType);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error giving reward: {ex.Message}");
            }
            finally
            {
                OnAdClosed();
            }
        }

        private async void OnAdClosed()
        {
            SoundManager.I.ResumeSound();
            await dim.DOFade(0, 0.2f).AsyncWaitForCompletion();
            dim.gameObject.SetActive(false);
        }

        public void OnStaminaRewardAdClick()
        {
            if (!Account.I.IsFullStamina)
                ShowRewardAd(GoogleAdManager.AdType.StaminaReward);
        } 

        public void OnAbilityRewardAdClick()
        {
            ShowRewardAd(GoogleAdManager.AdType.AbilityReward);
        } 
        
        #endregion
    }
}