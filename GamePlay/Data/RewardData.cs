using UnityEngine;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/RewardData", order = 1)]
    public class RewardData : ScriptableObject
    {
        public int rewardIndex;        
        public int rewardName;
        public int rewardValue;
        public int rewardCount;
        
        public RewardData() { }

        public RewardData(RewardData other)
        {

        }
    }
}