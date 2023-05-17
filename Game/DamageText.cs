using System.Collections;
using DG.Tweening;
using RandomFortress.Common.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace RandomFortress.Game
{
    public class DamageText : MonoBehaviour
    {
        // private float moveSpeed;
        // private float alphaSpeed;
        private float destroyTime;
        TextMeshPro text;
        Color alpha;
        public int damage;

        protected IObjectPool<GameObject> Pool;
        public void SetPool(IObjectPool<GameObject> pool) => Pool = pool;

        public void Show(Transform trf)
        {
            transform.position = trf.position;
            transform.ExMoveY(50f);
            
            // moveSpeed = 30.0f;
            // alphaSpeed = 2.0f;
            destroyTime = 2.0f;
            
            text = GetComponent<TextMeshPro>();
            alpha = text.color;
            text.text = damage.ToString();
            Invoke("DestroyObject", destroyTime);
            
            alpha.a = 1;
            text.color = alpha;
            float y = transform.position.y + 30f;
            transform.DOMoveY(y,1f);
            text.DOFade(0, 1f);
        }

        private void DestroyObject()
        {
            Pool.Release(gameObject);
            // Destroy(gameObject);
        }
    }
}