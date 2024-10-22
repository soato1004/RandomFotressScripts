


using UnityEngine;

namespace RandomFortress
{
    public class ElephantTower : TowerBase
    {
        [Header("개별")]
        [SerializeField] protected int stunDuration; // 스턴 지속시간
        [SerializeField] protected int stunChance; // 스턴확률

        protected override void SetInfo(TowerData data, int tier = 1)
        {
            base.SetInfo(data, tier);
            stunDuration = Info.extraInfo.stunDuration;
            stunChance = Info.extraInfo.stunChance;
        }

        protected override void Shooting()
        {
            //TODO: 확률적용
            int rand = Random.Range(0, 100);
            bool isStun = rand < stunChance; 
            
            DamageInfo damageInfo = GetDamage();
            AddDamage(damageInfo._damage);
            DoShooting(Target, damageInfo, isStun); 
            player.Shooting(TowerPosIndex, Target._unitID, damageInfo._damage, (int)damageInfo._type, true);   
        }
        
        public override void ReceiveShooting(int unitID, int damage, int damageType, bool isDebuff)
        {
            MonsterBase target = player.entityDic[unitID] as MonsterBase;
            DamageInfo damageInfo = new DamageInfo(damage, (TextType)damageType);
            DoShooting(target, damageInfo, isDebuff);
        }
        
        protected void DoShooting(MonsterBase target, DamageInfo damageInfo, bool isDebuff)
        {
            AddDamage(damageInfo._damage);
            
            SetState(TowerStateType.Attack);

            GameObject bulletGo = SpawnManager.I.GetBullet(GetBulletStartPos(), Info.bulletIndex);
            StonBullet bullet = bulletGo.GetComponent<StonBullet>();
            
            object[] paramsArr = { damageInfo, stunDuration, isDebuff };
            bullet.Init(player, Info.bulletIndex, target, paramsArr);
        }
    }
}