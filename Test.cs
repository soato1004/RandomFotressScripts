using System;
using System.Collections;
using GoogleAds;
using UnityEngine;

namespace DefaultNamespace
{
    public class Test : MonoBehaviour
    {
        void Start()
        {
            // GoogleAdMobController.Instance.RequestAndLoadInterstitialAd();

            if (GoogleAdMobController.Instance.Banner == null)
            {
                GoogleAdMobController.Instance.RequestBannerAd();
                GoogleAdMobController.Instance.Banner.OnBannerAdLoaded += BannerOpen;
            }
            else
            {
                GoogleAdMobController.Instance.Banner.Show();
            }
        }

        private void OnDestroy()
        {
        }

        private void ShowInterstitialAd()
        {
            GoogleAdMobController.Instance.RequestAndLoadInterstitialAd();
        }
        
        private void BannerOpen()
        {
        }
        
        public void OnAdsButtonClick()
        {
            // GoogleAdMobController.Instance.ShowInterstitialAd();
        }
        
        public void OnRemoveAdsButtonClick()
        {
            GoogleAdMobController.Instance.DestroyBannerAd();
        }
    }
}