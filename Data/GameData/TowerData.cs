using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RandomFortress
{
    [CreateAssetMenu(fileName = "TowerData", menuName = "ScriptableObjects/TowerData", order = 1)]
    public class TowerData : ScriptableObject
    {
        public int index = 0; // 타워 인덱스
        public string towerName = ""; // 타워이름
        public SerializedDictionary<int, TowerInfo> towerInfoDic = new SerializedDictionary<int, TowerInfo>();
    }
}