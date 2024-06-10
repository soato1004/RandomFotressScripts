namespace RandomFortress
{
    public class GameResult
    {
        public bool isWin;
        public int maxClearStage; // 최종 클리어 스테이지
        public int clearTime; // 클리어 시간
        public GameRank rank;
        
        public GameResult() {}
        
        public GameResult(bool isWin, int maxClearStage, int clearTime)
        {
            this.isWin = isWin;
            this.maxClearStage = maxClearStage;
            this.clearTime = clearTime;
            GetRank();
        }

        private void GetRank()
        {
            // 랭크를 정함
            if (maxClearStage < 10)
            {
                rank = GameRank.Beginner;
            }
            else if (maxClearStage < 20)
            {
                rank = GameRank.Bronze;
            }
            else if (maxClearStage < 30)
            {
                rank = GameRank.Silver;
            }
            else if (maxClearStage < 40)
            {
                rank = GameRank.Gold;
            }
            else if (maxClearStage < 50)
            {
                rank = GameRank.Platinum;
            }
            else
            {
                if (clearTime > 1000) // 스테이지당 20초 초과
                {
                    rank = GameRank.Diamond;
                }
                else if (clearTime > 750) // 스테이지당 20초
                {
                    rank = GameRank.Master;
                }
                else if (clearTime > 500) // 스테이지당 15초
                {
                    rank = GameRank.GrandMaster;
                }
                else // 스테이지당 10초 이하
                {
                    rank = GameRank.Challenger;
                }
            }
        }
    }
}