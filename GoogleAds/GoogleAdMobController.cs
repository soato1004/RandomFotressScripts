using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using RandomFortress;
using UnityEngine;
using UnityEngine.Events;

namespace GoogleAds
{
    public class GoogleAdMobController : Singleton<GoogleAdMobController>
    {
        enum AdType
        {
            Banner,                 // 배너
            Interstitial,           // 전면광고
            Rewarded,               // 보상형 광고
        }
        
#if UNITY_ANDROID
        private string[] adUnitIds =
        {
            "ca-app-pub-7560657077531827/8417023328", // 배너
            "ca-app-pub-7560657077531827/6155290672", // 전면광고
            "ca-app-pub-7560657077531827/4790238114", // 보상형광고
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
        
        public override void Reset()
        {
            JustDebug.LogColor("GoogleAdMobController Reset");
        }

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

        public void Init()
        {
            LoadInterstitialAd();
            LoadRewardedAd();
        }


        #endregion
        
        #region BANNER ADS
        
        public BannerView _bannerView;

        public void CreateBannerView()
        {
            Debug.Log("Creating banner view.");

            // If we already have a banner, destroy the old one.
            if(_bannerView != null)
            {
                DestroyBannerAd();
            }

            // Create a 320x50 banner at top of the screen.
            _bannerView = new BannerView(adUnitIds[(int)AdType.Banner], AdSize.Banner, AdPosition.Bottom);

            // Listen to events the banner may raise.
            BannerListenToAdEvents();

            Debug.Log("Banner view created.");
        }

        /// <summary>
        /// Creates the banner view and loads a banner ad.
        /// </summary>
        public void LoadBannerAd()
        {
            // Create an instance of a banner view first.
            if(_bannerView == null)
            {
                CreateBannerView();
            }

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // Send the request to load the ad.
            Debug.Log("Loading banner ad.");
            _bannerView.LoadAd(adRequest);
        }

        /// <summary>
        /// Shows the ad.
        /// </summary>
        public void ShowBannerAd()
        {
            if (_bannerView != null)
            {
                Debug.Log("Showing banner view.");
                _bannerView.Show();
            }
        }

        /// <summary>
        /// Hides the ad.
        /// </summary>
        public void HideBannerAd()
        {
            if (_bannerView != null)
            {
                Debug.Log("Hiding banner view.");
                _bannerView.Hide();
            }
        }

        /// <summary>
        /// Destroys the ad.
        /// When you are finished with a BannerView, make sure to call
        /// the Destroy() method before dropping your reference to it.
        /// </summary>
        public void DestroyBannerAd()
        {
            if (_bannerView != null)
            {
                Debug.Log("Destroying banner view.");
                _bannerView.Destroy();
                _bannerView = null;
            }
        }

        /// <summary>
        /// Listen to events the banner may raise.
        /// </summary>
        private void BannerListenToAdEvents()
        {
            // Raised when an ad is loaded into the banner view.
            _bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("Banner view loaded an ad with response : " + _bannerView.GetResponseInfo());
            };
            // Raised when an ad fails to load into the banner view.
            _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.LogError("Banner view failed to load an ad with error : " + error);
            };
            // Raised when the ad is estimated to have earned money.
            _bannerView.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Banner view paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            _bannerView.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Banner view recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            _bannerView.OnAdClicked += () =>
            {
                Debug.Log("Banner view was clicked.");
            };
            // Raised when an ad opened full screen content.
            _bannerView.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Banner view full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            _bannerView.OnAdFullScreenContentClosed += () =>
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
        
        private RewardedAd _rewardedAd;
        
        /// <summary>
        /// Loads the ad.
        /// </summary>
        public void LoadRewardedAd()
        {
            // Clean up the old ad before loading a new one.
            if (_rewardedAd != null)
            {
                DestroyRewardedAd();
            }

            Debug.Log("Loading rewarded ad.");

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // Send the request to load the ad.
            RewardedAd.Load(adUnitIds[(int)AdType.Rewarded], adRequest, (RewardedAd ad, LoadAdError error) =>
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
                _rewardedAd = ad;

                // Register to ad events to extend functionality.
                RewardedAdRegisterEventHandlers(ad);
            });
        }

        /// <summary>
        /// Shows the ad.
        /// </summary>
        public bool ShowRewardedAd(UnityAction action)
        {
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                Debug.Log("Showing rewarded ad.");
                _rewardedAd.Show((Reward reward) =>
                {
                    action?.Invoke();
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
            return _rewardedAd != null && _rewardedAd.CanShowAd();
        }

        /// <summary>
        /// Destroys the ad.
        /// </summary>
        public void DestroyRewardedAd()
        {
            if (_rewardedAd != null)
            {
                Debug.Log("Destroying rewarded ad.");
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }
        }

        private void RewardedAdRegisterEventHandlers(RewardedAd ad)
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
                Debug.Log("Rewarded ad full screen content closed.");
            };
            
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to open full screen content with error : "
                    + error);
            };
        }
 
        #endregion
    }
}