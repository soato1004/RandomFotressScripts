using System.Collections;
using DG.Tweening;
using Photon.Pun;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public enum Buttons
    {
        Build, Sell, MonsterInit, Skip
    }
    
    public class GameUIManager : Singleton<GameUIManager>
    {
        [Header("게임내 기능 UI")] 
        [SerializeField] private Image dim; // 일반딤. 플레이어 복귀시에 사용
        [SerializeField] private Image skillDim;
        [SerializeField] private SVGImage attackRange; // SVGImage
        
        [Header("솔로모드 UI")]
        [SerializeField] private TextMeshProUGUI stageText;
        [SerializeField] private Transform HpTrf;
        
        [Header("게임 내 버튼들")] 
        [SerializeField] private SkillButton heroSkill;
        [SerializeField] private SkillButton sub1Skill;
        [SerializeField] private SkillButton sub2Skill;
        [SerializeField] private Button skipBtn;
        [SerializeField] private Button buildBtn;
        
        [SerializeField] private Button sellBtn;
        [SerializeField] private Button monsterInitBtn;
        [SerializeField] private Button[] upgradeBtns;
        [SerializeField] private GameSpeedButton gameSpeedButton;
        
        [Header("게임에 사용되는 UI")] 
        [SerializeField] private TextMeshProUGUI timeText; // 게임 타임
        [SerializeField] private TextMeshProUGUI goldText; // 게임 머니
        [SerializeField] private TextMeshProUGUI sellMoneyText; // 판매금액
        
        
        public Canvas[] uICanvas; // 0:10, 1:13, 2:17

        // private int bgIndex = 3;
        // private bool isInitialize = false;
        // private bool isSkillUse = false;
        
        private const int UGRADE_BTN_WIDTH = 70;
        private const int UPGRADE_BTN_HEIGHT = 70;
        
        public SkillButton GetSkillButton(int index)
        {
            switch (index)
            {
                case 0: return heroSkill;
                case 1: return sub1Skill;
                case 2: return sub2Skill;
            }

            return null;
        }

        // 게임 최초 시작시 한번만
        public void InitializeGameUI()
        {
            // 타워 업그레이드
            UpdateUpgradeBtn();

            if (GameManager.I.gameType == GameType.Solo)
            {
                GameManager.I.myPlayer.HpTrf = HpTrf;
                GameManager.I.myPlayer.stageText = stageText;
            }
        }
        
        // 게임 UI 변경
        public void UpdateUI()
        {
            if (GameManager.I.isPaused) return;
            
            GamePlayer myPlayer = GameManager.I.myPlayer;
            GamePlayer otherPlayer = GameManager.I.otherPlayer;
            
            // 게임머니 업데이트
            goldText.text = myPlayer.gold.ToString();
            
            // 게임속도 변경
            gameSpeedButton.UpdateUI();
        }
        
        // 업그레이드 버튼 설정 보류
        public void UpdateUpgradeBtn()
        {
            for (int slotIndex = 0; slotIndex < GameConstants.TOWER_UPGRADE_COUNT; ++slotIndex)
            {
                int towerIndex = Account.I.TowerDeck(slotIndex);
                GameObject upgradeBtn = upgradeBtns[slotIndex].gameObject;
                TowerUpgrade upgrade = GameManager.I.towerUpgradeDic[towerIndex];
                
                Image icon = upgradeBtn.transform.GetChild(0).GetComponent<Image>(); // icon, level, gold
                TextMeshProUGUI level = upgradeBtn.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI gold = upgradeBtn.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                
                // TODO: 해당 인덱스 타워의 1티어 모습만 보여줌
                icon.sprite = ResourceManager.I.GetTower(towerIndex, 1);
                level.text = "LV."+ (upgrade.TowerUpgradeLv+1);
                gold.text = upgrade.UpgradeCost.ToString();

                Utils.ImageSizeToFit(UGRADE_BTN_WIDTH, UPGRADE_BTN_HEIGHT, ref icon);
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

        // public void ShowResult()
        // {
        //     GameManager.I.PauseGame();
        //     PopupManager.I.ShowPopup(PopupNames.GameResultPopup);
        // }

        public void StageStart()
        {
            stageText.text = GameManager.I.myPlayer.stageProcess.ToString();
        }

        public void UpdateTime()
        {
            // 시간 업데이트
            int minute = (int)GameManager.I.gameTime / 60;
            timeText.text = minute + " : " + ((int)GameManager.I.gameTime % 60).ToString("D2");
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
        
        // 스킬 사용시 딤
        public void SetSkillDim(bool show)
        {
            if (show)
            {
                skillDim.DOFade(0.6f, 0.2f);
                foreach (var dim in skillDim.transform.GetChildren())
                    dim.GetComponent<Image>().DOFade(1f, 0.2f);
            }
            else
            {
                skillDim.DOFade(0f, 0.2f);
                foreach (var dim in skillDim.transform.GetChildren())
                    dim.GetComponent<Image>().DOFade(0f, 0.2f);
            }
        }

        public void SetDim(bool show)
        {
            if (show)
                dim.DOFade(0.6f, 0.15f);
            else
                dim.DOFade(0f, 0.15f);
            
            dim.gameObject.SetActive(show);
        }
        
        // 공격 사정거리 보이기
        public void ShowAttackRange(Vector3 pos, float range)
        {
            range *= 2;
            
            attackRange.transform.position = pos;
            attackRange.rectTransform.sizeDelta = new Vector2(range, range);
            attackRange.gameObject.SetActive(true);
        }
        
        // 공격 사정거리 감추기
        public void HideAttackRange()
        {
            attackRange.gameObject.SetActive(false);
        }
        
        // 타워 판매시 골드획득 연출
        public void ShowGoldText(Vector3 pos, int gold)
        {
            GameObject go = SpawnManager.I.GetFloatingText(transform.position);
            FloatingText floatingText = go.GetComponent<FloatingText>();
            floatingText.ShowGold(pos, gold);
        }

        #region Button Event
        
        // 스킬 버튼
        public void OnSkillButtonClick(int skillBtnIdex)
        {
            SkillButton skillButton = GetSkillButton(skillBtnIdex);
            skillButton.button.interactable = false;
            
            GameManager.I.focusTower?.SetFocus(false);
         
            SoundManager.I.PlayOneShot(SoundKey.button_click);
            
            GameManager.I.myPlayer.SkillUse(skillBtnIdex);
        }
        
        // 업그레이드 버튼
        public void OnUpgradeButtonClick(int slotIndex)
        {
            int towerIndex = Account.I.TowerDeck(slotIndex);
            GameManager.I.TowerUpgrade(towerIndex);
        }
        
        // 타워 판매버튼
        public void OnSellButtonClick()
        {
        }
        




        #endregion
    }
}