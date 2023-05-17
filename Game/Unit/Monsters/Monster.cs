using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RandomFortress.Common.Extensions;
using RandomFortress.Common.Util;
using RandomFortress.Data;
using RandomFortress.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Game
{
    public class Monster : MonsterBase
    {
        [SerializeField] private Animator anim; // 몬스터 모션
        
        [Header("타워 기본능력치")]
        [SerializeField] private int index;
        [SerializeField] private string unitName;
        [SerializeField] private int hp;
        [SerializeField] private int moveSpeed;
        [SerializeField] private int monsterType;
        [SerializeField] private int damage;
        [SerializeField] private int reward;

        [Header("현재 능력치")] 
        public int currentHP;

        // private MonsterData currentInfo;    // 몬스터 기본 정보
        private HpBar hpBar;    // 몬스터 HP 오브젝트
        private int wayPoint;   // 어디까지 이동했는지
        private Vector3 dir;    // 이동 방향
        private float targetDistance;   // 목표물까지 이동거리
        private float moveDistance;     // 현재 이동거리 합산

        public enum MonsterState { idle, walk, hit, attack, die } // 정지, 왼쪽, 오른쪽, 위쪽, 아래쪽

        public MonsterState monsterState = MonsterState.idle;

        public override void Init(int index, float buffHP)
        {
            // 몬스터 초기정보값
            // currentInfo = ScriptableObject.CreateInstance<MonsterData>();
            // currentInfo = JTUtil.DeepCopy( DataManager.Instance.MonsterStateDic[index] );
            // currentInfo.hp = (int)((float)currentInfo.hp * buffHP);
            SetInfo(DataManager.Instance.MonsterStateDic[index]);
            currentHP = (int)((float)hp * buffHP);
            
            //
            anim = GetComponent<Animator>();
            
            
            Reset();
        }

        private void SetInfo(MonsterData data)
        {
            index = data.index;
            unitName = data.unitName;
            hp = data.hp;
            moveSpeed = data.moveSpeed;
            monsterType = data.monsterType;
            damage = data.damage;
            reward = data.reward;
        }

        public void Reset()
        {
            transform.localScale = new Vector3(40f, 40f, 40f);
            wayPoint = 0;
            SetNextWay();
            
            SetState(MonsterState.walk);

            // 체력바 설정
            hpBar = ObjectPoolManager.Instance.GetHpBar();
            hpBar.Reset(this);
            hpBar.OnSetText(currentHP);
            // hpBar.OnSetHP((float)currentHP / currentInfo.hp, 0);
            
            StartCoroutine(MonsterUpdate());
        }

        private void SetState(MonsterState state)
        {
            anim.SetBool("isHit", false);
            anim.SetBool("isIdle", true);
            anim.SetBool("isAttack", false);
            anim.SetBool("isDie", false);
            anim.SetBool("isWalk", false);
            
            monsterState = state;
            switch (monsterState)
            {
                case MonsterState.idle:
                    anim.SetBool("isIdle", true);
                    break;
                
                case MonsterState.walk:
                    anim.SetBool("isWalk", true);
                    break;
                
                case MonsterState.attack: 
                    anim.SetBool("isAttack", true);
                    break;
                
                case MonsterState.hit: 
                    anim.SetBool("isHit", true);
                    break;
                
                case MonsterState.die:
                    anim.SetBool("isDie", true);
                    break;
            }
        }

        private void SetNextWay()
        {
            Vector3 targetPos = GameManager.Instance.GetNext(wayPoint);
            dir = (targetPos - transform.position).normalized;
            targetDistance = Vector3.Distance(transform.position, targetPos);
            moveDistance = 0;
            transform.DORotate(new Vector3(0, (dir.x > 0) ? 180 : 0, 0), 0);
            
        }

        // 몬스터가 총알에 피격 받았을때
        public override void Hit(int damage)
        {
            if (monsterState == MonsterState.die)
                return;
            
            currentHP -= damage;
            
            //
            GameObject damageText = ObjectPoolManager.Instance.GetDamageText();
            damageText.GetComponent<DamageText>().Show(transform);
            damageText.GetComponent<DamageText>().damage = damage;
            
            // hp바 업데이트
            hpBar.OnSetText(currentHP);
            // hpBar.OnSetHP((float)currentHP / currentInfo.hp);
            
            if (currentHP <= 0)
            {
                Destroy();
            }
        }

        private IEnumerator MonsterUpdate()
        {
            while (gameObject.activeSelf)
            {
                if (monsterState == MonsterState.die)
                    yield break;
            
                moveDistance += moveSpeed * Time.deltaTime * timeScale;
                transform.position += moveSpeed * dir * Time.deltaTime * timeScale;
                
                // TODO: 목표지점 근처로 가면 멈추게
                if (targetDistance < moveDistance)
                {
                    wayPoint++;
                    if (wayPoint >= GameManager.Instance.wayPoints.Length)
                    {
                        Destroy();
                        GameManager.Instance.DamageToPlayer();
                    }
                    else
                        SetNextWay();
                }

                yield return null;
            }
        }

        // void Update()
        // {
        //     if (monsterState == MonsterState.die)
        //         return;
        //     
        //     moveDistance += currentInfo.moveSpeed * Time.deltaTime * timeScale;
        //     transform.position += currentInfo.moveSpeed * dir * Time.deltaTime * timeScale;
        //     if (targetDistance < moveDistance)
        //     {
        //         wayPoint++;
        //         if (wayPoint >= GameManager.Instance.wayPoints.Length)
        //         {
        //             Destroy();
        //             GameManager.Instance.DamageToPlayer();
        //         }
        //         else
        //             SetNextWay();
        //     }
        // }
        
        private void Destroy()
        {
            // 몬스터를 파괴할경우 호출
            OnUnitDestroy?.Invoke();
            OnUnitDestroy = null;
            GameManager.Instance.KillMonster(reward);
            GameManager.Instance.monsterList.Remove(this);
            hpBar.Release();
            Pool.Release(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
        }
    }
}