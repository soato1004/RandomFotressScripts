using System.Collections;
using UnityEngine;

namespace RandomFortress
{
    /// <summary>
    /// 지형에 남는 공격은 피격시 이부분을 따로 호출하여 생성한다
    /// </summary>
    public class StikyBase : EntityBase
    {
        protected float Interval = 20f;
        private CircleCollider2D _collider2D;
        protected TextType textType = TextType.Damage;

        protected override void Awake()
        {
            base.Awake();
            _collider2D = gameObject.GetComponent<CircleCollider2D>();
        }

        public override void Reset()
        {
            
        }
        
        public virtual void Init(GamePlayer gPlayer, params object[] values)
        {
            player = gPlayer;
            StartCoroutine(UpdateCor());
        }

        protected virtual IEnumerator UpdateCor()
        {
            while (gameObject.activeSelf)
            {
                // 일시정지
                if (GameManager.I.isPaused)
                {
                    yield return null;
                    continue;
                }
                
                // 현재 충돌 중인 오브젝트들의 정보를 가져옵니다.
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _collider2D.radius);
                
                foreach (Collider2D collider in colliders)
                {
                    MonsterBase monster = collider.GetComponent<MonsterBase>();
                    if (monster != null)
                        Hit(monster);
                }

                yield return Utils.WaitForSeconds(Interval);
            }
        }

        protected virtual void Hit(MonsterBase monster) { }
    }
}