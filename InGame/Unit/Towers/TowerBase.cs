using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using RandomFortress.Common.Extensions;
using RandomFortress.Common.Utils;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace RandomFortress.Game
{
    /// <summary>
    /// 타워는 로컬객체로 생성된다. 이는 플레이 하는사람의 진영은 항상 아래쪽으로 보여지기 위함이다.
    /// 타워에 연관된 총알, 피격이펙트, 데미지이펙트는 전부 각각의 클라이언트에서 따로 동작한다
    /// </summary>
    public class TowerBase : PlayBase
    {
        [Header("타워 수치")] 
        [SerializeField] protected TowerInfo Info;

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
        public int SellPrice => Info.sellPrice;
        public int Price => Info.price;
        public int TowerIndex => Info.index;
        public int TowerTier => Info.tier;
        public string GetSpriteKey => spriteKey;
        
        protected float waitTime = 0f;
        protected MonsterBase Target = null;
        protected float ratio;
        protected float dps; //   초당 공격력
        protected float dpsTime = 0;
        protected bool canDrag = true;
        protected bool isSkillUsed = false;
        protected Vector3 originPosition;
        protected TweenerCore<Vector3, Vector3, VectorOptions> character_ani;
        protected string spriteKey = "";
        
        protected const float updateInterval = 0.05f;

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
        
        protected virtual void Reset()
        {
            gameObject.SetActive(true);
            gameObject.name = Info.towerName + " " + Info.tier;
            isDestroyed = false;
            DOTween.Kill(transform);
            waitTime = 0f;
            Target = null;
            originPosition = transform.position;
            // ratio = body.transform.localScale.x;
            SetFocus(false);
            SetState(TowerStateType.Idle);
        }

        protected virtual void SetInfo(TowerData data)
        {
            if (Info == null)
                Info = new TowerInfo(data);
            else 
                Info.UpgradeTower(data);
        }

        protected virtual void SetBody()
        {
            seat.sprite = ResourceManager.Instance.GetTower(Info.index, Info.tier,"Site_" + Info.tier);
            body.sprite = ResourceManager.Instance.GetTower(Info.index, Info.tier);

            SetTowerSize();
            
            // 
            if (MainManager.Instance.gameType == GameType.Solo)
            {
                transform.localScale = new Vector3(1.333f, 1.333f, 1.333f);
            }
        }

        private void SetTowerSize()
        {
            // 지정된 크기만큼 타워크기를 조정
            float ratioX = GameConstants.TOWER_WIDTH_LIST[Info.tier-1] / body.sprite.rect.width;
            float ratioY = GameConstants.TOWER_HEIGHT_LIST[Info.tier-1] / body.sprite.rect.height;
            
            ratio = ratioX > ratioY ? ratioY : ratioX;
            
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(ratio, ratio);
        }
        
        // TODO: 차후에 Spine 이 적용된 애니매이션
        public void SetState(TowerStateType state)
        {
            if (character_ani != null)
                character_ani.Kill();
                
            SetTowerSize();
            
            switch (state)
            {
                case TowerStateType.Idle:
                    character_ani = body.transform.DOScale(ratio-(ratio*0.05f), 1f).
                        From(ratio).SetLoops(-1, LoopType.Yoyo);
                    break;
                
                // case gConstamts.TowerStateType.Attack:
                //     character_ani = body.transform.DOScale(ratio+(ratio*0.1f), 0.25f).
                //         From(ratio).SetLoops(-1, LoopType.Yoyo);
                //     break;
                
                case TowerStateType.Sell:
                    break;
                
                case TowerStateType.Upgrade:
                    break;
            }
        }
        
        protected virtual IEnumerator TowerUpdate()
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
        
        protected virtual void SearchMonstersInAttackRange()
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
        
        protected virtual void UpdateAttackTargetAndShooting()
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
                waitTime += Time.deltaTime;
                if (waitTime >= (float)Info.attackSpeed / 100 / timeScale)
                {
                    waitTime = 0;
                    Shooting();
                }
            }
        }

        protected void ShowDps()
        {
            // dps 표시
            dpsText.text = dps.ToString(CultureInfo.CurrentCulture);
            // dpsTime += Time.deltaTime * timeScale;
            // if (dpsTime > 1)
            // {
            //     dpsText.text = dps.ToString();
            //     dpsTime = 0;
            //     dps = 0;
            // }
        }

        protected virtual void Shooting()
        {
            // TODO: 트윈을 사용한 발사모션. Spine 사용시엔 공격모션으로 대체
            if (!isDragging)
            {
                Vector3 oriPos = transform.position;
                body.transform.DOLocalMoveX(-15f, 0.08f * timeScale).SetEase(Ease.OutBack);
                body.transform.DOMove(oriPos, 0.2f* timeScale).SetDelay(0.12f* timeScale);   
            }

            // 총알 발사시 타워마다 시작지점이 다르다
            Vector3 flashPos = originPosition;
            flashPos.z = -50f;
            switch ((TowerIndex)Info.index)
            {
                case   Data.TowerIndex.Elephant: flashPos.ExAddXY(18f, 23f); break;
                case   Data.TowerIndex.Sting: flashPos.ExAddXY(35.6f, -5.17f); break;
                case   Data.TowerIndex.Wolf: flashPos.ExAddXY(23.3f, -14f); break;
                case   Data.TowerIndex.Flame: flashPos.ExAddXY(19.5f, 9.3f); break;
            }
            
            // TODO : 총알 3번이 없다
            Info.bulletIndex = Info.bulletIndex == 3 ? 0 : Info.bulletIndex;

            GameObject bulletGo = SpawnManager.Instance.GetBullet(flashPos, Info.bulletIndex);
            BulletBase bullet = bulletGo.GetOrAddComponent<BulletBase>();
            bullet.Init(player, Info.bulletIndex, Target, Info.attackPower);

            //
            TotalDamege += Info.attackPower;
            dps += Info.attackPower;
        }

        public bool UpgradePossible(int index, int tier)
        {
            return (this.Info.index == index) && (this.Info.tier == tier);
        }
        
        public void UpgradeTower()
        {
            // originPosition = body.transform.position;

            // TODO: 공격력, 공속만 변경
            Info.attackPower = (int)((float)Info.attackPower * 1.9f);
            Info.attackSpeed = (int)((float)Info.attackSpeed * 1.2f);
            // attackRange = (int)((float)attackRange * 1.9f);
            Info.sellPrice *= 2;
            Info.tier += 1; 
            
            tearText.text = "Tear " + Info.tier;
            
            // 타워 초기화
            SetBody();
            SetState(TowerStateType.Idle);
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
                        if (target != null && target.UpgradePossible(Info.index, Info.tier))
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
                    SetFocus(true);
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
            (TowerPosIndex, other.TowerPosIndex) = (other.TowerPosIndex, TowerPosIndex);

            // 위치값 적용
            var transform1 = transform;
            var transform2 = other.transform;
            (transform1.position, transform2.position) = (transform2.position, transform1.position);

            // 바디값 초기화
            ReturnToOriPos();
            other.ReturnToOriPos();
        }
        
        public void OnStageClear()
        {
            dps = 0;
        }

        // public SpriteRenderer Body => body;
        
        #endregion

        public void TowerDestroy()
        {
            Remove();
        }
        
        protected override void Remove()
        {
            if (isDestroyed)
                return;

            isDestroyed = true;
            
            StopCoroutine(TowerUpdate());
            body.DOKill();
            transform.DOKill();
            Destroy(gameObject);
        }
    }
}
