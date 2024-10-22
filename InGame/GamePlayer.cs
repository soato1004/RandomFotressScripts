using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace RandomFortress
{
    public class GamePlayer : MonoBehaviour
    {
        #region 변수

#if UNITY_EDITOR
        public int CheatTowerIndex = -1;
#endif
        [Header("Photon")] 
        [SerializeField] private int actorNumber;
        [SerializeField] private PhotonView photonView;
        [SerializeField] private Player photonPlayer; // 포톤 플레이어
        public SerializedDictionary<int,EntityBase> entityDic = new(); // 모든유닛 저장
        
        [Header("플레이어 정보")]
        public int gold; // 골드
        public int playerHp; // 현재 체력
        public BaseSkill[] skillArr; // 보유한 스킬
        public List<ExtraInfo> abilityList = new(); // 선택한 어빌리티 인덱스를 가지고있음
        
        [Header("몬스터")]
        public List<MonsterBase> monsterList = new(); // 몬스터를 생성 순서대로 집어넣은 리스트

        [Header("타워")]
        public Transform towerParent; // 타워 생성될 부모위치
        public int towerCount = 0; // 생성된 타어 갯수
        public TowerBase[] Towers; // 타워
        public Transform[] TowerFixPos; // 타워 설치된 장소
        public int[] TowerDeck; // 타워덱
        public SerializedDictionary<int, TowerResultData> totalDmgDic = new (); // 타워별 데미지 누적

        [Header("총알")]
        public List<BulletBase> bulletList = new(); // 총알
        
        [Header("버프")] 
        private bool _isAbilityBuff = false; // 어빌리티 카드 버프

        [Header("스테이지")] 
        public GameMap gameMap; // 게임앱
        public int stageProcess; // 시작 스테이지
        
        [Header("어빌리티")] 
        [SerializeField] private Transform abilityTrf; // 어빌리티 부모
        private AbilityCard[] abilityCards; // 어빌리티 카드 표시
        private int abilitySelectCount = 0; // 획득한 어빌리티 카드 갯수
        
        [Header("스킬")]
        [SerializeField] private Image skillDim; // 스킬 사용시 딤
        
        [Header("UI")]
        public TextMeshProUGUI stageText; // 스테이지 표시 텍스트
        public Transform HpTrf; // 플레이어 체력
        public TextMeshProUGUI nicknameText; // 플레이어 닉네임
        
        
        private int total_tower_pos = 0; // 설치 가능한 타워 갯수
        private bool _isBuildProgress; // 현재 타워가 건설중인가
        
        
        public bool IsAbilityBuff => _isAbilityBuff; // 어빌리티 카드 추가선택
        public ExtraInfo extraInfo { get; private set; } // 어빌리티로 증가한 모든 능력치를 이곳에 적용시킴
        public bool IsLocalPlayer { get; private set; }
        public int ActorNumber { get; private set; }
        public string Nickname { get; private set; }
        
        public string Userid { get; set; } // firebase 구글 userdid를 사용함

        #endregion
        
        #region 설정

        public void Awake()
        {
            towerParent = transform.Find("Towers");
            extraInfo = new ExtraInfo();

            int i = 0;
            abilityCards = new AbilityCard[abilityTrf.childCount];
            foreach (var child in abilityTrf.GetChildren())
            {
                abilityCards[i++] = child.GetComponent<AbilityCard>();
            }

        }
        
        // 플레이어 설정
        public void SetPlayer(Player p, GameMap map)
        {
            // 게임맵 설정
            gameMap = map;

            // 플레이어 설정
            photonPlayer = p;
            
            nicknameText.SetText(p.NickName);
            
            
            IsLocalPlayer = p.IsLocal;
            actorNumber = ActorNumber = p.ActorNumber;
            Nickname = p.NickName;
            
            InitPlayer();
        }

        void InitPlayer()
        {
            // 타워설치할 공간
            Transform seatPos = gameMap.TowerSeatPos;
            total_tower_pos = seatPos.childCount;
            
            Towers = new TowerBase[total_tower_pos];
            TowerFixPos = seatPos.GetChildren();
            TowerDeck = Account.I.GetTowerDeck;
            
            // 게임 데이터 초기화
            gold = GameConstants.StartGold;
            playerHp = GameConstants.StartHp;
            
            // 스킬 세팅
            skillArr = new BaseSkill[GameConstants.SKILL_DECK_COUNT];
            
            int skillIndex = Account.I.Data.skillDeck[0];
            SkillCreator(0, skillIndex);
            
            skillIndex = Account.I.Data.skillDeck[1];
            SkillCreator(1, skillIndex);
            
            skillIndex = Account.I.Data.skillDeck[2];
            SkillCreator(2, skillIndex);
            
            // Ad 디버프 적용
            foreach (var adDebuff in Account.I.Data.adDebuffs)
            {
                switch (adDebuff.type)
                {
                    case AdRewardType.AbilityCard:
                        _isAbilityBuff = true;
                        break;
                }
            }
            
            GameUIManager.I.UpdateUI();
        }
        
        private GamePlayer GetPlayer(int actor) => GameManager.I.GetPlayer(actor);

        #endregion

        #region 플레이어

        public void AddTotalDamage(int index, int damage, int tier)
        {
            if (totalDmgDic.TryGetValue(index, out var data1))
            {
                data1.totalDamage += damage;
                if (data1.towerTier < tier)
                    data1.towerTier = tier;
            }
            else
            {
                TowerResultData data = new TowerResultData();
                data.towerIndex = index;
                data.totalDamage = damage;
                data.towerTier = tier;
                totalDmgDic.Add(index, data);
            }
        }
        
        // 몬스터 파괴시 골드획득
        public void KillMonster(int reward)
        {
            gold += reward + extraInfo.rewardMonster;
        }

        // 플레이어에게 데미지를 준다. 로컬플레이어만
        public void DamageToPlayer(int value = 1)
        {
            if (!IsLocalPlayer)
                return;
            
            playerHp -= value;
            UpdatePlayerHpUI();
            GameUIManager.I.UpdateUI();

            if (playerHp <= 0)
            {
                GameManager.I.EndGame();
            }

            if (GameManager.I.gameType != GameType.Solo)
                photonView.RPC(nameof(PlayerHpRpc), RpcTarget.Others, playerHp);
        }

        [PunRPC]
        private void PlayerHpRpc(int hp)
        {
            GamePlayer player = GameManager.I.otherPlayer;
            player.playerHp = hp;
            player.UpdatePlayerHpUI();
        }
        
        private void UpdatePlayerHpUI()
        {
            // 체력 업데이트
            for (int i = 0; i < 5; ++i)
            {
                HpTrf.GetChild(i).SetActive(false);
                if (i < playerHp)
                    HpTrf.GetChild(i).SetActive(true);
            }
        }
        
        // 게임 종료시에 호출
        public void UpdateGameResult()
        {
            foreach (var tower in Towers)
            {
                if (tower == null)
                    continue;
                tower.UpdateGameResult();
            }
        }
        
        public void SetAbility(AbilityData abilityData)
        {
            abilityList.Add(abilityData.extraInfo);
            extraInfo.AddExtraInfo(abilityData.extraInfo);
            abilityCards[abilitySelectCount++].SetCard(abilityData);
        }

        public void StageClear()
        {
            stageProcess++;
            stageText.text = stageProcess.ToString();
        }

        #endregion
        
        #region 타워
        
        // 타워 생성 조건 검사 및 초기화
        private bool CanCreateTower()
        {
            if (towerCount == total_tower_pos)
            {
                return false;
            }

            if (GameConstants.TowerCost > gold)
            {
                return false;
            }

            if (_isBuildProgress)
                return false;

            _isBuildProgress = true;
            return true;
        }

        // 타워 위치 랜덤 선택
        private int SelectRandomTowerPosition()
        {
            int posIndex = 0;
            float duration = 0f;

            do
            {
                posIndex = Random.Range(0, total_tower_pos);
                duration += Time.deltaTime;
                if (duration > GameConstants.MaxTowerSelectionDuration)
                {
                    Debug.Log("Tower Create 무한 루프!");
                    return -1; // 실패했을 때의 구분 값
                }
            } while (Towers[posIndex] != null);

            return posIndex;
        }

        // 타워 생성 메서드
        public void BuildRandomTower()
        {
            if (!CanCreateTower()) return;
            
            int posIndex = SelectRandomTowerPosition();
            if (posIndex == -1) return;
            
            GameUIManager.I.SetLockButton(Buttons.Build, false);
            
            gold -= GameConstants.TowerCost;

            BuildRandomTower(1, posIndex);
        }
        
        // 타워 업그레이드 시에는 티어 및 위치가 고정되서 온다
        public void BuildRandomTower(int tier , int posIndex)
        {
            // 타워 랜덤 선택
            int rand = Random.Range(0, TowerDeck.Length);
            int towerIndex = TowerDeck[rand];

#if UNITY_EDITOR
            towerIndex = CheatTowerIndex != -1 ? CheatTowerIndex : towerIndex;
#endif
            
            // 타워 생성
            int unitId = GameManager.I._unitID;
            
            if (GameManager.I.gameType == GameType.Solo)
                CreateTowerRpc(ActorNumber, posIndex, towerIndex, tier, unitId);
            else
                photonView.RPC(nameof(CreateTowerRpc), RpcTarget.All, ActorNumber, posIndex, towerIndex, tier, unitId);

            ++GameManager.I._unitID;
        }
        
        [PunRPC]
        public void CreateTowerRpc(int actor, int posIndex, int towerIndex, int tier, int unitId)
        {
            GamePlayer player = GetPlayer(actor);
            player.CreateTower(posIndex, towerIndex, tier, unitId);
        }

        private void CreateTower(int posIndex, int towerIndex, int tier, int unitId)
        {
            SoundManager.I.PlayOneShot(SoundKey.tower_create);
                        
            _isBuildProgress = false;
            
            // 생성되는 타워자리 처리
            Transform trf = TowerFixPos[posIndex];
            
            // 로컬로 타워생성
            GameObject towerGo = SpawnManager.I.GetTower(trf.position, towerParent, towerIndex);
            TowerBase tower = towerGo.GetComponent<TowerBase>();
            
            tower.Init(this, posIndex, towerIndex, tier);
            Towers[posIndex] = tower;
            towerCount += 1;
            
            entityDic.Add(unitId, tower);
            tower._unitID = unitId;

            GameUIManager.I.SetLockButton(Buttons.Build, true);
            GameUIManager.I.UpdateUI();
        }
        
        // 타워 판매
        public void SellTower(bool useSkill = false)
        {
            TowerBase focusTower = GameManager.I.focusTower;
            
            int getGold = useSkill ? focusTower.Info.salePrice*2 :focusTower.Info.salePrice;
            gold += getGold;
            
            GameUIManager.I.ShowGoldText(focusTower.transform.position, getGold);
            TowerDestroy(focusTower.TowerPosIndex);
        }

        // 타워 파괴
        public void TowerDestroy(int towerID)
        {
            if (GameManager.I.gameType == GameType.Solo)
                TowerDestroyRpc(ActorNumber, towerID);
            else
                photonView.RPC(nameof(TowerDestroyRpc), RpcTarget.All, ActorNumber, towerID);
        }

        [PunRPC]
        private void TowerDestroyRpc(int actor, int towerID)
        {
            GamePlayer player = GetPlayer(actor);
            player.DoTowerDestroy(towerID);
        }

        private void DoTowerDestroy(int towerID)
        {
            SoundManager.I.PlayOneShot(SoundKey.tower_sell);
            
            TowerBase tower = Towers[towerID];
            
            entityDic.Remove(tower._unitID);
            
            Towers[tower.TowerPosIndex] = null;
            towerCount -= 1;
            tower.TowerDestroy();
            
            GameManager.I.HideFocusTower();
            GameUIManager.I.UpdateUI();
        }

        // 총탄 발사시
        public void Shooting(int towerPosIndex, int unitID, int damage, int damageType, bool isDebuff = false)
        { 
            if (GameManager.I.gameType != GameType.Solo)
                photonView.RPC(nameof(ShootingRpc), RpcTarget.Others, ActorNumber,towerPosIndex, unitID, damage, damageType, isDebuff);
        }
        
        [PunRPC]
        private void ShootingRpc(int actor, int towerPosIndex, int unitID, int damage, int damageType, bool isDebuff)
        {
            GamePlayer player = GetPlayer(actor);
            
            if (player.entityDic.ContainsKey(unitID))
            {
                TowerBase tower = player.Towers[towerPosIndex];
                if (tower != null)
                    tower.ReceiveShooting(unitID, damage, damageType, isDebuff);
                else
                    Debug.Log("대상 타워를 찾을수없다");
            }
            else
                Debug.Log("대상 몬스터를 찾을수 없다");
        }
        
        #endregion
        
        #region 몬스터
        
        public void MonsterDestroy(int unitID)
        {
            photonView.RPC(nameof(MonsterDestroyRpc), RpcTarget.Others, ActorNumber, unitID);
        }
        
        [PunRPC]
        private void MonsterDestroyRpc(int actor, int unitID)
        {
            GamePlayer player = GetPlayer(actor);
            player.DoMonsterDestroy(unitID);
        }

        private void DoMonsterDestroy(int unitID)
        {
            if (entityDic.TryGetValue(unitID, out var value))
            {
                MonsterBase monster = value as MonsterBase;
                monster?.DestroyMonster();
            }
            else Debug.Log("플레이어 "+ ActorNumber +", 몬스터파괴: "+ unitID);
        }
        
        public Vector3 GetMonsterNextTargetPoint(int index)
        {
            bool isMine = GameManager.I.myPlayer.ActorNumber == ActorNumber;
            Vector3 next = GameManager.I.GetRoadPos(index);
            if (!isMine)
                next.y += GameManager.I.offset;
            return next;
        }
        
        #endregion
        
        #region 스킬
        
        // 스킬 생성
        private void SkillCreator(int slotIndex, int skillIndex)
        {
            BaseSkill skill = null;
            
            switch ((SkillIndex)skillIndex)
            {
                case SkillIndex.WaterSlice:  skill = gameObject.AddComponent<WaterSliceSkill>(); break;
                case SkillIndex.ChangePlace: skill = gameObject.AddComponent<ChangePlaceSkill>(); break;
                case SkillIndex.GoodSell: skill = gameObject.AddComponent<GoodSellSKill>(); break;
                default: Debug.LogError("Skill Index Not Found"); break;
            }

            SkillButton button = GameUIManager.I.GetSkillButton(slotIndex);
            skill.Init(skillIndex, this, button);
            skillArr[slotIndex] = skill;
        }
        
        // 스킬 사용시점에 사용할수있는 스킬이 있는지 체크
        public bool CheckUserAvailableSkill(BaseSkill.SkillAction skillAction, params object[] values)
        {
            foreach (BaseSkill skill in skillArr)
            {
                if (!skill.useSkill)
                    continue;
                    
                // 해당 상황에 맞는 스킬이 있는지
                foreach (BaseSkill.SkillAction action in skill._skillActions)
                {
                    if (skillAction == action && skill.CanUseSkill())
                    {
                        skill.UseSkill(values);
                        return true;
                    }
                }
            }
            return false;
        }
        
        // 스킬 딤 설정
        public void SetSkillDim(bool show)
        {
            if (show)
            {
                skillDim.DOFade(0.6f, 0.2f);
                foreach (var dim in skillDim.transform.GetChildren())
                    dim.GetComponent<Image>().DOFade(1f, 0.2f);
            }
            else
            {
                skillDim.DOFade(0f, 0.2f);
                foreach (var dim in skillDim.transform.GetChildren())
                    dim.GetComponent<Image>().DOFade(0f, 0.2f);
            }
        }
        
        // 스킬 사용
        public void SkillUse(int skillBtnIdex)
        {
            if (GameManager.I.gameType == GameType.Solo)
            {
                SkillUseRpc(PhotonNetwork.LocalPlayer.ActorNumber, skillBtnIdex);
            }else 
            {
                photonView.RPC(nameof(SkillUseRpc), RpcTarget.All, ActorNumber, skillBtnIdex);
            }
        }
        
        [PunRPC]
        private void SkillUseRpc(int actor, int skillBtnIdex)
        {
            GamePlayer player = GetPlayer(actor);
            
            GameManager.I.SetSkillDim(true,player);
            if (player.skillArr[skillBtnIdex].CanUseSkill())
                player.skillArr[skillBtnIdex].SkillStart();
        }

        // 스킬사용 종료시 (타인에게 호출할때만 사용됨)
        public void SkillEnd(int skillIndex, params object[] values)
        {
            Debug.Log("스킬종료 : " + skillIndex + ", "+ values);
            
            if (GameManager.I.gameType == GameType.Solo)
                return;

            switch ((SkillIndex)skillIndex)
            {
                case SkillIndex.WaterSlice: photonView.RPC(nameof(SkillEndRpc), RpcTarget.Others, ActorNumber); break;
                case SkillIndex.ChangePlace: photonView.RPC(nameof(SkillSwapTowerRpc), RpcTarget.Others, ActorNumber, (int)values[0], (int)values[1]);break;
                case SkillIndex.GoodSell: photonView.RPC(nameof(SkillSellTowerRpc), RpcTarget.Others, ActorNumber, (int)values[0], (int)values[1]);break;
            }
        }
        
        [PunRPC]
        private void SkillEndRpc(int playerActorNumber)
        {
            Debug.Log("SkillEndRpc");
            GamePlayer player = GetPlayer(playerActorNumber);
            
            player.SetSkillDim(false);
        }
        
        [PunRPC]
        private void SkillSwapTowerRpc(int playerActorNumber, int towerIndexA, int towerIndexB)
        {
            Debug.Log("SkillSwapTowerRpc : "+towerIndexA + ", "+ towerIndexB);
            GamePlayer player = GetPlayer(playerActorNumber);
            
            player.SetSkillDim(false);

            // 타워 배열과 타워 인덱스 확인
            TowerBase[] towers = player.Towers;
            
            TowerBase towerA = player.Towers[towerIndexA];
            TowerBase towerB = player.Towers[towerIndexB];
            
            if (towerA == null || towerB == null)
            {
                Debug.LogError("Invalid tower indices");
                return;
            }
            
            // 타워 위치 교환
            (towers[towerIndexA], towers[towerIndexB]) = (towers[towerIndexB], towers[towerIndexA]);

            towerA.Swap(towerB);
        }
        
        [PunRPC]
        private void SkillSellTowerRpc(int playerActorNumber, int towerPosIndex, int getGold)
        {
            Debug.Log("SkillSellTowerRpc");
            GamePlayer player = GetPlayer(playerActorNumber);
            
            player.SetSkillDim(false);
            
            Vector3 startPos = TowerFixPos[towerPosIndex].transform.position;
            GameUIManager.I.ShowGoldText(startPos, getGold);
        }
        
        
        #endregion

        #region 총알

        public void AddBullet(BulletBase bullet)
        {
            // entityDic.Add(bullet.unitID, bullet);
            bulletList.Add(bullet);
        }

        public void RemoveBullet(BulletBase bullet)
        {
            // entityDic.Remove(bullet.unitID);
            bulletList.Remove(bullet);
        }
        
        #endregion

        #region 네트웍

        // 네크워크를 통한 동기화
        public void SynchronizeOnReturn()
        {
            foreach (MonsterBase monster in monsterList)
                photonView.RPC(nameof(ReceiveSyncData), RpcTarget.Others, monster._unitID, monster.SerializeSyncData());

            foreach (TowerBase tower in Towers)
            {
                if (tower != null)
                    photonView.RPC(nameof(ReceiveSyncData), RpcTarget.Others, tower._unitID, tower.SerializeSyncData());
            }
            
            // foreach (var bullet in bulletList)
            //     photonView.RPC(nameof(ReceiveSyncData), RpcTarget.Others, bullet.unitID, bullet.SerializeSyncData());
        }
        
        // 네트워크를 통해 동기화 데이터 전송
        [PunRPC]
        protected void ReceiveSyncData(int unitID, object[] syncData)
        {
            GameManager.I.otherPlayer.EntitySync(unitID, syncData);
        }

        private void EntitySync(int unitID, object[] syncData)
        {
            if (entityDic.TryGetValue(unitID, out EntityBase entity))
            {
                entity.DeserializeSyncData(syncData);
            }
            else
                Debug.Log("Not Found Entity ID : "+unitID);
        }
        
        #endregion
        
        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}