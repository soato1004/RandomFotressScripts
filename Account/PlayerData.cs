using System;
using System.Collections.Generic;
using RandomFortress.Menu;
using UnityEngine;

namespace RandomFortress
{
    [Serializable]
    public class PlayerData
    {
        [Header("기본 정보")]
        public string id = "zero";
        public string nickname = "masterLee";

        [Header("재화")]
        public int gold = 0;
        public int gem = 0;

        [Header("멀티모드 데이터")]
        public int winCount = 0;
        public int loseCount = 0;
        public int eloRating = 1000;
        public int trophy = 0;

        [Header("솔로모드 데이터")]
        public int soloRank = 0;
        public GameResult bestGameResult = null;
        public List<GameResult> gameResultList = new List<GameResult>();

        [Header("튜토리얼 상태")]
        public bool isFirstPlay = true;
        public bool isTutorialLobby = true;
        public bool isTutorialGame = true;

        [Header("플레이어 덱")]
        public int[] towerDeck = new int[]
        {
            (int)TowerIndex.Elephant,
            (int)TowerIndex.Drumble,
            (int)TowerIndex.Sting,
            (int)TowerIndex.Flame,
            (int)TowerIndex.Machinegun,
            (int)TowerIndex.Shino,
            (int)TowerIndex.MaskMan,
            (int)TowerIndex.Swag
        };

        public int[] skillDeck = new int[]
        {
            (int)SkillIndex.WaterSlice,
            (int)SkillIndex.ChangePlace,
            (int)SkillIndex.GoodSell
        };

        [Header("인벤토리")]
        public ItemSlot[] myItem;

        [Header("계정 인증 정보")]
        public string androidAppid = "";
        public string iOSAppid = "";
        public string googleAppid = "";
        public string facebookAppid = "";

        [Header("광고 디버프")]
        public List<AdDebuffState> adDebuffList = new List<AdDebuffState>();

        [Header("계정 상태")]
        public bool isFirstAccountCreation = false;
        
        // [Header("ETC")] 
        
        // TODO: 최초 시작시에 기본으로 주는카드 설정.
        // public void Init()
        // {
        //     towerCardDic.Clear();
        //     for (int i = 0; i < 8; i++)
        //     {
        //         TowerCard towerCard = new TowerCard
        //         {
        //             Index = i + 1,
        //             CardLV = 1,
        //             CardCount = 1
        //         };
        //         towerCardDic.Add(towerCard.Index.ToString(),towerCard);
        //     }
        // }
        
        // [Header("보유 아이템")] 
        // public SerializedDictionary<string, TowerCard> towerCardDic = new SerializedDictionary<string, TowerCard>(); // 현재 보유한 타워정보
    }
}