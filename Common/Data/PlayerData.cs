using System;
using System.Collections.Generic;
using RandomFortress.Constants;
using RandomFortress.Game;
using RandomFortress.Menu;
using UnityEngine;

namespace RandomFortress.Data
{
    // [CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
    [System.Serializable]
    public class PlayerData : MonoBehaviour
    {
        [Header("기본")]
        public string id = "zero";
        public string nickname = "masterLee";

        [Header("재화")] 
        public int gold = 0;
        public int gem = 0;

        [Header("멀티모드 데이터")] 
        public int winCount; // 배틀모드시에 승리
        public int loseCount; // 배틀모드시 패배
        public int eloRating = 1000; // 대전모드 elo
        public int trophy = 0; // 대전모드 트로피
        
        [Header("솔로모드 데이터")] 
        public int soloRank; // 솔로모드 랭크
        public GameResult BestGameResult = null; // 최고기록
        public List<GameResult> gameResultList = new List<GameResult>();
        
        [Header("게임 데이터")] 
        public bool isFirstPlay = true; // 최초게임 시작시에 튜토리얼 재생을위하여
        public bool isTutorialLobby = true; // 최초 시작시에 튜토리얼 재생을 위함
        public bool isTutorialGame = true; // 최초 솔로모드 진입시
        
        [Header("플레이어 덱")]
        public int[] towerDeck = {         
            (int)TowerIndex.Elephant,   
            (int)TowerIndex.Drumble,   
            (int)TowerIndex.Sting,    
            (int)TowerIndex.Flame,  
            (int)TowerIndex.Machinegun,
            (int)TowerIndex.Shino,
            (int)TowerIndex.MaskMan,
            (int)TowerIndex.Swag,
        }; // 총8개. 첫자리가 메인타워
        public int[] skillDeck =
        {
            (int)Constants.Skill.WaterSlice,
            (int)Constants.Skill.ChangePlace,
            (int)Constants.Skill.GoodSell,
        }; // 총3개. 첫자리가 메인스킬. 2,3 서브스킬

        [Header("보유 아이템")] 
        public Dictionary<string, TowerCard> towerCardDic = new Dictionary<string, TowerCard>(); // 현재 보유한 타워정보
        
        [Header("인벤토리")] 
        public ItemSlot[] myItem;
        
        [Header("계정인증")]
        public string androidAppid = "";
        public string iOSAppid= "";
        public string googleAppid= "";
        public string facebookAppid= "";

        [Header("광고디버프")]
        public List<AdDebuffState> adDebuffList = new List<AdDebuffState>();
        
        // [Header("ETC")] 

        // TODO: 최초 시작시에 기본으로 주는카드 설정.
        public void Init()
        {
            towerCardDic.Clear();
            for (int i = 0; i < 8; i++)
            {
                TowerCard towerCard = new TowerCard
                {
                    Index = i + 1,
                    CardLV = 1,
                    CardCount = 1
                };
                towerCardDic.Add(towerCard.Index.ToString(),towerCard);
            }
        }
    }
}