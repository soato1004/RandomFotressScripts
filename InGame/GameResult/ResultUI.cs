using System.Collections;
using DG.Tweening;
using GoogleAds;
using RandomFortress.Common.Extensions;
using RandomFortress.Game.Netcode;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Game
{
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private Image dim;

        [SerializeField] private GameObject WinTitle;
        [SerializeField] private GameObject DefeatTitle;
        
        // [SerializeField] private ResultPlayer other;
        [SerializeField] private ResultPlayer player; 
        
        [SerializeField] private Transform rewardList; // enegy, buff, gold, gem, item

        [SerializeField] private Button claimBtn;
        [SerializeField] private Button continueBtn;
        
        
        public void ShowResult()
        {
            Reset();
            gameObject.SetActive(true);
            StartCoroutine(ResultCor());
        }

        private void Reset()
        {
            dim.DOFade(0, 0);
            WinTitle.SetActive(false);
            DefeatTitle.SetActive(false);
            // other.Reset();
            player.Reset();
            
            for (int i = 0; i < rewardList.childCount; ++i)
            {
                rewardList.transform.GetChild(i).SetActive(false);
            }

            claimBtn.gameObject.SetActive(false);
            continueBtn.gameObject.SetActive(false);
        }
        
        private IEnumerator ResultCor()
        {
            DOTween.timeScale = 1;
            DOTween.unscaledTimeScale = 1f;
            
            GamePlayer myPlayer = GameManager.Instance.myPlayer;
            GamePlayer otherPlayer = GameManager.Instance.otherPlayer;
            bool isWin = GameManager.Instance.isWin;
            
            // 1. 딤
            dim.DOFade(1, 0.5f);
            yield return new WaitForSecondsRealtime(0.5f);

            // 2. 타이틀표시
            WinTitle.SetActive(isWin);
            DefeatTitle.SetActive(!isWin);
            yield return new WaitForSecondsRealtime(0.25f);
            
            // 3. 결과창등장
            // other.gameObject.SetActive(true);
            player.gameObject.SetActive(true);
            
            // 4. 게임 수치 표시
            // other.ShowTower(otherPlayer.Towers, otherPlayer.stageProcess);
            player.ShowTower(myPlayer.Towers, myPlayer.stageProcess);
            
            // 5. 리워드 표시
            Transform goldReward = rewardList.transform.GetChild(2);
            goldReward.SetActive(true);
            TextMeshProUGUI goldText = goldReward.GetChild(2).GetComponent<TextMeshProUGUI>();
            goldText.text = "1000";
            yield return new WaitForSecondsRealtime(0.25f);
            
            // 6. 버튼표시
            claimBtn.ExSetActive(true);
            continueBtn.ExSetActive(true);
        }

        public void OnExitButtonClick()
        {
            PunManager.Instance.LeaveRoom();
        }

        public void OnClaimButtonClick()
        {
            PunManager.Instance.LeaveRoom();
        }
        
        public void OnGemButtonClick()
        {
            Debug.Log("Gem 버튼 클릭");
        }

        #region Reward Ad

        public void OpenAd()
        {
            Debug.Log("광고 시청시작");
        }

        public void CloseAd()
        {
            Debug.Log("보상 2배증가");
        }

        public void OnRewardButtonClick()
        {
            StartCoroutine(ShowAdCor());
            Debug.Log("Continue 버튼 클릭");

        }

        private IEnumerator ShowAdCor()
        {
            GoogleAdMobController.Instance.RequestAndLoadRewardedAd();
            GoogleAdMobController.Instance.OnAdOpeningEvent.AddListener(OpenAd);
            GoogleAdMobController.Instance.OnAdClosedEvent.AddListener(CloseAd);
            
            float waitTime = 0f;
            while (waitTime < 3f)
            {
                if (GoogleAdMobController.Instance.ShowRewardedAd())
                    break;
            
                yield return new WaitForSeconds(0.5f);
                waitTime += 0.5f;
            }
            
            GoogleAdMobController.Instance.OnAdOpeningEvent.RemoveListener(OpenAd);
            GoogleAdMobController.Instance.OnAdClosedEvent.RemoveListener(CloseAd);
            yield return null;
        }

        #endregion
        
    }
}