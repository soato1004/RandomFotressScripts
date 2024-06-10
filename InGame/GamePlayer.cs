using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;


using RandomFortress.Data;

using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace RandomFortress
{
    public class GamePlayer : MonoBehaviour, IPunObservable
    {
        [Header("포톤")]
        private PhotonView photonView;
        public bool isLocalPlayer = false;
        public int actorNumber;
        
        [Header("플레이어 정보")]
        public int gold;
        public int playerHp;
        public BaseSkill[] skillArr;
        public List<RewardData> rewardDatas = new List<RewardData>();
        public List<ExtraInfo> abilityList = new List<ExtraInfo>(); // 선택한 어빌리티 인덱스를 가지고있음
        
        [Header("몬스터")]
        public SerializedDictionary<int,MonsterBase> monsterDic = new SerializedDictionary<int, MonsterBase>(); // 유닛id로 식별
        public List<MonsterBase> monsterOrder = new List<MonsterBase>();

        [Header("타워")]
        public Transform towerParent;
        public int towerCount = 0;
        public TowerBase[] Towers; // 타워정보
        public Transform[] TowerFixPos; // 타워 설치된 장소
        public int[] TowerDeck; // 현재 선택된 8개의 타워값
        // public SerializedDictionary<int, TowerInfo> towerInfoDic = new SerializedDictionary<int, TowerInfo>(); // 타워 덱. 업그레이드시 Info 교체
        public SerializedDictionary<int, TowerResultData> totalDmgDic = new SerializedDictionary<int, TowerResultData>(); // 타워별 데미지 누적

        [Header("버프리스트")] 
        private bool _isAbilityBuff = false;
        public List<EntityBase> bulletList = new List<EntityBase>();

        [Header("스테이지")] 
        [HideInInspector] public GameMap gameMap;
        public int stageProcess; // 시작 스테이지
        public TextMeshProUGUI stageText;
        public Transform HpTrf;
        
        
        [Header("어빌리티")] 
        [SerializeField] private Transform abilityTrf;
        private AbilityCard[] abilityCards;
        private int abilitySelectCount = 0;
        
        [Header("스킬")]
        [SerializeField] private Image skillDim;
        
        
        public bool IsAbilityBuff => _isAbilityBuff;
        public ExtraInfo extraInfo { get; private set; } // 어빌리티로 증가한 모든 능력치를 이곳에 적용시킴
        
        
        private int total_tower_pos = 0;
        private bool _isBuildProgress; // 현재 타워가 건설중인가
        

        public void Awake()
        {
            towerParent = transform.Find("Towers");
            photonView = GetComponent<PhotonView>();
            extraInfo = new ExtraInfo();

            int i = 0;
            abilityCards = new AbilityCard[abilityTrf.childCount];
            foreach (var child in abilityTrf.GetChildren())
            {
                abilityCards[i++] = child.GetComponent<AbilityCard>();
            }

        }

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

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                // stream.SendNext(PlayerHp);
            }
            else
            {
                // Network player, receive data
                // this.PlayerHp = (int)stream.ReceiveNext();
            }
        }
        
        #endregion
        
        // 솔로모드 플레이어 설정
        public void SetPlayer(GameMap map)
        {
            gameMap = map;
            
            // 자기자신인지 체크
            isLocalPlayer = true;
            
            InitPlayer();
        }
        
        // 1대1 플레이어 설정
        public void SetPlayer(Player p, GameMap map)
        {
            gameMap = map;
            
            actorNumber = p.ActorNumber;

            // 자기자신인지 체크
            isLocalPlayer = p.IsLocal;

            InitPlayer();
        }

        void InitPlayer()
        {
            // 타워설치할 공간
            Transform seatPos = gameMap.TowerSeatPos;
            total_tower_pos = seatPos.childCount;
            
            Towers = new TowerBase[total_tower_pos];
            TowerFixPos = seatPos.GetChildren();
            TowerDeck = Account.Instance.GetTowerDeck;
            
            // 게임 데이터 초기화
            gold = 1000;
            playerHp = 5;
            
            // 스킬 세팅
            skillArr = new BaseSkill[GameConstants.SKILL_DECK_COUNT];
            
            int skillIndex = Account.Instance.Data.skillDeck[0];
            SkillCreator(0, skillIndex);
            
            skillIndex = Account.Instance.Data.skillDeck[1];
            SkillCreator(1, skillIndex);
            
            skillIndex = Account.Instance.Data.skillDeck[2];
            SkillCreator(2, skillIndex);
            
            // Ad 디버프 적용
            foreach (var adDebuff in Account.Instance.Data.adDebuffList)
            {
                switch (adDebuff.type)
                {
                    case AdDebuffType.AbilityCard:
                        _isAbilityBuff = true;
                        break;
                }
            }
            
            GameUIManager.Instance.UpdateInfo();
        }
        
        public void KillMonster(int reward)
        {
            if (!isLocalPlayer)
                return;
                
            gold += reward + extraInfo.rewardMonster;
        }

        public void DamageToPlayer(int value = 1)
        {
            if (!isLocalPlayer)
                return;
            
            playerHp -= value;
            UpdatePlayerHp();
            GameUIManager.Instance.UpdateInfo();
            
            if (playerHp <= 0)
                GameManager.Instance.EndGame();
            
            if (GameManager.Instance.gameType != GameType.Solo)
                photonView.RPC("PlayerHpSync", RpcTarget.Others, actorNumber, playerHp);
        }
        
        private GamePlayer GetPlayer(int actor)
        {
            if (GameManager.Instance.gameType == GameType.Solo)
                return GameManager.Instance.myPlayer;
            
            if (!GameManager.Instance.players.ContainsKey(actor))
            {
                Debug.Log("Not Found Player!! "+ actor);
                return GameManager.Instance.myPlayer;
            }
            
            return GameManager.Instance.players[actor];
        }

        [PunRPC]
        public void PlayerHpSync(int actor, int hp)
        {
            GamePlayer player = GetPlayer(actor);
            
            player.playerHp = hp;
            player.UpdatePlayerHp();
        }
        
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
            
            GameUIManager.Instance.SetLockButton(Buttons.Build, false);
            
            gold -= GameConstants.TowerCost;
            
            int rand = Random.Range(0, TowerDeck.Length);
            int towerIndex = TowerDeck[rand];

            // towerIndex = (int)TowerIndex.Elephant;
            
            // 타워 생성
            if (GameManager.Instance.gameType == GameType.Solo)
                CreateTowerRpc(actorNumber, posIndex, towerIndex, 1);
            else
                photonView.RPC("CreateTowerRpc", RpcTarget.AllViaServer, 
                    actorNumber, posIndex, towerIndex, 1);
            
        }
        
        // 타워합체 후 상위타워 생성
        public void BuildRandomTower(int tier , int posIndex)
        {
            // 타워 랜덤 선택
            int rand = Random.Range(0, TowerDeck.Length);
            int towerIndex = TowerDeck[rand];
            
            // 타워생성
             if (GameManager.Instance.gameType == GameType.Solo)
                CreateTowerRpc(actorNumber, posIndex, towerIndex, tier);
            else
                photonView.RPC("CreateTowerRpc", RpcTarget.AllViaServer, 
                actorNumber, posIndex, towerIndex, tier);
        }
        
        [PunRPC]
        public void CreateTowerRpc(int actor, int posIndex, int towerIndex, int tier)
        {
            GamePlayer player = GetPlayer(actor);
            
            // Debug.Log("actor : "+ actor + ", Tower Create " + towerIndex + "," + posIndex + ", tier : "+tier);
            
            SoundManager.Instance.PlayOneShot("tower_create");
                        
            player._isBuildProgress = false;
            
            // 생성되는 타워자리 처리
            Transform trf = player.TowerFixPos[posIndex];
            
            // 로컬로 타워생성
            GameObject towerGo = SpawnManager.Instance.GetTower(trf.position, towerIndex);
            TowerBase tower = towerGo.GetComponent<TowerBase>();
            
            tower.Init(player, posIndex, towerIndex, tier);
            player.Towers[posIndex] = tower;
            player.towerCount += 1;

            GameUIManager.Instance.SetLockButton(Buttons.Build, true);
            GameUIManager.Instance.UpdateInfo();
        }
        
        public void SellTower(bool useSkill = false)
        {
            TowerBase focusTower = GameManager.Instance.focusTower;
            
            int getGold = useSkill ? focusTower.Info.salePrice*2 :focusTower.Info.salePrice;
            gold += getGold;
            
            GameUIManager.Instance.ShowGoldText(focusTower.transform.position, getGold);
            
            if (GameManager.Instance.gameType == GameType.Solo)
                TowerDestroyRpc(actorNumber, focusTower.TowerPosIndex);
            else
                photonView.RPC("TowerDestroyRpc", RpcTarget.AllViaServer, actorNumber, focusTower.TowerPosIndex);
        }
        
        public void TowerDestroy(int towerID)
        {
            if (GameManager.Instance.gameType == GameType.Solo)
                TowerDestroyRpc(actorNumber, towerID);
            else
                photonView.RPC("TowerDestroyRpc", RpcTarget.AllViaServer, actorNumber, towerID);
        }
        
        [PunRPC]
        public void TowerDestroyRpc(int actor, int towerID)
        {
            GamePlayer player = GetPlayer(actor);
            
            SoundManager.Instance.PlayOneShot("tower_sell");
            
            TowerBase tower = player.Towers[towerID];
            player.Towers[tower.TowerPosIndex] = null;
            player.towerCount -= 1;
            tower.TowerDestroy();

            GameManager.Instance.HideForcusTower();
            GameUIManager.Instance.UpdateInfo();
        }

        public void UpgradeTower(int towerPosIndex, int tier)
        {
            if (GameManager.Instance.gameType == GameType.Solo)
                UpgradeTowerRpc(actorNumber, towerPosIndex, tier);
            else
                photonView.RPC("UpgradeTowerRpc", RpcTarget.AllViaServer, actorNumber, towerPosIndex, tier);
        }
        
        [PunRPC]
        public void UpgradeTowerRpc(int actor, int towerID, int tier)
        {
            GamePlayer player = GetPlayer(actor);
            
            TowerBase tower = player.Towers[towerID];
            tower.UpgradeTower(tier);
            
            GameManager.Instance.HideForcusTower();
            GameUIManager.Instance.UpdateInfo();
        }

        // 총탄 발사시
        public void Shooting(int towerPosIndex, int unitID, int damage, int damageType, bool isDebuff = false)
        { 
            photonView.RPC("ShootingRpc", RpcTarget.Others, 
                actorNumber,towerPosIndex, unitID, damage, damageType, isDebuff);
        }
        
        [PunRPC]
        public void ShootingRpc(int actor, int towerPosIndex, int unitID, int damage, int damageType, bool isDebuff)
        {
            GamePlayer player = GetPlayer(actor);
            TowerBase tower = player.Towers[towerPosIndex];
            if (tower != null)
                tower.ReceiveShooting(unitID, damage, damageType, isDebuff);
        }
        
        public Vector3 GetNext(int index)
        {
            bool isMine = GameManager.Instance.myPlayer.actorNumber == actorNumber;
            Vector3 next = GameManager.Instance.GetRoadPos(index);
            if (!isMine)
                next.y += GameManager.Instance.offset;
            return next;
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
        
        public void SkipReward(int reward)
        {
            if (!isLocalPlayer)
                return;
            
            gold += reward;
        }

        public void MonsterDestroy(int unitID)
        {
            photonView.RPC("MonsterDestroyRpc", RpcTarget.Others, actorNumber, unitID);
        }
        
        [PunRPC]
        public void MonsterDestroyRpc(int actor, int unitID)
        {
            GamePlayer player = GetPlayer(actor);
            if (player.monsterDic.ContainsKey(unitID))
                player.monsterDic[unitID].DestroyMonster();
            else Debug.Log("Not Found Monster!! "+ actor);
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
            stageText.text = "Stage " + stageProcess.ToString();
        }
        
        public void UpdatePlayerHp()
        {
            // 체력 업데이트
            for (int i = 0; i < 5; ++i)
            {
                HpTrf.GetChild(i).SetActive(false);
                if (i < playerHp)
                    HpTrf.GetChild(i).SetActive(true);
            }
        }
        
        #region Skill

        private void SkillCreator(int slotIndex, int skillIndex)
        {
            BaseSkill skill = null;
            
            switch ((SkillIndex)skillIndex)
            {
                case SkillIndex.WaterSlice:  skill = gameObject.AddComponent<WaterSliceSkill>(); break;
                case SkillIndex.ChangePlace: skill = gameObject.AddComponent<ChangePlaceSkill>(); break;
                case SkillIndex.GoodSell: skill = gameObject.AddComponent<GoodSellSKill>(); break;
                default: JustDebug.LogError("Skill Index Not Found"); break;
            }

            SkillButton button = GameUIManager.Instance.GetSkillButton(slotIndex);
            skill.Init(skillIndex, this, button);
            skillArr[slotIndex] = skill;
        }
        
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
        
        public void ShowSkillDim()
        {
            skillDim.DOFade(0.6f, 0.2f).SetUpdate(true);
        }
        
        public void HideSkillDim()
        {
            skillDim.DOFade(0f, 0.2f).SetUpdate(true);
        }
        
        public void SkillUse(int skillBtnIdex)
        { 
            photonView.RPC("SkillUseRpc", RpcTarget.Others, actorNumber, skillBtnIdex);
        }
        
        [PunRPC]
        public void SkillUseRpc(int actor, int skillBtnIdex)
        {
            GamePlayer player = GetPlayer(actor);
            
            GameUIManager.Instance.ShowSkillDim(false);
            
            if (player.skillArr[skillBtnIdex].CanUseSkill())
            {
                player.skillArr[skillBtnIdex].SkillStart();
            }
        }

        public void SkillEnd(int skillIndex, params object[] values)
        {
            Debug.Log("SkillEnd : " + skillIndex + ", "+ values);
            
            if (GameManager.Instance.gameType == GameType.Solo)
                return;

            switch ((SkillIndex)skillIndex)
            {
                case SkillIndex.WaterSlice: photonView.RPC("SkillEndRpc", RpcTarget.Others, actorNumber); break;
                case SkillIndex.ChangePlace: photonView.RPC("SkillSwapTowerRpc", RpcTarget.Others, actorNumber, (int)values[0], (int)values[1]);break;
                case SkillIndex.GoodSell: photonView.RPC("SkillSellTowerRpc", RpcTarget.Others, actorNumber, (int)values[0], (int)values[1]);break;
            }
        }
        
        [PunRPC]
        public void SkillEndRpc(int playerActorNumber)
        {
            Debug.Log("SkillEndRpc");
            GamePlayer player = GetPlayer(playerActorNumber);
            
            player.HideSkillDim();
        }
        
        [PunRPC]
        public void SkillSwapTowerRpc(int playerActorNumber, int towerIndexA, int towerIndexB)
        {
            Debug.Log("SkillSwapTowerRpc : "+towerIndexA + ", "+ towerIndexB);
            GamePlayer player = GetPlayer(playerActorNumber);
            
            player.HideSkillDim();
            

            if (player == null)
            {
                Debug.LogError("Player not found");
                return;
            }

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
        public void SkillSellTowerRpc(int playerActorNumber, int towerPosIndex, int getGold)
        {
            Debug.Log("SkillSellTowerRpc");
            GamePlayer player = GetPlayer(playerActorNumber);
            
            player.HideSkillDim();

            Vector3 startPos = TowerFixPos[towerPosIndex].transform.position;
            
            GameUIManager.Instance.ShowGoldText(startPos, getGold);
        }
        
        
        #endregion

        public void AddBullet(BulletBase bullet)
        {
            bulletList.Add(bullet);
        }

        public void RemoveBullet(BulletBase bullet)
        {
            bulletList.Remove(bullet);
        }

        public int GetTowerID(TowerBase tower)
        {
            for (int i = 0; i < Towers.Length; ++i)
            {
                if (tower == Towers[i])
                {
                    return i;
                }
            }

            Debug.Log("Not Found TowerID : "+ tower.name);
            return -1;
        }
    }
}