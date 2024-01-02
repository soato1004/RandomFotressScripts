using System.Collections;
using RandomFortress.Data;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    public class DrumBallBullet : BulletBase
    {
        private float attackArea = 0;

        /// <summary>
        /// 총알 생성
        /// </summary>
        /// <param name="gPlayer"></param>
        /// <param name="index"></param>
        /// <param name="monster"></param>
        /// <param name="value"></param>
        public override void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values)
        {
            attackArea = (int)values[1];
            
            base.Init(gPlayer, index, monster, values);
            
            // 시작 이펙트
            SpawnManager.Instance.GetEffect(BulletData.startName, transform.position);
        }

        /// <summary>
        /// 범위내 스플레쉬 데미지
        /// </summary>
        protected override void Hit()
        {
            if (Target == null)
                return;
            
            // 피격 이펙트
            SpawnManager.Instance.GetEffect(BulletData.hitName, transform.position);
            AudioManager.Instance.PlayOneShot("rock_impact_heavy_slam_01");
            
            //
            for(int i=0; i<player.monsterList.Count; ++i)
            {
                MonsterBase monster = player.monsterList[i];
                float distance = Vector3.Distance(monster.transform.position, TargetPos);
                if (distance <= attackArea)
                {
                    monster.Hit(Damage);
                } 
            }
        }
    }
}