using System.Collections;
using System.Collections.Generic;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    public class SwagTower : TowerBase
    { 
        [SerializeField] protected int iceDuration; // 슬로우 지속시간
        [SerializeField] protected int slowMove; // 슬로우 퍼센트 
        
        //TODO: 디버프에 관한 시스템도 구축해야함
        DebuffType debuffType = DebuffType.Ice;
        DebuffIndex debuffIndex = DebuffIndex.Ice;
        
        public override void Init(GamePlayer targetPlayer, int posIndex, int towerIndex)
        {
            base.Init(targetPlayer, posIndex, towerIndex);
        }
        
        protected override void SetInfo(TowerData data, int tier = 1)
        {
            base.SetInfo(data, tier);
            iceDuration = Info.dynamicData[0];
            slowMove = Info.dynamicData[1];
        }
        
        //TODO: 공격시 슬로우버프 여부와 이동순서를 기반으로 공격
        protected override void SearchMonstersInAttackRange()
        {
            MonsterBase safeTarget = null;
            float safeDistance = 0;
            
            // 공격 대상자 찾기
            List<MonsterBase> monsterList = player.monsterList;
            int length = monsterList.Count;
            for (int i = 0; i < length; ++i)
            {
                MonsterBase monster = monsterList[i];
                if (monster == null || monster.gameObject.activeSelf == false)
                    continue;
                    
                // 사정거리 체크
                float distance = Vector3.Distance(transform.position, monster.transform.position);
                if (distance > Info.attackRange)
                    continue;       
                
                // 디버프 체크
                if (!monster.ContainDebuff(debuffIndex))
                {
                    Target = monster;
                    return;
                }

                if (safeTarget == null)
                {
                    safeTarget = monster;
                    safeDistance = distance;
                }
            }

            if (safeTarget != null)
            {
                Target = safeTarget;
            }
        }
        
        protected override void UpdateAttackTargetAndShooting()
        {
            // 사거리 체크
            float distance = Vector3.Distance(transform.position, Target.transform.position);
            if (distance > Info.attackRange || !Target.gameObject.activeSelf)
            {
                Target = null;
            }
            else
            {
                // 공격 딜레이 대기
                AttackWaitTimer += Time.deltaTime * GameManager.Instance.timeScale;
                if (AttackWaitTimer >= (100 / (float)Info.attackSpeed))
                {
                    AttackWaitTimer = 0;
                    Shooting();
                    
                    // 슬로우 디버프를 위해 매번 적을검색
                    SearchMonstersInAttackRange();
                }
            }
        }

        protected override void Shooting()
        {
            SetState(TowerStateType.Attack);
            
            GameObject bulletGo = SpawnManager.Instance.GetBullet(GetBulletStartPos(), Info.bulletIndex);
            IceBullet bullet = bulletGo.GetComponent<IceBullet>();

            DamageInfo damage = Damage();
            object[] paramsArr = { damage, slowMove, iceDuration, debuffType, debuffIndex };
            bullet.Init(player, Info.bulletIndex, Target, paramsArr);
            
            //
            TotalDamege += damage._damage;
        }
    }
}