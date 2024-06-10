using System;
using UnityEngine;

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

// 100
// 200
// 302
// 402
// 604
// 805
// 1007
// 1209
// 1411
// 12104
// 1974
// 2412
// 2851
// 3289
// 3728
// 4167
// 4605
// 5044
// 5482
// 76754
// 8526
// 9789
// 11684
// 12316
// 13579
// 14842
// 16105
// 17526
// 18632
// 161056
// 35367
// 38735
// 42104
// 45472
// 48840
// 52209
// 55577
// 58946
// 62314
// 561395
// 77244
// 82437
// 87630
// 96068
// 101261
// 103858
// 110349
// 123332
// 129823
// 842105