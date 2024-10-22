using System;
using System.Collections.Generic;
using UnityEngine;

namespace RandomFortress
{
    [Serializable]
    public class PlayerData
    {
        [Header("기본 정보")] 
        public string id;
        public string nickname;
        public string lastActivityTime;

        [Header("인앱결제 정보")] 
        public List<object> purchases;
        public bool hasSuperPass = false;
        public string superPassExpiration;
        // public bool hasRemoveAds = false;

        [Header("재화")] 
        public int gold = 0;
        public int gem = 0;
        public int stamina = 5; // max stamina 5
        public readonly int STAMINA_MAX = 10; // 최대 스태미너량

        [Header("멀티모드 데이터")] 
        public int winCount = 0;
        public int loseCount = 0;
        public int eloRating = 1000;
        public int trophy = 0;

        [Header("랭크 데이터")] 
        public GameResult bestGameResult;
        public int soloRank = 0;
        public int pvpRank = 0;
        public List<GameResult> gameResultList = new ();

        [Header("튜토리얼 상태")] 
        public bool isFirstPlay = true;
        public bool isTutorialLobby = true;
        public bool isTutorialGame = true;

        [Header("플레이어 덱")] 
        public int[] towerDeck = {
            (int)TowerIndex.Elephant,
            (int)TowerIndex.Drumble,
            (int)TowerIndex.Sting,
            (int)TowerIndex.Flame,
            (int)TowerIndex.Machinegun,
            (int)TowerIndex.Shino,
            (int)TowerIndex.MaskMan,
            (int)TowerIndex.Swag
        };

        public int[] skillDeck = {
            (int)SkillIndex.WaterSlice,
            (int)SkillIndex.ChangePlace,
            (int)SkillIndex.GoodSell
        };

        [Header("인벤토리")] 
        public List<ItemSlot> myItem = new ();

        [Header("광고 디버프")] 
        public List<AdDebuff> adDebuffs = new ();
    }
}