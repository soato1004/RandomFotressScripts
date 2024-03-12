using System.Collections;
using System.Collections.Generic;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    public class MachinegunTower : TowerBase
    {
        [SerializeField] protected int maxAttackableEnemy; // 다중공격 최대 몇마리까지 되는지
        
        private MonsterBase[] _targets = null;
        
        public override void Init(GamePlayer targetPlayer, int posIndex, int towerIndex)
        {
            // 초기설정
            player = targetPlayer;
            TowerPosIndex = posIndex;
            SetInfo(DataManager.Instance.GetTowerData(towerIndex));

            // 타워 내부정보 리셋
            Reset();

            // 다중공격을 위한 셋업
            _targets = new MonsterBase[maxAttackableEnemy];
            
            // 코루틴으로 업데이트 처리
            StartCoroutine(TowerUpdate());
            
        }
        
        protected override void SetInfo(TowerData data, int tier = 1)
        {
            base.SetInfo(data, tier);
            maxAttackableEnemy = Info.dynamicData[0];
        }
        
        public override void UpgradeTower()
        {
            base.UpgradeTower();
            
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
                
                if (isDestroyed || GameManager.Instance.isGameOver)
                    break;

                // 1.목표타겟만큼 총알이 발사되고있지 않다면 몬스터 찾기
                SearchMonstersInAttackRange();

                // 2. 사거리 이내에 공격대상이 하나라도 있을경우 총알발사
                UpdateAttackTargetAndShooting();

                ShowDps();

                yield return null;
            }
            
            if (GameManager.Instance.isGameOver)
                player.AddTotalDamage(Info.index, TotalDamege, Info.tier);
        }

        protected override void SearchMonstersInAttackRange()
        {
            for (int j = 0; j < maxAttackableEnemy; ++j)
            {
                // 현재 공격 대상이 없다면
                if (_targets[j] == null)
                {
                    // 공격 대상자 찾기
                    List<MonsterBase> monsterList = player.monsterList;
                    int length = monsterList.Count;
                    for (int i = 0; i < length; ++i)
                    {
                        MonsterBase monster = monsterList[i];
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
                if (_targets[j] == null)
                    continue;
                        
                // 사거리 체크
                float distance = Vector3.Distance(transform.position, _targets[j].transform.position);
                if (distance > Info.attackRange || !_targets[j].gameObject.activeSelf)
                    _targets[j] = null;
                else
                    isShooting = true;
            }

            // 3. 총알 발사시에 여러발을쏜다
            if (isShooting)
            {
                AttackWaitTimer += Time.deltaTime * GameManager.Instance.timeScale;
                float atkSpeed = Info.attackSpeed * ((100 + player.extraInfo.atkSpeed) / 100f);
                float attackWaitTime = 100f / atkSpeed;
                if (AttackWaitTimer >= attackWaitTime)
                {
                    AttackWaitTimer = 0;
                    Shooting();
                }
            }
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
            DamageInfo damage = Damage();
            for (int j = 0; j < maxAttackableEnemy; ++j)
            {
                if (_targets[j] == null)
                    continue;
                
                GameObject bulletGo = SpawnManager.Instance.GetBullet(GetBulletStartPos(), Info.bulletIndex);
                ShotBullet bullet = bulletGo.GetComponent<ShotBullet>();
                bullet.Init(player, Info.bulletIndex, _targets[j], damage);
            }

            //
            TotalDamege += damage._damage;
        }
    }
}