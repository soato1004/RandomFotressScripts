using System;

namespace Meta.Server
{
    public class EloSystem
    {
        private static int InitialRating = 1200; // 초기 Elo 등급
        private static int KFactor = 32; // K-팩터 (시스템의 민감도 조절)

        // 두 플레이어의 경기 결과에 따라 Elo 등급을 업데이트하는 메서드
        public static void UpdateRatings(ref int myElo, ref int opponentElo, bool isWin)
        {
            double expectedScorePlayer1 = 1 / (1 + Math.Pow(10, (opponentElo - myElo) / 400.0));
            double expectedScorePlayer2 = 1 / (1 + Math.Pow(10, (myElo - opponentElo) / 400.0));

            int player1Result = isWin ? 1 : 0;
            int player2Result = isWin ? 0 : 1;

            int player1NewRating = (int)(myElo + KFactor * (player1Result - expectedScorePlayer1));
            int player2NewRating = (int)(opponentElo + KFactor * (player2Result - expectedScorePlayer2));

            myElo = player1NewRating;
            opponentElo = player2NewRating;
        }
    }
}