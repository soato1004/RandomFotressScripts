using System.Collections;
using System.Collections.Generic;
using RandomFortress.Common.Utils;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    public class ToxicBullet : BulletBase
    {
        [SerializeField] protected GameObject bulletImage;
        [SerializeField] private ToxicStiky toxicStiky;
        
        protected int poisonDamage; // 독 총 데미지 
        protected int poisonDuration; // 독 지속시간. 100 = 1초
        private PoisonDebuff poisonDebuff;
        
        // public float movementSpeed = 5f; // 이동 속도
        public float rotationSpeed = 300f; // 회전 속도

        public override void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values)
        {
            base.Init(gPlayer, index, monster, values);
         
            bulletImage = transform.GetChild(0).gameObject;
            
            poisonDamage = (int)values[1];
            poisonDuration = (int)values[2];
            
            bulletImage.gameObject.SetActive(true);
            toxicStiky.gameObject.SetActive(false);
            
            // 시작 이펙트
            // SpawnManager.Instance.GetEffect(BulletData.startEffName, transform.position);
        }
        
        protected override IEnumerator HitCor()
        {
            Hit();
            
            yield return null;

            CreateToxicStiky();
            
            yield return new WaitForSeconds(poisonDuration/100);
            
            Remove();
        }
        
        protected override void Hit()
        {
            if (Target == null)
                return;
            
            // Target.Hit(Damage, textType);
            
            // 피격 이펙트
            SoundManager.Instance.PlayOneShot("bullet_hit_base");
            TargetPos.y += 8f;
            GameObject go = SpawnManager.Instance.GetEffect(BulletData.hitEffName, TargetPos);
            
            // TODO: 모드별 크기다르게

            
        }
        
        private void CreateToxicStiky()
        {
            bulletImage.gameObject.SetActive(false);
            toxicStiky.gameObject.SetActive(true);
            
            // 피격 지역에 독웅덩이를 생성
            GameObject go = toxicStiky.gameObject;
            go.SetActive(true);
                
            // TODO: 모드별 크기다르게
            if (MainManager.Instance.gameType == GameType.Solo)
            {
                go.transform.localScale = new Vector3(1.333f, 1.333f, 1.333f);
            }

            TargetPos.y -= 7f;
            go.transform.position = TargetPos;
            
            object[] paramsArr = { poisonDamage, poisonDuration };
            toxicStiky.Init(player, paramsArr);
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