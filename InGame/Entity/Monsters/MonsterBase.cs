using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

using Spine.Unity;
using UnityEngine;

namespace RandomFortress
{
    /// <summary>
    /// 몬스터는 로컬객체로 생성된다. 플레이어와 상대플레이어에서 각각 생성된다
    /// </summary>
    public class MonsterBase : EntityBase
    {
        [Header("몬스터 기본능력치")] 
        [SerializeField] protected MonsterInfo info;

        [Header("현재 능력치")] 
        public int currentHp;

        [Header("관련오브젝트")] 
        // 현재 몬스터는 Spine과 Animator를 사용하는 두가지가있다
        [SerializeField] protected SkeletonAnimation spineBody = null;
        [SerializeField] protected Animator anim = null; // 몬스터 모션
        [SerializeField] protected HpBar hpBar; // 몬스터 HP 오브젝트

        [Header("디버프 처리")]
        public float moveDebuff = 1f;

        protected List<DebuffBase> _debuffList = new List<DebuffBase>();
        protected int wayPoint; // 어디까지 이동했는지
        protected Vector3 targetPos; // 현재 목표지점
        protected float totalDistance = 0;
        protected Vector3 dir; // 이동 방향
        protected MonsterState monsterState = MonsterState.idle;
        
        public MonsterType monsterType = MonsterType.None;
        
        public bool UseSpine() => spineBody != null;
        
        public override void Reset()
        {
            IsDestroyed = false;
            wayPoint = 0;
            
            gameObject.SetActive(true);
        }
        
        public virtual void Init(GamePlayer targetPlayer, int index, int hp, MonsterType type)
        {
            Reset();

            player = targetPlayer;
            currentHp = hp;
            monsterType = type;

            SetMonsterScale(monsterType); // 몬스터 크기 설정
            SetInfo(DataManager.I.monsterDataDic[index]); // 몬스터 초기 정보 설정
            AdjustMonsterPropertiesByType(monsterType); // 몬스터 유형에 따른 속성 조정
            SetNextWay();
            SetState(MonsterState.walk);
            
            StartCoroutine(MonsterUpdate());
        }
        
        // 게임 모드 및 속성에 따른 몬스터 크기설정
        private void SetMonsterScale(MonsterType monsterType)
        {
            float baseScale = GameManager.I.gameType == GameType.Solo ? 45 : 30;
            if (spineBody)
            {
                baseScale = GameManager.I.gameType == GameType.Solo ? 21 : 14;
            }

            // 몬스터 유형별 추가 조정
            float scaleModifier = 1.0f;
            switch (monsterType)
            {
                case MonsterType.Speed:
                    scaleModifier = 0.9f;
                    break;
                case MonsterType.Tank:
                    scaleModifier = 1.15f;
                    break;
                case MonsterType.Boss:
                    baseScale = GameManager.I.gameType == GameType.Solo ? 25 : 18;
                    scaleModifier = 0; // 보스는 개별적인 스케일 조정을 하지 않음
                    break;
            }

            if (scaleModifier != 0) // 보스를 제외한 모든 몬스터 유형에 대해 스케일 조정
            {
                float finalScale = baseScale * scaleModifier;
                transform.localScale = new Vector3(finalScale, finalScale, 1);
            }
        }

        // 유닛의 크기 정의
        private void AdjustMonsterPropertiesByType(MonsterType monsterType)
        {
            if (GameManager.I.gameType != GameType.Solo)
                info.moveSpeed -= 50;
            
            switch (monsterType)
            {
                case MonsterType.Speed:
                    info.moveSpeed *= 1.5f;
                    // SpawnManager.Instance.SpawnMonsterTypeEff(GameConstants.SpeedMonsterEffPrefab, transform);
                    break;
                case MonsterType.Tank:
                    info.moveSpeed *= 1.05f;
                    currentHp *= 2;
                    // SpawnManager.Instance.SpawnMonsterTypeEff(GameConstants.TankMonsterEffPrefab, transform);
                    break;
                case MonsterType.Boss:
                    info.monsterType = monsterType;
                    // 보스 몬스터에 대한 추가적인 설정
                    break;
                default:
                    // 기본 몬스터 유형에 대한 처리
                    break;
            }
        }

        private void SetInfo(MonsterData data)
        {
            if (info == null)
                info = new MonsterInfo(data);
            else
                info.SetData(data);
        }

        public virtual void SetState(MonsterState state) { }

        private IEnumerator MonsterUpdate()
        {
            while (gameObject.activeSelf)
            {
                // 죽거나 게임종료시 빠져나감
                if (monsterState == MonsterState.die || GameManager.I.isGameOver )
                    break;
                
                // 일시정지
                if (GameManager.I.isPaused)
                {
                    yield return null;
                    continue;
                }

                // 디버프 처리
                moveDebuff = 1f;
                for (int i = 0; i < _debuffList.Count; ++i)
                    _debuffList[i].UpdateDebuff();

                // 이동처리
                float moveFactor = info.moveSpeed * moveDebuff * Time.deltaTime * GameManager.I.gameSpeed;
                transform.position += moveFactor * dir;
                
                // 목표지점 도착시
                totalDistance -= moveFactor;
                if (totalDistance < 0)
                {
                    transform.position = targetPos;
                    
                    wayPoint++;
                    
                    // 목표지까지 도달시 데미지
                    if (wayPoint >= GameManager.I.GetWayLength)
                    {
                        int damage = info.monsterType == MonsterType.Boss ? GameConstants.BossDamage : GameConstants.MonsterDamage;
                        player.DamageToPlayer(damage);
                        Remove();
                        yield break;
                    }
                    
                    SetNextWay();
                }

                yield return null;
            }
            
            // 게임오버시 애니메이션 정지
            if (spineBody != null)
                spineBody.timeScale = 0;
            if (anim != null)
                anim.speed = 0;
        }
        
        protected virtual void SetNextWay()
        {
            targetPos = player.GetMonsterNextTargetPoint(wayPoint);
            dir = (targetPos - transform.position).normalized;
            totalDistance = Vector3.Distance(targetPos, transform.position);
            transform.DORotate(new Vector3(0, (dir.x > 0) ? 180 : 0, 0), 0);
        }
        
        public void Hit(int damage, TextType type = TextType.Damage)
        {
            if (monsterState == MonsterState.die)
                return;

            currentHp -= damage;
            
            CreateDamageText(damage, type);

            hpBar?.OnSetText(currentHp);

            if (currentHp <= 0)
            {
                Remove();
            }
        }
        
        private void CreateDamageText(int damage, TextType type = TextType.Damage)
        {
            GameObject go = SpawnManager.I.GetFloatingText(transform.position);
            FloatingText floatingText = go.GetComponent<FloatingText>();
            floatingText.Show(transform.position, damage, type);
        }
        
        protected override void Remove()
        {
            if (!player.IsLocalPlayer)
                return;
                
            if (IsDestroyed)
                return;

            IsDestroyed = true;
            
            int reward = info.monsterType == MonsterType.Boss ? GameConstants.BossReward : GameConstants.MonsterReward;
            player.KillMonster(reward);
            
            if (GameManager.I.gameType != GameType.Solo)
                player.MonsterDestroy(_unitID);

            DestroyMonster();
        }
        
        public void DestroyMonster()
        {
            OnUnitDestroy?.Invoke();
            OnUnitDestroy = null;
            
            StopCoroutine(MonsterUpdate());

            player.monsterList.Remove(this);
            player.entityDic.Remove(_unitID);
            transform.DOKill();
            
            GameUIManager.I.UpdateUI();
            
            Release();
        }
        
        #region Debuff

        public void ApplyDebuff(DebuffBase debuffBase)
        {
            _debuffList.Add(debuffBase);
        }
        
        public void RemoveDebuff(DebuffBase target)
        {
            _debuffList.Remove(target);
        }
        
        public bool ContainDebuff(DebuffIndex target)
        {
            foreach (var debuffBase in _debuffList)
            {
                if (debuffBase.debuffIndex == target)
                    return true;
            }
            return false;
        }
        
        public DebuffBase GetDebuff(DebuffIndex target)
        {
            foreach (var debuffBase in _debuffList)
            {
                if (debuffBase.debuffIndex == target)
                    return debuffBase;
            }
            return null;
        }

        public int GetDebuffCount(DebuffIndex target)
        {
            int count = 0;
            foreach (var debuffBase in _debuffList)
            {
                if (debuffBase.debuffIndex == target)
                    ++count;
            }

            return count;
        }

        #endregion
    }
}
