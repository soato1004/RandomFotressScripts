using DG.Tweening;
using RandomFortress.Common.Extensions;
using TMPro;
using UnityEngine;

namespace RandomFortress.Game
{
    /// <summary>
    /// 데미지이펙트는 로컬객체로 생성된다.
    /// 타워에 연관된 총알, 피격이펙트, 데미지이펙트는 전부 각각의 클라이언트에서 따로 동작한다
    /// </summary>
    public class DamageText : ObjectBase
    {
        // private float moveSpeed;
        // private float alphaSpeed;
        private float destroyTime;
        TextMeshPro text;
        Color alpha;
        [SerializeField] private int damage;

        public void Show(Transform trf, int damage)
        {
            this.damage = damage;
            transform.position = trf.position;
            transform.ExMoveY(50f);
            
            // moveSpeed = 30.0f;
            // alphaSpeed = 2.0f;
            destroyTime = 2.0f;
            
            text = GetComponent<TextMeshPro>();
            alpha = text.color;
            text.text = damage.ToString();
            Invoke("Remove", destroyTime);
            
            alpha.a = 1;
            text.color = alpha;
            float y = transform.position.y + 30f;
            transform.DOMoveY(y,1f);
            text.DOFade(0, 1f);
        }

        protected override void Remove()
        {
            if (isPooling)
                pool.Release(gameObject);
            else
                Destroy(gameObject);
        }
    }
}