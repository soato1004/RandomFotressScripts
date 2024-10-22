


using UnityEngine;

namespace RandomFortress
{
    /// <summary>
    /// 마스크맨. 독데미지.
    /// </summary>
    public class MaskManTower : TowerBase
    {
        [Header("개별")]
        [SerializeField] protected int poisonDuration; // 독 지속시간
        [SerializeField] protected int poisonDamage; // 틱 독 데미지
        [SerializeField] protected int tickTime; // 틱 시간
        
        public override void Init(GamePlayer targetPlayer, int posIndex, int towerIndex, int tier)
        {
            base.Init(targetPlayer, posIndex, towerIndex, tier);
        }
        
        protected override void SetInfo(TowerData data, int tier = 1)
        {
            base.SetInfo(data, tier);
            poisonDuration = Info.extraInfo.stikyDuration;
            poisonDamage = Info.extraInfo.stikyAtk;
            tickTime = Info.extraInfo.tickTime;
        }
        
        protected override void DoShooting(MonsterBase target, DamageInfo damageInfo)
        {
            AddDamage(damageInfo._damage);
            
            SetState(TowerStateType.Attack);
            
            GameObject bulletGo = SpawnManager.I.GetBullet(GetBulletStartPos(), Info.bulletIndex);
            ToxicBullet bullet = bulletGo.GetComponent<ToxicBullet>();
            
            object[] paramsArr = { damageInfo, poisonDuration, poisonDamage, tickTime, this };
            bullet.Init(player, Info.bulletIndex, target, paramsArr);
        }
    }
}