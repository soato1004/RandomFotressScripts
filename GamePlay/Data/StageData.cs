using System;
using UnityEngine;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/StageData", order = 1)]
    public class StageData : ScriptableObject
    {
        public StageInfo[] infos; // 스테이지 몬스터 나오는 순서
        public int StartDelayTime; // 1 = 0.1f delay. 스테이지 변경시 딜레이타임
    }
    
    [Serializable]
    public class StageInfo
    {
        public int[] appearMonster; // 등장 몬스터의 인덱스
        public float buffHP; // 스테이지 지날때 몬스터 체력버프
        public int appearDelay; // 1당 0.1f초의 delay
        public int stageReward; // 스테이지 클리어시 리워드
    }
}