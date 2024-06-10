
using RandomFortress.Data;

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
            SetState(TowerStateType.Attack);
            
            GameObject bulletGo = SpawnManager.Instance.GetBullet(GetBulletStartPos(), Info.bulletIndex);
            StonBullet bullet = bulletGo.GetComponent<StonBullet>();

            DamageInfo damageInfo = GetDamage();
            
            // 확률적용
            int rand = Random.Range(0, 100);
            bool isStun = rand < stunChance; 
            
            object[] paramsArr = { damageInfo, stunDuration, isStun };
            bullet.Init(player, Info.bulletIndex, Target, paramsArr);
            
            //
            TotalDamege += damageInfo._damage;
            
            if (GameManager.Instance.gameType != GameType.Solo)
                player.Shooting(TowerPosIndex, Target.unitID, damageInfo._damage, (int)damageInfo._type, true);   
        }
        
        public override void ReceiveShooting(int unitID, int damage, int damageType, bool isDebuff)
        {
            if (!player.monsterDic.ContainsKey(unitID))
            {
                Debug.Log("Not Found Target!!!");
                return;
            }
            MonsterBase target = player.monsterDic[unitID];
            
            SetState(TowerStateType.Attack);

            GameObject bulletGo = SpawnManager.Instance.GetBullet(GetBulletStartPos(), Info.bulletIndex);
            StonBullet bullet = bulletGo.GetComponent<StonBullet>();
            
            DamageInfo damageInfo = new DamageInfo(damage, (TextType)damageType);
            object[] paramsArr = { damageInfo, stunDuration, isDebuff };
            bullet.Init(player, Info.bulletIndex, target, paramsArr);
        }
    }
}