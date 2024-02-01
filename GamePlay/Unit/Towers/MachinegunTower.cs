using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RandomFortress.Common.Extensions;
using RandomFortress.Data;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    public class MachinegunTower : TowerBase
    {
        protected MonsterBase[] Targets = null;
     
        private BulletData bulletData;
        
        private int maxAttackableEnemy; // 다중공격 최대 몇마리까지 되는지
        private int currentAttackingEnemy; // 현재 공격중인 목표갯수
        // private List<MonsterBase> targetsIndexList = new List<MonsterBase>(); // 총알 발사시에 Targets안에 목표타겟의 인덱스
        
        public override void Init(GamePlayer targetPlayer, int posIndex, int towerIndex)
        {
            gameObject.SetActive(true);
            player = targetPlayer;
            
            isDestroyed = false;
            TowerPosIndex = posIndex;
            
            // 타워 데이터 불러오기
            SetInfo(DataManager.Instance.TowerDataDic[towerIndex]);
            
            bulletData = DataManager.Instance.BulletDataDic[bulletIndex];

            gameObject.name = towerName + " " + tier;

            DOTween.Kill(transform);
            // canAttack = true;
            waitTime = 0f;
            Target = null;
            originPosition = transform.position;
            SetFocus(false);
            
            maxAttackableEnemy = multipleCount[tier-1];
            currentAttackingEnemy = 0;
            Targets = new MonsterBase[maxAttackableEnemy];
            
            // 타워 초기화
            SetBody();

            // 코루틴으로 업데이트 처리
            StartCoroutine(TowerUpdate());
        }

        /// <summary>
        /// 컨트롤하는 플레이어만 업데이트하고, 변동상황을 Rpc로 연동. 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator TowerUpdate()
        {
            float distance;
            const float updateInterval = 0.05f;
            while (gameObject.activeSelf)
            {
                if (isDestroyed || GameManager.Instance.isGameOver)
                    yield break;

                // 1.목표타겟만큼 총알이 발사되고있지 않다면 몬스터 찾기
                for (int j = 0; j < maxAttackableEnemy; ++j)
                {
                    // 현재 공격 대상이 없다면
                    if (Targets[j] == null)
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
                            distance = Vector3.Distance(transform.position, monster.transform.position);
                            if (distance <= attackRange)
                            {
                                // 중복 공격대상인지 체크
                                if (!CheckOverlapWithTargets(monster))
                                {
                                    Targets[j] = monster;
                                    break;
                                }
                            }
                        }
                    }
                }

                // targetsIndexList.Clear();
                
                // 2. 사거리 이내에 공격대상이 하나라도 있을경우 총알발사
                bool isShooting = false;
                for (int j = 0; j < maxAttackableEnemy; ++j)
                {
                    if (Targets[j] == null)
                        continue;
                        
                    // 사거리 체크
                    distance = Vector3.Distance(transform.position, Targets[j].transform.position);
                    if (distance > attackRange || !Targets[j].gameObject.activeSelf)
                    {
                        Targets[j] = null;
                        // SetState(TowerStateType.Idle);
                    }
                    else
                    {
                        // targetsIndexList.Add(j);
                        isShooting = true;
                    }
                }

                // 3. 총알 발사시에 여러발을쏜다
                if (isShooting)
                {
                    waitTime += updateInterval;
                    if (waitTime >= (100 / (float)attackSpeed))
                    {
                        waitTime = 0;
                        Shooting();
                    }
                }
                else
                {
                    SetState(TowerStateType.Idle);
                }



                // dps 표시
                dpsText.text = dps.ToString();
                // dpsTime += updateInterval;
                // if (dpsTime > 1)
                // {
                //     dpsText.text = dps.ToString();
                //     dpsTime = 0;
                //     dps = 0;
                // }

                yield return new WaitForSeconds(updateInterval);
            }
        }
        
        private bool CheckOverlapWithTargets(MonsterBase monsterBase)
        {
            for (int j = 0; j < maxAttackableEnemy; ++j)
            {
                MonsterBase target = Targets[j];
                if (target == monsterBase)
                    return true;
            }

            return false;
        }
                
                
        private void Shooting()
        {
            // TODO: 트윈을 사용한 발사모션. Spine 사용시엔 공격모션으로 대체
            if (!isDragging)
            {
                Vector3 oriPos = transform.position;
                Body.transform.DOLocalMoveX(-15f, 0.08f).SetEase(Ease.OutBack);
                Body.transform.DOMove(oriPos, 0.2f).SetDelay(0.12f);
            }

            // 총알 발사시 타워마다 시작지점이 다르다
            Vector3 flashPos = originPosition;
            flashPos.x += 30.5f;
            flashPos.y += 3.8f;
            flashPos.z -= 50f;

            // 시작 이펙트
            SpawnManager.Instance.GetEffect(bulletData.startName, transform.position);
            
            
            // 다중총알
            for (int j = 0; j < maxAttackableEnemy; ++j)
            {
                if (Targets[j] == null)
                    continue;
                
                GameObject bulletGo = SpawnManager.Instance.GetBullet(flashPos, bulletIndex);
                BulletBase bullet = bulletGo.GetComponent<BulletBase>();
                bullet.Init(player, bulletIndex, Targets[j], attackPower);
            }

            //
            TotalDamege += attackPower;
            dps += attackPower;
        }
    }
}