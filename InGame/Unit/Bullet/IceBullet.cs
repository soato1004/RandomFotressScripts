using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    public class IceBullet : BulletBase
    {
        [SerializeField] protected int iceDuration; // 슬로우 지속시간
        [SerializeField] protected int slowMove; // 슬로우 퍼센트 
            
        public override void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values)
        {
            slowMove = (int)values[1];
            iceDuration = (int)values[2];
            
            base.Init(gPlayer, index, monster, values);
            
            // 시작 이펙트
            SpawnManager.Instance.GetEffect(BulletData.startName, transform.position);
        }
        
        protected override void Hit()
        {
            if (Target == null)
                return;
            
            AudioManager.Instance.PlayOneShot("rock_impact_heavy_slam_01");
            SpawnManager.Instance.GetEffect(BulletData.hitName, transform.position);
            Target.Hit(Damage);

            if (Target != null)
            {
                IceDebuff ice = Target.gameObject.AddComponent<IceDebuff>();
                object[] paramsArr = { slowMove, iceDuration };
                ice.Init(paramsArr);
                Target.ApplyDebuff(ice);
            }
        }
    }
}