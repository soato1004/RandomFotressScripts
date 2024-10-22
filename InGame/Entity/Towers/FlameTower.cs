


using UnityEngine;

namespace RandomFortress
{
    public class FlameTower : TowerBase
    {
        [Header("개별")]
        [SerializeField] protected int buffDuration; // 지속시간
        [SerializeField] protected int burnDamage; // 틱 화상 데미지
        [SerializeField] protected int tickTime; // 틱 시간
        
        public override void Init(GamePlayer targetPlayer, int posIndex, int towerIndex, int tier)
        {
            base.Init(targetPlayer, posIndex, towerIndex, tier);
        }
        
        protected override void SetInfo(TowerData data, int tier = 1)
        {
            base.SetInfo(data, tier);
            buffDuration = Info.extraInfo.burnDuration;
            burnDamage = Info.extraInfo.burnAtk;
            tickTime = Info.extraInfo.tickTime;
        }
        
        protected override void DoShooting(MonsterBase target, DamageInfo damageInfo)
        {
            AddDamage(damageInfo._damage);
            
            SetState(TowerStateType.Attack);

            GameObject bulletGo = SpawnManager.I.GetBullet(GetBulletStartPos(), Info.bulletIndex);
            BurnBullet bullet = bulletGo.GetComponent<BurnBullet>();
            
            object[] paramsArr = { damageInfo, buffDuration, burnDamage, tickTime, this };
            bullet.Init(player, Info.bulletIndex, target, paramsArr);
        }
    }
}