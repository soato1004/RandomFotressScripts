using System.Collections;
using System.Collections.Generic;
using System.Linq;



using UnityEngine;

namespace RandomFortress
{
    public class SwagTower : TowerBase
    { 
        [Header("개별")]
        //TODO: 이부분도 타워데이터 정의에서 결정
        [SerializeField] protected int buffDuration; // 슬로우 지속시간
        [SerializeField] protected int slowMove; // 슬로우 퍼센트 
        private DebuffIndex debuffIndex = DebuffIndex.Ice;
        
        public override void Init(GamePlayer targetPlayer, int posIndex, int towerIndex, int tier)
        {
            base.Init(targetPlayer, posIndex, towerIndex, tier);
        }
        
        protected override void SetInfo(TowerData data, int tier = 1)
        {
            base.SetInfo(data, tier);
            buffDuration = Info.extraInfo.slowDuration;
            slowMove = Info.extraInfo.slowPercent;
        }
        
        protected override IEnumerator TowerUpdate()
        {
            while (gameObject.activeSelf)
            {
                // 일시정지
                if (GameManager.I.isPaused)
                {
                    yield return null;
                    continue;
                }
                
                // 게임오버 또는 타워 제거시
                if (IsDestroyed || GameManager.I.isGameOver)
                    break;
                
                // 공격 딜레이 대기
                AttackWaitTimer += Time.deltaTime * GameManager.I.gameSpeed;
                if (AttackWaitTimer >= (100 / (float)Info.attackSpeed))
                {
                    // 공격 시점에 사정거리 이내의 타겟 찾기
                    SearchMonstersInAttackRange();

                    if (Target != null)
                    {
                        AttackWaitTimer = 0;
                        Shooting();   
                    }
                }

                ShowDps();

                yield return null;
            }
            
            // 게임오버시 애니메이션 정지
            if ( spineBody != null)
                spineBody.AnimationState.TimeScale = 0;
        }
        
        //TODO: 공격시 슬로우버프 여부와 이동순서를 기반으로 공격
        protected override void SearchMonstersInAttackRange()
        {
            MonsterBase safeTarget = null;
            float safeDistance = 0;
            
            // 공격 대상자 찾기
            var array = player.monsterList.ToArray();
            for(int i=0; i<array.Length; ++i)
            {
                MonsterBase monster = array[i];
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
        
        protected override void DoShooting(MonsterBase target, DamageInfo damageInfo)
        {
            AddDamage(damageInfo._damage);
            
            SetState(TowerStateType.Attack);

            GameObject bulletGo = SpawnManager.I.GetBullet(GetBulletStartPos(), info.bulletIndex);
            IceBullet bullet = bulletGo.GetComponent<IceBullet>();
            
            object[] paramsArr = { damageInfo, slowMove, buffDuration };
            bullet.Init(player, Info.bulletIndex, target, paramsArr);
        }
    }
}