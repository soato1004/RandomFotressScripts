

using UnityEngine;

namespace RandomFortress
{
    public class IceBullet : BulletBase
    {
        [SerializeField] protected int buffDuration; // 슬로우 지속시간
        [SerializeField] protected int slowMove; // 슬로우 퍼센트 
        [SerializeField] DebuffIndex debuffIndex = DebuffIndex.Ice;
            
        public override void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values)
        {
            slowMove = (int)values[1];
            buffDuration = (int)values[2];
            
            if (monster.monsterType == MonsterType.Boss)
            {
                buffDuration /= 2;
            }
            
            base.Init(gPlayer, index, monster, values);
            
            // 시작 이펙트
            // SpawnManager.Instance.GetEffect(BulletData.startEffName, transform.position);
        }
        
        protected override void Hit()
        {
            if (Target == null) return;
            
            SoundManager.Instance.PlayOneShot("bullet_hit_base");
            SpawnManager.Instance.GetEffect(BulletData.hitEffName, transform.position);
            Target.Hit(Damage, textType);
            
            if (Target == null) return;
            
            // 동일버프가 있을경우 지우고
            DebuffBase debuff = Target.GetDebuff(debuffIndex);
            if (debuff != null)
                debuff.Remove();
            

            // 신규적용
            IceDebuff ice = Target.gameObject.AddComponent<IceDebuff>();
            object[] paramsArr = { buffDuration, slowMove, debuffIndex };
            ice.Init(paramsArr);
            Target.ApplyDebuff(ice);
        }
    }
}