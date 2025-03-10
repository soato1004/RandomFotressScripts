



using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class TowerSlot : SlotBase
    {
        [SerializeField] private GameObject SlotEmpty;
        [SerializeField] private GameObject SlotTower;
        
        [SerializeField] private int TowerIndex;
        [SerializeField] private int SlotRank;
        [SerializeField] private Image Frame;
        [SerializeField] private Transform Rank;
        [SerializeField] private Image Icon;
        [SerializeField] private TextMeshProUGUI Name;
        [SerializeField] private TextMeshProUGUI CardLevel;
        [SerializeField] private TextMeshProUGUI CardCount;

        private const int TARGET_WIDTH = 150;
        private const int TARGET_HEIGHT = 160;
        
        private const int MAIN_WIDTH = 260;
        private const int MAIN_HEIGHT = 280;
        
        // private TowerData data;

        public void UpdateSlot(int slotIndex)
        {
            int towerIndex = Account.I.TowerDeck(slotIndex);
            
            if (towerIndex == 0)
            {
                SlotTower.SetActive(false);
                return;
            }

            SlotTower.SetActive(true);
            
            TowerData data = DataManager.I.GetTowerData(towerIndex);
            
            TowerIndex = towerIndex;
            Icon.sprite = ResourceManager.I.GetTower(data.index, 1);
            Name.text = LocalizationManager.I.GetLocalizedString(data.index.ToString());//data.name;
            
            if (SlotRank != -1)
                Rank.GetChild(SlotRank).gameObject.SetActive(true);

            int width = slotIndex == 0 ? MAIN_WIDTH : TARGET_WIDTH;
            int height = slotIndex == 0 ? MAIN_HEIGHT : TARGET_HEIGHT;
            
            Utils.ImageSizeToFit(width, height, ref Icon);
        }
    }
}