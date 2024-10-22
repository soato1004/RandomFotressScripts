using UnityEngine;

namespace RandomFortress
{
    // 불타워 총알
    public class BurnBullet : BulletBase
    {
        [SerializeField] protected int buffDuration; // 지속시간
        [SerializeField] protected int burnDamage; // 틱 화상 데미지
        [SerializeField] protected int tickTime; // 틱 시간. 100 = 1초
        
        [SerializeField] DebuffIndex debuffIndex = DebuffIndex.Burn;
        
        [SerializeField] protected TowerBase attacker;
        
        public override void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values)
        {
            buffDuration = (int)values[1];
            burnDamage = (int)values[2];
            tickTime = (int)values[3];
            attacker = (TowerBase)values[4];
            
            base.Init(gPlayer, index, monster, values);
        }
        
        protected override void Hit()
        {
            if (Target == null) return;
            
            SoundManager.I.PlayOneShot(SoundKey.bullet_hit_base);
            SpawnManager.I.GetBulletEffect(BulletData.hitEffName, transform.position);
            Target.Hit(Damage, textType);
            
            // 위의 데미지로 몬스터가 죽을수도 있다.
            if (Target == null) return;
            
            // 동일버프가 있을경우 지우고
            // DebuffBase debuff = Target.GetDebuff(debuffIndex);
            // if (debuff != null)
            //     debuff.Remove();

            // 신규적용
            BurnDebuff burn = Target.gameObject.AddComponent<BurnDebuff>();
            object[] paramsArr = { buffDuration, burnDamage,tickTime, debuffIndex, attacker };
            burn.Init(paramsArr);
            Target.ApplyDebuff(burn);
        }
    }
}