
using RandomFortress.Data;

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
            SetState(TowerStateType.Attack);
            
            GameObject bulletGo = SpawnManager.Instance.GetBullet(GetBulletStartPos(), Info.bulletIndex);
            DrumBallBullet bullet = bulletGo.GetComponent<DrumBallBullet>();
            
            DamageInfo damageInfo = GetDamage();
            object[] paramsArr = { damageInfo, attackArea };
            bullet.Init(player, Info.bulletIndex, Target, paramsArr);
            
            //
            TotalDamege += damageInfo._damage;
            
            if (GameManager.Instance.gameType != GameType.Solo)
                player.Shooting(TowerPosIndex, Target.unitID, damageInfo._damage, (int)damageInfo._type);   
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
            DrumBallBullet bullet = bulletGo.GetComponent<DrumBallBullet>();
            
            DamageInfo damageInfo = new DamageInfo(damage, (TextType)damageType);
            object[] paramsArr = { damageInfo, attackArea };
            bullet.Init(player, Info.bulletIndex, target, paramsArr);
        }
    }
}