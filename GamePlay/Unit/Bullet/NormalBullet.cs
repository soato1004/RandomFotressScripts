using System;
using System.Collections;
using System.Runtime.Serialization;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using RandomFortress.Common.Extensions;
using RandomFortress.Game.Netcode;

using RandomFortress.Manager;
using UnityEngine;
using UnityEngine.Pool;

namespace RandomFortress.Game
{
    /// <summary>
    /// 총알은 로컬객체로 생성된다.
    /// 타워에 연관된 총알, 피격이펙트, 데미지이펙트는 전부 각각의 클라이언트에서 따로 동작한다
    /// </summary>
    public class NormalBullet : BulletBase
    {
        private float speed = 2000f;
        private float lifeTime = 0f;
        private Vector3 targetPos;

        public override void Init(GamePlayer gPlayer, int index, MonsterBase monster, int value)
        {
            base.Init(gPlayer, index, monster, value);
            lifeTime = 0;
            StartCoroutine(UpdateCor());
        }

        private IEnumerator UpdateCor()
        {
            while (!GameManager.Instance.isGameOver)
            {
                // 파괴됬다면 빠져나감
                if (isDestroyed || !gameObject.activeSelf || photonView == null)
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
                        CreateHitEffect(transform.position);
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
        
        public void CreateHitEffect(Vector3 pos)
        {
            // TODO: 히트이펙트
            GameObject hitGo = ObjectPoolManager.Instance.GetEffect("Hit ", transform.position, bulletIndex);
            // GameObject prefab = ResourceManager.Instance.GetPrefab("Hit "+bulletIndex);
            // GameObject hitGo = Instantiate(prefab, transform.position, Quaternion.identity);
            // hitGo.transform.SetParent(GameManager.Instance.game.effectParent);
            Destroy(hitGo, 2f);
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

            if (isPooling)
            {
                pool.Release(gameObject);
            }
            else
                Destroy(gameObject);
        }
    }
}