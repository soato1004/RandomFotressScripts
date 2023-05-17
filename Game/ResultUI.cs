using System.Collections;
using DG.Tweening;
using GoogleAds;
using RandomFortress.Common.Extensions;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RandomFortress.Game
{
    public class ResultUI : MonoBehaviour
    {
        // [SerializeField] private Button exitBtn;

        [SerializeField] private Image dim;
        
        [SerializeField] private TextMeshProUGUI gameOverLabel;
        
        [SerializeField] private TextMeshProUGUI towerDamageLabel;
        [SerializeField] private Transform towerList; // icon, text
        
        [SerializeField] private TextMeshProUGUI stageProgressLabel;
        [SerializeField] private TextMeshProUGUI stageText;

        [SerializeField] private GameObject rewardObj;
        [SerializeField] private Transform rewardList; // enegy, buff, gold, gem, item

        [SerializeField] private Button claimBtn;
        [SerializeField] private Button gemBtn;
        [SerializeField] private Button continueBtn;
        
        
        public void ShowResult()
        {
            Reset();
            gameObject.SetActive(true);
            StartCoroutine(ResultCor());
        }

        private void Reset()
        {
            gameOverLabel.gameObject.SetActive(false);
            
            towerDamageLabel.gameObject.SetActive(false);
            for (int i = 0; i < towerList.childCount; ++i)
            {
                Transform temps = towerList.transform.GetChild(i);
                for (int j = 0; j < temps.childCount; ++j)
                {
                    temps.GetChild(j).SetActive(false);
                }
            }


            stageProgressLabel.gameObject.SetActive(false);
            stageText.text = "";
            
            rewardObj.gameObject.SetActive(false);
            rewardList.gameObject.SetActive(false);
            for (int i = 0; i < rewardList.childCount; ++i)
            {
                rewardList.transform.GetChild(i).SetActive(false);
            }
            
            
            claimBtn.gameObject.SetActive(false);
            gemBtn.gameObject.SetActive(false);
            continueBtn.gameObject.SetActive(false);
        }
        
        private IEnumerator ResultCor()
        {
            DOTween.timeScale = 1;
            DOTween.unscaledTimeScale = 1f;
            
            // 1. 딤
            dim.DOFade(1, 0.25f);
            yield return new WaitForSecondsRealtime(0.25f);

            // 2. 게임오버 텍스트 박힘
            gameOverLabel.transform.DOScale(1, 0.25f).From(2);
            gameOverLabel.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(0.25f);
            
            // 3. 타워데미지 출력
            towerDamageLabel.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(0.25f);
            
            // tower 리스트 표시
            int length = GameManager.Instance.towerCount;
            int count = 0;
            Transform character;
            
            TowerBase[] towers = GameManager.Instance.Towers;

            // int width, height;
            
            for (int i = 0; i < towers.Length; ++i)
            {
                TowerBase tower = towers[i];
                if (tower == null)
                    continue;

                character = towerList.GetChild(count/7).GetChild(count);
                character.gameObject.SetActive(true);
                //
                Image icon = character.GetChild(0).GetComponent<Image>();
                icon.sprite = ResourceManager.Instance.GetTower(tower.GetSpriteKey);
                float ratioW = 80 / (float)icon.sprite.texture.width;
                float ratioH = 80 / (float)icon.sprite.texture.height;
                float ratio = ratioW > ratioH ? ratioH : ratioW;
                icon.rectTransform.sizeDelta = new Vector2(icon.sprite.texture.width * ratio, icon.sprite.texture.height * ratio);
                
                TextMeshProUGUI text = character.GetChild(1).GetComponent<TextMeshProUGUI>();
                text.text = tower.TotalDamege.ToString();

                ++count;
                yield return new WaitForSecondsRealtime(0.05f);
            }

            // 판매한 Tower 데미지 총합
            if (GameManager.Instance.OtherDamage > 0)
            {
                character = towerList.GetChild(count/7).GetChild(count);
                character.gameObject.SetActive(true);
                character.GetChild(1).GetComponent<TextMeshProUGUI>().text = GameManager.Instance.OtherDamage.ToString();
                yield return new WaitForSecondsRealtime(0.25f);
            }

            
            
            // 4. 스테이지 표시
            stageProgressLabel.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(0.25f);
            
            stageText.text = GameManager.Instance.CurrentStage.ToString();
            yield return new WaitForSecondsRealtime(0.25f);

            
            // 5. 리워드표시
            rewardObj.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(0.25f);
            
            // TODO: 리워드 넣기
            Transform goldReward = rewardList.transform.GetChild(2);
            TextMeshProUGUI goldText = goldReward.GetChild(2).GetComponent<TextMeshProUGUI>();
            goldText.text = "1000";
            yield return new WaitForSecondsRealtime(0.25f);
            
            // 6. 버튼표시
            claimBtn.ExSetActive(true);
            continueBtn.ExSetActive(true);
        }

        public void OnExitButtonClick()
        {
            SceneManager.LoadScene(2);
        }

        public void OnClaimButtonClick()
        {
            SceneManager.LoadScene(2);
        }
        
        public void OnGemButtonClick()
        {
            Debug.Log("Gem 버튼 클릭");
        }

        public void OnContinueButtonClick()
        {
            Debug.Log("Continue 버튼 클릭");
            GoogleAdMobController.Instance.ShowInterstitialAd();
        }
    }
}