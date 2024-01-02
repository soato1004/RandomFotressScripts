using System.Collections;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    public class ShotBullet : BulletBase
    {
        public override void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values)
        {
            base.Init(gPlayer, index, monster, values);
            
            // 시작 이펙트
            SpawnManager.Instance.GetEffect(BulletData.startName, transform.position);
        }
    }
}