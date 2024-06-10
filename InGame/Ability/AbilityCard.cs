using RandomFortress.Data;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class AbilityCard : MonoBehaviour
    {
        public AbilityData data;

        public Transform Bg;
        public GameObject Select;
        public Image Icon;
        public TextMeshProUGUI Name;
        public GameObject Lock;
        
        // public bool isLock = false;

        void Reset()
        {
            // 초기화
            foreach (var trf in Bg.GetChildren())
            {
                trf.gameObject.SetActive(false);
            }   
        }

        public void SetSelect(bool isActive) => Select.SetActive(isActive);
        
        public void SetCard(AbilityData value, bool isLock = false)
        {
            Reset();
            data = value;
            Icon.sprite = ResourceManager.Instance.GetSprite(data.iconName);
            if (Name != null)
                Name.text = data.abilityName;
            
            Bg.GetChild((int)data.rarity).gameObject.SetActive(true);

            if (Lock != null)
            {
                Lock.SetActive(isLock);
                // this.isLock = isLock;
            }
            
            gameObject.SetActive(true);
        }
        
        // 화면에 보여주는 어빌카드 세팅
        public void SetCard(AbilityCard card)
        {
            Reset();
            
            data = card.data;
            
            Icon.sprite = card.Icon.sprite;
            if (Name != null)
                Name.text = data.abilityName;
            
            Bg.GetChild((int)data.rarity).gameObject.SetActive(true);
            gameObject.SetActive(true);
        }
    }
}