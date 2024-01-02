using DG.Tweening;
using RandomFortress.Common;
using RandomFortress.Common.Extensions;
using RandomFortress.Common.Utils;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Manager
{
    public enum Buttons
    {
        Build, Sell, MonsterInit, Skip
    }

    public class GameUIManager : Singleton<GameUIManager>
    {
        // [Header("배경화면")] 
        // [SerializeField] private Image changeBG;
        // [SerializeField] private Image upBG;
        // [SerializeField] private Image downBG;

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

        [SerializeField] private Button[] upgradeBtns;
        
        [Header("게임에 사용되는 UI")] 
        [SerializeField] private TextMeshProUGUI timeText; // 게임 타임
        [SerializeField] private TextMeshProUGUI goldText; // 게임 머니
        [SerializeField] private TextMeshProUGUI sellMoneyText; // 판매금액

        [Header("플레이어 정보")]
        [SerializeField] private TextMeshProUGUI myPlayerHP;
        [SerializeField] private TextMeshProUGUI otherPlayerHP;
        // [SerializeField] private Slider otherHP;

        // private int bgIndex = 3;
        // private bool isInitialize = false;
        
        private const int UGRADE_BTN_WIDTH = 70;
        private const int UPGRADE_BTN_HEIGHT = 70;
        
        public override void Reset()
        {
            JTDebug.LogColor("GameUIManager Reset");
            resultUI.gameObject.SetActive(false);
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("GameUIManager Terminate");
            Destroy(Instance);
        }

        // 게임 최초 시작시 한번만
        public void InitializeGameUI()
        {
            // if (isInitialize)
            //     return;
            //
            // isInitialize = true;
            // 타워 업그레이드
            UpdateUpgradeBtn();
        }
        
        public void UpdateInfo()
        {
            GamePlayer myPlayer = GameManager.Instance.myPlayer;
            // GamePlayer otherPlayer = GameManager.Instance.otherPlayer;
            TowerBase focusTower = GameManager.Instance.focusTower;
            
            // 체력 업데이트
            myPlayerHP.text = "x " + myPlayer.playerHp;
            // otherPlayerHP.text = "x " + otherPlayer.playerHp;
            
            // 게임머니 업데이트
            goldText.text = myPlayer.gold.ToString();
            
            // 판매금액 업데이트
            sellBtn.gameObject.SetActive(GameManager.Instance.isFocus);
            if (GameManager.Instance.isFocus && focusTower != null)
            {
                Vector3 targetPos = focusTower.transform.position;
                targetPos.y -= 100f;
                sellBtn.transform.position = targetPos;
                sellMoneyText.text = focusTower.SellPrice.ToString();
            }
        }
        
        public void UpdateUpgradeBtn()
        {
            // 업그레이드 버튼   
            for (int slotIndex = 0; slotIndex < GameConstants.TOWER_DECK_COUNT; ++slotIndex)
            {
                int towerIndex = MainManager.Instance.account.TowerDeck(slotIndex);
                GameObject upgradeBtn = upgradeBtns[slotIndex].gameObject;
                TowerUpgrade upgrade = GameManager.Instance.towerUpgradeDic[towerIndex];
                
                Image icon = upgradeBtn.transform.GetChild(0).GetComponent<Image>(); // icon, level, gold
                TextMeshProUGUI level = upgradeBtn.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI gold = upgradeBtn.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                
                // TODO: 해당 인덱스 타워의 1티어 모습만 보여줌
                icon.sprite = ResourceManager.Instance.GetTower(towerIndex, 1);
                level.text = upgrade.towerUpgradeLV.ToString();
                gold.text = upgrade.upgradeCost.ToString();

                ImageUtils.ImageSizeToFit(UGRADE_BTN_WIDTH, UPGRADE_BTN_HEIGHT, ref icon);
            }
        }
        
        public void SetLockButton(Buttons buttons, bool active)
        {
            switch (buttons)
            {
                case Buttons.Build:
                    buildBtn.interactable = active;
                    break;
                case Buttons.Sell:
                    sellBtn.interactable = active;
                    break;
                // case Buttons.Skip:
                //     sellBtn.interactable = active;
                //     break;
                // case Buttons.MonsterInit:
                //     sellBtn.interactable = active;
                //     break;
            }
        }
        
        public void ShowResult() => resultUI.ShowResult();

        public void ShowMidText(string text)
        {
            float width = Screen.width;
            Sequence sequence = DOTween.Sequence();
            
            // 배경등장
            // sequence.Append(midBG.DOFade(1, 0.1f));
            
            // 글자 등장
            sequence.AppendCallback(() => { midText.text = text; });
            // sequence.Append(midText.transform.DOMoveX(0f, 0.25f).From(width));
            sequence.AppendInterval(1f);
            
            // 글자 빠짐
            sequence.AppendCallback(() => { midText.text = ""; });
            // sequence.Append(midText.transform.DOMoveX(-width, 0.25f));
            
            // 배경빠짐
            // sequence.Append(midBG.DOFade(0, 0.1f));
            sequence.Play();
        }

        public void UpdateTime()
        {
            // 시간 업데이트
            int minute = (int)GameManager.Instance.gameTime / 60;
            timeText.text = minute + " : " + ((int)GameManager.Instance.gameTime % 60).ToString("D2");
            
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

        #region Result

        

        #endregion

        #region Button Event
        
        // TODO: 0: hero, 1: sub1, 2: sub2
        public void OnSkillButtonClick(int index)
        {
            AudioManager.Instance.PlayOneShot("DM-CGS-01");
            GamePlayer player = GameManager.Instance.myPlayer;
            if (player.skillArr[index].CanUseSkill())
            {
                switch (index)
                {
                    case 0: player.skillArr[index].Init(player,heroSkill); break;
                    case 1: player.skillArr[index].Init(player,sub1Skill); break;
                    case 2: player.skillArr[index].Init(player,sub2Skill); break;
                }
            }
        }

        // 업그레이드 버튼
        public void OnUpgradeButtonClick(int slotIndex)
        {
            int towerIndex = MainManager.Instance.account.TowerDeck(slotIndex);
            GameManager.Instance.TowerUpgrade(towerIndex);
        }
        
        // 영웅타워를 랜덤으로 짓는다
        public void OnBuild_ButtonClick()
        {
            GameManager.Instance.BuildRandomTower();
            AudioManager.Instance.PlayOneShot("DM-CGS-01");
        }
        
        // 타워 판매버튼
        public void OnSellButtonClick()
        {
            GameManager.Instance.myPlayer.SellTower();
            AudioManager.Instance.PlayOneShot("DM-CGS-01");
        }

        // public void OnMixButtonClick()
        // {
        //     AudioManager.Instance.PlayOneShot("DM-CGS-01");
        // }

        // 스테이지 스킵 버튼
        public void OnSkipButtonClick()
        {
            GameManager.Instance.SkipWaitTime();
        }

        // public void OnPauseButtonClick()
        // {
        //     if (GameManager.Instance.IsPaused)
        //         GameManager.Instance.ResumeAllCoroutines();
        //     else
        //         GameManager.Instance.PauseAllCoroutines();
        // }

        #endregion
    }
}