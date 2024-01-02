using RandomFortress.Data;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Menu
{
    public class ItemSlot : SlotBase
    {
        [SerializeField] private Image Frame;
        [SerializeField] private Image Icon;
        [SerializeField] private TextMeshProUGUI ItemCount;
        [SerializeField] private Image Focus;

        public int itemIndex; // tower or skill Index save
        
        private const int TARGET_WIDTH = 180;
        private const int TARGET_HEIGHT = 180;

        public void SetEmpty()
        {
            Frame.gameObject.SetActive(false);
            Icon.gameObject.SetActive(false);
        }
        
        public void SetTower(TowerData data)
        {
            itemIndex = data.index;
            
            //
            Frame.gameObject.SetActive(true);
            
            
            // 아이콘
            Icon.gameObject.SetActive(true);
            Icon.sprite = ResourceManager.Instance.GetTower(data.index, data.tier);
            
            Common.Utils.ImageUtils.ImageSizeToFit(TARGET_WIDTH, TARGET_HEIGHT, ref Icon);
        }

        public void SetSkill(SkillData data)
        {
            itemIndex = data.index;
            
            //
            Frame.gameObject.SetActive(true);
            
            // 아이콘
            Icon.gameObject.SetActive(true);
            Icon.sprite = ResourceManager.Instance.GetSkill(data.index, data.skillName);

            Common.Utils.ImageUtils.ImageSizeToFit(TARGET_WIDTH, TARGET_HEIGHT, ref Icon);
        }
    }
}