
using RandomFortress.Data;

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

        protected override void Shooting()
        {
            SetState(TowerStateType.Attack);
            
            GameObject bulletGo = SpawnManager.Instance.GetBullet(GetBulletStartPos(), Info.bulletIndex);
            BurnBullet bullet = bulletGo.GetComponent<BurnBullet>();

            DamageInfo damage = GetDamage();
            object[] paramsArr = { damage, buffDuration, burnDamage, tickTime };
            bullet.Init(player, Info.bulletIndex, Target, paramsArr);
            
            //
            TotalDamege += damage._damage;
            
            if (GameManager.Instance.gameType != GameType.Solo)
                player.Shooting(TowerPosIndex, Target.unitID, damage._damage, (int)damage._type);   
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
            BurnBullet bullet = bulletGo.GetComponent<BurnBullet>();
            
            DamageInfo damageInfo = new DamageInfo(damage, (TextType)damageType);
            object[] paramsArr = { damageInfo, buffDuration, burnDamage, tickTime };
            bullet.Init(player, Info.bulletIndex, target, paramsArr);
        }
    }
}