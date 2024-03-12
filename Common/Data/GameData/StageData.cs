using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "StageData", menuName = "ScriptableObjects/StageData", order = 1)]
    public class StageData : ScriptableObject
    {
        // public List<StageDataInfo> stageDataList;
        public StageDataInfo[] Infos;

    }
    
    [Serializable]
    public class StageDataInfo
    {
        public int appearMonster; // 등장 몬스터의 인덱스
        public int hp; // 스테이지 몬스터 체력
        public int appearDelay; // 1당 0.1f초의 delay
        public int stageReward; // 스테이지 클리어시 리워드
        // public int startDelayTime; // 1 = 0.1f delay. 스테이지 변경시 딜레이타임
        public int monsterCount;
    }
}