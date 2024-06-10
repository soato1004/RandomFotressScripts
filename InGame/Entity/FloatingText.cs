using DG.Tweening;

using TMPro;
using UnityEngine;

namespace RandomFortress
{
    /// <summary>
    /// 데미지이펙트는 로컬객체로 생성된다.
    /// 타워에 연관된 총알, 피격이펙트, 데미지이펙트는 전부 각각의 클라이언트에서 따로 동작한다
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private int damage;
        [SerializeField] private TextType type;
        [SerializeField] private TextMeshProUGUI text;
        
        private Color alpha;
        
        public void Show(Vector3 pos, int value, TextType type)
        {
            this.damage = value;
            transform.position = pos;
            transform.ExMoveY(50f);
            
            text.text = value.ToString();
            
            switch (type)
            {
                case TextType.Damage: break;
                case TextType.DamageCritical: text.color = new Color(255,215,0); break;
                case TextType.DamagePoison: text.color = Color.green; break;
                case TextType.DamageSlow: text.color = Color.cyan; break;
                case TextType.DamageBurn: text.color = Color.red; break;
            }
            
            float y = transform.position.y + 30f;
            transform.DOMoveY(y,1f);
            text.DOFade(0, 1f);

            Destroy(gameObject, 2f);
        }
        
        public void ShowGold(Vector3 pos, int value)
        {
            this.damage = value;
            transform.position = pos;
            transform.ExMoveY(50f);
            
            text.text = "+"+value+"G";
            text.color = Color.yellow;
            
            transform.localScale = 1.5f * Vector3.one;
            
            float y = transform.position.y + 30f;
            transform.DOMoveY(y,1.5f).SetDelay(0.2f);
            text.DOFade(0, 1.5f).SetDelay(0.2f);

            Destroy(gameObject, 2f);
        }
    }
}