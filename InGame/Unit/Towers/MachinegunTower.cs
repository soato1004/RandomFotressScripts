using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using Unity.VisualScripting;
using UnityEngine;

namespace RandomFortress.Game
{
    public class MachinegunTower : TowerBase
    {
        [SerializeField] protected int maxAttackableEnemy; // 다중공격 최대 몇마리까지 되는지
        
        protected MonsterBase[] Targets = null;
     
        private BulletData bulletData;
        
        // private int currentAttackingEnemy = 0; // 현재 공격중인 목표갯수
        // private List<MonsterBase> targetsIndexList = new List<MonsterBase>(); // 총알 발사시에 Targets안에 목표타겟의 인덱스

        public override void Init(GamePlayer targetPlayer, int posIndex, int towerIndex)
        {
            // 초기설정
            player = targetPlayer;
            TowerPosIndex = posIndex;
            SetInfo(DataManager.Instance.TowerDataDic[towerIndex]);

            // 타워 내부정보 리셋
            Reset();
            
            // 타워 초기화
            SetBody();
            
            // 다중공격을 위한 셋업
            Targets = new MonsterBase[maxAttackableEnemy];

            // 코루틴으로 업데이트 처리
            StartCoroutine(TowerUpdate());
        }
        
        protected override void SetInfo(TowerData data)
        {
            base.SetInfo(data);
            maxAttackableEnemy = (int)data.dynamicData[0];
        }

        /// <summary>
        /// 컨트롤하는 플레이어만 업데이트하고, 변동상황을 Rpc로 연동. 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator TowerUpdate()
        {

            while (gameObject.activeSelf)
            {
                if (isDestroyed || GameManager.Instance.isGameOver)
                    yield break;

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
                        float distance = Vector3.Distance(transform.position, monster.transform.position);
                        if (distance <= Info.attackRange)
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
        }

        protected override void UpdateAttackTargetAndShooting()
        {
            bool isShooting = false;
            for (int j = 0; j < maxAttackableEnemy; ++j)
            {
                if (Targets[j] == null)
                    continue;
                        
                // 사거리 체크
                float distance = Vector3.Distance(transform.position, Targets[j].transform.position);
                if (distance > Info.attackRange || !Targets[j].gameObject.activeSelf)
                    Targets[j] = null;
                else
                    isShooting = true;
            }

            // 3. 총알 발사시에 여러발을쏜다
            if (isShooting)
            {
                waitTime += Time.deltaTime * timeScale;
                if (waitTime >= (100 / (float)Info.attackSpeed))
                {
                    waitTime = 0;
                    Shooting();
                }
            }
            else
            {
                SetState(TowerStateType.Idle);
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
                
                
        protected override void Shooting()
        {
            // TODO: 트윈을 사용한 발사모션. Spine 사용시엔 공격모션으로 대체
            if (!isDragging)
            {
                Vector3 oriPos = transform.position;
                body.transform.DOLocalMoveX(-15f, 0.08f).SetEase(Ease.OutBack);
                body.transform.DOMove(oriPos, 0.2f).SetDelay(0.12f);
            }

            // 총알 발사시 타워마다 시작지점이 다르다
            Vector3 flashPos = originPosition;
            flashPos.x += 30.5f;
            flashPos.y += 3.8f;
            flashPos.z -= 50f;

            // 다중총알
            for (int j = 0; j < maxAttackableEnemy; ++j)
            {
                if (Targets[j] == null)
                    continue;
                
                GameObject bulletGo = SpawnManager.Instance.GetBullet(flashPos, Info.bulletIndex);
                ShotBullet bullet = bulletGo.GetOrAddComponent<ShotBullet>();
                bullet.Init(player, Info.bulletIndex, Targets[j], Info.attackPower);
            }

            //
            TotalDamege += Info.attackPower;
            dps += Info.attackPower;
        }
    }
}