using System.Collections.Generic;




namespace RandomFortress
{
    public class TowerUpgrade
    {
        public int TowerIndex;
        public int TowerUpgradeLv;
        public float CardLvBuff;
        public int UpgradeCost;
        public List<CardUpgradeInfo> CardLvData { get; private set; }  // 카드 업그레이드 정보
        public List<TowerUpgradeInfo> UpgradeData { get; private set; } // 인게임 타워 업그레이드 수치
        
        public TowerUpgrade(int towerIndex, int cardLv)
        {
            TowerIndex = towerIndex;
            TowerUpgradeLv = 0;
            UpgradeCost = GameConstants.TowerCost;

            TowerUpgradeData data = DataManager.I.towerUpgradeData;
            CardLvData = data.CardLvData;
            UpgradeData = data.UpgradeData;
            
            // TODO : 해당 부분은 타워별로 파싱하는부분을 따로둔다
            CardLvBuff = cardLv == 1 ? 1 : ((float)CardLvData[cardLv - 1].CardLVData[0] / 100); // 현재 어택데미지로 되어있음
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