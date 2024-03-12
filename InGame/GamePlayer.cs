using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using RandomFortress.Common.Extensions;
using RandomFortress.Common.Utils;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RandomFortress.Game
{
    public class GamePlayer : MonoBehaviour, IPunObservable
    {
        [Header("포톤")]
        public PhotonView photonView;
        // public Player roomPlayer;
        public bool isLocalPlayer = false;
        public int actorNumber;

        [Header("플레이어 정보")]
        public int gold;
        public int playerHp;
        public ExtraInfo extraInfo { get; private set; } // 어빌리티로 증가한 모든 능력치를 이곳에 적용시킴
        public BaseSkill[] skillArr;
        public List<RewardData> rewardDatas = new List<RewardData>();
        public List<ExtraInfo> abilityList = new List<ExtraInfo>(); // 선택한 어빌리티 인덱스를 가지고있음
        
        [Header("몬스터")]
        public List<MonsterBase> monsterList;

        [Header("타워")]
        public Transform towerParent;
        public int towerCount = 0;
        public TowerBase[] Towers; // 타워정보
        public Transform[] TowerFixPos; // 타워 설치된 장소
        public int[] TowerDeck; // 현재 선택된 8개의 타워값
        // public Dictionary<int, TowerInfo> towerInfoDic = new Dictionary<int, TowerInfo>(); // 타워 덱. 업그레이드시 Info 교체
        public Dictionary<int, TowerResultData> totalDmgDic = new Dictionary<int, TowerResultData>(); // 타워별 데미지 누적

        [Header("버프리스트")] 
        private bool _isAbilityBuff = false;
        // private bool isAbilityBuff = false;
        // private bool isAbilityBuff = false;

        public bool IsAbilityBuff => _isAbilityBuff;
        
        [Header("생성된 오브젝트 리스트")] 
        public List<EntityBase> bulletList = new List<EntityBase>();

        [Header("기타")] 
        public GameMap gameMap;
        public int stageProcess = 1;
        
        private int total_tower_pos = 0;
        private bool _isBuildProgress;
        

        public void Awake()
        {
            monsterList = new List<MonsterBase>();
            towerParent = transform.Find("Towers");
            photonView = GetComponent<PhotonView>();
            extraInfo = new ExtraInfo();
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

        private void Start()
        {
            GameManager.Instance.onStageClear.RemoveListener(OnStageClear);
            GameManager.Instance.onStageClear.AddListener(OnStageClear);
        }

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                // stream.SendNext(PlayerHp);
                // stream.SendNext(GameMoney);
            }
            else
            {
                // Network player, receive data
                // this.PlayerHp = (int)stream.ReceiveNext();
                // this.GameMoney = (int)stream.ReceiveNext();
            }
        }

        #endregion
        
        // 솔로모드
        public void SetPlayer()
        {
            // roomPlayer = p;
            actorNumber = 1;
            
            // 자기자신인지 체크
            isLocalPlayer = true;
            
            // 게임모드에 맞는 맵 설정하기
            // roadTilemap = Map.GetChild(1).GetChild(1).GetComponent<Tilemap>();
            
            InitPlayer();
        }
        
        // 멀티
        public void SetPlayer(Player p)
        {
            // roomPlayer = p;
            actorNumber = p.ActorNumber;
            
            // 자기자신인지 체크
            isLocalPlayer = p.IsLocal;
            
            // 게임모드에 맞는 맵 설정하기
            // roadTilemap = Map.GetChild(1).GetChild(1).GetComponent<Tilemap>();// PlayerGrid.RoadTilemap

            InitPlayer();
        }

        void InitPlayer()
        {
            // 타워 인포 설정
            // for (int slotIndex = 0; slotIndex < GameConstants.TOWER_DECK_COUNT; ++slotIndex)
            // {
            //     int towerIndex = Account.Instance.TowerDeck(slotIndex);
            //     TowerData data = DataManager.Instance.GetTowerData(towerIndex);
            //
            //     TowerInfo info = new TowerInfo(data);
            //     towerInfoDic.Add(towerIndex, info);
            // }
            
            
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

            if (MainManager.Instance.gameType != GameType.Solo)
                photonView.RPC("PlayerHpSync", RpcTarget.Others, playerHp);
            
            GameUIManager.Instance.UpdateInfo();
            
            if (playerHp <= 0)
                GameManager.Instance.GameOver();
        }

        [PunRPC]
        public void PlayerHpSync(int hp)
        {
            GamePlayer other = GameManager.Instance.otherPlayer;
            other.playerHp = hp;
            GameUIManager.Instance.UpdateInfo();
        }
        
        public void OnStageClear()
        {
            for (int i = 0; i < Towers.Length; ++i)
            {
                TowerBase tower = Towers[i];
                if (tower != null)
                {
                    tower.OnStageClear();
                }
            }
        }
        
        public void BuildRandomTower()
        {
            if (towerCount == total_tower_pos)
            {
                // Debug.Log("최대타워 겟수도달");
                return;
            }

            if (GameConstants.TowerCost > gold)
            {
                // Debug.Log("돈부족");
                return;   
            }

            if (_isBuildProgress)
                return;

            _isBuildProgress = true;
            gold -= GameConstants.TowerCost;
            GameUIManager.Instance.SetLockButton(Buttons.Build, false);
            
            // 타워 위치 랜덤선택
            int posIndex;
            float duration = 0f;
            do
            {
                posIndex = Random.Range(0, total_tower_pos);
                duration += Time.deltaTime;
                if (duration > 2f)
                {
                    Debug.Log("Tower Create 무한루프!!");
                    return;
                }
            } while (Towers[posIndex] != null);

            // 타워 랜덤 선택
            int rand = Random.Range(0, TowerDeck.Length-1);
            int towerIndex = TowerDeck[rand];
            
            // TODO: 타워생성 테스트코드
            // towerIndex = (int)TowerIndex.Machinegun;
            // towerIndex = temp++;//(int)TowerIndex.MaskMan;
            // temp = temp > 8 ? 1 : temp;
            
            // 타워생성
            if (MainManager.Instance.gameType == GameType.Solo)
                CreateTower(actorNumber, posIndex, towerIndex);
            else
                photonView.RPC("CreateTower", RpcTarget.AllViaServer, 
                actorNumber, posIndex, towerIndex);
        }
        
        [PunRPC]
        public void CreateTower(int actorNumber, int posIndex, int towerIndex)
        {
            JTDebug.Log("Tower Create " + towerIndex + "," + posIndex);
            
            SoundManager.Instance.PlayOneShot("tower_create");
            
            bool isMine = this.actorNumber == actorNumber;

            // 어느 진영인지 찾기
            GamePlayer player = isMine ? this : GameManager.Instance.otherPlayer;
            if (isMine)
                player._isBuildProgress = false;
            
            // 생성되는 타워자리 처리
            Transform trf = player.TowerFixPos[posIndex];
            // trf.SetActive(false);
            
            // 로컬로 타워생성
            GameObject towerGo = SpawnManager.Instance.GetTower(trf.position, towerIndex);

            TowerBase tower = null;
            switch ((TowerIndex)towerIndex)
            {
                case TowerIndex.Drumble: tower = towerGo.GetComponent<DrumbleTower>(); break;
                case TowerIndex.Machinegun: tower = towerGo.GetComponent<MachinegunTower>(); break;
                case TowerIndex.MaskMan: tower = towerGo.GetComponent<MaskManTower>(); break;
                case TowerIndex.Swag: tower = towerGo.GetComponent<SwagTower>(); break;
                default: tower = towerGo.GetComponent<TowerBase>(); break;
            }
            
            tower.Init(player, posIndex, towerIndex);
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
            
            if (MainManager.Instance.gameType == GameType.Solo)
                TowerDestroy(actorNumber, focusTower.TowerPosIndex);
            else
                photonView.RPC("TowerDestroy", RpcTarget.AllViaServer, actorNumber, focusTower.TowerPosIndex);
        }
        
        public void TowerDestroy(int towerID)
        {
            if (MainManager.Instance.gameType == GameType.Solo)
                TowerDestroy(actorNumber, towerID);
            else
                photonView.RPC("TowerDestroy", RpcTarget.AllViaServer, actorNumber, towerID);
        }
        
        [PunRPC]
        public void TowerDestroy(int actorNumber, int towerID)
        {
            SoundManager.Instance.PlayOneShot("tower_sell");
            
            GamePlayer player = this.actorNumber == actorNumber ? this : GameManager.Instance.otherPlayer;
            
            TowerBase tower = player.Towers[towerID];
            player.Towers[tower.TowerPosIndex] = null;
            player.towerCount -= 1;
            tower.TowerDestroy();

            GameManager.Instance.HideForcusTower();
            GameUIManager.Instance.UpdateInfo();
        }

        public void UpgradeTower(int towerID)
        {
            if (MainManager.Instance.gameType == GameType.Solo)
                UpgradeTowerRpc(actorNumber, towerID);
            else
                photonView.RPC("UpgradeTowerRpc", RpcTarget.AllViaServer, actorNumber, towerID);
        }
        
        [PunRPC]
        public void UpgradeTowerRpc(int actorNumber, int towerID)
        {
            GamePlayer player = this.actorNumber == actorNumber ? this : GameManager.Instance.otherPlayer;
            
            TowerBase tower = player.Towers[towerID];
            tower.UpgradeTower();
            GameManager.Instance.HideForcusTower();
            GameUIManager.Instance.UpdateInfo();
        }
        
        public Vector3 GetNext(int index)
        {
            return  GameManager.Instance.GetRoadPos(index);
        }
        
        public void SkipReward(int reward)
        {
            if (!isLocalPlayer)
                return;
            
            gold += reward;
        }
        
        public void SelectAbilityCard(AbilityData abilityData)
        {
            abilityList.Add(abilityData.extraInfo);
            extraInfo.AddExtraInfo(abilityData.extraInfo);
        }
        
        #region Skill

        private void SkillCreator(int slotIndex, int skillIndex)
        {
            BaseSkill skill = null;
            SkillData data =  DataManager.Instance.skillDataDic[skillIndex];
            
            switch ((Skill)skillIndex)
            {
                case Skill.WaterSlice:  skill = gameObject.AddComponent<WaterSliceSkill>(); break;
                case Skill.ChangePlace: skill = gameObject.AddComponent<ChangePlaceSkill>(); break;
                case Skill.GoodSell: skill = gameObject.AddComponent<GoodSellSKill>(); break;
                default:
                    JTDebug.LogError("Skill Index Not Found");
                    break;
            }
            
            skill.Init(skillIndex, this, GameUIManager.Instance.GetSkillButton(slotIndex));
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

        #endregion

        public void AddBullet(BulletBase bullet)
        {
            bulletList.Add(bullet);
        }

        public void RemoveBullet(BulletBase bullet)
        {
            bulletList.Remove(bullet);
        }
    }
}