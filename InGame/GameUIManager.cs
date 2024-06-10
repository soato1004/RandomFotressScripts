using System.Collections;
using DG.Tweening;

using TMPro;
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
        // [Header("배경화면")] 
        // [SerializeField] private Image changeBG;
        // [SerializeField] private Image upBG;
        // [SerializeField] private Image downBG;
        
        [Header("테스트시에만 나오는 버튼")]
        [SerializeField] private GameObject surrender;
        // [SerializeField] private GameObject pause;
        
        [Header("게임내 기능 UI")] 
        [SerializeField] private Image[] skillDim;
        [SerializeField] private Image attackRange;
        [SerializeField] private GameResultPopup gameResultPopup;
        [SerializeField] private AbilityPopup abilityPopup;
        [SerializeField] private Transform playerOutPopup;
        
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
        [SerializeField] private Button gameSpeedButton;
        
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
        
        
        public override void Reset()
        {
            JustDebug.LogColor("GameUIManager Reset");
        }

        // 게임 최초 시작시 한번만
        public void InitializeGameUI()
        {
            // 타워 업그레이드
            UpdateUpgradeBtn();

            bool isSppedOn = GameManager.Instance.gameType == GameType.Solo;
            gameSpeedButton.gameObject.SetActive(isSppedOn);

            if (GameManager.Instance.gameType == GameType.Solo)
            {
                GameManager.Instance.myPlayer.HpTrf = HpTrf;
                GameManager.Instance.myPlayer.stageText = stageText;
            }

#if UNITY_EDITOR
            surrender.SetActive(true);
            // pause.SetActive(true);
#endif
        }
        
        public void UpdateInfo()
        {
            GamePlayer myPlayer = GameManager.Instance.myPlayer;
            GamePlayer otherPlayer = GameManager.Instance.otherPlayer;
            
            // 게임머니 업데이트
            goldText.text = myPlayer.gold.ToString();
            
            // TODO: 판매기능 보류
            // TowerBase focusTower = GameManager.Instance.focusTower;
            // sellBtn.gameObject.SetActive(GameManager.Instance.isFocus);
            // if (GameManager.Instance.isFocus && focusTower != null)
            // {
            //     Vector3 targetPos = focusTower.transform.position;
            //     targetPos.y -= 100f;
            //     sellBtn.transform.position = targetPos;
            //     sellMoneyText.text = focusTower.SellPrice.ToString();
            // }
        }
        
        private void SetPlayerHp(GamePlayer player, Transform hp)
        {
            // 체력 업데이트
            for (int i = 0; i < 5; ++i)
            {
                hp.GetChild(i).SetActive(false);
                if (i < player.playerHp)
                    hp.GetChild(i).SetActive(true);
            }
        }
        
        public void UpdateUpgradeBtn()
        {
            // 업그레이드 버튼   
            for (int slotIndex = 0; slotIndex < GameConstants.TOWER_UPGRADE_COUNT; ++slotIndex)
            {
                int towerIndex = Account.Instance.TowerDeck(slotIndex);
                GameObject upgradeBtn = upgradeBtns[slotIndex].gameObject;
                TowerUpgrade upgrade = GameManager.Instance.TowerUpgradeDic[towerIndex];
                
                Image icon = upgradeBtn.transform.GetChild(0).GetComponent<Image>(); // icon, level, gold
                TextMeshProUGUI level = upgradeBtn.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI gold = upgradeBtn.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                
                // TODO: 해당 인덱스 타워의 1티어 모습만 보여줌
                icon.sprite = ResourceManager.Instance.GetTower(towerIndex, 1);
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
        
        public IEnumerator ShowAbilityUI(int loopCount = 1)
        {
            yield return abilityPopup.ShowAbilityCard(loopCount);
        }

        public void AbilityReady(double time)
        {
            StartCoroutine(abilityPopup.WaitForGameStart(time));
        }

        public void ShowResult()
        {
            GameManager.Instance.PauseGame();
            gameResultPopup.ShowResult();
        }

        public void StageStart()
        {
            stageText.text = "Stage " + GameManager.Instance.myPlayer.stageProcess.ToString();
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

        public void ShowSkillDim(bool isMine = true)
        {
            if (GameManager.Instance.gameType == GameType.Solo)
            {
                // isSkillUse = true;
                skillDim[0].DOFade(0.25f, 0.2f).SetUpdate(true);
                skillDim[1].DOFade(1f, 0.2f).SetUpdate(true);
            }
            else
            {
                if (isMine)
                    GameManager.Instance.myPlayer.ShowSkillDim();
                else
                    GameManager.Instance.otherPlayer.ShowSkillDim();
            }

        }
        
        public void HideSkillDim(bool isMine = true)
        {
            if (GameManager.Instance.gameType == GameType.Solo)
            {
                // isSkillUse = true;
                skillDim[0].DOFade(0f, 0.2f).SetUpdate(true);
                skillDim[1].DOFade(0f, 0.2f).SetUpdate(true);
            }
            else
            {
                if (isMine)
                    GameManager.Instance.myPlayer.HideSkillDim();
                else
                    GameManager.Instance.otherPlayer.HideSkillDim();
            }
        }

        public void ShowAttackRange(Vector3 pos, float range)
        {
            range *= 2;
            
            attackRange.transform.position = pos;
            attackRange.rectTransform.sizeDelta = new Vector2(range, range);
            attackRange.gameObject.SetActive(true);
        }
        
        public void HideAttackRange()
        {
            attackRange.gameObject.SetActive(false);
        }
        
        // 타워 판매시 골드획득 연출
        public void ShowGoldText(Vector3 pos, int gold)
        {
            GameObject go = SpawnManager.Instance.GetFloatingText(transform.position);
            FloatingText floatingText = go.GetComponent<FloatingText>();
            floatingText.ShowGold(pos, gold);
        }

        public void SetPlayerOutPopup(bool active)
        {
            playerOutPopup.SetActive(active);
        }


        #region Button Event
        
        // 스킬 버튼
        public void OnSkillButtonClick(int skillBtnIdex)
        {
            SkillButton skillButton = GetSkillButton(skillBtnIdex);
            skillButton.button.interactable = false;
            
            if (GameManager.Instance.isFocus)
                GameManager.Instance.focusTower.SetFocus(false);
         
            SoundManager.Instance.PlayOneShot("button_click");
            
            ShowSkillDim();
            
            GamePlayer player = GameManager.Instance.myPlayer;
            if (player.skillArr[skillBtnIdex].CanUseSkill())
            {
                player.skillArr[skillBtnIdex].SkillStart();
            }
            
            if (GameManager.Instance.gameType != GameType.Solo)
                GameManager.Instance.myPlayer.SkillUse(skillBtnIdex);
        }

        public void SkillUse(int index)
        {
            
        }
        

        // 업그레이드 버튼
        public void OnUpgradeButtonClick(int slotIndex)
        {
            int towerIndex = Account.Instance.TowerDeck(slotIndex);
            GameManager.Instance.TowerUpgrade(towerIndex);
        }
        
        // 타워 판매버튼
        public void OnSellButtonClick()
        {
            GameManager.Instance.myPlayer.SellTower();
        }

        // 게임종료 버튼
        public void OnExitButtonClick()
        {
            if (GameManager.Instance.gameType == GameType.Solo)
                MainManager.Instance.ChangeScene(SceneName.Lobby);
            else
                PhotonManager.Instance.LeaveRoom();
        }

        // 일시정지 버튼
        public void OnPauseButtonClick()
        {
            if (GameManager.Instance.TimeScale == 0)
            {
                HideSkillDim();
                GameManager.Instance.ResumeGame();
            }
            else
            {
                ShowSkillDim();
                GameManager.Instance.PauseGame();
            }
        }

        #endregion
    }
}