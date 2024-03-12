namespace RandomFortress.Common.Utils
{
    public static class MathUtil
    {
        // 확률의 덧셈을 계산하는 함수
        public static double CalculateCombinedProbability(double probA, double probB)
        {
            // 두 확률이 독립적이라고 가정
            double combinedProb = 1 - (1 - probA) * (1 - probB);
            return combinedProb;
        }

        // 기대 데미지를 역산하여 새로운 데미지 배율을 계산하는 함수
        public static double CalculateExpectedDamageMultiplier(double probA, double multiplierA, double probB, double multiplierB)
        {
            // 각 버프가 적용되지 않을 확률
            double nonCritProb = (1 - probA) * (1 - probB);

            // 기대 데미지 계산
            double expectedDamage = nonCritProb * 1 + probA * (1 - probB) * multiplierA + probB * (1 - probA) * multiplierB + probA * probB * multiplierA * multiplierB;

            // 기대 데미지를 기반으로 새로운 데미지 배율을 역산
            double newMultiplier = expectedDamage / (1 - CalculateCombinedProbability(probA, probB)) - 1;

            return newMultiplier;
        }
    }
}