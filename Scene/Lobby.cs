using System;
using System.Collections;
using DefaultNamespace;
using RandomFortress.Constants;
using RandomFortress.Manager;
using UnityEngine;
using GoogleAds;
using RandomFortress.Common.Utils;
using RandomFortress.Game;
using RandomFortress.Menu;
using UnityEngine.UI;
using Application = UnityEngine.Device.Application;

namespace RandomFortress.Scene
{
    public class Lobby : MonoBehaviour
    {
        public RectTransform bottomUI;
        public TutorialController tutorialController;
        public Button rewardAdButton; 
        [SerializeField] private RankBoard rankBoard;

        private bool isReward = false;
        

        private Vector3 ori;
        private float banner_height;

        private void Awake()
        {
            ori = bottomUI.anchoredPosition;
        }

        private void Start()
        {
            // 최초 접근시 업데이트
            UpdateLobbyUI();
            
            // 배경음
            SoundManager.Instance.PlayBgm("bgm_lobby");
            
            // if ( Account.Instance.Data.isTutorialLobby )
            // {
            //     StartCoroutine(tutorialController.StartTutorialCor());
            // }

            // 광고제거 패키지를 구입시에 배너 제거
            // if ()
            {
                ShowBanner();
            }

            Account.Instance.InitAdDebuff();
        }
        
        public void OnPlayButtonClick()
        {
            SoundManager.Instance.PlayOneShot("button_click");
            
            if(MainManager.Instance.GamePlayCount != 0)
                StartCoroutine(ShowInterstitialAdCor());
            else
                GameStart();
        }

        // 게임 모드별로 다르게 구현
        private void GameStart()
        {
            // 배너광고 닫기
            BannerClose();
            
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
            BannerOpen();
        }

        // 배너광고 표시시에 하단 UI 위치 조정
        private void BannerOpen()
        {
            if (GoogleAdMobController.Instance.ShowBannerAd())
            {
                banner_height = GoogleAdMobController.Instance.Banner.GetHeightInPixels();
                Vector3 pos = bottomUI.anchoredPosition;
                pos.y += banner_height;
                bottomUI.anchoredPosition = pos;
            }
        }

        private void BannerClose()
        {
            GoogleAdMobController.Instance.Banner.Hide();
            bottomUI.anchoredPosition = ori;
        }

        public void OnRewardAdClick(int index)
        {
            rewardAdButton.interactable = false;
            StartCoroutine(ShowRewardAdCor());
        }
        
        // 보상형광고 시청로직
        private IEnumerator ShowRewardAdCor()
        {
            float timer = 0;
            bool isShow = false;

            do
            {
                isShow = GoogleAdMobController.Instance.ShowRewardedAd(() =>
                {
                    // 광고 시청완료시 보상
                    Account.Instance.AddAdDebuff(AdDebuffType.AbilityCard);
                    rewardAdButton.interactable = false;
                    rewardAdButton.gameObject.SetActive(false);
                    SoundManager.Instance.ResumeBgm();
                } );
                if (isShow)
                    break;

                yield return new WaitForSeconds(0.5f);
                timer += 0.5f;
            } while (timer < 5f) ;

            // 광고재생 성공시에 배경음 끄기
            SoundManager.Instance.PauseBgm();
            
            if (!isShow)
            {
                JTDebug.LogColor("Reward Ad Show Fail");
                rewardAdButton.interactable = true;
            }
        }

        // 최초 게임시작을 제외하고 게임시작하기전 광고를 봐야한다
        private IEnumerator ShowInterstitialAdCor()
        {
            GoogleAdMobController.Instance.RequestAndLoadInterstitialAd();
            GoogleAdMobController.Instance.interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                SoundManager.Instance.ResumeBgm();
                GameStart();
            };
            
            float timer = 0;
            bool isShow = false;

            do
            {
                isShow = GoogleAdMobController.Instance.ShowInterstitialAd();
                if (isShow)
                    break;

                yield return new WaitForSeconds(0.5f);
                timer += 0.5f;
            } while (timer < 5f) ;
            
            if (!isShow)
                JTDebug.LogColor("Interstitial Ad Show Fail");
            else
            {
                // 광고재생 성공시에 배경음 끄기
                SoundManager.Instance.PauseBgm();
            }
        }

        #endregion
    }
}