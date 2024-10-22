using UnityEngine;

namespace RandomFortress
{
    /// <summary>
    /// 아이스디버프. 중첩되지않고 한종류에 한번씩만 걸린다
    /// </summary>
    public class IceBullet : BulletBase
    {
        [SerializeField] protected int buffDuration; // 슬로우 지속시간
        [SerializeField] protected int slowMove; // 슬로우 퍼센트 
        [SerializeField] DebuffIndex debuffIndex = DebuffIndex.Ice;
            
        public override void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values)
        {
            slowMove = (int)values[1];
            buffDuration = (int)values[2];
            
            //TODO: 보스는 디버프효과를 2배 적게받음
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
            
            SoundManager.I.PlayOneShot(SoundKey.bullet_hit_base);
            SpawnManager.I.GetBulletEffect(BulletData.hitEffName, transform.position);
            Target.Hit(Damage, textType);
            
            // 위의 데미지로 몬스터가 죽을수도 있다
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