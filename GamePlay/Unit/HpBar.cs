using System.Collections;
using DG.Tweening;
using RandomFortress.Common.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Game
{
    public class HpBar : ObjectBase
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

        public void Reset(UnitBase target, float posY = -30f)
        {
            gameObject.SetActive(true);
            
            Target = target;
            this.posY = posY;
            transform.position = Target.transform.position;
            transform.localScale = Vector3.one;
            transform.ExMoveY(posY);
            StartCoroutine(UpdateCor());
        }
        
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
        
        private IEnumerator UpdateCor()
        {
            while (Target.gameObject.activeSelf)
            {
                transform.position = Target.transform.position;
                transform.ExMoveY(posY);
            
                // transform.Translate(new Vector3(0, moveSpeed * Time.deltaTime, 0)); // 텍스트 위치
                //
                // alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * alphaSpeed); // 텍스트 알파값
                // text.color = alpha;
                
                yield return null;
            }
            gameObject.SetActive(false);
        }
        
        public void OnSetText(float value, float duration = 0.25f)
        {
            hpText.text = "" + value;
        }

        public void OnSetHP(float value, float duration = 0.25f)
        {
            slider.DOValue(value, duration);
        }

        protected override void Remove()
        {
            // 몬스터가 삭제될때 삭제됨
            // if (isPooling)
            //     pool.Release(gameObject);
            // else
            //     Destroy(gameObject);
        }
    }
}