using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Common;
using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
using RandomFortress.Common;
using RandomFortress.Common.Utils;

namespace GoogleAds
{
    public class GoogleAdMobController : Singleton<GoogleAdMobController>
    {
        private readonly TimeSpan APPOPEN_TIMEOUT = TimeSpan.FromHours(4);
        private DateTime appOpenExpireTime;
       
        public AppOpenAd appOpenAd { get; private set; }
        public BannerView bannerView { get; private set; }
        public InterstitialAd interstitialAd { get; private set; }
        public RewardedAd rewardedAd { get; private set; }
        // public RewardedInterstitialAd rewardedInterstitialAd { get; private set; }

        public UnityEvent onAdLoadedEvent = new UnityEvent();
        public UnityEvent onAdFailedToLoadEvent;
        public UnityEvent onAdOpeningEvent;
        public UnityEvent onAdFailedToShowEvent;
        public UnityEvent onUserEarnedRewardEvent;
        public UnityEvent onAdClosedEvent;

        
        enum AdType
        {
            Banner,                 // 배너
            Interstitial,           // 전면광고
            Rewarded,               // 보상형 광고
        }

#if UNITY_EDITOR
        private string[] adUnitIds =
        {
            "ca-app-pub-7560657077531827/5683863444", // 배너
            "ca-app-pub-3940256099942544/1033173712", // 전면광고
            "ca-app-pub-3940256099942544/5224354917", // 보상형광고
        };
#elif UNITY_ANDROID
        private string[] adUnitIds =
        {
            "ca-app-pub-3940256099942544/6300978111", // 배너
            "ca-app-pub-7560657077531827/7272128278", // 전면광고
            "ca-app-pub-7560657077531827/9958964424", // 보상형광고
        };
#endif
        
        // App Open	                ca-app-pub-3940256099942544/9257395921
        // Adaptive Banner	        ca-app-pub-3940256099942544/9214589741
        // Banner	                ca-app-pub-3940256099942544/6300978111
        // Interstitial	            ca-app-pub-3940256099942544/1033173712
        // Interstitial Video	    ca-app-pub-3940256099942544/8691691433
        // Rewarded	                ca-app-pub-3940256099942544/5224354917
        // Rewarded Interstitial	ca-app-pub-3940256099942544/5354046379
        // Native Advanced	        ca-app-pub-3940256099942544/2247696110
        // Native Advanced Video	ca-app-pub-3940256099942544/1044960115
        
        //test Android device IDs: 358074511828629
        
        
        // 테스트
        // string adUnitId = "ca-app-pub-3940256099942544/6300978111";
        
        // 리얼
        string real_adUnitId = "ca-app-pub-7560657077531827~4111703380";
        
        #region UNITY MONOBEHAVIOR METHODS
        
        public override void Reset()
        {
            JTDebug.LogColor("GoogleAdMobController Reset");
        }

        public override void Terminate()
        {
            JTDebug.LogColor("GoogleAdMobController Terminate");
        }

        public void Start()
        {
            MobileAds.SetiOSAppPauseOnBackground(true);
            MobileAds.RaiseAdEventsOnUnityMainThread = true;

            List<String> deviceIds = new List<String>() { AdRequest.TestDeviceSimulator };

            // Add some test device IDs (replace with your own device IDs).
#if UNITY_IPHONE
             deviceIds.Add("");
#elif UNITY_ANDROID
             deviceIds.Add("358074511828629");
#endif

            // Configure TagForChildDirectedTreatment and test device IDs.
            RequestConfiguration requestConfiguration =
                new RequestConfiguration.Builder()
                    .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified)
                    .SetTestDeviceIds(deviceIds).build();
            MobileAds.SetRequestConfiguration(requestConfiguration);

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(HandleInitCompleteAction);
        }

        public void Init()
        {
            RequestBannerAd();
            RequestAndLoadInterstitialAd();
            RequestAndLoadRewardedAd();
        }

        private void HandleInitCompleteAction(InitializationStatus initstatus)
        {
            Debug.Log("Initialization complete.");

            // Callbacks from GoogleMobileAds are not guaranteed to be called on
            // the main thread.
            // In this example we use MobileAdsEventExecutor to schedule these calls on
            // the next Update() loop.
            // MobileAdsEventExecutor.ExecuteInUpdate(() => { statusText.text = "Initialization complete."; });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DestroyBannerAd();
            DestroyInterstitialAd();
            DestroyRewardedAd();
        }

        #endregion

        #region HELPER METHODS

        private float adLoadTime = 0f;
        
        private AdRequest CreateAdRequest()
        {
            return new AdRequest.Builder()
                .AddKeyword("unity-admob-sample")
                .Build();
        }
        
        #endregion
        
        #region BANNER ADS
        
        public BannerView Banner => bannerView;
        // public bool IsLoadedBanner => isLoadedBanner;
        // private bool isLoadedBanner = false;
        // private float reloadInterval = 3600f;
        // private int tryLoadAdCount = 0;

        // 배너
        public void RequestBannerAd()
        {
            PrintStatus("Requesting Banner ad.");
            
            string adUnitId = adUnitIds[(int)AdType.Banner];
            
            // Clean up banner before reusing
            if (bannerView != null)
            {
                bannerView.Destroy();
            }

            // Create a 320x50 banner at top of the screen
            bannerView = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Bottom);

            // Add Event Handlers
            bannerView.OnBannerAdLoaded += () =>
            {
                // isLoadedBanner = true;
                // tryLoadAdCount = 0;
                // adLoadTime = Time.realtimeSinceStartup;
                //
                // // 1시간 간격으로 다시로드
                // InvokeRepeating("RequestBannerAd", reloadInterval, reloadInterval);
                bannerView.Hide();
                
                PrintStatus("Banner ad loaded.");
                onAdLoadedEvent?.Invoke();

            };
            bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                PrintStatus("Banner ad failed to load with error: " + error.GetMessage());
                
                // 5번 시도 후에는 다시 시도하지 않음
                // isLoadedBanner = false;
                // ++tryLoadAdCount;
                // if (tryLoadAdCount < 5)
                // {
                //     CancelInvoke("RequestBannerAd");
                //     Invoke("ReloadBannerAd", 5f);
                // }
                onAdFailedToLoadEvent?.Invoke();

            };
            bannerView.OnAdImpressionRecorded += () => { PrintStatus("Banner ad recorded an impression."); };
            bannerView.OnAdClicked += () => { PrintStatus("Banner ad recorded a click."); };
            bannerView.OnAdFullScreenContentOpened += () =>
            {
                PrintStatus("Banner ad opening.");
                onAdOpeningEvent?.Invoke();

            };
            bannerView.OnAdFullScreenContentClosed += () =>
            {
                PrintStatus("Banner ad closed.");
                onAdClosedEvent?.Invoke();

            };
            bannerView.OnAdPaid += (AdValue adValue) =>
            {
                string msg = string.Format("{0} (currency: {1}, value: {2}",
                    "Banner ad received a paid event.",
                    adValue.CurrencyCode,
                    adValue.Value);
                PrintStatus(msg);
            };

            // Load a banner ad
            bannerView.LoadAd(CreateAdRequest());
        }

        public void DestroyBannerAd()
        {
            CancelInvoke("RequestBannerAd");
            if (bannerView != null)
            {
                bannerView.Destroy();
                bannerView = null;
            }
        }

        public bool ShowBannerAd()
        {
            if (bannerView != null)
            {
                bannerView.Show();
                return true;
            }
            return false;
        }
        
        #endregion

        #region INTERSTITIAL ADS 

        public void RequestAndLoadInterstitialAd()
        {
            PrintStatus("Requesting Interstitial ad.");
            
            string adUnitId = adUnitIds[(int)AdType.Interstitial];

            // Clean up interstitial before using it
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
            }

            // Load an interstitial ad
            InterstitialAd.Load(adUnitId, CreateAdRequest(),
                (InterstitialAd ad, LoadAdError loadError) =>
                {
                    if (loadError != null)
                    {
                        PrintStatus("Interstitial ad failed to load with error: " +
                                    loadError.GetMessage());
                        return;
                    }
                    else if (ad == null)
                    {
                        PrintStatus("Interstitial ad failed to load.");
                        return;
                    }

                    PrintStatus("Interstitial ad loaded.");
                    interstitialAd = ad;

                    ad.OnAdFullScreenContentOpened += () => { PrintStatus("Interstitial ad opening."); };
                    ad.OnAdFullScreenContentClosed += () => { PrintStatus("Interstitial ad closed."); };
                    ad.OnAdImpressionRecorded += () => { PrintStatus("Interstitial ad recorded an impression."); };
                    ad.OnAdClicked += () => { PrintStatus("Interstitial ad recorded a click."); };
                    ad.OnAdFullScreenContentFailed += (AdError error) => { PrintStatus("Interstitial ad failed to show with error: " + error.GetMessage()); };
                    ad.OnAdPaid += (AdValue adValue) =>
                    {
                        string msg = string.Format("{0} (currency: {1}, value: {2}",
                            "Interstitial ad received a paid event.",
                            adValue.CurrencyCode,
                            adValue.Value);
                        PrintStatus(msg);
                    };
                });
        }

        public bool ShowInterstitialAd()
        {
            if (interstitialAd != null && interstitialAd.CanShowAd())
            {
                interstitialAd.Show();
                return true;
            }
            else
            {
                PrintStatus("Interstitial ad is not ready yet.");
            }
            return false;
        }

        public void DestroyInterstitialAd()
        {
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }
        }

        #endregion

        #region REWARDED ADS
        
        private bool isLoadedReward = false;
        public bool IsLoadedReward => isLoadedReward;
        
        
        public void RequestAndLoadRewardedAd()
        {
            PrintStatus("Requesting Rewarded ad.");
            
            string adUnitId = adUnitIds[(int)AdType.Rewarded];

            // create new rewarded ad instance
            RewardedAd.Load(adUnitId, CreateAdRequest(),
                (RewardedAd ad, LoadAdError loadError) =>
                {
                    isLoadedReward = true;
                    
                    if (loadError != null)
                    {
                        PrintStatus("Rewarded ad failed to load with error: " +
                                    loadError.GetMessage());
                        return;
                    }
                    else if (ad == null)
                    {
                        PrintStatus("Rewarded ad failed to load.");
                        return;
                    }

                    PrintStatus("Rewarded ad loaded.");
                    rewardedAd = ad;

                    ad.OnAdFullScreenContentOpened += () => { PrintStatus("Rewarded ad opening."); };
                    ad.OnAdFullScreenContentClosed += () => { PrintStatus("Rewarded ad closed."); };
                    ad.OnAdImpressionRecorded += () => { PrintStatus("Rewarded ad recorded an impression."); };
                    ad.OnAdClicked += () => { PrintStatus("Rewarded ad recorded a click."); };
                    ad.OnAdFullScreenContentFailed += (AdError error) => { PrintStatus("Rewarded ad failed to show with error: " + error.GetMessage()); };
                    ad.OnAdPaid += (AdValue adValue) =>
                    {
                        string msg = string.Format("{0} (currency: {1}, value: {2}",
                            "Rewarded ad received a paid event.",
                            adValue.CurrencyCode,
                            adValue.Value);
                        PrintStatus(msg);
                    };
                });
        }

        public bool ShowRewardedAd(Action action)
        {
            const string rewardMsg =
                "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";
            
            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                rewardedAd.Show((Reward reward) =>
                {
                    action?.Invoke();
                });
                // rewardedAd.Show((Reward reward) =>
                // {
                //     // TODO: Reward the user.
                //     Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                // });
                return true;
            }
            else
            {
                PrintStatus("Rewarded ad is not ready yet.");
                return false;
            }
        }

        public void DestroyRewardedAd()
        {
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Loads the Google Ump sample scene.
        /// </summary>
        public void LoadUmpScene()
        {
            SceneManager.LoadScene("GoogleUmpScene");
        }

        ///<summary>
        /// Log the message and update the status text on the main thread.
        ///<summary>
        private void PrintStatus(string message)
        {
            Debug.Log(message);
            // MobileAdsEventExecutor.ExecuteInUpdate(() => { statusText.text = message; });
        }

        #endregion
    }
}