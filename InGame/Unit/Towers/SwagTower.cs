using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RandomFortress.Data;
using RandomFortress.Manager;
using Unity.VisualScripting;
using UnityEngine;

namespace RandomFortress.Game
{
    public class SwagTower : TowerBase
    { 
        [SerializeField] protected int iceDuration; // 슬로우 지속시간
        [SerializeField] protected int slowMove; // 슬로우 퍼센트 
        
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

            // 코루틴으로 업데이트 처리
            StartCoroutine(TowerUpdate());
        }
        
        protected override void SetInfo(TowerData data)
        {
            base.SetInfo(data);
            iceDuration = (int)data.dynamicData[0];
            slowMove = (int)data.dynamicData[1];
        }
        
        protected override IEnumerator TowerUpdate()
        {
            while (gameObject.activeSelf)
            {
                // 게임 일시정지시
                if (timeScale == 0)
                {
                    yield return null;
                    continue;
                }
                
                // 게임오버 또는 타워 제거시
                if (isDestroyed || GameManager.Instance.isGameOver)
                    yield break;

                // 현재 공격 대상이 없다면
                if (Target == null)
                {
                    SearchMonstersInAttackRange();
                }
                else
                {
                    UpdateAttackTargetAndShooting();
                }

                ShowDps();

                yield return null;
            }
        }
        
        protected override void SearchMonstersInAttackRange()
        {
            // 공격 대상자 찾기
            List<MonsterBase> monsterList = player.monsterList;
            int length = monsterList.Count;
            for (int i = 0; i < length; ++i)
            {
                MonsterBase monster = monsterList[i];
                if (monster == null || monster.gameObject.activeSelf == false)
                    continue;
                    
                float distance = Vector3.Distance(transform.position, monster.transform.position);
                if (distance <= Info.attackRange)
                {
                    Target = monster;
                    break;
                }
            }
        }
        
        protected override void UpdateAttackTargetAndShooting()
        {
            // 사거리 체크
            float distance = Vector3.Distance(transform.position, Target.transform.position);
            if (distance > Info.attackRange || !Target.gameObject.activeSelf)
            {
                Target = null;
                SetState(TowerStateType.Idle);
            }
            else
            {
                // 공격 딜레이 대기
                waitTime += Time.deltaTime * timeScale;
                if (waitTime >= (100 / (float)Info.attackSpeed))
                {
                    waitTime = 0;
                    Shooting();
                }
            }
        }

        protected override void Shooting()
        {
            if (!isDragging)
            {
                Vector3 oriPos = transform.position;
                body.transform.DOLocalMoveX(-15f, 0.08f).SetEase(Ease.OutBack);
                body.transform.DOMove(oriPos, 0.2f).SetDelay(0.12f);
            }

            // 총알 발사시 타워마다 시작지점이 다르다
            Vector3 flashPos = originPosition;
            flashPos.x += 23.9f;
            flashPos.y += 1.7f;
            flashPos.z = -50f;
            
            GameObject bulletGo = SpawnManager.Instance.GetBullet(flashPos, Info.bulletIndex);
            IceBullet bullet = bulletGo.GetOrAddComponent<IceBullet>();

            object[] paramsArr = { Info.attackPower, slowMove, iceDuration };
            bullet.Init(player, Info.bulletIndex, Target, paramsArr);
            
            //
            TotalDamege += Info.attackPower;
            dps += Info.attackPower;
        }
    }
}