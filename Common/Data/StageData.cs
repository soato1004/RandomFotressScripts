using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/StageData", order = 1)]
    public class StageData : ScriptableObject
    {
        public int[] appearMonster = new int[10]; // 등장 몬스터의 인덱스
        public int hp; // 스테이지 지날때 몬스터 체력버프
        public int appearDelay; // 1당 0.1f초의 delay
        public int stageReward; // 스테이지 클리어시 리워드
        public int startDelayTime; // 1 = 0.1f delay. 스테이지 변경시 딜레이타임
    }
}