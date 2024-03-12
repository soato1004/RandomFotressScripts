using System;
using System.Collections;
using DG.Tweening;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using RandomFortress.Manager;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace RandomFortress.Game
{
    public class BulletBase : EntityBase
    {
        protected BulletData BulletData;
        protected MonsterBase Target = null;
        protected int Damage = 1;
        protected TextType textType = TextType.Damage;
        protected Vector3 TargetPos;
        
        private float bulletMoveTime = 0.2f;
        
        public virtual void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values)
        {
            //TODO: 총알부분 제대로 구현필요
            if (DataManager.Instance.bulletDataDic.ContainsKey(index) == false)
                index = 0;
            
            BulletData = DataManager.Instance.bulletDataDic[index];
            
            gameObject.name = BulletData.bulletName;
            
            //TODO: 하드코딩. 모드별 크기다르게
            // if (MainManager.Instance.gameType == GameType.Solo)
            // {
            //     transform.localScale = new Vector3(2f, 2f, 2f);
            // }
            
            
            gameObject.SetActive(true);
            player = gPlayer;
            player.AddBullet(this);
            
            Target = monster;
            DamageInfo damageInfo = (DamageInfo)values[0];
            Damage = damageInfo._damage;
            textType = damageInfo._type;

            
            isDestroyed = false;

            // 몬스터가 제거될경우 총알의 타겟을 없앤다
            Target.OnUnitDestroy += () => { Target = null; };
            
            // TODO: 총알 발사 이펙트
            
         
            // 타겟으로 이동
            // Vector3 targetPos = Target.transform.position;
            // transform.DOMove(targetPos, bulletMoveTime);
            // StartCoroutine(HitCor());

            StartCoroutine(UpdateCor());
        }
        
        /// <summary>
        /// 총알 업데이트
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator UpdateCor()
        {
            while (!GameManager.Instance.isGameOver)
            {
                // 일시정지
                if (GameManager.Instance.isPaused)
                {
                    yield return null;
                    continue;
                }
                
                // 타겟이 사라졌다면 최종위치로 이동
                TargetPos = Target != null ? Target.transform.position : TargetPos;
                
                // 타겟 최종위치로 이동
                Vector3 disVec = (TargetPos - transform.position).normalized; // 방향
                float moveFactor = GameConstants.BulletMoveSpeed * Time.deltaTime * GameManager.Instance.timeScale;
                transform.position += moveFactor * disVec;
        
                float distance = Vector3.Distance(TargetPos, transform.position);
                if (distance < 30f)
                {
                    StartCoroutine(HitCor());
                    yield break;
                }
                
                yield return null;
            }
        }

        protected virtual IEnumerator HitCor()
        {
            Hit();
            
            yield return null;
            
            Remove();
        }

        protected virtual void Hit()
        {
            if (Target == null)
                return;
            
            SoundManager.Instance.PlayOneShot("bullet_hit_base");
            // TODO: 총알 적중 이펙트
            // SpawnManager.Instance.GetEffect(BulletData.hitName, transform.position);
            Target.Hit(Damage, textType);
        }
        
        protected override void Remove()
        {
            if (isDestroyed)
                return;
            
            isDestroyed = true;
            player.RemoveBullet(this);
            Release();
            // Destroy(gameObject);
        }
    }
}