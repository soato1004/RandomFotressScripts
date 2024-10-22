using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace RandomFortress
{
    public class MachinegunTower : TowerBase
    {
        [Header("개별")]
        [SerializeField] protected int maxAttackableEnemy; // 다중공격 최대 몇마리까지 되는지
        
        private const float AttackSpeedMultiplier = 100f;
        
        private MonsterBase[] _targets = null;
        
        public override void Init(GamePlayer targetPlayer, int posIndex, int towerIndex, int tier)
        {
            base.Init(targetPlayer, posIndex, towerIndex, tier);
        }
        
        protected override void SetInfo(TowerData data, int tier = 1)
        {
            base.SetInfo(data, tier);
            maxAttackableEnemy = Info.extraInfo.mulAtk;
            
            // 다중공격을 위한 셋업
            _targets = new MonsterBase[maxAttackableEnemy];
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
                
                if (IsDestroyed || GameManager.I.isGameOver)
                    break;

                // 1.목표타겟만큼 총알이 발사되고있지 않다면 몬스터 찾기
                SearchMonstersInAttackRange();

                // 2. 사거리 이내에 공격대상이 하나라도 있을경우 총알발사
                UpdateAttackTargetAndShooting();

                ShowDps();

                yield return null;
            }
            
            // 게임오버시 애니메이션 정지
            if ( spineBody != null)
                spineBody.AnimationState.TimeScale = 0;
        }

        protected override void SearchMonstersInAttackRange()
        {
            for (int j = 0; j < maxAttackableEnemy; ++j)
            {
                // 현재 공격 대상이 없다면
                if (_targets[j] == null)
                {
                    // 공격 대상자 찾기
                    List<MonsterBase> list = player.monsterList;
                    for(int i=0; i<list.Count; ++i)
                    {
                        MonsterBase monster = list[i];
                        if (monster == null || monster.gameObject.activeSelf == false)
                            continue;

                        // 사거리안에 들어온 몬스터 체크
                        float distance = Vector3.Distance(transform.position, monster.transform.position);
                        if (distance <= Info.attackRange)
                        {
                            // 중복 공격대상인지 체크
                            if (!CheckOverlapWithTargets(monster))
                            {
                                _targets[j] = monster;
                                break;
                            }
                        }
                    }
                }
            }
        }
        
        protected override void UpdateAttackTargetAndShooting()
        {
            bool isShooting = false;
            for (int j = 0; j < maxAttackableEnemy; ++j)
            {
                if (_targets[j] == null || !_targets[j].gameObject.activeSelf)
                    continue;

                float distance = Vector3.Distance(transform.position, _targets[j].transform.position);
                if (distance > Info.attackRange)
                    _targets[j] = null;
                else
                    isShooting = true;
            }

            if (isShooting)
            {
                UpdateAttackTimer();
                if (IsReadyToAttack())
                {
                    AttackWaitTimer = 0;
                    Shooting();
                }
            }
        }

        void UpdateAttackTimer()
        {
            AttackWaitTimer += Time.deltaTime * GameManager.I.gameSpeed;
        }

        bool IsReadyToAttack()
        {
            float atkSpeed = Info.attackSpeed * ((AttackSpeedMultiplier + player.extraInfo.atkSpeed) / AttackSpeedMultiplier);
            float attackWaitTime = AttackSpeedMultiplier / atkSpeed;
            return AttackWaitTimer >= attackWaitTime;
        }
        
        private bool CheckOverlapWithTargets(MonsterBase monsterBase)
        {
            for (int j = 0; j < maxAttackableEnemy; ++j)
            {
                MonsterBase target = _targets[j];
                if (target == monsterBase)
                    return true;
            }

            return false;
        }
                
        protected override void Shooting()
        {
            SetState(TowerStateType.Attack);
            DamageInfo damageInfo = GetDamage();
            
            for (int j = 0; j < maxAttackableEnemy; ++j)
            {
                if (_targets[j] == null)
                    continue;
                
                DoShooting(_targets[j], damageInfo);
                player.Shooting(TowerPosIndex, _targets[j]._unitID, damageInfo._damage, (int)damageInfo._type);   
            }
        }
        
        protected override void DoShooting(MonsterBase target, DamageInfo damageInfo)
        {
            AddDamage(damageInfo._damage);
            
            SetState(TowerStateType.Attack);

            GameObject bulletGo = SpawnManager.I.GetBullet(GetBulletStartPos(), Info.bulletIndex);
            ShotBullet bullet = bulletGo.GetComponent<ShotBullet>();
            
            bullet.Init(player, Info.bulletIndex, target, damageInfo);
        }
    }
}