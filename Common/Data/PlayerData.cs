using RandomFortress.Menu;
using UnityEngine;
using UnityEngine.Serialization;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerData", order = 1)]
    public class PlayerData : ScriptableObject
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
        
        // [Header("게임 셋업")]
        // public int mainSkill = 1001;
        // public int subSkill_1 = 2001;
        // public int subSkill_2 = 2002;
        
        [Header("플레이어 덱")]
        public int[] towerDeck = { 0, 0, 0, 0, 0 }; // 총5개. 첫자리가 메인타워
        public int[] skillDeck = { 0, 0, 0 }; // 총3개. 첫자리가 메인스킬. 2,3 서브스킬

        [Header("인벤토리")] 
        public ItemSlot[] myItem;
        
        [Header("계정인증")]
        public string androidAppid = "";
        public string iOSAppid= "";
        public string googleAppid= "";
        public string facebookAppid= "";
    }
}