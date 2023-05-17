using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using RandomFortress.Common.Extensions;
using RandomFortress.Common.Script.Touch;
using RandomFortress.Common.Util;
using RandomFortress.Data;
using RandomFortress.Game.Skill;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace RandomFortress.Game
{
    public enum TowerStateType
    {
        Idle, Attack, Upgrade, Sell
    }

    public class TowerBase : UnitBase
    {
        [FormerlySerializedAs("info")]
        [Header("타워 수치")]
        [SerializeField] private int index; // 타워 인덱스
        [SerializeField] private string towerName; // 타워이름
        [SerializeField] private int attackPower; // 타워에 한번 공격 당할때
        [SerializeField] private int attackSpeed; // 1초당 몇번 공격하는지로
        [SerializeField] private int attackRange; // 어느 거리안에 들어와야 공격하는지
        [SerializeField] private int attackType; // 공격타입이 무엇인지 (단일, 다중, 스플, 뿌리기)
        [SerializeField] private int bulletIndex; // 발사될 총알 인덱스 (근접일경우 0)
        [SerializeField] private int tier; // 타워 등급
        [SerializeField] private int price; // 구입금액 ( 특수한 루트로 금화구입시 사용 )
        [SerializeField] private int salePrice; // 판매금액
        
        // [SerializeField] protected TowerData currentInfo;
        // [SerializeField] private TowerStateType currentState;
        
        [Header("기본")]
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private SpriteRenderer select;
        [SerializeField] private TextMeshPro dpsText;
        [SerializeField] private TextMeshPro tearText;

        public int TowerPosIndex { get; set; }
        public int TotalDamege { get; private set; } // 현재 타워가 준 모든 데미지 통합
        public int SalePrice => salePrice;
        public int Price => price;
        
        private bool canAttack = true;
        private float waitTime = 0f;
        private MonsterBase Target = null;

        private const int TARGET_WIDTH = 120;
        private const int TARGET_HEIGHT = 120;

        private float ratio;

        private float dps; //   초당 공격력
        private float dpsTime = 0;
        private bool canDrag = true;
        private bool isSkillUsed = false;

        // public int SalePrice => currentInfo.salePrice;
        // public Vector3 originPosition;

        // TODO: 기본모션 임시대체
        private TweenerCore<Vector3, Vector3, VectorOptions> character_ani;

        private string spriteKey = "";
        public string GetSpriteKey => spriteKey;
        
        protected override void Awake()
        {
            base.Awake();
            body = transform.GetChild(0).GetComponent<SpriteRenderer>();
            select = transform.GetChild(1).GetComponent<SpriteRenderer>();
        }

        // 이 시점에 랜덤으로 생성될 타워를 결정한다
        public void Init(int posIndex, int towerIndex)
        {
            // originPosition = body.transform.position;
            
            TowerPosIndex = posIndex;
            
            // 타워 데이터 불러오기
            // currentInfo = ScriptableObject.CreateInstance<TowerData>();
            // currentInfo = JTUtil.DeepCopy( DataManager.Instance.TowerDataDic[towerIndex] );
            SetInfo(DataManager.Instance.TowerDataDic[towerIndex]);

            gameObject.name = towerName + " " + tier;
            // tearText.text = "Tear " + currentInfo.tier;

            DOTween.Kill(transform);
            
            // 타워 초기화
            SetBody();

            // 코루틴으로 업데이트 처리
            StartCoroutine(TowerUpdate());
        }
        
        private void SetInfo(TowerData data)
        {
            index = data.index;
            towerName = data.towerName;
            attackPower= data.attackPower; 
            attackSpeed= data.attackSpeed;
            attackRange= data.attackRange;
            attackType= data.attackType; 
            bulletIndex= data.bulletIndex;
            tier= data.tier; 
            price= data.price; 
            salePrice= data.salePrice; 
        }

        private void SetBody()
        {
            string[] nameMath =
            {
                "elephant", "drum", "cactus", "dog", "fire", "machine", "ninja", "poison", "swag"
            };
            spriteKey = nameMath[index] + "_" + tier;

            Sprite sprite = ResourceManager.Instance.GetTower(spriteKey);
            if (sprite == null)
            {
                Debug.Log("sprite not found : "+spriteKey);
            }
            else
                body.sprite = sprite;
            
            float ratioX = TARGET_WIDTH / sprite.rect.width;
            float ratioY = TARGET_HEIGHT / sprite.rect.height;
            ratio = ratioX > ratioY ? ratioY : ratioX;
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
                    character_ani = body.transform.DOScale(ratio-(ratio*0.05f), 1f).From(ratio).SetLoops(-1, LoopType.Yoyo);
                    break;
                
                case TowerStateType.Attack:
                    character_ani = body.transform.DOScale(ratio+(ratio*0.1f), 0.25f).From(ratio).SetLoops(-1, LoopType.Yoyo);
                    break;
                
                case TowerStateType.Sell:
                    break;
                
                case TowerStateType.Upgrade:
                    break;
            }
        }

        public void Reset()
        {
            canAttack = true;
            waitTime = 0f;
            Target = null;
            SetFocus(false);
        }

        public void RemoveTower()
        {
            Reset();
            Pool.Release(gameObject);
        }

        private IEnumerator TowerUpdate()
        {
            const float updateInterval = 0.05f;
            while (gameObject.activeSelf)
            {
                if (!canAttack)
                    yield return new WaitForSeconds(updateInterval);

                if (Target == null)
                {
                    // 공격 대상자 찾기
                    List<MonsterBase> monsterList = GameManager.Instance.monsterList;
                    int length = monsterList.Count;
                    for (int i = 0; i < length; ++i)
                    {
                        MonsterBase monster = monsterList[i];
                        if (monster.gameObject.activeSelf == false)
                            continue;
                    
                        float distance = Vector3.Distance(transform.position, monster.transform.position);
                        if (distance <= attackRange)
                        {
                            Target = monster;
                            SetState(TowerStateType.Attack);
                            // 총알이 없어지면 타워타겟을 NULL로
                            Target.OnUnitDestroy += () =>
                            {
                                Target = null;
                                SetState(TowerStateType.Idle);
                            };
                            Shooting();
                            break;
                        }
                    }
                }
                else
                {
                    // 사거리 체크
                    float distance = Vector3.Distance(transform.position, Target.transform.position);
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

        private void Shooting()
        {
            // EffectManager.Instance.ShootingEffect(currentInfo.bulletIndex, transform.position);
            
            // 공격 시작
            waitTime = 0;
            GameObject go = ObjectPoolManager.Instance.GetBullet(bulletIndex);
            BaseBullet bullet = go.GetComponent<BaseBullet>();
            bullet.damage = attackPower;
            bullet.Target = Target;
            bullet.transform.position = transform.position;
            bullet.Init();

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

        private Vector3 offSet;
        private bool isDragging = false;

        private void OnMouseDown()
        {
            if (canDrag)
            {
                offSet = body.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                body.GetComponent<SpriteRenderer>().sortingOrder = 13;
            }

            JTUtil.DelayCallCoroutine(this, 0.1f, () =>
            {
                isSkillUsed = GameManager.Instance.CheckUserAvailableSkill(this, BaseSkill.SkillAction.Touch_Start);
                if (!isDragging && !isSkillUsed)
                    SetFocus();
            });
        }

        private void OnMouseDrag()
        {
            isDragging = true;
            if (isDragging)
                body.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offSet;
        }
        
        private void OnMouseUp()
        {
            if (isDragging)
            {
                isDragging = false;
                canDrag = false;

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
                            SetFocus(false);
                            GameManager.Instance.ResetTowerPos(this);
                            // body.transform.position = originPosition;
                            body.GetComponent<SpriteRenderer>().sortingOrder = 12;
                            target.UpgradeTower();
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
                body.GetComponent<SpriteRenderer>().sortingOrder = 12;
            });
        }

        private void SetFocus()
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
            // Body.transform.localPosition = Vector3.zero;
            // other.Body.transform.localPosition = Vector3.zero;

            // originPosition = Body.transform.position;
            // other.originPosition = other.Body.transform.localPosition;
        }

        public SpriteRenderer Body => body;
        
        #endregion
        
        
    }
}
