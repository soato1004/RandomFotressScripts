using System;
using UnityEngine;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerData", order = 1)]
    public class StageData : ScriptableObject
    {
        public StageInfo[] infos; // 스테이지 몬스터 나오는 순서
        public int StartDelayTime; // 1 = 0.1f delay. 스테이지 변경시 딜레이타임
    }
    
    [Serializable]
    public class StageInfo
    {
        public int[] appearMonster;
        public float buffHP;
        public int appearDelay; // 1 = 0.1f delay
        public int stageReward;
    }
}