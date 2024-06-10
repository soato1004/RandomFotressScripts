using System.Collections;

using RandomFortress.Data;

using UnityEngine;

namespace RandomFortress
{
    public class BulletBase : EntityBase
    {
        protected BulletData BulletData;
        protected MonsterBase Target = null;
        protected int Damage = 1;
        protected TextType textType = TextType.Damage;
        protected Vector3 TargetPos;
        
        public virtual void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values) // params object[] values
        {
            //TODO: 총알부분 제대로 구현필요
            if (DataManager.Instance.bulletDataDic.ContainsKey(index) == false)
                index = 0;
            
            BulletData = DataManager.Instance.bulletDataDic[index];
            
            gameObject.name = BulletData.bulletName;
            
            gameObject.SetActive(true);
            player = gPlayer;
            player.AddBullet(this);
            
            Target = monster;
            DamageInfo damageInfo = (DamageInfo)values[0];
            Damage = damageInfo._damage;
            textType = damageInfo._type;

            
            IsDestroyed = false;

            // 몬스터가 제거될경우 총알의 타겟을 없앤다
            Target.OnUnitDestroy += () => { Target = null; };

            StartCoroutine(UpdateCor());
        }
        
        /// <summary>
        /// 총알 업데이트
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator UpdateCor()
        {
            // 조정된 게임크기에 맞게 이동속도도 보간해야한다
            float mainScale = GameManager.Instance.mainScale;
            
            while (!GameManager.Instance.isGameOver)
            {
                // 일시정지
                if (GameManager.Instance.isPaused)
                {
                    yield return null;
                    continue;
                }

                // 타겟이 사라졌다면, 마지막위치까지 이동후 삭제
                if (Target != null)
                    TargetPos = Target.transform.position;
                
                // 타겟 최종위치로 이동
                Vector3 disVec = (TargetPos - transform.position).normalized; // 방향
                float moveFactor = GameConstants.BulletMoveSpeed * Time.deltaTime * GameManager.Instance.TimeScale * mainScale;
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
            if (Target == null) return;
            
            SoundManager.Instance.PlayOneShot("bullet_hit_base");
            SpawnManager.Instance.GetEffect(BulletData.hitEffName, TargetPos);
            Target.Hit(Damage, textType);
        }
        
        protected override void Remove()
        {
            if (IsDestroyed) return;
            
            IsDestroyed = true;
            player.RemoveBullet(this);
            Release();
        }
    }
}