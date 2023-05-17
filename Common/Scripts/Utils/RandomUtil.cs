namespace RandomFortress.Common.Util
{
    public static class RandomUtil
    {
        // 배열중 랜덤으로 선택하여 반환
        public static int ChooseWithProbabilities(params float[] probs)
        {
            var total = 0f;
            foreach (var prob in probs)
            {
                total += prob;
            }
            
            var randomPoint = UnityEngine.Random.value * total; // 0 ~ total 값.

            for (var i = 0; i < probs.Length; i++)
            {
                if (randomPoint < probs[i])
                    return i;
                randomPoint -= probs[i];
            }

            return probs.Length - 1;
        }
    }
}