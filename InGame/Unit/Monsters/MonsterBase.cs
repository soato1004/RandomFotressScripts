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
    public class MonsterBase : PlayBase
    {
        [SerializeField] private Animator anim; // 몬스터 모션

        [Header("타워 기본능력치")] 
        [SerializeField] private int index;
        [SerializeField] private string unitName;
        [SerializeField] private int hp;
        [SerializeField] private int moveSpeed;
        [SerializeField] private int monsterType;
        [SerializeField] private int attackDamage;
        [SerializeField] private int reward;

        [Header("현재 능력치")] 
        public int currentHP;

        [Header("관련오브젝트")] 
        [SerializeField] private HpBar hpBar; // 몬스터 HP 오브젝트

        [Header("디버프 처리")]
        public float moveDebuff = 1f;

        private List<DebuffBase> _debuffList = new List<DebuffBase>();

        private int wayPoint; // 어디까지 이동했는지
        private Vector3 targetPos; // 현재 목표지점
        float totalDistance = 0;
        private Vector3 dir; // 이동 방향

        public GameConstants.MonsterState monsterState = GameConstants.MonsterState.idle;

        protected override void Awake()
        {
            base.Awake();
            hpBar = SpawnManager.Instance.GetHpBar(transform.position);
            hpBar.Init(this);
        }

        public virtual void Init(GamePlayer targetPlayer, int index, int hp)
        {
            gameObject.SetActive(true);
            player = targetPlayer;

            // 모드별 크기다르게
            if (MainManager.Instance.gameType == GameType.Solo)
            {
                transform.localScale = new Vector3(45, 45, 45);
            }
            
            // 몬스터 초기정보값
            SetInfo(DataManager.Instance.MonsterStateDic[index]);
            currentHP = hp;
            anim = GetComponent<Animator>();
            isDestroyed = false;
            wayPoint = 0;
            SetNextWay();

            SetState(GameConstants.MonsterState.walk);

            // 체력바 설정
            hpBar.transform.SetParent(GameManager.Instance.game.uiEffectParent);
            hpBar.Init(this);
            StartCoroutine(MonsterUpdate());
        }

        private void SetInfo(MonsterData data)
        {
            index = data.index;
            unitName = data.unitName;
            hp = data.hp;
            moveSpeed = data.moveSpeed;
            monsterType = data.monsterType;
            attackDamage = data.damage;
            reward = data.reward;
        }

        private void SetState(GameConstants.MonsterState state)
        {
            anim.SetBool("isHit", false);
            anim.SetBool("isIdle", true);
            anim.SetBool("isAttack", false);
            anim.SetBool("isDie", false);
            anim.SetBool("isWalk", false);

            monsterState = state;
            switch (monsterState)
            {
                case GameConstants.MonsterState.idle: anim.SetBool("isIdle", true); break;
                case GameConstants.MonsterState.walk: anim.SetBool("isWalk", true); break;
                case GameConstants.MonsterState.attack: anim.SetBool("isAttack", true); break;
                case GameConstants.MonsterState.hit: anim.SetBool("isHit", true); break;
                case GameConstants.MonsterState.die: anim.SetBool("isDie", true); break;
            }
        }

        private IEnumerator MonsterUpdate()
        {
            while (gameObject.activeSelf)
            {
                // 죽거나 게임종료시 빠져나감
                if (monsterState == GameConstants.MonsterState.die || GameManager.Instance.isGameOver )
                    break;

                // 디버프 처리
                moveDebuff = 1f;
                for (int i = 0; i < _debuffList.Count; ++i)
                {
                    DebuffBase debuff = _debuffList[i];
                    debuff.UpdateDebuff();
                }

                // 이동처리
                float moveFactor = moveSpeed * moveDebuff * Time.deltaTime * timeScale;
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
                        player.DamageToPlayer();
                        Remove();
                        break;
                    }
                    
                    SetNextWay();
                }


                // // 목표지점 도착시
                // float distance = Vector3.Distance(targetPos, transform.position);
                // if (distance < 20f)
                // {
                //     transform.position = targetPos;
                //     
                //     wayPoint++;
                //     // 목표지까지 도달시 데미지
                //     if (wayPoint >= GameManager.Instance.GetWayLength)
                //     {
                //         player.DamageToPlayer();
                //         Remove();
                //         break;
                //     }
                //     else SetNextWay();
                // }
                //
                // distance = Vector3.Distance(Vector3.zero, transform.position);
                // if (distance > 3000)
                // {
                //     Debug.Log("Monster Road Find Error!!!!");
                //     player.DamageToPlayer();
                //     Remove();
                //     break;
                // }

                yield return null;
            }
            Remove();
        }
        
        private void SetNextWay()
        {
            targetPos = player.GetNext(wayPoint);
            dir = (targetPos - transform.position).normalized;
            totalDistance = Vector3.Distance(targetPos, transform.position);
            transform.DORotate(new Vector3(0, (dir.x > 0) ? 180 : 0, 0), 0);
        }
        
        public virtual void Hit(int damage)
        {
            if (monsterState == GameConstants.MonsterState.die)
                return;

            currentHP -= damage;
            
            CreateDamageText(damage);

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

        public void CreateDamageText(int damage)
        {
            GameObject go = SpawnManager.Instance.GetDamageText(transform.position);
            DamageText damageText = go.GetComponent<DamageText>();
            damageText.Show(transform, damage);
        }

        protected override void Remove()
        {
            if (isDestroyed)
                return;

            isDestroyed = true;
            
            OnUnitDestroy?.Invoke();
            OnUnitDestroy = null;
            
            player.KillMonster(reward);
            
            player.monsterList.Remove(this);
            
            transform.DOKill();
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
