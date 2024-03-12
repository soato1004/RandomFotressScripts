using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    /// <summary>
    /// 몬스터는 네트웍 객체로 생성된다. 하나가 생성되면, 플레이어와 상대플레이어 두곳에 생성되므로
    /// 죽였을때의 보상만 한곳에서 처리하고, 나머지는 각각의 클라이언트에서 동일하게 연출
    /// </summary>
    public class MonsterBase : EntityBase
    {
        [Header("몬스터 기본능력치")] 
        [SerializeField] private MonsterInfo info;

        [Header("현재 능력치")] 
        public int currentHP;

        [Header("관련오브젝트")] 
        [SerializeField] private HpBar hpBar; // 몬스터 HP 오브젝트

        [Header("디버프 처리")]
        public float moveDebuff = 1f;

        protected List<DebuffBase> _debuffList = new List<DebuffBase>();
        protected int wayPoint; // 어디까지 이동했는지
        protected Vector3 targetPos; // 현재 목표지점
        protected float totalDistance = 0;
        protected Vector3 dir; // 이동 방향

        protected MonsterState monsterState = MonsterState.idle;

        
        protected override void Awake()
        {
            base.Awake();
            hpBar = SpawnManager.Instance.GetHpBar(transform);
        }

        public virtual void Init(GamePlayer targetPlayer, int index, int hp, MonsterType monsterType = MonsterType.None)
        {
            gameObject.SetActive(true);
            player = targetPlayer;

            //TODO: 모드별 몬스터 크기다르게
            if (MainManager.Instance.gameType == GameType.Solo)
            {
                transform.localScale = new Vector3(45, 45, 45);
            }
            else transform.localScale = new Vector3(30, 30, 30);
            
            // 몬스터 초기정보값
            SetInfo(DataManager.Instance.monsterStateDic[index]);
            currentHP = hp;
            
            if (MonsterType.Speed == monsterType)
            {
                info.moveSpeed *= 1.5f;
                transform.localScale = new Vector3(40,40,1);
                SpawnManager.Instance.SpawnMonsterTypeEff(GameConstants.SpeedMonsterEffPrefab, transform);
            }
            else if (MonsterType.Tank == monsterType)
            {
                info.moveSpeed *= 1.05f;
                currentHP *= 2;
                transform.localScale = new Vector3(55,55,1);
                SpawnManager.Instance.SpawnMonsterTypeEff(GameConstants.TankMonsterEffPrefab,transform);
            }
            else if (MonsterType.Boss == monsterType)
            {
                info.monsterType = monsterType;
            }
            
            isDestroyed = false;
            wayPoint = 0;
            SetNextWay();

            SetState(MonsterState.walk);

            // 체력바 설정
            hpBar.Init(this);
            StartCoroutine(MonsterUpdate());
        }

        private void ShowEffect()
        {
            
        }

        private void SetInfo(MonsterData data)
        {
            if (info == null)
                info = new MonsterInfo(data);
            else
                info.SetData(data);
        }

        protected virtual void SetState(MonsterState state) { }

        private IEnumerator MonsterUpdate()
        {
            while (gameObject.activeSelf)
            {
                // 죽거나 게임종료시 빠져나감
                if (monsterState == MonsterState.die || GameManager.Instance.isGameOver )
                    break;
                
                // 일시정지
                if (GameManager.Instance.isPaused)
                {
                    yield return null;
                    continue;
                }

                // 디버프 처리
                moveDebuff = 1f;
                for (int i = 0; i < _debuffList.Count; ++i)
                    _debuffList[i].UpdateDebuff();

                // 이동처리
                float moveFactor = info.moveSpeed * moveDebuff * Time.deltaTime * GameManager.Instance.timeScale;
                transform.position += moveFactor * dir;
                
                // 목표지점 도착시
                totalDistance -= moveFactor;
                if (totalDistance < 0)
                {
                    transform.position = targetPos;
                    
                    wayPoint++;
                    
                    // 목표지까지 도달시 데미지
                    if (wayPoint >= GameManager.Instance.GetWayLength)
                    {
                        int damage = info.monsterType == MonsterType.Boss ? GameConstants.BossDamage : GameConstants.MonsterDamage;
                        player.DamageToPlayer(damage);
                        Remove();
                        break;
                    }
                    
                    SetNextWay();
                }

                yield return null;
            }
            Remove();
        }
        
        protected virtual void SetNextWay()
        {
            targetPos = player.GetNext(wayPoint);
            dir = (targetPos - transform.position).normalized;
            totalDistance = Vector3.Distance(targetPos, transform.position);
            transform.DORotate(new Vector3(0, (dir.x > 0) ? 180 : 0, 0), 0);
        }
        
        public virtual void Hit(int damage, TextType type = TextType.Damage)
        {
            if (monsterState == MonsterState.die)
                return;

            currentHP -= damage;
            
            CreateDamageText(damage, type);

            hpBar.OnSetText(currentHP);

            if (currentHP <= 0)
            {
                StopCoroutine(MonsterUpdate());
                GameUIManager.Instance.UpdateInfo();
                Remove();
            }
        }

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

        public void CreateDamageText(int damage, TextType type = TextType.Damage)
        {
            GameObject go = SpawnManager.Instance.GetFloatingText(transform.position);
            FloatingText floatingText = go.GetComponent<FloatingText>();
            floatingText.Show(transform.position, damage, type);
        }

        protected override void Remove()
        {
            if (isDestroyed)
                return;

            isDestroyed = true;
            
            OnUnitDestroy?.Invoke();
            OnUnitDestroy = null;

            int reward = info.monsterType == MonsterType.Boss ? GameConstants.BossReward : GameConstants.MonsterReward;

            player.KillMonster(reward);
            
            player.monsterList.Remove(this);
            
            transform.DOKill();
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
