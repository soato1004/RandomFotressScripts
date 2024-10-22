using System.Collections.Generic;

namespace RandomFortress
{
    [System.Serializable]
    public class GameResult
    {
        public GameType gameType;
        public bool isWin; // 승패여부
        public int maxClearStage; // 최종 클리어 스테이지
        public int clearTime; // 클리어 시간
        public string roomName; // 게임을 진행항 방의 이름. 고유값으로 사용
        public GameRank rank; // 솔로모드 랭크. 서버에서 받는다
        public int[] towerList; // 자신의 타워리스트

        public string otherUserid; // 상대방의 유저아이디
        public int[] otherTowerList; // 상대방의 타워리스트
        
        public GameResult() {}
        
        public GameResult(bool isWin, int maxClearStage, int clearTime)
        {
            this.isWin = isWin;
            this.maxClearStage = maxClearStage;
            this.clearTime = clearTime;
        }
        
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { "gameType", (int)gameType },
                { "isWin", isWin },
                { "maxClearStage", maxClearStage },
                { "clearTime", clearTime },
                { "roomName", roomName},
                { "towerList", towerList },
                { "otherUserId", otherUserid },
                { "otherTowerList", otherTowerList }
            };
        }
    }
}