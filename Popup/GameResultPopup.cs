using DG.Tweening;
using UnityEngine;

namespace RandomFortress
{
    public class GameResultPopup : PopupBase
    {
        [SerializeField] private GameObject[] gameMode;
        [SerializeField] private SoloResult soloResult;
        [SerializeField] private OneOnOneResult oneononeResult;
        
        public override void ShowPopup(params object[] values)
        {
            GameType gameType = GameManager.I.gameType;
            gameMode[(int)gameType].SetActive(true);
            
            // Reset();
            gameObject.SetActive(true);
            
            //
            DOTween.timeScale = 1;
            DOTween.unscaledTimeScale = 1f;

            int index = (int)GameManager.I.gameType;
            gameMode[index].gameObject.SetActive(true);
            
            //
            if (GameManager.I.gameType == GameType.Solo)
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
            if (GameManager.I.gameType == GameType.Solo)
                MainManager.I.ChangeScene(SceneName.Lobby);
            else
                PhotonManager.I.GameOver();
        }

        public void OnClaimButtonClick()
        {
            ClosePopup();
            if (GameManager.I.gameType != GameType.Solo)
            {
                PhotonManager.I.GameOver();
            }
            MainManager.I.ChangeScene(SceneName.Lobby);

        }
    }
}