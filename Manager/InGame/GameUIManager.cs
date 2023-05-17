using DG.Tweening;
using RandomFortress.Common;
using RandomFortress.Common.Extensions;
using RandomFortress.Common.Util;
using RandomFortress.Game;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RandomFortress.Manager
{
    public class GameUIManager : Singleton<GameUIManager>
    {
        [Header("배경화면")] 
        [SerializeField] private Image changeBG;
        [SerializeField] private Image upBG;
        [SerializeField] private Image downBG;

        [Header("게임내 기능 UI")] 
        [SerializeField] private ResultUI resultUI;
        
        [Header("MiddleUI")] 
        [SerializeField] private Image midBG;
        [SerializeField] private TextMeshProUGUI midText;
        
        [Header("게임 내 버튼들")] 
        [SerializeField] private SkillButton heroSkill;
        [SerializeField] private SkillButton sub1Skill;
        [SerializeField] private SkillButton sub2Skill;
        [SerializeField] private Button buildBtn;
        [SerializeField] private Button sellBtn;
        [SerializeField] private Button monsterInitBtn;
        [SerializeField] private Button skipBtn;

        [Header("게임에 사용되는 UI")] 
        [SerializeField] private TextMeshProUGUI time; // 게임 타임
        [SerializeField] private TextMeshProUGUI gold; // 게임 머니
        [SerializeField] private TextMeshProUGUI sellMoney; // 판매금액

        
        
        
        // [Header("상태값")]
        [SerializeField] private TextMeshProUGUI playerHP;
        // [SerializeField] private Slider otherHP;

        private int bgIndex = 3;
        
        public override void Reset()
        {
            JTDebug.LogColor("GameUIManager Reset");
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("GameUIManager Terminate");
        }

        // public void ChangeBackground()
        // {
        //     Transform parent = changeBG.transform.parent;
        //     changeBG.sprite = ResourceManager.Instance.GetSprite("BG_Atlas", bgIndex++.ToString());
        //     
        //     parent.DOMoveY(-810f, 2f).OnComplete(() =>
        //     {
        //         downBG.sprite = upBG.sprite;
        //         upBG.sprite = changeBG.sprite;
        //         parent.transform.ExSetPositionY(0);
        //     });
        // }
        
        public void ShowResult()
        {
            resultUI.ShowResult();
        }

        public void HideResult()
        {
            resultUI.gameObject.SetActive(false);
        }
        
        // TODO: 알림을 표시
        public void ShowMidText(string text)
        {
            float width = Screen.width;
            Sequence sequence = DOTween.Sequence();
            
            // 배경등장
            // sequence.Append(midBG.DOFade(1, 0.1f));
            
            // 글자 등장
            sequence.AppendCallback(() => { midText.text = text; });
            sequence.Append(midText.transform.DOMoveX(0f, 0.25f).From(width));
            sequence.AppendInterval(1f);
            
            // 글자 빠짐
            sequence.Append(midText.transform.DOMoveX(-width, 0.25f));
            
            // 배경빠짐
            // sequence.Append(midBG.DOFade(0, 0.1f));
            sequence.Play();
        }

        public void UpdateInfo()
        {
            // 체력 업데이트
            // playerHP.DOValue((float)GameManager.Instance.hp / 10, 0.25f);
            playerHP.text = "x " + GameManager.Instance.PlayerHp;

            // 게임머니 업데이트
            gold.text = GameManager.Instance.GameMoney.ToString();

            // 판매금액 업데이트
            sellBtn.gameObject.SetActive(GameManager.Instance.isFocus);
            if (GameManager.Instance.isFocus)
            {
                Vector3 targetPos = GameManager.Instance.focusTower.transform.position;
                targetPos.y -= 100f;
                sellBtn.transform.position = targetPos;
                sellMoney.text = GameManager.Instance.focusTower.SalePrice.ToString();
            }
        }

        public void UpdateTime()
        {
            // 시간 업데이트
            int minute = (int)GameManager.Instance.GameTime / 60;
            time.text = minute + " : " + ((int)GameManager.Instance.GameTime % 60).ToString("D2");
            
            // 스킬버튼 쿨타임 업데이트
        }

        // 몬스터 생성 업데이트
        public void StartMonsterInit(float time)
        {
            monsterInitBtn.gameObject.SetActive(true);
            skipBtn.gameObject.SetActive(false);
            Transform coolTime = monsterInitBtn.transform.GetChild(0);
            coolTime.SetActive(true);
            Image img = coolTime.GetComponent<Image>();
            img.DOFillAmount(0, time).From(1).SetEase(Ease.Linear);

        }

        // 스테이지 딜레이
        public void StartStageDelay(float time)
        {
            skipBtn.gameObject.SetActive(true);
            monsterInitBtn.gameObject.SetActive(false);
            Transform coolTime = skipBtn.transform.GetChild(0);
            Image img = coolTime.GetComponent<Image>();
            img.DOFillAmount(1, time).From(0).SetEase(Ease.Linear);
        }

        #region Button Event
        
        // 0: hero, 1: sub1, 2: sub2
        public void OnSkillButtonClick(int index)
        {
            SoundManager.Instance.PlayOneShot("DM-CGS-01");
            if (GameManager.Instance.skillArr[index].CanUseSkill())
            {
                switch (index)
                {
                    case 0: GameManager.Instance.skillArr[index].Init(heroSkill); break;
                    case 1: GameManager.Instance.skillArr[index].Init(sub1Skill); break;
                    case 2: GameManager.Instance.skillArr[index].Init(sub2Skill); break;
                }
            }
        }

        // hero, human, beast, machine, element
        public void OnUpgradeButtonClick(int index)
        {
            SoundManager.Instance.PlayOneShot("DM-CGS-01");
        }
        
        // 영웅타워를 랜덤으로 짓는다
        public void OnBuild_ButtonClick()
        {
            GameManager.Instance.BuildRandomTower();
            SoundManager.Instance.PlayOneShot("DM-CGS-01");
        }

        public void OnSellButtonClick()
        {
            GameManager.Instance.SellTower();
            SoundManager.Instance.PlayOneShot("DM-CGS-01");
        }

        // 화면 상의 조합가능한 타워를 섞는다
        public void OnMixButtonClick()
        {
            SoundManager.Instance.PlayOneShot("DM-CGS-01");
        }

        public void OnSkipButtonClick()
        {
            GameManager.Instance.SkipWaitTime();
        }

        public void OnPauseButtonClick()
        {
            if (GameManager.Instance.IsPaused)
                GameManager.Instance.ResumeAllCoroutines();
            else
                GameManager.Instance.PauseAllCoroutines();
        }

        #endregion
    }
}