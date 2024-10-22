



using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class SkillSlot : SlotBase
    {
        [SerializeField] private int SkillIndex;
        [SerializeField] private Image Icon;
        [SerializeField] private TextMeshProUGUI Name;
        
        private const int TARGET_WIDTH = 150;
        private const int TARGET_HEIGHT = 150;
        
        private const int MAIN_WIDTH = 200;
        private const int MAIN_HEIGHT = 200;
        
        public void UpdateSlot(int slotIndex)
        {
            int skillIndex = Account.I.SkillDeck(slotIndex);
            if (skillIndex == 0)
            {
                Icon.gameObject.SetActive(false);
                return;
            }
            
            Icon.gameObject.SetActive(true);
            
            SkillData data = DataManager.I.skillDataDic[skillIndex];
            
            SkillIndex = skillIndex;
            Icon.sprite = ResourceManager.I.GetSprite(data.skillName);
            Name.text = LocalizationManager.I.GetLocalizedString(data.index.ToString());//data.skillName;

            int width = slotIndex == 0 ? MAIN_WIDTH : TARGET_WIDTH;
            int height = slotIndex == 0 ? MAIN_HEIGHT : TARGET_HEIGHT;
            
            Utils.ImageSizeToFit(width, height, ref Icon);
        }
    }
}