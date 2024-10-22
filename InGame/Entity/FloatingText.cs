using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace RandomFortress
{
    /// <summary>
    /// 데미지이펙트는 로컬객체로 생성된다.
    /// 타워에 연관된 총알, 피격이펙트, 데미지이펙트는 전부 각각의 클라이언트에서 따로 동작한다
    /// </summary>
    public class FloatingText : EntityBase
    {
        [SerializeField] private int damage;
        [SerializeField] private TextType type;
        [SerializeField] private TextMeshProUGUI text;
        
        private Color alpha;
        private const float lifeTime = 1.2f;
        private const float goldlifeTime = 1.5f;

        public override void Reset()
        {
            IsDestroyed = false;
            text.SetText("");
            text.color = new Color(255f, 255f, 255f, 255f);
            transform.position = Vector3.zero;
            transform.localScale = Vector3.one;
            gameObject.SetActive(true);
        }
        
        public void Show(Vector3 pos, int value, TextType type)
        {
            Reset();
            
            damage = value;
            transform.position = pos;
            transform.ExMoveY(50f);
            
            text.SetText(value.ToString());
            
            switch (type)
            {
                case TextType.Damage: break;
                case TextType.DamageCritical: 
                    text.color = new Color((float)238/255,(float)67/255,(float)39/255); 
                    transform.localScale = 1.5f * Vector3.one; 
                    break;
                case TextType.DamagePoison: text.color = Color.green; break;
                case TextType.DamageSlow: text.color = Color.cyan; break;
                case TextType.DamageBurn: text.color = Color.red; break;
            }

            float x = transform.position.x + Random.Range(-10f, 10f);
            float y = transform.position.y + Random.Range(25f, 35f);
            transform.DOMoveX(x, lifeTime);
            transform.DOMoveY(y,lifeTime);
            text.DOFade(0, lifeTime).SetDelay(0.2f);
            
            StartCoroutine(DestroyCor(lifeTime));
        }
        
        public void ShowGold(Vector3 pos, int value)
        {
            Reset();
            
            damage = value;
            transform.position = pos;
            transform.ExMoveY(50f);
            
            text.SetText("+"+value+"G");
            text.color = Color.yellow;
            
            transform.localScale = 1.5f * Vector3.one;
            
            float y = transform.position.y + 30f;
            transform.DOMoveY(y,goldlifeTime);
            text.DOFade(0, goldlifeTime).SetDelay(0.2f);

            StartCoroutine(DestroyCor(goldlifeTime));
        }

        private IEnumerator DestroyCor(float time)
        {
            yield return new WaitForSeconds(time);
            Remove();
        }

        protected override void Remove()
        {
            if (IsDestroyed) return;
            IsDestroyed = true;
            Release();
        }
    }
}