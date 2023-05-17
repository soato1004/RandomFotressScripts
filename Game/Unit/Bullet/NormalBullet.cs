using System;
using System.Runtime.Serialization;
using RandomFortress.Common.Extensions;
using RandomFortress.Manager;
using UnityEngine;
using UnityEngine.Pool;

namespace RandomFortress.Game
{
    public class NormalBullet : BaseBullet
    {
        // private bool isView = false;
        private float speed = 2000f;
        // private float distance = 0f;
        private float lifeTime = 0f;

        private void Awake()
        {
            
        }

        public override void Init()
        {
            base.Init();
            // isView = true;
        }

        public void Reset()
        {
            lifeTime = 0;
        }

        private void Update()
        {
            //
            if (Target == null)
            {
                Pool.Release(gameObject);
                Reset();
                return;
            }
            
            // 업데이트
            Vector3 _disVec = (Target.transform.position - transform.position).normalized;
            transform.position += _disVec * Time.deltaTime * speed * timeScale;
            
            float distance = Vector3.Distance(transform.position, Target.transform.position);
            if (distance <= 50f)
            {
                SoundManager.Instance.PlayOneShot("DM-CGS-48");
                EffectManager.Instance.ShowHitEffect(bulletIndex, transform.position);
                Target.Hit(damage);
                Pool.Release(gameObject);
                Reset();
                return;
            }
            
            // 라이프타임
            lifeTime += Time.deltaTime;
            if (lifeTime > 5f)
            {
                Debug.Log("5초내로 목표도착 실패");
                Pool.Release(gameObject);
                Reset();
                return;
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.gameObject == Target.gameObject)
            {
                other.collider.GetComponent<MonsterBase>().Hit(damage);
                Pool.Release(gameObject);
                Reset();
            }
        }
    }
}