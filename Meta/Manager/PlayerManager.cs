using Meta.Server;
using RandomFortress.Common;
using RandomFortress.Common.Util;
using RandomFortress.Data;
using UnityEngine;

namespace Meta.Manager
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        [Header("기본")]
        public string id = "zero";
        public string nickname = "masterLee";

        [Header("재화")] 
        public int gold = 0;
        public int gem = 0;

        [Header("게임플레이 데이터")] 
        public int eloRating = 1000;
        public int trophy = 0;
        
        [Header("게임 셋업")]
        public int mainSkill = 0;
        public int subSkill_1 = 0;
        public int subSkill_2 = 0;


        [Header("인벤토리")] 
        // public item[] myItem;
        
        [Header("계정인증")]
        public string androidAppid = "";
        public string iOSAppid= "";
        public string googleAppid= "";
        public string facebookAppid= "";

        private PlayerData playerData;
        
        public override void Reset()
        {
            JTDebug.LogColor("PlayerManager Reset");
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("PlayerManager Terminate");
        }

        public void SetData(PlayerData data)
        {
            playerData = data;
            id = data.id;
            nickname = data.nickname;
            gold = data.gold;
            gem = data.gem;
            eloRating = data.eloRating;
            trophy = data.trophy;
            mainSkill = data.mainSkill;
            subSkill_1 = data.subSkill_1;
            subSkill_2 = data.subSkill_2;
            // public item[] myItem;
            androidAppid = "";
            iOSAppid = "";
            googleAppid = "";
            facebookAppid = "";
        }

        public void GameResultProcess(bool isWin)
        {
            int opponentElo = 0;
            EloSystem.UpdateRatings(ref eloRating, ref eloRating, true);
        }
    }
}