using System;
using System.Collections;
using DG.Tweening;
using RandomFortress.Data;
using RandomFortress.Manager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace RandomFortress.Game
{
    public class BulletBase : PlayBase
    {
        protected BulletData BulletData;
        protected MonsterBase Target = null;
        protected int Damage = 1;
        protected Vector3 TargetPos;
        // protected float MoveSpeed = 1500f;
        private float bulletMoveTime = 0.2f;
        
        public virtual void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values)
        {
            //TODO: 총알부분 제대로 구현필요
            if (DataManager.Instance.BulletDataDic.ContainsKey(index) == false)
                index = 6;
            
            BulletData = DataManager.Instance.BulletDataDic[index];
            
            gameObject.name = BulletData.bulletName;
            
            // 모드별 크기다르게
            if (MainManager.Instance.gameType == GameType.Solo)
            {
                transform.localScale = new Vector3(2f, 2f, 2f);
            }
            
            
            gameObject.SetActive(true);
            player = gPlayer;
            player.AddBullet(this);
            
            Target = monster;
            Damage = (int)values[0];
            
            isDestroyed = false;
            Destroy(gameObject, 4f);

            // 몬스터가 제거될경우 총알의 타겟을 없앤다
            Target.OnUnitDestroy += () =>
            {
                Target = null;
            };
         
            // 타겟으로 이동
            Vector3 targetPos = Target.transform.position;
            transform.DOMove(targetPos, bulletMoveTime);
            StartCoroutine(HitCor());

            // StartCoroutine(UpdateCor());
        }
        
        /// <summary>
        /// 총알 업데이트
        /// </summary>
        /// <returns></returns>
        // protected virtual IEnumerator UpdateCor()
        // {
        //     while (!GameManager.Instance.isGameOver)
        //     {
        //         // 타겟이 사라졌다면 최종위치로 이동
        //         TargetPos = Target != null ? Target.transform.position : TargetPos;
        //         
        //         // 타겟 최종위치로 이동
        //         Vector3 disVec = (TargetPos - transform.position).normalized; // 방향
        //         float moveFactor = MoveSpeed * Time.deltaTime * timeScale;
        //         transform.position += moveFactor * disVec;
        //
        //         float distance = Vector3.Distance(TargetPos, transform.position);
        //         if (distance < 20f)
        //         {
        //             transform.position = TargetPos;
        //             StartCoroutine(HitCor());
        //             yield break;
        //         }
        //         
        //         yield return null;
        //     }
        // }

        // protected virtual void OnTriggerEnter2D(Collider2D collision)
        // {
        //     if (Target != null && collision.gameObject == Target.gameObject)
        //     {
        //         StartCoroutine(HitCor());
        //     }
        // }

        protected virtual IEnumerator HitCor()
        {
            yield return new WaitForSeconds(0.2f);
            
            Hit();
            
            yield return null;
            
            Remove();
        }

        protected virtual void Hit()
        {
            if (Target == null)
                return;
            
            AudioManager.Instance.PlayOneShot("rock_impact_heavy_slam_01");
            SpawnManager.Instance.GetEffect(BulletData.hitName, transform.position);
            Target.Hit(Damage);
        }
        
        protected override void Remove()
        {
            if (isDestroyed)
                return;
            
            isDestroyed = true;
            // StopCoroutine(UpdateCor());
            player.RemoveBullet(this);
            Destroy(gameObject);
        }
    }
}