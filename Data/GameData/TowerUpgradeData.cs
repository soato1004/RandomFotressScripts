using System;
using System.Collections.Generic;
using UnityEngine;

namespace RandomFortress.Data
{
    /// <summary>
    /// 타워 업그레이드 수치 저장
    /// </summary>
    [CreateAssetMenu(fileName = "TowerUpgradeData", menuName = "ScriptableObjects/TowerData", order = 1)]
    public class TowerUpgradeData : ScriptableObject
    {
        public int index = 0; // 타워 인덱스
        
        public List<CardUpgradeInfo> CardLvData = new List<CardUpgradeInfo>();  // 카드 업그레이드 정보. 2~10
        public List<TowerUpgradeInfo> UpgradeData = new List<TowerUpgradeInfo>(); // 인게임 타워 업그레이드 수치. 2~5
    }
    
    [Serializable]
    public class CardUpgradeInfo
    {
        public int NeedCard; // 업그레이드 시에 필요한 카드
        public List<int> CardLVData = new List<int>(); // 카드 업그레이드 시 추가 능력치데이터
    };

    [Serializable]
    public class TowerUpgradeInfo
    {
        public List<int> Data = new List<int>(); // 타워 업그레이드 시 추가 능력치데이터
    }
}