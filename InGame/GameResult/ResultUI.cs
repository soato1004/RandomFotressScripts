using System.Collections;
using DG.Tweening;
using GoogleAds;
using RandomFortress.Common.Extensions;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Game.Netcode;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Game
{
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private GameType gameType = GameType.None;
        [SerializeField] private Image dim;

        [SerializeField] private GameObject[] gameMode;

        [SerializeField] private GameObject WinTitle;
        [SerializeField] private GameObject DefeatTitle;
        
        [SerializeField] private ResultPlayer other;
        [SerializeField] private ResultPlayer player; 
        
        [SerializeField] private Transform rewardList; // enegy, buff, gold, gem, item

        [SerializeField] private Button claimBtn;
        [SerializeField] private Button continueBtn;
        
        
        public void ShowResult()
        {
            gameType = MainManager.Instance.gameType;
            gameMode[(int)gameType].SetActive(true);
            
            // TODO: 게임모드가 바뀌면 해당 링크도 바꿔준다
            // WinTitle;
            // DefeatTitle;
            // other;
            // player; 
            // rewardList;

            
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
            
            if (isWin) 
                SoundManager.Instance.PlayOneShot("result_win");
            else
                SoundManager.Instance.PlayOneShot("result_lose");
            
            
            GameResult result = new GameResult(isWin, myPlayer.stageProcess, (int)GameManager.Instance.gameTime);
            
            Account.Instance.SaveStageResult(result);
            
            // 1. 딤
            dim.DOFade(1, 0.2f);
            yield return new WaitForSecondsRealtime(0.2f);

            // 2. 타이틀표시
            WinTitle.SetActive(isWin);
            DefeatTitle.SetActive(!isWin);
            yield return new WaitForSecondsRealtime(0.05f);
            
            // 3. 게임 수치 표시
            // other.gameObject.SetActive(true);
            player.gameObject.SetActive(true);
            
            // other.ShowTower(otherPlayer.Towers, otherPlayer.stageProcess);
            player.ShowTower(myPlayer.totalDmgDic, myPlayer.stageProcess);
            yield return new WaitForSecondsRealtime(0.05f);
            
            // 4. 리워드 표시
            // Transform goldReward = rewardList.transform.GetChild(2);
            // goldReward.SetActive(true);
            // TextMeshProUGUI goldText = goldReward.GetChild(2).GetComponent<TextMeshProUGUI>();
            // goldText.text = "1000";
            // yield return new WaitForSecondsRealtime(0.05f);
            
            // 6. 버튼표시
            claimBtn.ExSetActive(true);
            continueBtn.ExSetActive(true);
        }

        public void OnExitButtonClick()
        {
            if (MainManager.Instance.gameType == GameType.Solo)
                MainManager.Instance.ChangeScene(SceneName.Lobby);
            else
                PunManager.Instance.LeaveRoom();
        }

        public void OnClaimButtonClick()
        {
            if (MainManager.Instance.gameType == GameType.Solo)
                MainManager.Instance.ChangeScene(SceneName.Lobby);
            else
                PunManager.Instance.LeaveRoom();
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
            GoogleAdMobController.Instance.RequestAndLoadInterstitialAd();
            
            float waitTime = 0f;
            do {
                if (GoogleAdMobController.Instance.ShowInterstitialAd())
                    break;

                waitTime += 0.1f;
                yield return new WaitForSeconds(0.1f);
            } while (waitTime < 3f) ;
        }

        #endregion
        
    }
}