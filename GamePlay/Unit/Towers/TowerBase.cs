using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Photon.Pun;
using RandomFortress.Common.Extensions;
using RandomFortress.Common.Util;
using RandomFortress.Data;

using RandomFortress.Manager;
using TMPro;
using UnityEngine;

namespace RandomFortress.Game
{
    public enum TowerStateType
    {
        Idle, Attack, Upgrade, Sell
    }

    
    /// <summary>
    /// 타워는 로컬객체로 생성된다. 이는 플레이 하는사람의 진영은 항상 아래쪽으로 보여지기 위함이다.
    /// 타워에 연관된 총알, 피격이펙트, 데미지이펙트는 전부 각각의 클라이언트에서 따로 동작한다
    /// </summary>
    public class TowerBase : UnitBase
    {
        [Header("타워 수치")]
        [SerializeField] protected int index; // 타워 인덱스
        [SerializeField] protected string towerName; // 타워이름
        [SerializeField] protected int attackPower; // 타워에 한번 공격 당할때
        [SerializeField] protected int attackSpeed; // 1초당 몇번 공격하는지로
        [SerializeField] protected int attackRange; // 어느 거리안에 들어와야 공격하는지
        [SerializeField] protected TowerAttackType attackType; // 공격타입이 무엇인지 (단일, 다중, 스플, 뿌리기)
        [SerializeField] protected int bulletIndex; // 발사될 총알 인덱스 (근접일경우 0)
        [SerializeField] protected int tier; // 타워 등급
        [SerializeField] protected int price; // 구입금액 ( 특수한 루트로 금화구입시 사용 )
        [SerializeField] protected int salePrice; // 판매금액
        [SerializeField] protected int[] attackArea; // 스플, 스티키 피격범위
        [SerializeField] protected int[] multipleCount; // 다중공격시 몇개를 공격가능한지 티어 1~5까지
        [SerializeField] protected int[] stickyTime; // 스티키 유지시간
        
        // [SerializeField] protected TowerData currentInfo;
        // [SerializeField] private TowerStateType currentState;
        
        [Header("기본")]
        [SerializeField] protected SpriteRenderer body;
        [SerializeField] protected SpriteRenderer select;
        [SerializeField] protected SpriteRenderer seat;
        [SerializeField] protected SpriteRenderer seatBG;
        [SerializeField] protected TextMeshPro dpsText;
        [SerializeField] protected TextMeshPro tearText;


        /// <summary> Towers 배열의 Index역할 and 현재타워위치 </summary>
        public int TowerPosIndex { get; set; } 
        
        /// <summary> 타워 준 모든 데미지 </summary>
        public int TotalDamege { get; protected set; } 
        public int SalePrice => salePrice;
        public int Price => price;
        public int TowerIndex => index;
        public int TowerTier => tier;
        
        // private bool canAttack = true;
        protected float waitTime = 0f;
        protected MonsterBase Target = null;

        protected const int TARGET_WIDTH = 120;
        protected const int TARGET_HEIGHT = 120;

        protected float ratio;

        protected float dps; //   초당 공격력
        protected float dpsTime = 0;
        protected bool canDrag = true;
        protected bool isSkillUsed = false;

        // public int SalePrice => currentInfo.salePrice;
        public Vector3 originPosition;

        // TODO: 기본모션 임시대체
        protected TweenerCore<Vector3, Vector3, VectorOptions> character_ani;

        protected string spriteKey = "";
        public string GetSpriteKey => spriteKey;
        
        protected override void Awake()
        {
            base.Awake();
            body = transform.GetChild(0).GetComponent<SpriteRenderer>();
            select = transform.GetChild(1).GetComponent<SpriteRenderer>();
            seat = transform.GetChild(2).GetComponent<SpriteRenderer>();
            seatBG = transform.GetChild(3).GetComponent<SpriteRenderer>();
            dpsText = transform.GetChild(4).GetComponent<TextMeshPro>();
            tearText = transform.GetChild(5).GetComponent<TextMeshPro>();
        }
        
        public virtual void Init(GamePlayer targetPlayer, int posIndex, int towerIndex)
        {
            gameObject.SetActive(true);
            player = targetPlayer;
            
            isDestroyed = false;
            TowerPosIndex = posIndex;
            
            // 타워 데이터 불러오기
            SetInfo(DataManager.Instance.TowerDataDic[towerIndex]);

            gameObject.name = towerName + " " + tier;

            DOTween.Kill(transform);
            // canAttack = true;
            waitTime = 0f;
            Target = null;
            originPosition = transform.position;
            SetFocus(false);
            
            // 타워 초기화
            SetBody();

            // 코루틴으로 업데이트 처리
            StartCoroutine(TowerUpdate());
        }

        protected void SetInfo(TowerData data)
        {
            index = data.index;
            towerName = data.towerName;
            attackPower= data.attackPower; 
            attackSpeed= data.attackSpeed;
            attackRange= data.attackRange;
            attackType= (TowerAttackType)data.attackType; 
            bulletIndex= data.bulletIndex;
            tier= data.tier; 
            price= data.price; 
            salePrice= data.salePrice; 
            attackArea= data.attackArea; 
            multipleCount= data.multipleCount; 
            stickyTime= data.stickyTime; 
        }

        protected void SetBody()
        {
            body.sprite = ResourceManager.Instance.GetTower(index, tier);
            seat.sprite = ResourceManager.Instance.GetTower(index, tier,"Site_" + tier);
            
            float ratioX = TARGET_WIDTH / body.sprite.rect.width;
            float ratioY = TARGET_HEIGHT / body.sprite.rect.height;
            ratio = ratioX > ratioY ? ratioY : ratioX;
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(ratio, ratio);
            SetState(TowerStateType.Idle);
        }


        // TODO: 임시 애니메이션
        public void SetState(TowerStateType state)
        {
            if (character_ani != null)
                character_ani.Kill();
                
            switch (state)
            {
                case TowerStateType.Idle:
                    character_ani = body.transform.DOScale(ratio-(ratio*0.05f), 1f).
                        From(ratio).SetLoops(-1, LoopType.Yoyo);
                    break;
                
                case TowerStateType.Attack:
                    character_ani = body.transform.DOScale(ratio+(ratio*0.1f), 0.25f).
                        From(ratio).SetLoops(-1, LoopType.Yoyo);
                    break;
                
                case TowerStateType.Sell:
                    break;
                
                case TowerStateType.Upgrade:
                    break;
            }
        }

        /// <summary>
        /// 컨트롤하는 플레이어만 업데이트하고, 변동상황을 Rpc로 연동. 
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator TowerUpdate()
        {
            float distance;
            const float updateInterval = 0.05f;
            while (gameObject.activeSelf)
            {
                if (isDestroyed || GameManager.Instance.isGameOver)
                    yield break;

                // 현재 공격 대상이 없다면
                if (Target == null)
                {
                    // 공격 대상자 찾기
                    List<MonsterBase> monsterList = player.monsterList;
                    int length = monsterList.Count;
                    for (int i = 0; i < length; ++i)
                    {
                        MonsterBase monster = monsterList[i];
                        if (monster == null || monster.gameObject.activeSelf == false)
                            continue;
                    
                        distance = Vector3.Distance(transform.position, monster.transform.position);
                        if (distance <= attackRange)
                        {
                            Target = monster;
                            // SetState(TowerStateType.Attack);
                            break;
                        }
                    }
                }
                else
                {
                    // 사거리 체크
                    distance = Vector3.Distance(transform.position, Target.transform.position);
                    if (distance > attackRange || !Target.gameObject.activeSelf)
                    {
                        Target = null;
                        SetState(TowerStateType.Idle);
                    }
                    else
                    {
                        // 공격 딜레이 대기
                        waitTime += updateInterval;
                        if (waitTime >= (100 / (float)attackSpeed))
                        {
                            waitTime = 0;
                            Shooting();
                        }
                    }
                }

                // dps 표시
                dpsTime += updateInterval;
                if (dpsTime > 1)
                {
                    dpsText.text = dps.ToString();
                    dpsTime = 0;
                    dps = 0;
                }

                yield return new WaitForSeconds(updateInterval);
            }
        }

        protected virtual void Shooting()
        {
            // TODO: 트윈을 사용한 발사모션. Spine 사용시엔 공격모션으로 대체
            if (!isDragging)
            {
                Vector3 oriPos = transform.position;
                Body.transform.DOLocalMoveX(-15f, 0.08f).SetEase(Ease.OutBack);
                Body.transform.DOMove(oriPos, 0.2f).SetDelay(0.12f);   
            }

            // 총알 발사시 타워마다 시작지점이 다르다
            Vector3 flashPos = originPosition;//transform.position;
            flashPos.z = -50f;
            switch ((Data.TowerIndex)index)
            {
                case   Data.TowerIndex.Elephant: flashPos.ExAddXY(18f, 23f); break;
                case   Data.TowerIndex.Drum: flashPos.ExAddXY(31.5f, 2.2f); break;
                case   Data.TowerIndex.Sting: flashPos.ExAddXY(35.6f, -5.17f); break;
                case   Data.TowerIndex.Wolf: flashPos.ExAddXY(23.3f, -14f); break;
                case   Data.TowerIndex.Flame: flashPos.ExAddXY(19.5f, 9.3f); break;
                case   Data.TowerIndex.Machinegun: flashPos.ExAddXY(30.5f, 3.8f); break;
                case   Data.TowerIndex.Shino: flashPos.ExAddXY(10.6f, 2.5f); break;
                case   Data.TowerIndex.MaskMan: flashPos.ExAddXY(30.8f, 20.3f); break;
                case   Data.TowerIndex.Swag: flashPos.ExAddXY(23.9f, 1.7f); break;
            }
            
            // TODO : 현재 확정이 안된 총알은 0번인데, 이것을 10번으로 임시로사용
            bulletIndex = bulletIndex == 0 ? 10 : bulletIndex;

            switch (attackType)
            {
                case TowerAttackType.Single:
                    GameObject bulletGo = SpawnManager.Instance.GetBullet(flashPos, bulletIndex);
                    BulletBase bullet = bulletGo.GetComponent<BulletBase>();
                    bullet.Init(player, bulletIndex, Target, attackPower);
                    break;
                case TowerAttackType.Multiple: break;
                case TowerAttackType.Splash: break;
                case TowerAttackType.Sticky: break;
            }
            

            //
            TotalDamege += attackPower;
            dps += attackPower;
        }

        public bool UpgradePossible(int index, int tier)
        {
            return (this.index == index) && (this.tier == tier);
        }
        
        public void UpgradeTower()
        {
            // originPosition = body.transform.position;

            // TODO: 공격력, 공속만 변경
            attackPower = (int)((float)attackPower * 1.9f);
            attackSpeed = (int)((float)attackSpeed * 1.2f);
            // attackRange = (int)((float)attackRange * 1.9f);
            salePrice *= 2;
            tier += 1; 
            
            tearText.text = "Tear " + tier;
            
            // 타워 초기화
            SetBody();
        }
        
        #region Input

        protected Vector3 offSet;
        protected bool isDragging = false;

        protected void OnMouseDown()
        {
            if (player != GameManager.Instance.myPlayer)
                return;
            
            if (canDrag)
            {
                offSet = body.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                body.GetComponent<SpriteRenderer>().sortingOrder = 13;
            }

            JTUtil.DelayCallCoroutine(this, 0.1f, () =>
            {
                isSkillUsed = player.CheckUserAvailableSkill(this, BaseSkill.SkillAction.Touch_Start);
                if (!isDragging && !isSkillUsed)
                    SetFocus();
            });
        }

        protected void OnMouseDrag()
        {
            if (player != GameManager.Instance.myPlayer)
                return;
            
            isDragging = true;
            if (isDragging)
                body.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offSet;
        }
        
        protected void OnMouseUp()
        {
            if (player != GameManager.Instance.myPlayer)
                return;
                
            if (isDragging)
            {
                
                Debug.Log("Moust UP!!");
                isDragging = false;
                canDrag = false;

                body.GetComponent<SpriteRenderer>().sortingOrder = 12;
                
                // TODO: 자리바꾸기 스킬 사용시에만 처리로 코드변경
                // if (isSkillUsed)
                // {
                //     isSkillUsed = false;
                //     SetFocus(false);
                //     return;
                // }


                // 드래그로 타워에 놓을경우, 같은 티어&종류 타워라면 업그레이드
                Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition); // 터치한 좌표 가져옴
                Ray2D ray = new Ray2D(wp, Vector2.zero); // 원점에서 터치한 좌표 방향으로 Ray를 쏨
                RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction);
                foreach (var hit in hits)
                {
                    if (hit && hit.collider.gameObject != gameObject)
                    {
                        TowerBase target = hit.collider.gameObject.GetComponent<TowerBase>();
                        if (target != null && target.UpgradePossible(index, tier))
                        {
                            // 포커스 해제
                            SetFocus(false);
                            
                            // 드래그한 타워 삭제
                            player.TowerDestroy(TowerPosIndex);

                            // 대상 타워자리에 업그레이드
                            player.UpgradeTower(target.TowerPosIndex);
                            return;
                        }
                    }
                }

                ReturnToOriPos();
            }
        }

        public void ReturnToOriPos(float duration = 0.5f)
        {
            body.transform.DOLocalMove(Vector3.zero, duration).OnComplete(() =>
            {
                canDrag = true;
            });
        }

        protected void SetFocus()
        {
            if (GameManager.Instance.isFocus)
            {
                if (GameManager.Instance.focusTower == this) // 자기자신을 클릭시 해제
                    SetFocus(false);
                else // 다른 포커스에서 자신으로 올경우
                {
                    SetFocus(true);
                }
            }
            else // 포커스가 비활성화인 경우
                SetFocus(true);
            
            GameUIManager.Instance.UpdateInfo();
        }
        
        public void SetFocus(bool active)
        {
            if (active)
            {
                GameManager.Instance.isFocus = true;
                GameManager.Instance.focusTower = this;
                select.gameObject.SetActive(true);
            }
            else
            {
                GameManager.Instance.isFocus = false;
                GameManager.Instance.focusTower = null;
                select.gameObject.SetActive(false);
            }
        }

        public void Swap(TowerBase other)
        {
            // 위치값 변경
            int tempPos = TowerPosIndex;
            TowerPosIndex = other.TowerPosIndex;
            other.TowerPosIndex = tempPos;

            // 위치값 적용
            Vector3 tempVec = transform.position;
            transform.position = other.transform.position;
            other.transform.position = tempVec;

            // 바디값 초기화
            ReturnToOriPos();
            other.ReturnToOriPos();
        }

        public SpriteRenderer Body => body;
        
        #endregion

        public void TowerDestroy()
        {
            StopCoroutine(TowerUpdate());
            Remove();
        }
        
        protected override void Remove()
        {
            Destroy(gameObject);
        }
    }
}
