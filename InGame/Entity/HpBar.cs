using System.Collections;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class HpBar : EntityBase
    {
        // [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI hpText;
        
        private MonsterBase monster;
        private int maxHP;
        private float posY;

        public void Init(MonsterBase target, float posY = -30f)
        {
            monster = target;
            this.posY = posY;

            maxHP = monster.currentHp;
            OnSetText(maxHP);
            StartCoroutine(UpdateCor());
            
            gameObject.SetActive(true);
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

        protected override void Remove()
        {
            hpText.text = "";
            Release();
            // Destroy(gameObject);
        }
    }
}