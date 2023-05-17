using DG.Tweening;
using RandomFortress.Common.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Vector3 = System.Numerics.Vector3;

namespace RandomFortress.Game
{
    public class HpBar : MonoBehaviour
    {
        public UnitBase Target = null;
        public float posY;

        public Slider slider;
        public TextMeshProUGUI hpText;
        // private float moveSpeed;
        // private float alphaSpeed;
        // private float destroyTime;
        // TextMeshPro text;
        // Color alpha;
        // public int damage;

        protected IObjectPool<GameObject> Pool;
        public void SetPool(IObjectPool<GameObject> pool) => Pool = pool;
        
        public void Reset(UnitBase target, float posY = -30f)
        {
            Target = target;
            this.posY = posY;
            transform.position = Target.transform.position;
            transform.ExMoveY(posY);
        }
        
        // Start is called before the first frame update
        void Start()
        {
            // moveSpeed = 30.0f;
            // alphaSpeed = 2.0f;
            // destroyTime = 2.0f;
            //
            //
            // text = GetComponent<TextMeshPro>();
            // alpha = text.color;
            // text.text = damage.ToString();
            // Invoke("DestroyObject", destroyTime);
        }

        // Update is called once per frame
        void Update()
        {
            if (Target == null)
            {
                Debug.Log("Target 설정 안됨");
                return;
            }

            transform.position = Target.transform.position;
            transform.ExMoveY(posY);
            
            // transform.Translate(new Vector3(0, moveSpeed * Time.deltaTime, 0)); // 텍스트 위치
            //
            // alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * alphaSpeed); // 텍스트 알파값
            // text.color = alpha;
        }
        
        public void OnSetText(float value, float duration = 0.25f)
        {
            hpText.text = "" + value;
        }

        public void OnSetHP(float value, float duration = 0.25f)
        {
            // 체력 업데이트
            slider.DOValue(value, duration);

        }
        
        public void Release() => Pool.Release(gameObject);
    }
}