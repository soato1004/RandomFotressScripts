using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    public class DrumbleTower : TowerBase
    {
        [SerializeField] protected int attackArea; // 스플, 스티키 피격범위
        
        public override void Init(GamePlayer targetPlayer, int posIndex, int towerIndex)
        {
            base.Init(targetPlayer, posIndex, towerIndex);
        }
        
        protected override void SetInfo(TowerData data, int tier = 1)
        {
            base.SetInfo(data, tier);
            attackArea = Info.dynamicData[0];
        }

        protected override void Shooting()
        {
            SetState(TowerStateType.Attack);
            
            GameObject bulletGo = SpawnManager.Instance.GetBullet(GetBulletStartPos(), Info.bulletIndex);
            DrumBallBullet bullet = bulletGo.GetComponent<DrumBallBullet>();
            
            DamageInfo damage = Damage();
            object[] paramsArr = { damage, attackArea };
            bullet.Init(player, Info.bulletIndex, Target, paramsArr);
            
            //
            TotalDamege += damage._damage;
        }
    }
}