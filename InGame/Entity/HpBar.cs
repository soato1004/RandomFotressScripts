using System.Collections;
using TMPro;
using UnityEngine;

namespace RandomFortress
{
    public class HpBar : EntityBase
    {
        [SerializeField] private TextMeshProUGUI hpText;
        
        private MonsterBase target;
        private int maxHP;
        private float posY;

        public override void Reset()
        {
            IsDestroyed = false;
            hpText.text = "";
            hpText.color = new Color32(255, 255, 255, 255);
            transform.position = Vector3.zero;
            gameObject.SetActive(true);
        }
        
        public void Init(MonsterBase monster, float targetY = -30f)
        {
            Reset();
            
            target = monster;
            posY = targetY;
            maxHP = target.currentHp;
            
            OnSetText(maxHP);
            StartCoroutine(UpdateCor());
        }
        
        private IEnumerator UpdateCor()
        {
            while (target != null && target.gameObject != null && !GameManager.I.isGameOver)
            {
                transform.position = target.transform.position;
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
            hpText.SetText(((int)value).ToString());
        }

        protected override void Remove()
        {
            if (IsDestroyed) return;
            IsDestroyed = true;
            
            Release();
        }

    }
}