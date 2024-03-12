namespace RandomFortress.Common.Utils
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
        
        public static int GetRandomIndexWithWeight(float[] weights)
        {
            float totalWeight = 0;
        
            // 모든 가중치의 합을 계산합니다.
            foreach (float weight in weights)
            {
                totalWeight += weight;
            }

            // 랜덤한 값(0과 가중치 합 사이)을 선택합니다.
            float randomPoint = UnityEngine.Random.value * totalWeight;

            // 선택된 랜덤 값이 어떤 가중치 범위에 속하는지 찾아 인덱스를 반환합니다.
            for (int i = 0; i < weights.Length; i++)
            {
                if (randomPoint < weights[i])
                {
                    return i;
                }
                randomPoint -= weights[i];
            }

            return weights.Length - 1; // 만약 모든 가중치를 넘어선 경우, 마지막 인덱스 반환
        }
    }
}