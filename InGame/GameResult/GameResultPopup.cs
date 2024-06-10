using DG.Tweening;
using GoogleAds;

using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class GameResultPopup : MonoBehaviour
    {
        [SerializeField] private Image dim;
        [SerializeField] private GameObject[] gameMode;
        [SerializeField] private SoloResult soloResult;
        [SerializeField] private OneOnOneResult oneononeResult;
        
        public void ShowResult()
        {
            GameType gameType = GameManager.Instance.gameType;
            gameMode[(int)gameType].SetActive(true);
            
            // Reset();
            gameObject.SetActive(true);
            
            //
            DOTween.timeScale = 1;
            DOTween.unscaledTimeScale = 1f;

            int index = (int)GameManager.Instance.gameType;
            gameMode[index].gameObject.SetActive(true);
            
            // 1. 딤
            dim.DOFade(1, 0.2f);
            
            //
            if (GameManager.Instance.gameType == GameType.Solo)
                StartCoroutine( soloResult.ResultCor());
            else
                StartCoroutine( oneononeResult.ResultCor());
        }

        // private void Reset()
        // {
        //     dim.DOFade(0, 0);
        //     WinTitle.SetActive(false);
        //     DefeatTitle.SetActive(false);
        //     // other.Reset();
        //     player.Reset();
        //
        //     foreach (var mode in gameMode)
        //     {
        //         mode.SetActive(false);
        //     }
        //     
        //     for (int i = 0; i < rewardList.childCount; ++i)
        //     {
        //         rewardList.transform.GetChild(i).SetActive(false);
        //     }
        //
        //     claimBtn.gameObject.SetActive(false);
        //     continueBtn.gameObject.SetActive(false);
        // }

        public void OnExitButtonClick()
        {
            if (GameManager.Instance.gameType == GameType.Solo)
                MainManager.Instance.ChangeScene(SceneName.Lobby);
            else
                PhotonManager.Instance.LeaveRoom();
        }

        public void OnClaimButtonClick()
        {
            if (GameManager.Instance.gameType == GameType.Solo)
                MainManager.Instance.ChangeScene(SceneName.Lobby);
            else
                PhotonManager.Instance.LeaveRoom();
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
            GoogleAdMobController.Instance.ShowInterstitialAd();
            // StartCoroutine(ShowAdCor());
            // Debug.Log("Continue 버튼 클릭");

        }

        // private IEnumerator ShowAdCor()
        // {
        //     GoogleAdMobController.Instance.ShowInterstitialAd();
        //     
        //     float waitTime = 0f;
        //     do {
        //         if (GoogleAdMobController.Instance.ShowInterstitialAd())
        //             break;
        //
        //         waitTime += 0.1f;
        //         yield return new WaitForSeconds(0.1f);
        //     } while (waitTime < 3f) ;
        // }

        #endregion
        
    }
}