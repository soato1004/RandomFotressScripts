using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    public class MaskManTower : TowerBase
    {
        [SerializeField] protected int poisonDuration; // 독 지속시간
        [SerializeField] protected int poisonDamage; // 독 총 데미지
        
        public override void Init(GamePlayer targetPlayer, int posIndex, int towerIndex)
        {
            base.Init(targetPlayer, posIndex, towerIndex);
        }
        
        protected override void SetInfo(TowerData data, int tier = 1)
        {
            base.SetInfo(data, tier);
            poisonDuration = Info.dynamicData[0];
            poisonDamage = Info.dynamicData[1];
        }
        
        protected override void Shooting()
        {
            SetState(TowerStateType.Attack);
            
            GameObject bulletGo = SpawnManager.Instance.GetBullet(GetBulletStartPos(), Info.bulletIndex);
            ToxicBullet bullet = bulletGo.GetComponent<ToxicBullet>();

            DamageInfo damage = Damage();
            object[] paramsArr = { damage, poisonDamage, poisonDuration };
            bullet.Init(player, Info.bulletIndex, Target, paramsArr);
            
            //
            TotalDamege += damage._damage;
        }
    }
}