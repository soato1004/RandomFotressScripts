using System.Collections;
using DG.Tweening;
using GoogleAds;
using RandomFortress.Common.Utils;
using RandomFortress.Game;
using RandomFortress.Manager;
using RandomFortress.Menu;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class TutorialController : MonoBehaviour
    {
        [SerializeField] private Image dim;
        [SerializeField] private Image dimGlow;
        [SerializeField] private GameObject textBox;
        [SerializeField] private TextMeshProUGUI explan;
        [SerializeField] private GameObject SkipButton;
        [SerializeField] private GameObject[] Steps;

        private int currentPage = 1;
        private bool canTouch = true;
        
        private void Reset()
        {
            dim.DOFade(0, 0);
            dimGlow.DOFade(0, 0);
            textBox.SetActive(false);
            explan.text = "";
            SkipButton.SetActive(false);

            Steps[0].transform.position = Vector3.zero;
        }
        
        public IEnumerator StartTutorialCor()
        {
            Reset();
            gameObject.SetActive(true);
            
            dim.DOFade(0.6f, 0.25f).From(0);
            dimGlow.DOFade(1f, 0.25f).From(0);
            yield return new WaitForSeconds(0.25f);
            
            textBox.SetActive(true);
            SkipButton.SetActive(true);
            
            SetTutorialText();
            
            while (currentPage < 8)
            {
                yield return null;
            }

            canTouch = false;

            EndTutorial();
            gameObject.SetActive(false);
        }

        private void SetTutorialText()
        {
            explan.text = "";
            
            Locale currentLocale = LocalizationSettings.SelectedLocale;
            string key = "tutorial_lobby_" + currentPage.ToString("D2");
            string text = DataManager.Instance.stringTableDic[key];
            explan.DOText(text, 1f);
            // explan.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable",
                // "tutorial_lobby_"+currentPage, currentLocale);
        }
        
        private void EndTutorial()
        {
            Account.Instance.Data.isTutorialLobby = false;
            Reset();
            gameObject.SetActive(false);
        }
        
        public void OnTouchScreen()
        {
            if (canTouch == false)
                return;

            canTouch = false;
            DelayCallUtils.DelayCall(1f, () => canTouch = true);
            
            ++currentPage;
            SetTutorialText();
            
            // 랜덤 포트리스에 온 것을 환영해!!
            // 이곳저곳에서 몰려드는 적을 막으면 된다구
            // 이곳을 클릭해서 덱을 확인해보자
            if (currentPage == 3)
            {
                float banner_height = 0;
                if (GoogleAdMobController.Instance != null && GoogleAdMobController.Instance.Banner != null)
                {
                    banner_height = GoogleAdMobController.Instance.Banner.GetHeightInPixels();
                }
                
                canTouch = false;
                Steps[0].transform.DOMoveY(banner_height, 0);
                Steps[0].SetActive(true);
            }
            // 최대 8개의 타워를 선택 및 사용하고
            // 최대 3개의 스킬을 선택 및 사용할수있어
            // 아직은 부족하지만, 곧 새로운 타워와 스킬들이 추가 될거야!
            else if (currentPage == 4)
            {
                Steps[0].SetActive(false);
                
                dim.gameObject.SetActive(false);
                dimGlow.gameObject.SetActive(false);
                textBox.transform.DOMoveY(-215f, 0.25f);
                    
                Steps[1].SetActive(true);
            }

            // 다른 사람보다 높은 스테이지를 빠르게 도달하는것이 이 게임의 목표야!
            // 다른 사람과의 경쟁은 추가 중이니 조금만 기다려줘
            // 이제 끝없이 몰려드는 적들을 물리쳐보자!
                
            else if (currentPage == 7)
            {
                Steps[1].SetActive(false);
                
                dim.gameObject.SetActive(true);
                dimGlow.gameObject.SetActive(true);
                    
                textBox.transform.DOMoveY(0f, 0.25f);
                MainManager.Instance.ChangePlage(PageController.PageType.Battle);
            }
        }

        // 인벤토리 클릭
        public void OnStepButtonClick(int index)
        {
            Steps[index].SetActive(false);
            
            ++currentPage;
            SetTutorialText();
            
            DelayCallUtils.DelayCall(0.5f, () => canTouch = true);
            
            MainManager.Instance.ChangePlage(PageController.PageType.Inventory);
        }
        
        public void OnSkipButtonClick()
        {
            StopCoroutine(StartTutorialCor());
            EndTutorial();
        }

        // void OnChangeLocale(Locale locale)
        // {
        //     StartCoroutine(ChangeLocaleCor);
        // }
        //
        // IEnumerator ChangeLocaleCor(Locale locale)
        // {
        //     var loadingOperation = LocalizationSettings.StringDatabase.GetTableAsync("StringTable");
        //     yield return loadingOperation;
        //
        //     if (loadingOperation.Status == AsyncOperationStatus.Succeeded)
        //     {
        //         StringTable table = loadingOperation.Result;
        //         
        //     }
        // }
    }
}