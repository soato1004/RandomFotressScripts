

using UnityEngine;
using UnityEngine.Serialization;

namespace RandomFortress
{
    public class StonBullet : BulletBase
    {
        [SerializeField] protected int stunDuration;
        [FormerlySerializedAs("stunChance")] [SerializeField] protected bool isStun;
        [SerializeField] DebuffIndex debuffIndex = DebuffIndex.Stun;
            
        public override void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values)
        {
            stunDuration = (int)values[1];
            isStun = (bool)values[2];
            
            base.Init(gPlayer, index, monster, values);
        }
        
        protected override void Hit()
        {
            if (Target == null) return;
            
            SoundManager.Instance.PlayOneShot("bullet_hit_base");
            SpawnManager.Instance.GetEffect(BulletData.hitEffName, transform.position);
            Target.Hit(Damage, textType);
            
            if (Target == null) return;
            
            // 확률적용
            if (!isStun) return;
            
            // 동일버프가 있을경우 지우고
            DebuffBase debuff = Target.GetDebuff(debuffIndex);
            if (debuff != null)
                debuff.Remove();

            // 신규적용
            StunDebuff stun = Target.gameObject.AddComponent<StunDebuff>();
            object[] paramsArr = { stunDuration, debuffIndex };
            stun.Init(paramsArr);
            Target.ApplyDebuff(stun);
        }
    }
}