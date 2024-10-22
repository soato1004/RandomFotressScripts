


using UnityEngine;

namespace RandomFortress
{
    public class DrumbleTower : TowerBase
    {
        [Header("개별")]
        [SerializeField] protected int attackArea; // 스플, 스티키 피격범위
        
        public override void Init(GamePlayer targetPlayer, int posIndex, int towerIndex, int tier)
        {
            base.Init(targetPlayer, posIndex, towerIndex, tier);
        }
        
        protected override void SetInfo(TowerData data, int tier = 1)
        {
            base.SetInfo(data, tier);
            attackArea = Info.extraInfo.atkRadius;
        }

        protected override void Shooting()
        {
            DamageInfo damageInfo = GetDamage();
            AddDamage(damageInfo._damage);
            DoShooting(Target, damageInfo);
            player.Shooting(TowerPosIndex, Target._unitID, damageInfo._damage, (int)damageInfo._type);   
        }
        
        public override void ReceiveShooting(int unitID, int damage, int damageType, bool isDebuff)
        {
            MonsterBase target = player.entityDic[unitID] as MonsterBase;
            DamageInfo damageInfo = new DamageInfo(damage, (TextType)damageType);
            DoShooting(target, damageInfo);
        }
        
        protected override void DoShooting(MonsterBase target, DamageInfo damageInfo)
        {
            SetState(TowerStateType.Attack);

            GameObject bulletGo = SpawnManager.I.GetBullet(GetBulletStartPos(), Info.bulletIndex);
            DrumBallBullet bullet = bulletGo.GetComponent<DrumBallBullet>();
            
            object[] paramsArr = { damageInfo, attackArea, this };
            bullet.Init(player, Info.bulletIndex, target, paramsArr);
        }
    }
}