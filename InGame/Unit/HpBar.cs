using System.Collections;
using RandomFortress.Common.Extensions;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Game
{
    public class HpBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI hpText;
        
        private MonsterBase monster;
        private int maxHP;
        private float posY;

        public void Init(MonsterBase target, float posY = -30f)
        {
            gameObject.SetActive(true);
            
            monster = target;
            this.posY = posY;

            maxHP = monster.currentHP;
            OnSetText(maxHP);
            StartCoroutine(UpdateCor());
        }
        
        private IEnumerator UpdateCor()
        {
            while (monster != null)
            {
                if (GameManager.Instance.isGameOver)
                    break;
                
                transform.position = monster.transform.position;
                transform.ExMoveY(posY);
                yield return null;
            }
            Remove();
        }

        public void OnSetText(float value, float duration = 0.25f)
        {
            float hpParcent = value == 0 ? 0 : value / maxHP;
            byte colorValue = (byte)(50 + (205f * hpParcent));
            hpText.color = new Color32(255, colorValue, colorValue, 255);
            hpText.text = "" + value;
            // slider.DOValue(value, duration);
        }

        protected void Remove()
        {
            Destroy(gameObject);
        }
    }
}