using System.Collections;
using System.Globalization;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace RandomFortress
{
    /// <summary>
    /// 타워는 로컬객체로 생성된다.
    /// 타워에 연관된 총알, 피격이펙트, 데미지이펙트는 전부 각각의 클라이언트에서 따로 관리.
    /// 실제 데미지 적용 및 생성 파괴 시점과같은것은 타워객체의 오너가 진행
    /// </summary>
    public class TowerBase : EntityBase
    {
        [SerializeField] protected TowerInfo info;
        public TowerInfo Info => info;

        [Header("공통")] 
        [SerializeField] protected SkeletonAnimation spineBody;
        [SerializeField] protected SpriteRenderer select;
        [SerializeField] protected SpriteRenderer seat;
        [SerializeField] protected TextMeshPro dpsText;
        [SerializeField] protected TextMeshPro tearText;
        [SerializeField] protected SortingGroup sortingGroup;
        
        /// <summary> Towers 배열의 Index역할 and 현재타워위치 </summary>
        public int TowerPosIndex { get; protected set; }

        protected float AttackWaitTimer = 0f; // 타워 공격시 딜레이시간 타이머
        protected MonsterBase Target = null;
        protected Vector3 originPosition;
        protected TweenerCore<Vector3, Vector3, VectorOptions> character_ani;
        protected TowerStateType currentState = TowerStateType.Idle;
        protected float attackAniSpeed; // 공격속도 증가시 애니메이션 속도 조정
        protected const int NormalLayer = 13;
        protected const int DragLayer = 14;

        protected int TotalDamege; // 타워가 준 총 데미지

        // TODO: 업그레이드시 스킨 하드코딩 되어있음
        protected int[][] skinIndex =
        {
            new[] { 0, 2, 3, 1 }, // 1
            new[] { 0, 3, 2, 1 }, // 2
            new[] { 0, 3, 2, 1 }, // 3
            new[] { 0, 1, 2, 3 }, // 4
            new[] { 0, 2, 3, 1 }, // 5
            new[] { 0, 2, 3, 1 }, // 6
            new[] { 0, 2, 3, 1 }, // 7
            new[] { 0, 2, 3, 1 }, // 8
        };
        
        private Vector3 oriScale; // 티어별 타워크기
        private float[] multiScale = { 0.7f, 1f, 1.05f, 1.1f, 1.15f };
        
        private const float MaxAttackSpeedMultiplier = 100f;

        protected override void Awake()
        {
            base.Awake();
            sortingGroup = GetComponent<SortingGroup>();
        }

        public virtual void Init(GamePlayer targetPlayer, int posIndex, int towerIndex, int tier)
        {
            // 초기설정
            player = targetPlayer;
            TowerPosIndex = posIndex;
            SetInfo(DataManager.I.GetTowerData(towerIndex), tier);
            sortingGroup.sortingOrder = NormalLayer;

            // 타워 내부정보 리셋
            Reset();

            // 타워는 로컬플레이어만 업데이트를 처리한다.
            if (player.IsLocalPlayer)
                StartCoroutine(TowerUpdate());
        }

        public override void Reset()
        {
            gameObject.SetActive(true);
            gameObject.name = info.towerName + " " + info.tier;
            IsDestroyed = false;
            DOTween.Kill(transform);
            AttackWaitTimer = 100f; // 생성시에 바로공격을 위하여
            Target = null;
            originPosition = spineBody.transform.localPosition;
            TotalDamege = 0;
            _canInput = true;
            
            // TODO: 공격속도가 변할때마다 이코드가 필요하다
            float duration = SpineUtils.GetAnimationDuration(spineBody, "Attack");
            attackAniSpeed = ((float)info.attackSpeed / 100) * duration;
            
            // 타워 초기화
            SetBody();
            SetFocus(false);
            SetState();
        }

        protected virtual void SetInfo(TowerData data, int tier = 1)
        {
            if (info == null)
                info = new TowerInfo(data.towerInfoDic[tier]);
            else
                info.UpgradeTower(data.towerInfoDic[tier]);

            // 높은 티어의 타워일경우
            if (tier > 1)
            {
                string skinNmae = "S0" + skinIndex[info.index - 1][info.tier - 2];
                spineBody.skeleton.SetSkin(skinNmae);
                spineBody.skeleton.SetSlotsToSetupPose();

                tearText.text = "Tear " + info.tier;
            }

            if (GameManager.I.gameType == GameType.Solo)
                info.attackRange = (int)(info.attackRange * GameConstants.atkRangeMul);
        }
        
        protected virtual void SetBody()
        {
            // 솔로모드 
            if (GameManager.I.gameType == GameType.Solo)
                transform.localScale = new Vector3(1.333f, 1.333f, 1.333f);

            spineBody.transform.localScale = spineBody.transform.localScale * multiScale[info.tier - 1];

            // 티어1일때는
            seat.sprite = ResourceManager.I.GetSprite(GameConstants.TowerSeatImageName + info.tier);
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

        public void UpdateGameResult()
        {
            player.AddTotalDamage(info.index, TotalDamege, info.tier);
        }

        protected virtual IEnumerator TowerUpdate()
        {
            while (gameObject.activeSelf)
            {
                // 일시정지
                if (GameManager.I.isPaused)
                {
                    yield return null;
                    continue;
                }

                // 게임오버 또는 타워 제거시
                if (IsDestroyed || GameManager.I.isGameOver)
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

            // 게임오버시 애니메이션 정지
            if ( spineBody != null)
                spineBody.AnimationState.TimeScale = 0;
        }

        protected virtual void SearchMonstersInAttackRange()
        {
            // 공격 대상자 찾기
            var array = player.monsterList.ToArray();
            for (int i = 0; i < array.Length; ++i)
            {
                MonsterBase monster = array[i];
                if (monster == null || monster.gameObject.activeSelf == false)
                    continue;

                float distance = Vector3.Distance(transform.position, monster.transform.position);
                if (distance <= info.attackRange)
                {
                    Target = monster;
                    break;
                }
            }
        }
        
        protected virtual void UpdateAttackTargetAndShooting()
        {
            UpdateAttackTimer();

            if (IsReadyToAttack())
            {
                if (IsTargetInRangeAndActive())
                {
                    AttackWaitTimer = 0;
                    Shooting();
                }
                else
                {
                    Target = null;
                }
            }
        }

        void UpdateAttackTimer()
        {
            float timeScaleAdjustedDeltaTime = Time.deltaTime * GameManager.I.gameSpeed;
            AttackWaitTimer += timeScaleAdjustedDeltaTime;
        }

        bool IsReadyToAttack()
        {
            float atkSpeedMultiplier = (100 + player.extraInfo.atkSpeed) / MaxAttackSpeedMultiplier;
            float atkSpeed = info.attackSpeed * atkSpeedMultiplier;
            float attackWaitTime = MaxAttackSpeedMultiplier / atkSpeed;
            return AttackWaitTimer >= attackWaitTime;
        }

        bool IsTargetInRangeAndActive()
        {
            if (Target == null || !Target.gameObject.activeSelf)
                return false;

            float distance = Vector3.Distance(transform.position, Target.transform.position);
            return distance <= info.attackRange;
        }

        protected void ShowDps()
        {
            dpsText.text = TotalDamege.ToString(CultureInfo.CurrentCulture);
        }

        // 타워 업그레이드와 카드레벨과 디버프를 계산하여 데미지를 준다
        protected DamageInfo GetDamage()
        {
            TextType type = TextType.Damage;
            float damage = info.attackPower;

            // 타워 데미지 계산
            float upgradeDmg = GameManager.I.towerUpgradeDic[info.index].GetDamage();
            float abilityDmg = (100f + player.extraInfo.atk) / 100;
            damage *= upgradeDmg * abilityDmg;

            // 크리티컬시
            float totalDamage = damage;
            for (int i = 0; i < player.abilityList.Count; ++i)
            {
                ExtraInfo extraInfo = player.abilityList[i];
                if (extraInfo.criChance > 0)
                {
                    int criChance = extraInfo.criChance;
                    bool isCri = Random.Range(0, 100) < criChance;
                    if (isCri)
                    {
                        type = TextType.DamageCritical;
                        totalDamage += damage * (extraInfo.criAtk / 100f);
                    }
                }
            }
            
#if UNITY_EDITOR
            totalDamage *= GameManager.I.CheatDamage;
#endif

            return new DamageInfo((int)totalDamage, type);
        }

        // 총알 발사시 타워마다 시작지점이 다르다
        protected virtual Vector3 GetBulletStartPos()
        {
            // Vector3 flashPos = transform.position;
            // flashPos.z = -50f;
            // switch ((TowerIndex)_info.index)
            // {
            //     case   Data.TowerIndex.Elephant: flashPos.ExAddXY(18f, 23f); break;
            //     case   Data.TowerIndex.Sting: flashPos.ExAddXY(35.6f, -5.17f); break;
            //     case   Data.TowerIndex.Flame: flashPos.ExAddXY(19.5f, 9.3f); break;
            // }
            return transform.position;
        }
        
        public void AddDamage(int damage) => TotalDamege += damage;

        protected virtual void Shooting()
        {
            DamageInfo damage = GetDamage();
            DoShooting(Target, damage);
            player.Shooting(TowerPosIndex, Target._unitID, damage._damage, (int)damage._type);
        }
        
        public virtual void ReceiveShooting(int unitID, int damage, int damageType, bool isDebuff)
        {
            MonsterBase target = player.entityDic[unitID] as MonsterBase;
            DamageInfo damageInfo = new DamageInfo(damage, (TextType)damageType);
            DoShooting(target, damageInfo);
        }

        protected virtual void DoShooting(MonsterBase target, DamageInfo damageInfo)
        {
            AddDamage(damageInfo._damage);
            
            SetState(TowerStateType.Attack);

            GameObject bulletGo = SpawnManager.I.GetBullet(GetBulletStartPos(), info.bulletIndex);
            BulletBase bullet = bulletGo.GetComponent<BulletBase>();
            
            bullet.Init(player, info.bulletIndex, target, damageInfo);
        }
        
        #region Input
        
        private const float ClickThreshold = 0.2f; // 클릭인지 아닌지 체크시간
        
        private Vector3 _offSet;
        private bool _canInput = true;
        private float _dragTime = 0f;

        private bool CanInput()
        {
            return player == GameManager.I.myPlayer && _canInput && !PopupManager.I.isOpenPopup;
        }
        
        public void MouseDown()
        {
            if (!CanInput()) return;
            
            _dragTime = 0f;
            
            _offSet = spineBody.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            sortingGroup.sortingOrder = DragLayer; // 드래그할때에만 오더증가
        }
        
        public void MouseDrag()
        {
            if (!CanInput()) return;

            if (!GameManager.I.canTowerDrag) return; // 드래그만 막는다
            
            _dragTime += Time.deltaTime;
            spineBody.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + _offSet;
        }
        
        public void MouseUp()
        {
            if (!CanInput()) return;
            
            // 클릭으로 간주
            if (_dragTime < ClickThreshold)
            {
                SoundManager.I.PlayOneShot(SoundKey.tower_select);
                SetFocus();
                ReturnToOriPos(0);
                player.CheckUserAvailableSkill(BaseSkill.SkillAction.Touch_Tower, this);
            }
            // 드래그로 간주
            else
            {
                sortingGroup.sortingOrder = NormalLayer; // 드래그할때에만 오더증가 삭제
        
                TowerBase target = DragAndDropTower(); // 합체 대상이 되는 타워를 찾는다.
                if (target != null && CanUpgrade(target))
                {
                    // 타워 업그레이드
                    SetFocus(false);
                    player.TowerDestroy(TowerPosIndex);
                    player.TowerDestroy(target.TowerPosIndex);
                    player.BuildRandomTower(info.tier + 1, target.TowerPosIndex);
                }
                else
                    ReturnToOriPos();
            }
        }
        
        public bool UpgradePossible(int index, int tier) => (info.index == index) && (info.tier == tier);
        
        private bool CanUpgrade(TowerBase target) => target.info.tier < 5 && target.UpgradePossible(info.index, info.tier);
        
        // 타워를 합체
        private TowerBase DragAndDropTower()
        {
            // 현재 마우스 포인터의 월드 좌표 가져오기
            Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 마우스 포인터 위치에서 아래로 Raycast 시도
            RaycastHit2D[] hits = Physics2D.RaycastAll(wp, Vector2.zero);

            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject != gameObject)
                {
                    // TowerBase 컴포넌트 캐싱
                    TowerBase target = hit.collider.GetComponent<TowerBase>();
                    if (target != null)
                    {
                        return target; // 타워가 감지되면 바로 반환
                    }
                }
            }

            return null; // 타워가 감지되지 않으면 null 반환
        }
        
        // 원래 위치로
        private void ReturnToOriPos(float duration = 0.5f)
        {
            _canInput = false;
            spineBody.transform.DOLocalMove(originPosition, duration).OnComplete(() => { _canInput = true; });
        }

        // 타워의 포커스를 설정 또는 해제한다
        private void SetFocus()
        {
            TowerBase focusTower = GameManager.I.focusTower;
            
            if (focusTower == this)
            {
                // 자기자신을 클릭시 해제
                SetFocus(false);
            }
            else
            {
                if (focusTower != null)
                {
                    // 다른 포커스에서 자신으로 올경우
                    focusTower.SetFocus(false); // 해당타워 포커스 해제
                    SetFocus(true); // 현재타워 포커스
                }
                else
                {
                    // 선택한 타워가 없다면
                    bool isForcus = select.gameObject.activeSelf;
                    SetFocus(!isForcus);
                }
            }
        }

        // 타워 포커스를 설정. 외부에서 조작시엔 게임매니저 호출을 하지않는다.
        public void SetFocus(bool active, bool callManager = true)
        {
            select.gameObject.SetActive(active);
            
            if (callManager)
            {
                if (active)
                    GameManager.I.ShowFocusTower(this);
                else
                    GameManager.I.HideFocusTower();
            }
        }

        // 타워 스왑
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

            SetFocus(false);
            other.SetFocus(false);
        }

        #endregion

        public void TowerDestroy()
        {
            Remove();
        }

        protected override void Remove()
        {
            if (IsDestroyed) return;
            IsDestroyed = true;

            player.AddTotalDamage(info.index, TotalDamege, info.tier);

            StopCoroutine(TowerUpdate());
            transform.DOKill();
            Destroy(gameObject);
        }
    }
}