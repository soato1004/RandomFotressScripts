using System.Collections.Generic;
using DefaultNamespace;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Game;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Menu
{
    public class BattlePage : BasePage
    {
        [SerializeField] private TextMeshProUGUI mainTowerText;
        [SerializeField] private Image mainTowerImage;
        [SerializeField] private TextMeshProUGUI GameModeText;
        [SerializeField] private GameType currentGameType;
        [SerializeField] private AdDebuff[] adDebuffs; // 광고버프 0:어빌리티카드 선택지
        
        private int currentMainTowerIndex = 0;
        private int modeIndex = 0;
        
        private const int TARGET_WIDTH = 400;
        private const int TARGET_HEIGHT = 400;

        public override void UpdateUI()
        {
            // 메인 캐릭터 변경
            int towerIndex = Account.Instance.TowerDeck(0);
            if (currentMainTowerIndex == towerIndex)
                return;
            
            TowerData data = DataManager.Instance.GetTowerData(towerIndex);
            
            currentMainTowerIndex = towerIndex;
            mainTowerImage.sprite = ResourceManager.Instance.GetTower(data.index, 1);
            mainTowerText.text = data.name;
            Common.Utils.ImageUtils.ImageSizeToFit(TARGET_WIDTH, TARGET_HEIGHT, ref mainTowerImage);
        }
        
        void UpdateGameMode()
        {
            // 모드 변경
            switch (modeIndex)
            {
                case 0: GameModeText.text = DataManager.Instance.stringTableDic["lobby_mode_01"]; break;
                case 1: GameModeText.text = DataManager.Instance.stringTableDic["lobby_mode_02"]; break;
                case 2: GameModeText.text = DataManager.Instance.stringTableDic["lobby_mode_03"]; break;
            }
            MainManager.Instance.gameType = (GameType)modeIndex;
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

        public void SetAdDebuff(AdDebuffType type, float time)
        {
            adDebuffs[(int)type].ShowAdDebuff(time);
        }
    }
}
