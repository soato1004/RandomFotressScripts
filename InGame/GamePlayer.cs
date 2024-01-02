using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using RandomFortress.Common.Extensions;
using RandomFortress.Common.Utils;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using UnityEngine;
using UnityEngine.Tilemaps;
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

        #region Public

        [Header("플레이어 정보")]
        public BaseSkill[] skillArr;
        public int gold;
        public int playerHp;
        public List<RewardData> rewardDatas = new List<RewardData>();
        public Transform Map; // 게임모드, 선택된 맵
        
        [Header("몬스터")]
        public List<MonsterBase> monsterList;

        [Header("타워")]
        public Transform towerParent;
        public int towerCount = 0;
        public TowerBase[] Towers;
        public Transform[] TowerFixPos;
        // public Tilemap towerSearPos;
        public Dictionary<int, TowerInfo> towerInfoDic = new Dictionary<int, TowerInfo>(); // 타워 덱. 업그레이드시 Info 교체
        
        [Header("생성된 오브젝트 리스트")] 
        public List<PlayBase> bulletList = new List<PlayBase>();

        [Header("기타")] 
        public Tilemap roadTilemap;
        public int stageProcess = 1;
        
        public int OtherDamage { get; private set; } // 타워 판매시에 그 타워의 데미지 누적합산
        
        #endregion
        
        #region Private
        

        [SerializeField] private const int TOTAL_TOWERS = 9;
        [SerializeField] private const int SKILL_SLOT = 3;
        
       private int total_tower_pos = 0;
        
        
        private bool _isBuildProgress;

        #endregion

        public void Awake()
        {
            monsterList = new List<MonsterBase>();
            towerParent = transform.Find("Towers");
            photonView = GetComponent<PhotonView>();
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

        /// <summary>
        /// 게임 시작시 한번 설정
        /// </summary>
        public void SetPlayer(Player p)
        {
            // roomPlayer = p;
            actorNumber = p.ActorNumber;
            
            // 자기자신인지 체크
            isLocalPlayer = p.IsLocal;
            
            // 게임모드에 맞는 맵 설정하기
            roadTilemap = Map.GetChild(1).GetChild(1).GetComponent<Tilemap>();// PlayerGrid.RoadTilemap
            
            // 타워 인포 설정
            for (int slotIndex = 0; slotIndex < GameConstants.TOWER_DECK_COUNT; ++slotIndex)
            {
                int towerIndex = MainManager.Instance.account.TowerDeck(slotIndex);
                TowerData data = DataManager.Instance.TowerDataDic[towerIndex];

                TowerInfo info = new TowerInfo(data);
                towerInfoDic.Add(towerIndex, info);
            }
            
            
            // 타워설치할 공간
            Transform seatPos = Map.GetChild(1).GetChild(3);
            total_tower_pos = seatPos.childCount;
            
            Towers = new TowerBase[total_tower_pos];
            TowerFixPos = seatPos.GetChildren();
            
            
            // 게임 데이터 초기화
            gold = 1000;
            playerHp = 5;
            
            // 스킬 세팅
            skillArr = new BaseSkill[SKILL_SLOT];
            
            int skillIndex = DataManager.Instance.playerData.skillDeck[0];
            SkillCreator(0, skillIndex);
            
            skillIndex = DataManager.Instance.playerData.skillDeck[1];
            SkillCreator(1, skillIndex);
            
            skillIndex = DataManager.Instance.playerData.skillDeck[2];
            SkillCreator(2, skillIndex);
            
            GameUIManager.Instance.UpdateInfo();
        }
        
        public void KillMonster(int reward)
        {
            if (!isLocalPlayer)
                return;
                
            gold += reward;
        }

        public void DamageToPlayer(int value = 1)
        {
            if (!isLocalPlayer)
                return;
            
            playerHp -= value;
            photonView.RPC("PlayerHpSync", RpcTarget.Others, playerHp);
            GameUIManager.Instance.UpdateInfo();
            if (playerHp == 0)
                GameManager.Instance.GameOver();
        }

        [PunRPC]
        public void PlayerHpSync(int hp)
        {
            GamePlayer player = GameManager.Instance.otherPlayer;
            player.playerHp = hp;
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
                Debug.Log("최대타워 겟수도달");
                return;
            }

            if (GameConstants.TowerCost > gold)
            {
                Debug.Log("돈부족");
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
            int towerIndex = Random.Range(1, 10);
            
            // 타워생성
            photonView.RPC("CreateTower", RpcTarget.AllViaServer, 
                actorNumber, posIndex, towerIndex);
        }

        /// <summary>
        /// 타워의 위치는 동기화되지않는다. 다만 정보만 동기화한다
        /// </summary>
        /// <param name="posIndex"></param>
        /// <param name="towerIndex"></param>
        /// <param name="actorNumber"></param>
        [PunRPC]
        public void CreateTower(int actorNumber, int posIndex, int towerIndex)
        {
            JTDebug.Log("Tower Create " + towerIndex + "," + posIndex);
            
            AudioManager.Instance.PlayOneShot("DM-CGS-22");
            
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

        /// <summary>
        /// 타워 판매
        /// </summary>
        /// <param name="useSkill"> 스킬 사용시 100% 판매금액 </param>
        public void SellTower(bool useSkill = false)
        {
            TowerBase focusTower = GameManager.Instance.focusTower;
            OtherDamage += focusTower.TotalDamege;
            gold += useSkill ? focusTower.Price :focusTower.SellPrice;
            photonView.RPC("TowerDestroy", RpcTarget.AllViaServer, actorNumber, focusTower.TowerPosIndex);
        }


        public void TowerDestroy(int towerID)
        {
            photonView.RPC("TowerDestroy", RpcTarget.AllViaServer, actorNumber, towerID);
        }
        
        /// <summary>
        /// 타워를 파괴하고 그자리를 초기화시킨다
        /// </summary>
        /// <param name="towerID"></param>
        [PunRPC]
        public void TowerDestroy(int actorNumber, int towerID)
        {
            GamePlayer player = this.actorNumber == actorNumber ? this : GameManager.Instance.otherPlayer;
            
            TowerBase tower = player.Towers[towerID];
            player.Towers[tower.TowerPosIndex] = null;
            // player.TowerFixPos[tower.TowerPosIndex].gameObject.SetActive(true);
            player.towerCount -= 1;
            tower.TowerDestroy();
            
            GameManager.Instance.isFocus = false;
            GameManager.Instance.focusTower = null;
            GameUIManager.Instance.UpdateInfo();
        }

        public void UpgradeTower(int towerID)
        {
            photonView.RPC("UpgradeTowerRpc", RpcTarget.AllViaServer, actorNumber, towerID);
        }
        
        [PunRPC]
        public void UpgradeTowerRpc(int actorNumber, int towerID)
        {
            GamePlayer player = this.actorNumber == actorNumber ? this : GameManager.Instance.otherPlayer;
            
            TowerBase tower = player.Towers[towerID];
            tower.UpgradeTower();
            GameManager.Instance.focusTower = null;
            GameUIManager.Instance.UpdateInfo();
        }
        
        public Vector3 GetNext(int index)
        {
            return roadTilemap.GetCellCenterWorld(GameManager.Instance.GetWayPoint(index));
        }
        
        public void SkipReward(int reward)
        {
            if (!isLocalPlayer)
                return;
            
            gold += reward;
        }
        
        #region Skill

        private void SkillCreator(int slotIndex, int skillIndex)
        {
            // GameObject go = new GameObject();
            // go.transform.parent = GameManager.Instance.game.skillParent;

            // SkillData data;
            
            switch ((Skill)skillIndex)
            {
                case Skill.WATER_SLICE:
                    SkillData data =  DataManager.Instance.SkillDataDic[skillIndex];
                    WaterSliceSkill skill_water = gameObject.AddComponent<WaterSliceSkill>();
                    skillArr[slotIndex] = skill_water;
                    break;
                
                case Skill.CHANGE_SLICE:
                    ChangePlaceSkill skill_change = gameObject.AddComponent<ChangePlaceSkill>();
                    skillArr[slotIndex] = skill_change;
                    break;
                
                case Skill.GOOD_SELL:
                    GoodSellSKill skill_good = gameObject.AddComponent<GoodSellSKill>();
                    skillArr[slotIndex] = skill_good;
                    break;

                default:
                    JTDebug.LogError("Skill Index Not Found");
                    break;
            }
        }
        
        public bool CheckUserAvailableSkill(TowerBase tower, BaseSkill.SkillAction skillAction)
        {
            foreach (BaseSkill skill in skillArr)
            {
                // 해당 상황에 맞는 스킬이 있는지
                foreach (BaseSkill.SkillAction action in skill._skillActions)
                {
                    if (skillAction == action && skill.CanUseSkill())
                    {
                        skill.UseSkill(tower);
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