using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RandomFortress.Game
{
    public class AbilityCard : MonoBehaviour
    {
        public AbilityData data;

        public Transform Bg;
        public GameObject Select;
        public Image Icon;
        public TextMeshProUGUI Name;
        public GameObject Lock;
        
        public bool isLock = false;
        
        public void SetCard(AbilityData data, bool isLock = false)
        {
            this.data = data;
            Icon.sprite = ResourceManager.Instance.GetSprite(data.iconName);
            Name.text = data.abilityName;
            GameObject btnGo = Bg.GetChild((int)data.rarity).gameObject;
            btnGo.SetActive(true);
            Lock.SetActive(isLock);
            this.isLock = isLock;
        }
        
        public void SetCard(AbilityCard card)
        {
            data = card.data;
            Icon.sprite = card.Icon.sprite;
            Name.text = card.Name.text;
            Bg.GetChild((int)data.rarity).gameObject.SetActive(true);
            Lock.SetActive(card.isLock);
            isLock = card.isLock;
            gameObject.SetActive(true);
        }
    }
}