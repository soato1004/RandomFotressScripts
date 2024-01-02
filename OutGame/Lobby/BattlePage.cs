using System.Collections;
using System.Collections.Generic;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using PageType = RandomFortress.Menu.PageController.PageType;

namespace RandomFortress.Menu
{
    public class BattlePage : BasePage
    {
        [SerializeField] private TextMeshProUGUI mainTowerText;
        [SerializeField] private Image mainTowerImage;
        [SerializeField] private TextMeshProUGUI GameModeText;
        [SerializeField] private GameType currentGameType;
        
        private int currentMainTowerIndex = 0;
        
        private GameType[] gameModes =
        {
            GameType.Solo, GameType.OneOnOne,  GameType.BattleRoyal
        };
        private int modeIndex = 0;
        
        private const int TARGET_WIDTH = 150;
        private const int TARGET_HEIGHT = 150;

        public override void UpdateUI()
        {
            // 메인 캐릭터 변경
            int towerIndex = MainManager.Instance.account.TowerDeck(0);
            if (currentMainTowerIndex == towerIndex)
                return;
            
            TowerData data = DataManager.Instance.TowerDataDic[towerIndex ];
            
            currentMainTowerIndex = towerIndex;
            mainTowerImage.sprite = ResourceManager.Instance.GetTower(data.index, data.tier);
            mainTowerText.text = data.towerName;
            Common.Utils.ImageUtils.ImageSizeToFit(TARGET_WIDTH, TARGET_HEIGHT, ref mainTowerImage);
        }
        
        void UpdateGameMode()
        {
            // 모드 변경
            switch (modeIndex)
            {
                case 0: GameModeText.text = "Solo"; break;
                case 1: GameModeText.text = "1vs1"; break;
                case 2: GameModeText.text = "BattleRoyal"; break;
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
    }
}
