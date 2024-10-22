using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using RandomFortress;
using UnityEngine;
using UnityEngine.Events;
#pragma warning disable CS0618
namespace GoogleAds
{
    public class GoogleAdManager : Singleton<GoogleAdManager>
    {
        public enum AdType
        {
            Banner = 0,         // 배너
            Interstitial = 1,   // 전면광고
            AbilityReward = 2,  // 보상형 광고
            StaminaReward = 3,  // 스테미너
            None
        }
        
#if UNITY_ANDROID
        private string[] adUnitIds =
        {
            "ca-app-pub-7560657077531827/8417023328", // 배너
            "ca-app-pub-7560657077531827/6155290672", // 전면광고
            "ca-app-pub-7560657077531827/4790238114", // 어빌리티 카드 보상 보상형광고
            "ca-app-pub-7560657077531827/3769430713", // 스테미너 지급
        };
#else
        private string[] adUnitIds =
        {
            "ca-app-pub-7560657077531827/5683863444", // 배너
            "ca-app-pub-3940256099942544/1033173712", // 전면광고
            "ca-app-pub-3940256099942544/5224354917", // 보상형광고
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
        
        // Always use test ads.
        internal static List<string> TestDeviceIds = new List<string>()
        {
            AdRequest.TestDeviceSimulator,
#if UNITY_IPHONE
            "",
#elif UNITY_ANDROID
            "358074511828629"
#endif
        };
        
        // The Google Mobile Ads Unity plugin needs to be run only once.
        private static bool? _isInitialized;
        
        // Helper class that implements consent using the
        // Google User Messaging Platform (UMP) Unity plugin.
        // [SerializeField, Tooltip("Controller for the Google User Messaging Platform (UMP) Unity plugin.")]
        // private GoogleMobileAdsConsentController _consentController;
        
        #region UNITY MONOBEHAVIOR METHODS

        /// <summary>
        /// Demonstrates how to configure Google Mobile Ads Unity plugin.
        /// </summary>
        private void Start()
        {
            // On Android, Unity is paused when displaying interstitial or rewarded video.
            // This setting makes iOS behave consistently with Android.
            MobileAds.SetiOSAppPauseOnBackground(true);

            // When true all events raised by GoogleMobileAds will be raised
            // on the Unity main thread. The default value is false.
            // https://developers.google.com/admob/unity/quick-start#raise_ad_events_on_the_unity_main_thread
            // MobileAds.RaiseAdEventsOnUnityMainThread = true;

            // Configure your RequestConfiguration with Child Directed Treatment
            // and the Test Device Ids.
            MobileAds.SetRequestConfiguration(new RequestConfiguration
            {
                TestDeviceIds = TestDeviceIds
            });
        }
        
        #endregion
        
        #region BANNER ADS
        
        public BannerView bannerView;

        public void CreateBannerView(UnityAction action)
        {
            // Debug.Log("Creating banner view.");

            // If we already have a banner, destroy the old one.
            if(bannerView != null)
            {
                DestroyBannerAd();
            }

            // Create a 320x50 banner at top of the screen.
            bannerView = new BannerView(adUnitIds[(int)AdType.Banner], AdSize.SmartBanner, AdPosition.Bottom);

            // Listen to events the banner may raise.
            BannerListenToAdEvents(action);

            // Debug.Log("Banner view created.");
        }

        /// <summary>
        /// Creates the banner view and loads a banner ad.
        /// </summary>
        public void LoadBannerAd(UnityAction action)
        {
            // Create an instance of a banner view first.
            if(bannerView == null)
            {
                CreateBannerView(action);
            }

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // Send the request to load the ad.
            // Debug.Log("Loading banner ad.");
            bannerView.LoadAd(adRequest);
        }

        /// <summary>
        /// Shows the ad.
        /// </summary>
        public void ShowBannerAd()
        {
            if (bannerView != null)
            {
                Debug.Log("Showing banner view.");
                bannerView.Show();
            }
        }

        /// <summary>
        /// Hides the ad.
        /// </summary>
        public void HideBannerAd()
        {
            if (bannerView != null)
            {
                Debug.Log("Hiding banner view.");
                bannerView.Hide();
            }
        }

        /// <summary>
        /// Destroys the ad.
        /// When you are finished with a BannerView, make sure to call
        /// the Destroy() method before dropping your reference to it.
        /// </summary>
        public void DestroyBannerAd()
        {
            if (bannerView != null)
            {
                Debug.Log("Destroying banner view.");
                bannerView.Destroy();
                bannerView = null;
            }
        }

        /// <summary>
        /// Listen to events the banner may raise.
        /// </summary>
        private void BannerListenToAdEvents(UnityAction action)
        {
            // Raised when an ad is loaded into the banner view.
            bannerView.OnBannerAdLoaded += () =>
            {
                action?.Invoke();
                Debug.Log("Banner view loaded an ad with response : " + bannerView.GetResponseInfo());
            };
            // Raised when an ad fails to load into the banner view.
            bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.LogError("Banner view failed to load an ad with error : " + error);
            };
            // Raised when the ad is estimated to have earned money.
            bannerView.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Banner view paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            bannerView.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Banner view recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            bannerView.OnAdClicked += () =>
            {
                Debug.Log("Banner view was clicked.");
            };
            // Raised when an ad opened full screen content.
            bannerView.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Banner view full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            bannerView.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Banner view full screen content closed.");
            };
        }
        
        #endregion

        #region INTERSTITIAL ADS 

        private InterstitialAd _interstitialAd;
        public InterstitialAd InterstitialAd => _interstitialAd;
        
        /// <summary>
        /// Loads the ad.
        /// </summary>
        public void LoadInterstitialAd()
        {
            // Clean up the old ad before loading a new one.
            if (_interstitialAd != null)
            {
                DestroyInterstitialAd();
            }

            Debug.Log("Loading interstitial ad.");

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // Send the request to load the ad.
            InterstitialAd.Load(adUnitIds[(int)AdType.Interstitial], adRequest, (InterstitialAd ad, LoadAdError error) =>
            {
                // If the operation failed with a reason.
                if (error != null)
                {
                    Debug.LogError("Interstitial ad failed to load an ad with error : " + error);
                    return;
                }
                // If the operation failed for unknown reasons.
                // This is an unexpected error, please report this bug if it happens.
                if (ad == null)
                {
                    Debug.LogError("Unexpected error: Interstitial load event fired with null ad and null error.");
                    return;
                }

                // The operation completed successfully.
                Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());
                _interstitialAd = ad;

                // Register to ad events to extend functionality.
                InterstitialAdRegisterEventHandlers(ad);
            });
        }

        /// <summary>
        /// Shows the ad.
        /// </summary>
        public bool ShowInterstitialAd()
        {
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                Debug.Log("Showing interstitial ad.");
                _interstitialAd.Show();
                return true;
            }
            else
            {
                Debug.LogError("Interstitial ad is not ready yet.");
                return false;
            }
        }

        public bool CanShowInterstitialAd()
        {
            return _interstitialAd != null && _interstitialAd.CanShowAd();
        }

        /// <summary>
        /// Destroys the ad.
        /// </summary>
        public void DestroyInterstitialAd()
        {
            if (_interstitialAd != null)
            {
                Debug.Log("Destroying interstitial ad.");
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }
        }

        private void InterstitialAdRegisterEventHandlers(InterstitialAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Interstitial ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Interstitial ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial ad full screen content closed.");
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content with error : "
                    + error);
            };
        }

        #endregion

        #region REWARDED ADS
        
        public RewardedAd rewardedAd;
        
        /// <summary>
        /// Loads the ad.
        /// </summary>
        public void LoadRewardedAd(AdType type, UnityAction action)
        {
            // Clean up the old ad before loading a new one.
            if (rewardedAd != null)
            {
                DestroyRewardedAd();
            }

            Debug.Log("Loading rewarded ad.");

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // Send the request to load the ad.
            RewardedAd.Load(adUnitIds[(int)type], adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                // If the operation failed with a reason.
                if (error != null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
                    return;
                }
                // If the operation failed for unknown reasons.
                // This is an unexpected error, please report this bug if it happens.
                if (ad == null)
                {
                    Debug.LogError("Unexpected error: Rewarded load event fired with null ad and null error.");
                    return;
                }

                // The operation completed successfully.
                Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
                rewardedAd = ad;

                // Register to ad events to extend functionality.
                RewardedAdRegisterEventHandlers(ad, action);
            });
        }

        /// <summary>
        /// Shows the ad.
        /// </summary>
        public bool ShowRewardedAd(UnityAction<Reward> action)
        {
            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                Debug.Log("Showing rewarded ad.");
                rewardedAd.Show((Reward reward) =>
                {
                    action?.Invoke(reward);
                    Debug.Log(String.Format("Rewarded ad granted a reward: {0} {1}",
                                            reward.Amount,
                                            reward.Type));
                });
                return true;
            }
            else
            {
                Debug.LogError("Rewarded ad is not ready yet.");
                return false;
            }
        }

        public bool CanShowRewardedAd()
        {
            return rewardedAd != null && rewardedAd.CanShowAd();
        }

        /// <summary>
        /// Destroys the ad.
        /// </summary>
        public void DestroyRewardedAd()
        {
            if (rewardedAd != null)
            {
                Debug.Log("Destroying rewarded ad.");
                rewardedAd.Destroy();
                rewardedAd = null;
            }
        }

        private void RewardedAdRegisterEventHandlers(RewardedAd ad, UnityAction actions)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded ad was clicked.");
            };
            // Raised when the ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                actions?.Invoke();
                Debug.Log("Rewarded ad full screen content closed.");
            };
            
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                actions?.Invoke();
                Debug.LogError("Rewarded ad failed to open full screen content with error : "
                    + error);
            };
        }
 
        #endregion
    }
}