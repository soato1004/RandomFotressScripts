using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RandomFortress
{
    public class BattlePage : BasePage
    {
        [SerializeField] private TextMeshProUGUI mainTowerText;
        [SerializeField] private Image mainTowerImage;
        [SerializeField] private TextMeshProUGUI GameModeText;
        [SerializeField] private GameType currentGameType;
        [SerializeField] private List<AdDebuffCard> adDebuffs; // 광고버프 0:어빌리티카드 선택지
        [SerializeField] private List<PassButton> passButtons;
        

        
        private int currentMainTowerIndex = 0;
        private int modeIndex = 0;
        
        private const int TARGET_WIDTH = 400;
        private const int TARGET_HEIGHT = 400;
        
        public override void UpdateUI()
        {
            // 메인 캐릭터 변경
            int towerIndex = Account.I.TowerDeck(0);
            if (currentMainTowerIndex != towerIndex)
            {
                TowerData data = DataManager.I.GetTowerData(towerIndex);
            
                currentMainTowerIndex = towerIndex;
                mainTowerImage.sprite = ResourceManager.I.GetTower(data.index, 1);
                mainTowerText.text = LocalizationManager.I.GetLocalizedString(data.index.ToString());//data.name;
                Utils.ImageSizeToFit(TARGET_WIDTH, TARGET_HEIGHT, ref mainTowerImage);
            }

            // 광고제거 구입시 영구 버프적용되므로 필요없음.
            if (Account.I.Data.hasSuperPass)
                foreach (var adDebuff in adDebuffs)
                    adDebuff.gameObject.SetActive(false);
            
            // 패스아이템 적용
            passButtons[(int)PassType.SuperPass].SetApplyPass(Account.I.Data.hasSuperPass);
        }
        
        void UpdateGameMode()
        {
            // 모드 변경
            switch (modeIndex)
            {
                case 0: GameModeText.text = DataManager.I.stringTableDic["lobby_mode_01"]; break;
                case 1: GameModeText.text = DataManager.I.stringTableDic["lobby_mode_02"]; break;
                case 2: GameModeText.text = DataManager.I.stringTableDic["lobby_mode_03"]; break;
            }
            GameManager.I.gameType = (GameType)(modeIndex);
        }

        public void OnPrevButtonClick()
        {
            modeIndex--;
            if (modeIndex < 0)
                modeIndex = 2;

            UpdateGameMode();
        }
        
        public void OnNextButtonClick()
        {
            modeIndex++;
            if (modeIndex > 2)
                modeIndex = 0;
            
            UpdateGameMode();
        }
        
        // 보상형 광고버프 버튼 설정
        public void SetAdDebuff(AdRewardType type, float waitTime)
        {
            for (int i = 0; i < adDebuffs.Count; ++i)
            {
                AdDebuffCard adDebuff =  adDebuffs[i];
                if (adDebuff.type == type)
                {
                    adDebuff.ShowAdDebuff(waitTime);
                }
            }
        }
        
        #region 구독 인앱결제
        
        // 슈퍼패스 클릭
        public void OnSuperPassClick()
        {
            UnityAction action = SuperPassPurchase;
            PopupManager.I.ShowPopup(PopupNames.SuperPassPopup, action);
        }
        void SuperPassPurchase()
        {
            IAPManager.I.PurchaseStart(IAPManager.superPass);
        }
        
        // 광고제거 클릭
        public void OnRemoveAdClick()
        {
        }
        
        #endregion
        
    }
}
