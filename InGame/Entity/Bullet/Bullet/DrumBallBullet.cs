using System.Linq;

using UnityEngine;

namespace RandomFortress
{
    public class DrumBallBullet : BulletBase
    {
        private float attackArea = 0;
        
        [SerializeField] protected TowerBase attacker;
        
        /// 총알 생성
        public override void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values)
        {
            attackArea = (int)values[1];
            attacker = (TowerBase)values[2];
            
            base.Init(gPlayer, index, monster, values);
        }

        /// <summary>
        /// 범위내 스플레쉬 데미지
        /// </summary>
        protected override void Hit()
        {
            if (Target == null) return;
            
            // 피격 이펙트
            SpawnManager.I.GetBulletEffect(BulletData.hitEffName, transform.position);
            SoundManager.I.PlayOneShot(SoundKey.bullet_hit_base);
            
            // 딕셔너리 키 배열 생성
            var array = player.monsterList.ToArray();

            for (int i = 0; i < array.Length; i++)
            {
                MonsterBase monster = array[i];
                float distance = Vector3.Distance(monster.transform.position, TargetPos);
                if (distance <= attackArea)
                {
                    attacker.AddDamage(Damage);
                    monster.Hit(Damage, textType);
                } 
            }
        }
    }
}