using System.Collections.Generic;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    public class TowerUpgrade : MonoBehaviour
    {
        public int TowerIndex;
        public int TowerUpgradeLv;
        public float CardLvBuff;
        public int UpgradeCost;
        public List<CardUpgradeInfo> CardLvData { get; private set; }  // 카드 업그레이드 정보
        public List<TowerUpgradeInfo> UpgradeData { get; private set; } // 인게임 타워 업그레이드 수치
        
        public TowerUpgrade(int towerIndex, int cardLV)
        {
            this.TowerIndex = towerIndex;
            this.TowerUpgradeLv = 0;
            this.UpgradeCost = GameConstants.TowerCost;

            TowerUpgradeData data = DataManager.Instance.towerUpgradeData;
            CardLvData = data.CardLvData;
            UpgradeData = data.UpgradeData;
            
            // TODO : 해당 부분은 타워별로 파싱하는부분을 따로둔다
            CardLvBuff = cardLV == 1 ? 1 : ((float)CardLvData[cardLV - 1].CardLVData[0] / 100); // 현재 어택데미지로 되어있음
        }

        public void Upgrade()
        {
            ++TowerUpgradeLv;
            UpgradeCost = TowerUpgradeLv * 100;
        }

        public float GetDamage()
        {
            // 카드레벨 버프 x 업그레이드 버프
            float upgrade = TowerUpgradeLv == 0 ? 1 : ((float)UpgradeData[TowerUpgradeLv - 1].Data[0] / 100);
            return upgrade * CardLvBuff;
            // return 100;
        }
    }
}