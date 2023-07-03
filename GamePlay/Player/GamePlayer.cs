using System.Collections.Generic;
using GoogleMobileAds.Api;
using Photon.Pun;
using Photon.Realtime;
using RandomFortress.Common.Extensions;
using RandomFortress.Common.Util;
using RandomFortress.Data;
using RandomFortress.Manager;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;


namespace RandomFortress.Game
{
    public class GamePlayer : MonoBehaviour, IPunObservable
    {
        [Header("포톤")]
        public PhotonView photonView;
        public Player roomPlayer;
        [FormerlySerializedAs("ActorNumber")] public int actorNumber;

        #region Public

        [Header("플레이어 정보")]
        public BaseSkill[] skillArr;
        [FormerlySerializedAs("GameMoney")] public int gold;
        [FormerlySerializedAs("PlayerHp")] public int playerHp;
        public List<RewardData> rewardDatas = new List<RewardData>();
        
        [Header("몬스터")]
        public List<MonsterBase> monsterList;

        [Header("타워")] 
        public Transform towerParent;
        public int towerCount = 0;
        public TowerBase[] Towers = new TowerBase[15];
        public Transform[] TowerFixPos = new Transform[15];

        public int stageProcess = 1;

        public Tilemap tilemap { get; private set; }
        public int OtherDamage { get; private set; } // 타워 판매시에 그 타워의 데미지 누적합산
        
        #endregion
        
        #region Private
        
        private const int TOTAL_TOWERS = 15;
        private const int SKILL_SLOT = 3;
        private bool _isBuildProgress;
        
        // TODO: 이부분을 다른곳으로 옴기던 리펙토링해야함
        public Vector3Int[] wayPoints = new[]
        {
            new Vector3Int(-9, 0),
            new Vector3Int(8, 0),
            new Vector3Int(8, -4),
            new Vector3Int(-8, -4),
            new Vector3Int(-8, -8),
            new Vector3Int(8, -8),
            new Vector3Int(8, -12),
            new Vector3Int(-9, -12),
        };
        
        #endregion

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
        
        public void Awake()
        {
            photonView = GetComponent<PhotonView>();
            monsterList = new List<MonsterBase>();
            tilemap = GetComponentInChildren<Tilemap>();
            towerParent = transform.Find("Towers");
            TowerFixPos = transform.Find("TowerSeatPos").GetChildren();
        }

        /// <summary>
        /// 게임 시작시 한번 설정
        /// </summary>
        public void SetPlayer(Player p)
        {
            roomPlayer = p;
            actorNumber = p.ActorNumber;
            
            // 게임 데이터 초기화
            gold = 1000;
            playerHp = 5;
            
            // 스킬 세팅
            skillArr = new BaseSkill[SKILL_SLOT];
            
            int skillIndex = DataManager.Instance.playerData.mainSkill;
            SkillCreator(0, skillIndex);
            
            skillIndex = DataManager.Instance.playerData.subSkill_1;
            SkillCreator(1, skillIndex);
            
            skillIndex = DataManager.Instance.playerData.subSkill_2;
            SkillCreator(2, skillIndex);
            
            GameUIManager.Instance.UpdateInfo();
        }
        
        public void KillMonster(int reward)
        {
            gold += reward;
        }

        public void DamageToPlayer(int value = 1)
        {
            playerHp -= value;
            GameUIManager.Instance.UpdateInfo();
            if (playerHp == 0)
            {
                GameManager.Instance.GameOver();
            }
        }
        
        public bool CanBuildTower()
        {
            if (GameManager.Instance.TowerCost > gold)
                return false;
            
            return true;
        }
        
        public void BuildRandomTower()
        {
            if (towerCount == TOTAL_TOWERS)
            {
                Debug.Log("최대타워 겟수도달");
                return;
            }

            if (!CanBuildTower())
            {
                Debug.Log("돈부족");
                return;   
            }

            if (_isBuildProgress)
                return;

            _isBuildProgress = true;
            gold -= GameManager.Instance.TowerCost;
            GameUIManager.Instance.SetLockButton(Buttons.Build, false);

            // 타워 위치 랜덤선택
            int posIndex;
            do
            {
                posIndex = Random.Range(0, TOTAL_TOWERS-1);
            } while (Towers[posIndex] != null);

            // 타워 랜덤 선택
            int towerIndex = Random.Range(0, 8);
            
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
            AudioManager.Instance.PlayOneShot("DM-CGS-22");
            
            bool isMine = this.actorNumber == actorNumber;

            // 어느 진영인지 찾기
            GamePlayer player = isMine ? this : GameManager.Instance.otherPlayer;
            if (isMine)
                player._isBuildProgress = false;
            
            // 생성되는 타워자리 처리
            Transform trf = player.TowerFixPos[posIndex];
            trf.SetActive(false);
            
            // 로컬로 타워생성
            GameObject towerGo = ObjectPoolManager.Instance.GetTower(trf.position);
            
            TowerBase tower = towerGo.GetComponent<TowerBase>();
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
            gold += useSkill ? focusTower.Price :focusTower.SalePrice;
            focusTower.isDestroyed = true;
            GameManager.Instance.isFocus = false;
            GameManager.Instance.focusTower = null;
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
            player.TowerFixPos[tower.TowerPosIndex].gameObject.SetActive(true);
            player.towerCount -= 1;
            tower.TowerDestroy();
            
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
        }

        public Vector3 GetNext(int index)
        {
            return tilemap.GetCellCenterWorld(wayPoints[index]);
        }
        
        public void SkipReward(int reward)
        {
            gold += reward;
        }
        
        #region Skill

        private void SkillCreator(int slotIndex, int skillIndex)
        {
            GameObject go = new GameObject();
            go.transform.parent = GameManager.Instance.game.skillParent;

            SkillData data;
            
            switch ((Skill)skillIndex)
            {
                case Skill.WATER_SLICE:
                    data =  DataManager.Instance.SkillDataDic[skillIndex];
                    WaterSliceSkill skill_water = go.AddComponent<WaterSliceSkill>();
                    skillArr[slotIndex] = skill_water;
                    break;
                
                case Skill.CHANGE_SLICE:
                    ChangePlaceSkill skill_change = go.AddComponent<ChangePlaceSkill>();
                    skillArr[slotIndex] = skill_change;
                    break;
                
                case Skill.GOOD_SELL:
                    GoodSellSKill skill_good = go.AddComponent<GoodSellSKill>();
                    skillArr[slotIndex] = skill_good;
                    break;

                default:
                    Destroy(go);
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
    }
}