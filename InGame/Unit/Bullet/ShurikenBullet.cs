using System.Collections;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    /// <summary>
    /// 총알은 로컬객체로 생성된다.
    /// 타워에 연관된 총알, 피격이펙트, 데미지이펙트는 전부 각각의 클라이언트에서 따로 동작한다
    /// </summary>
    public class ShurikenBullet : BulletBase
    {
        public override void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values)
        {
            base.Init(gPlayer, index, monster, values);
            
            // 시작 이펙트
            // SpawnManager.Instance.GetEffect(BulletData.startEffName, transform.position);
        }
    }
}