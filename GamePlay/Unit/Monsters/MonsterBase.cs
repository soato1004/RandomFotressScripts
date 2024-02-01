using System.Collections;
using DG.Tweening;
using Photon.Pun;
using RandomFortress.Data;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    /// <summary>
    /// 몬스터는 네트웍 객체로 생성된다. 하나가 생성되면, 플레이어와 상대플레이어 두곳에 생성되므로
    /// 죽였을때의 보상만 한곳에서 처리하고, 나머지는 각각의 클라이언트에서 동일하게 연출
    /// </summary>
    public class MonsterBase : UnitBase
    {
        [SerializeField] private Animator anim; // 몬스터 모션

        [Header("타워 기본능력치")] [SerializeField] private int index;
        [SerializeField] private string unitName;
        [SerializeField] private int hp;
        [SerializeField] private int moveSpeed;
        [SerializeField] private int monsterType;
        [SerializeField] private int attackDamage;
        [SerializeField] private int reward;

        [Header("현재 능력치")] public int currentHP;

        [Header("링크")] 
        [SerializeField] private HpBar hpBar; // 몬스터 HP 오브젝트

        // private MonsterData currentInfo;    // 몬스터 기본 정보
        private int wayPoint; // 어디까지 이동했는지
        private Vector3 dir; // 이동 방향
        private float targetDistance; // 목표물까지 이동거리
        private float moveDistance; // 현재 이동거리 합산

        public enum MonsterState
        {
            idle,
            walk,
            hit,
            attack,
            die
        } // 정지, 왼쪽, 오른쪽, 위쪽, 아래쪽

        public MonsterState monsterState = MonsterState.idle;

        protected override void Awake()
        {
            base.Awake();
            hpBar = transform.GetComponentInChildren<HpBar>();
        }

        public virtual void Init(GamePlayer targetPlayer, int index, float buffHP)
        {
            gameObject.SetActive(true);
            player = targetPlayer;

            // 몬스터 초기정보값
            SetInfo(DataManager.Instance.MonsterStateDic[index]);
            currentHP = (int)((float)hp * buffHP);
            anim = GetComponent<Animator>();
            isDestroyed = false;
            // transform.localScale = new Vector3(40f, 40f, 40f);
            wayPoint = 0;
            SetNextWay();

            SetState(MonsterState.walk);

            // 체력바 설정
            hpBar.transform.SetParent(GameManager.Instance.game.uiEffectParent);
            hpBar.Reset(this);
            hpBar.OnSetText(currentHP);
            
            
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
                case MonsterState.idle: anim.SetBool("isIdle", true); break;
                case MonsterState.walk: anim.SetBool("isWalk", true); break;
                case MonsterState.attack: anim.SetBool("isAttack", true); break;
                case MonsterState.hit: anim.SetBool("isHit", true); break;
                case MonsterState.die: anim.SetBool("isDie", true); break;
            }
        }

        private void SetNextWay()
        {
            Vector3 targetPos = player.GetNext(wayPoint);
            dir = (targetPos - transform.position).normalized;
            targetDistance = Vector3.Distance(transform.position, targetPos);
            moveDistance = 0;
            transform.DORotate(new Vector3(0, (dir.x > 0) ? 180 : 0, 0), 0);
        }

        /// <summary>
        /// 몬스터가 피격받을때. 각각의 클라이언트에서 처리중.
        /// </summary>
        /// <param name="damage"></param>
        public virtual void Hit(int damage)
        {
            if (monsterState == MonsterState.die)
                return;

            currentHP -= damage;
            
            CreateDamageText(damage);

            hpBar.OnSetText(currentHP);

            if (currentHP <= 0)
            {
                Remove();
                StopCoroutine(MonsterUpdate());
                GameUIManager.Instance.UpdateInfo();
            }
        }

        public void CreateDamageText(int damage)
        {
            GameObject go = SpawnManager.Instance.GetDamageText(transform.position);
            DamageText damageText = go.GetComponent<DamageText>();
            damageText.Show(transform, damage);
        }

        private IEnumerator MonsterUpdate()
        {
            while (gameObject.activeSelf)
            {
                if (monsterState == MonsterState.die || GameManager.Instance.isGameOver )
                    break;

                moveDistance += moveSpeed * Time.deltaTime * timeScale;
                transform.position += moveSpeed * dir * Time.deltaTime * timeScale;

                if (targetDistance < moveDistance)
                {
                    wayPoint++;
                    if (wayPoint >= GameManager.Instance.roadWayPoints.Length)
                    {
                        Debug.Log("wayPoint "+wayPoint+", wayLength : "+GameManager.Instance.roadWayPoints.Length);
                        player.DamageToPlayer();
                        Remove();
                        break;
                    }
                    else
                        SetNextWay();
                }

                yield return null;
            }
            Remove();
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
            
            Destroy(gameObject);
        }
    }
}
