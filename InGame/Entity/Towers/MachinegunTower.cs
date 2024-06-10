using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RandomFortress.Data;

using UnityEngine;
using UnityEngine.Rendering;

namespace RandomFortress
{
    public class MachinegunTower : TowerBase
    {
        [Header("개별")]
        [SerializeField] protected int maxAttackableEnemy; // 다중공격 최대 몇마리까지 되는지
        
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
        
        public override void UpgradeTower(int tier)
        {
            base.UpgradeTower(tier);
            
            // 다중공격을 위한 셋업
            _targets = new MonsterBase[maxAttackableEnemy];
        }

        /// <summary>
        /// 컨트롤하는 플레이어만 업데이트하고, 변동상황을 Rpc로 연동. 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator TowerUpdate()
        {
            while (gameObject.activeSelf)
            {
                // 일시정지
                if (GameManager.Instance.isPaused)
                {
                    yield return null;
                    continue;
                }
                
                if (IsDestroyed || GameManager.Instance.isGameOver)
                    break;

                // 1.목표타겟만큼 총알이 발사되고있지 않다면 몬스터 찾기
                SearchMonstersInAttackRange();

                // 2. 사거리 이내에 공격대상이 하나라도 있을경우 총알발사
                UpdateAttackTargetAndShooting();

                ShowDps();

                yield return null;
            }
        }

        protected override void SearchMonstersInAttackRange()
        {
            for (int j = 0; j < maxAttackableEnemy; ++j)
            {
                // 현재 공격 대상이 없다면
                if (_targets[j] == null)
                {
                    // 공격 대상자 찾기
                    SerializedDictionary<int,MonsterBase> monsterList = player.monsterDic;
                    var array = monsterList.Values.ToArray();
                    for(int i=0; i<array.Length; ++i)
                    {
                        MonsterBase monster = array[i];
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

        private const float AttackSpeedMultiplier = 100f;

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
                    ResetAttackTimer();
                    Shooting();
                }
            }
        }

        void UpdateAttackTimer()
        {
            AttackWaitTimer += Time.deltaTime * GameManager.Instance.TimeScale;
        }

        bool IsReadyToAttack()
        {
            float atkSpeed = Info.attackSpeed * ((AttackSpeedMultiplier + player.extraInfo.atkSpeed) / AttackSpeedMultiplier);
            float attackWaitTime = AttackSpeedMultiplier / atkSpeed;
            return AttackWaitTimer >= attackWaitTime;
        }

        void ResetAttackTimer()
        {
            AttackWaitTimer = 0;
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
            
            // 다중총알
            DamageInfo damage = GetDamage();
            for (int j = 0; j < maxAttackableEnemy; ++j)
            {
                if (_targets[j] == null)
                    continue;
                
                GameObject bulletGo = SpawnManager.Instance.GetBullet(GetBulletStartPos(), Info.bulletIndex);
                ShotBullet bullet = bulletGo.GetComponent<ShotBullet>();
                bullet.Init(player, Info.bulletIndex, _targets[j], damage);
                
                if (GameManager.Instance.gameType != GameType.Solo)
                    player.Shooting(TowerPosIndex, _targets[j].unitID, damage._damage, (int)damage._type);   
            }

            //
            TotalDamege += damage._damage;
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
            ShotBullet bullet = bulletGo.GetComponent<ShotBullet>();
            
            DamageInfo damageInfo = new DamageInfo(damage, (TextType)damageType);
            bullet.Init(player, Info.bulletIndex, target, damageInfo);
        }
    }
}