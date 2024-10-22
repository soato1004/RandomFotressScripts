

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class AbilityCard : MonoBehaviour
    {
        public Button button;
        public AbilityData data;
        public Transform Bg;
        public GameObject Select;
        public Image Icon;
        public TextMeshProUGUI Name; // 없을수도있다.
        public GameObject Lock;
        
        public void SetSelect(bool isActive) => Select.SetActive(isActive);
        
        public void SetCard(AbilityData value, bool isLock = false)
        {
            data = value;
            
            foreach (var child in Bg.GetChildren())
                child.gameObject.SetActive(false);
            
            if (Select != null)
                Select.SetActive(false);
            
            Icon.sprite = ResourceManager.I.GetSprite( data.iconName);
            
            if (Name != null)
                Name.SetText(LocalizationManager.I.GetLocalizedString(data.index.ToString()));
            
            Bg.GetChild((int)data.rarity).gameObject.SetActive(true);
            
            if (Lock != null)
                Lock.SetActive(isLock);
            
            
            if (button != null)
                button.interactable = !isLock;

            gameObject.SetActive(true);
        }
    }
}