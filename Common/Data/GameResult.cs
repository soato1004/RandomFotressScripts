using RandomFortress.Constants;

namespace RandomFortress.Data
{
    public class GameResult
    {
        public bool isWin;
        public int maxClearStage; // 최종 클리어 스테이지
        public int clearTime; // 클리어 시간
        public GameRank rank;
        
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
                if (clearTime > 800)
                {
                    rank = GameRank.Diamond;
                }
                else if (clearTime > 500)
                {
                    rank = GameRank.Master;
                }
                else if (clearTime > 300)
                {
                    rank = GameRank.GrandMaster;
                }
                else
                {
                    rank = GameRank.Challenger;
                }
            }
        }
    }
}