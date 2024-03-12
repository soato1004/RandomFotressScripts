using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using RandomFortress.Common.Utils;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;

namespace RandomFortress.Game
{
    /// <summary>
    /// 타워는 로컬객체로 생성된다. 이는 플레이 하는사람의 진영은 항상 아래쪽으로 보여지기 위함이다.
    /// 타워에 연관된 총알, 피격이펙트, 데미지이펙트는 전부 각각의 클라이언트에서 따로 동작한다
    /// </summary>
    public class TowerBase : EntityBase
    {
        public TowerInfo Info { get; protected set; } // 현재 타워정보

        [Header("기본")]
        [SerializeField] protected SkeletonAnimation spineBody;
        [SerializeField] protected SpriteRenderer select;
        [SerializeField] protected SpriteRenderer seat;
        [SerializeField] protected TextMeshPro dpsText;
        [SerializeField] protected TextMeshPro tearText;


        /// <summary> Towers 배열의 Index역할 and 현재타워위치 </summary>
        public int TowerPosIndex { get; protected set; }
        
        
        protected string spriteKey = "";
        public string GetSpriteKey => spriteKey;

        protected float AttackWaitTimer = 0f; // 타워 공격시 딜레이시간 타이머
        protected MonsterBase Target = null;
        protected bool canDrag = true;
        protected Vector3 originPosition;
        protected TweenerCore<Vector3, Vector3, VectorOptions> character_ani;
        protected TowerStateType currentState = TowerStateType.Idle;
        protected float attackAniSpeed;
        protected const int NormalLayer = 13;
        protected const int DragLayer = 14;
        
        protected int TotalDamege; // 타워가 준 총 데미지

        protected override void Awake()
        {
            base.Awake();
        }
        
        public virtual void Init(GamePlayer targetPlayer, int posIndex, int towerIndex)
        {
            // 초기설정
            player = targetPlayer;
            TowerPosIndex = posIndex;
            SetInfo(DataManager.Instance.GetTowerData(towerIndex));

            // 타워 내부정보 리셋
            Reset();

            // 코루틴으로 업데이트 처리
            StartCoroutine(TowerUpdate());
        }
        
        protected virtual void Reset()
        {
            gameObject.SetActive(true);
            gameObject.name = Info.towerName + " " + Info.tier;
            isDestroyed = false;
            DOTween.Kill(transform);
            AttackWaitTimer = 0f;
            Target = null;
            originPosition = spineBody.transform.localPosition;
            TotalDamege = 0;
            
            // TODO: 공격속도가 변할때마다 이코드가 필요하다
            float duration =  SpineUtils.GetAnimationDuration(spineBody, "Attack");
            attackAniSpeed = ((float)Info.attackSpeed/100) * duration;
            
            // ratio = body.transform.localScale.x;
            // 타워 초기화
            SetBody();
            SetFocus(false);
            SetState();
        }

        protected virtual void SetInfo(TowerData data, int tier = 1)
        {
            if (Info == null)
            {
                Info = new TowerInfo(data.towerInfoDic[tier]);
            }
            else
            {
                Info.UpgradeTower(data.towerInfoDic[tier]);
            }
        }

        private Vector3 oriScale;
        // private float[] soloScale = { 1f, 1.3f, 1.35f, 1.4f, 1.45f,};
        private float[] multiScale = { 0.7f, 1f, 1.05f, 1.1f, 1.15f};
        protected virtual void SetBody()
        {
            if (Info.tier==1)
                oriScale = spineBody.transform.localScale;
            
            // 솔로모드 
            if (MainManager.Instance.gameType == GameType.Solo)
            {
                transform.localScale = new Vector3(1.333f, 1.333f, 1.333f);
            }
            
            spineBody.transform.localScale = oriScale * multiScale[Info.tier - 1];
            
            // 티어1일때는
            seat.sprite = ResourceManager.Instance.GetSprite("Site_" + Info.tier);
        }
        
        public void SetState(TowerStateType state = TowerStateType.Idle)
        {
            currentState = state;
            
            if (spineBody != null)
            {
                switch (state)
                {
                    case TowerStateType.Idle:
                        spineBody.AnimationState.TimeScale = 1f;
                        spineBody.AnimationState.SetAnimation(0, "Idle", true);
                        break;
                
                    case TowerStateType.Attack:
                        TrackEntry attackEntry = spineBody.AnimationState.SetAnimation(0, "Attack", false);
                        attackEntry.TimeScale = attackAniSpeed < 1 ? 1 : attackAniSpeed;
                        TrackEntry idleEntry = spineBody.AnimationState.AddAnimation(0, "Idle", true, 0);
                        idleEntry.TimeScale = 1f;
                        break;
                
                    case TowerStateType.Sell:
                        break;
                
                    case TowerStateType.Upgrade:
                        break;
                }
                return;
            }
        }
        
        protected virtual IEnumerator TowerUpdate()
        {
            while (gameObject.activeSelf)
            {
                // 일시정지
                if (GameManager.Instance.isPaused)
                {
                    yield return null;
                    continue;
                }
                
                // 게임오버 또는 타워 제거시
                if (isDestroyed || GameManager.Instance.isGameOver)
                    break;

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
            
            if (GameManager.Instance.isGameOver)
                player.AddTotalDamage(Info.index, TotalDamege, Info.tier);
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
            }
            else
            {
                // 공격 딜레이 대기
                AttackWaitTimer += Time.deltaTime * GameManager.Instance.timeScale;
                float atkSpeed = Info.attackSpeed * ((100 + player.extraInfo.atkSpeed) / 100f);
                float attackWaitTime = 100f / atkSpeed;
                if (AttackWaitTimer >= attackWaitTime)
                {
                    AttackWaitTimer = 0;
                    Shooting();
                }
            }
        }

        protected void ShowDps()
        {
            // dps 표시
            dpsText.text = TotalDamege.ToString(CultureInfo.CurrentCulture);
        }
        
        
        
        // 타워 업그레이드와 카드레벨과 디버프를 계산하여 데미지를 준다
        protected virtual DamageInfo Damage()
        {
            TextType type = TextType.Damage;
            float totalDmg = 0;
            
            // 타워 데미지 계산
            float upgradeDmg = GameManager.Instance.TowerUpgradeDic[Info.index].GetDamage();
            float abilityDmg = (100f + player.extraInfo.atk) / 100;
            totalDmg = Info.attackPower * upgradeDmg * abilityDmg;
            
            // 크리티컬시
            float criDmg = 0;
            foreach (var extraInfo in player.abilityList)
            {
                if (extraInfo.criChance > 0)
                {
                    int criChance = extraInfo.criChance ;
                    bool isCri = Random.Range(0, 100) < criChance;
                    if (isCri)
                    {
                        type = TextType.DamageCritical;
                        criDmg += totalDmg * (extraInfo.criAtk / 100f);
                    }
                }
            }

            totalDmg = criDmg > 0 ? criDmg : totalDmg;
            totalDmg *= 10; // TODO:테스트

            return new DamageInfo((int)totalDmg, type);
        }

        // 총알 발사시 타워마다 시작지점이 다르다
        protected virtual Vector3 GetBulletStartPos()
        {
            Vector3 flashPos = transform.position;
            flashPos.z = -50f;
            // switch ((TowerIndex)Info.index)
            // {
            //     case   Data.TowerIndex.Elephant: flashPos.ExAddXY(18f, 23f); break;
            //     case   Data.TowerIndex.Sting: flashPos.ExAddXY(35.6f, -5.17f); break;
            //     case   Data.TowerIndex.Flame: flashPos.ExAddXY(19.5f, 9.3f); break;
            // }
            return flashPos;
        }
        
        protected virtual void Shooting()
        {
            SetState(TowerStateType.Attack);
            
            GameObject bulletGo = SpawnManager.Instance.GetBullet(GetBulletStartPos(), Info.bulletIndex);
            BulletBase bullet = bulletGo.GetComponent<BulletBase>();

            DamageInfo damage = Damage();
            bullet.Init(player, Info.bulletIndex, Target, damage);

            //
            TotalDamege += damage._damage;
        }

        public bool UpgradePossible(int index, int tier)
        {
            return (this.Info.index == index) && (this.Info.tier == tier);
        }

        // TODO: 업그레이드시 스킨 하드코딩 되어있음
        protected int[][] skinIndex = {
            new[]{ 0,2,3,1 },
            new[]{ 0,3,2,1 },
            new[]{ 0,3,2,1 },
            new[]{ 0,1,2,3 },
            new[]{ 0,2,3,1 },
            new[]{ 0,2,3,1 },
            new[]{ 0,2,3,1 },
            new[]{ 0,2,3,1 },
        };
        
        public virtual void UpgradeTower()
        {
            TowerData data = DataManager.Instance.GetTowerData(Info.index);
            int tier = Info.tier + 1;
            SetInfo(data, tier);
            
            JTDebug.Log("index "+Info.index+" tier "+Info.tier);
            string skinNmae = "S0" + skinIndex[Info.index-1][Info.tier-2];
            spineBody.skeleton.SetSkin(skinNmae);
            spineBody.skeleton.SetSlotsToSetupPose();
            
            tearText.text = "Tear " + Info.tier;
            
            // 타워 초기화
            Reset();
        }
        
        #region Input

        protected Vector3 offSet;
        protected bool isDragging = false;
        private float mouseButtonDownTime = 0f;

        protected void OnMouseDown()
        {
            if (player != GameManager.Instance.myPlayer)
                return;
            
            mouseButtonDownTime = 0;
            isDragging = true;
            
            offSet = spineBody.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            spineBody.GetComponent<MeshRenderer>().sortingOrder = DragLayer; // 드래그할때에만 오더증가
        }

        protected void OnMouseDrag()
        {
            if (player != GameManager.Instance.myPlayer)
                return;
            
            if (!canDrag)
                return;

            if (!GameManager.Instance.canTowerDrag)
                return;
            
            mouseButtonDownTime += Time.deltaTime;

            spineBody.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offSet;
        }
        
        protected void OnMouseUp()
        {
            if (player != GameManager.Instance.myPlayer)
                return;

            isDragging = false;
            
            // 클릭으로 간주
            if (mouseButtonDownTime < 0.2f)
            {
                SoundManager.Instance.PlayOneShot("tower_select");
                SetFocus();
                ReturnToOriPos(0);
                
                player.CheckUserAvailableSkill(BaseSkill.SkillAction.Touch_Tower, this);
            }
            // 드래그로 간주
            else
            {
                isDragging = false;
                spineBody.GetComponent<MeshRenderer>().sortingOrder = NormalLayer; // 드래그할때에만 오더증가 삭제

                TowerBase target = DragAndDropTower(); 
                if (target != null)
                {
                    // // TODO: 자리바꾸기 스킬 사용시에만 처리로 코드변경
                    // isSkillUsed = player.CheckUserAvailableSkill(BaseSkill.SkillAction.Drag_Tower, new object[]{this, target});
                    // if (isSkillUsed)
                    // {
                    //     return;
                    // }
                    
                    if (target.Info.tier < 6  &&target.UpgradePossible(Info.index, Info.tier))
                    {
                        // 포커스 해제
                        SetFocus(false);
                        
                        // 현재 드래그한 타워 삭제
                        player.TowerDestroy(TowerPosIndex);
                        
                        // 드랍한 대상 타워자리에 업그레이드
                        player.UpgradeTower(target.TowerPosIndex);
                        return;
                    }
                }

                ReturnToOriPos();
            }
        }

        private TowerBase DragAndDropTower()
        {
            // 드래그로 타워에 놓을경우, 같은 티어&종류 타워라면 업그레이드
            Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition); // 터치한 좌표 가져옴
            Ray2D ray = new Ray2D(wp, Vector2.zero); // 원점에서 터치한 좌표 방향으로 Ray를 쏨
            RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction);
            foreach (var hit in hits)
            {
                if (hit && hit.collider.gameObject != gameObject)
                {
                    TowerBase target = hit.collider.gameObject.GetComponent<TowerBase>();
                    // 같은 티어 & 종류 타워라면 업그레이드
                    if (target != null)
                    {
                        return target;
                    }
                }
            }

            return null;
        }

        public void ReturnToOriPos(float duration = 0.5f)
        {
            canDrag = false;
            spineBody.transform.DOLocalMove(originPosition, duration).OnComplete(() =>
            {
                canDrag = true;
            });
        }

        protected void SetFocus()
        {
            if (GameManager.Instance.isFocus)
            {   // 자기자신을 클릭시 해제
                if (GameManager.Instance.focusTower == this) 
                    SetFocus(false);
                else 
                { // 다른 포커스에서 자신으로 올경우
                    GameManager.Instance.focusTower.SetFocus(false);
                    SetFocus(true);
                }
            }
            else // 클릭
                SetFocus(true);
            
            GameUIManager.Instance.UpdateInfo();
        }
        
        public void SetFocus(bool active)
        {
            if (active)
            {
                GameManager.Instance.ShowForcusTower(this);
                select.gameObject.SetActive(true);
            }
            else
            {
                GameManager.Instance.HideForcusTower();
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
            ReturnToOriPos(0);
            other.ReturnToOriPos(0);
        }
        
        public void OnStageClear()
        {
        }
        
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

            player.AddTotalDamage(Info.index, TotalDamege, Info.tier);

            StopCoroutine(TowerUpdate());
            transform.DOKill();
            Destroy(gameObject);
        }
    }
}
