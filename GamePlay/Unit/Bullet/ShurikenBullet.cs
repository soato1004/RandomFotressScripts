using System.Collections;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    /// <summary>
    /// 총알은 로컬객체로 생성된다.
    /// 타워에 연관된 총알, 피격이펙트, 데미지이펙트는 전부 각각의 클라이언트에서 따로 동작한다
    /// </summary>
    public class ShurikenBullet : BulletBase
    {
        private float speed = 2000f;
        private float lifeTime = 0f;
        private Vector3 targetPos;

        public override void Init(GamePlayer gPlayer, int index, MonsterBase monster, int value)
        {
            base.Init(gPlayer, index, monster, value);
            lifeTime = 0;

            // 시작 이펙트
            SpawnManager.Instance.GetEffect(data.startName, transform.position);

            StartCoroutine(UpdateCor());
        }

        private IEnumerator UpdateCor()
        {
            while (!GameManager.Instance.isGameOver)
            {
                // 파괴됬다면 빠져나감
                if (isDestroyed || !gameObject.activeSelf)
                {
                    break;
                }

                // 타겟이 사라졌다면 최종위치로 이동
                if (target != null)
                    targetPos = target.transform.position;
                
                // 타겟 최종위치로 이동
                Vector3 _disVec = (targetPos - transform.position).normalized;
                transform.position += _disVec * Time.deltaTime * speed * timeScale;
                float distance = Vector3.Distance(transform.position, targetPos);
                if (distance <= 50f)
                {
                    AudioManager.Instance.PlayOneShot("rock_impact_heavy_slam_01");
                    if (target != null)
                    {
                        SpawnManager.Instance.GetEffect(data.hitName, transform.position);
                        target.Hit(damage);
                    }
                    break;
                }
                
                // 라이프타임
                lifeTime += Time.deltaTime;
                if (lifeTime > 5f)
                {
                    break;
                }

                yield return null;
            }

            Remove();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.gameObject == target.gameObject)
            {
                other.collider.GetComponent<MonsterBase>().Hit(damage);
                Remove();
            }
        }

        protected override void Remove()
        {
            if (isDestroyed)
                return;
            
            isDestroyed = true;
            Destroy(gameObject);
        }
    }
}