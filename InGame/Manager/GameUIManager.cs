using System.Collections;
using DG.Tweening;
using RandomFortress.Common;
using RandomFortress.Common.Extensions;
using RandomFortress.Common.Utils;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Game;
using RandomFortress.Game.Netcode;
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
        [SerializeField] private Image[] skillDim;
        [SerializeField] private Image attackRange;
        [SerializeField] private ResultUI resultUI;
        [SerializeField] private AbilityController abilityUI;
        [SerializeField] private TextMeshProUGUI[] StageText;
        
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
        [SerializeField] private TextMeshProUGUI[] myPlayerHP;
        [SerializeField] private TextMeshProUGUI[] otherPlayerHP;
        // [SerializeField] private Slider otherHP;

        public Canvas[] uICanvas; // 0:10order, 1:13order

        // private int bgIndex = 3;
        // private bool isInitialize = false;
        private bool isSkillUse = false;
        
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
            JTDebug.LogColor("GameUIManager Reset");
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
            int i = (int)MainManager.Instance.gameType;
            
            GamePlayer myPlayer = GameManager.Instance.myPlayer;
            
            // 게임머니 업데이트
            goldText.text = myPlayer.gold.ToString();
            
            // 체력 업데이트
            myPlayerHP[i].text = "x " + myPlayer.playerHp;
            
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
            
            if (MainManager.Instance.gameType == GameType.Solo)
            {
                
            }
            else if (MainManager.Instance.gameType == GameType.OneOnOne)
            {
                GamePlayer otherPlayer = GameManager.Instance.otherPlayer;
                
                // 체력 업데이트
                otherPlayerHP[0].text = "x " + otherPlayer.playerHp;
            }
            else if (MainManager.Instance.gameType == GameType.BattleRoyal)
            {
                
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
        
        public IEnumerator ShowAbilityUI()
        {
            yield return abilityUI.ShowAbilityCard();
        }

        public void ShowResult()
        {
            GameManager.Instance.PauseGame();
            resultUI.ShowResult();
        }

        public void ShowStageClearEffect()
        {
            string stageText;
            int i = (int)MainManager.Instance.gameType;
            
            switch (MainManager.Instance.gameType)
            {
                case GameType.Solo:
                    StageText[i].text = GameManager.Instance.myPlayer.stageProcess.ToString();
                    break;
                case GameType.OneOnOne:
                    stageText = "Stage    " + GameManager.Instance.myPlayer.stageProcess + "    Start";
                    StageText[i].text = stageText;
                    // // 배경등장
                    // // sequence.Append(midBG.DOFade(1, 0.1f));
                    //
                    // // 글자 등장
                    // sequence.AppendCallback(() => { midText.text = text; });
                    // // sequence.Append(midText.transform.DOMoveX(0f, 0.25f).From(width));
                    // sequence.AppendInterval(1f);
                    //
                    // // 글자 빠짐
                    // sequence.AppendCallback(() => { midText.text = ""; });
                    // // sequence.Append(midText.transform.DOMoveX(-width, 0.25f));
                    //
                    // // 배경빠짐
                    // // sequence.Append(midBG.DOFade(0, 0.1f));
                    // sequence.Play();
                    break;
                case GameType.BattleRoyal: break;
            }
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

        public void ShowSkillDim()
        {
            isSkillUse = true;
            skillDim[0].DOFade(0.25f, 0.2f).SetUpdate(true);
            skillDim[1].DOFade(1f, 0.2f).SetUpdate(true);
        }
        
        public void HideSkillDim()
        {
            isSkillUse = false;
            skillDim[0].DOFade(0f, 0.2f).SetUpdate(true);
            skillDim[1].DOFade(0f, 0.2f).SetUpdate(true);
        }

        public void ShowAttackRange(Vector3 pos, float range)
        {
            attackRange.transform.position = pos;
            attackRange.rectTransform.sizeDelta = new Vector2(range*2, range*2);
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


        #region Button Event
        
        // 스킬버튼
        public void OnSkillButtonClick(int index)
        {
            if (isSkillUse)
                return;

            if (GameManager.Instance.isFocus)
                GameManager.Instance.focusTower.SetFocus(false);
            
            ShowSkillDim();
            
            SoundManager.Instance.PlayOneShot("button_click");
            GamePlayer player = GameManager.Instance.myPlayer;
            player.skillArr[index].SkillStart();
        }

        // 업그레이드 버튼
        public void OnUpgradeButtonClick(int slotIndex)
        {
            int towerIndex = Account.Instance.TowerDeck(slotIndex);
            GameManager.Instance.TowerUpgrade(towerIndex);
        }
        
        // 영웅타워를 랜덤으로 짓는다
        public void OnBuild_ButtonClick()
        {
            GameManager.Instance.BuildRandomTower();
        }
        
        // 타워 판매버튼
        public void OnSellButtonClick()
        {
            GameManager.Instance.myPlayer.SellTower();
        }

        public void OnExitButtonClick()
        {
            if (MainManager.Instance.gameType == GameType.Solo)
                MainManager.Instance.ChangeScene(SceneName.Lobby);
            else
                PunManager.Instance.LeaveRoom();
        }

        public void OnPauseButtonClick()
        {
            if (GameManager.Instance.timeScale == 0)
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