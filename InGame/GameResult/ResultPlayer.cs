using System.Collections.Generic;
using RandomFortress.Common.Extensions;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Game
{
    [System.Serializable]
    public class ResultPlayer : MonoBehaviour
    {
        public Transform towerList;
        public TextMeshProUGUI stageText;
        
        private const int TARGET_WIDTH = 100;
        private const int TARGET_HEIGHT = 100;
        
        public void Reset()
        {
            stageText.text = "";
            for (int i = 0; i < towerList.childCount; ++i)
            {
                Transform temps = towerList.transform.GetChild(i);
                for (int j = 0; j < temps.childCount; ++j)
                {
                    temps.GetChild(j).GetChild(0).SetActive(false);
                    temps.GetChild(j).GetChild(1).SetActive(false);
                }
            }
        }

        private Dictionary<int,TowerResultData> towerDic = new Dictionary<int,TowerResultData>();

        struct TowerResultData
        {
            public int towerIndex;
            public int towerTier;
            public int totalDamage;
        }
        
        /// <summary>
        /// 게임 내에서 사용된 타워 종류별로 1개씩만 표시하고 데미지를 통합. 가장 티어가 높았던 이미지로
        /// </summary>
        /// <param name="towers"></param>
        /// <param name="stage"></param>
        public void ShowTower(TowerBase[] towers, int stage)
        {
            // 스테이지 진행상황 표시

            stageText.text = "Stage Progress    <#FFDC30>" + stage + "</color>";
            
            // 타워 종류별로 분류 후 데미지 합산
            int i = 0;
            for (i = 0; i < towers.Length; ++i)
            {
                TowerBase tower = towers[i];
                if (tower == null)
                    continue;


                if (towerDic.ContainsKey(tower.TowerIndex))
                {
                    TowerResultData data = towerDic[tower.TowerIndex];
                    data.towerTier = data.towerTier > tower.TowerTier ? data.towerTier : tower.TowerTier;
                    data.totalDamage += tower.TotalDamege;
                }
                else
                {
                    TowerResultData data;
                    data.towerIndex = tower.TowerIndex;
                    data.towerTier = tower.TowerTier;
                    data.totalDamage = tower.TotalDamege;
                    towerDic.Add(tower.TowerIndex, data);
                }
            }

            // 타워 종류 및 데미지 표시
            i = 0;
            foreach(KeyValuePair<int, TowerResultData> data in towerDic)
            {
                TowerResultData towerResult = data.Value;
                
                Transform character = towerList.GetChild(i/4).GetChild(i%4);

                // 아이콘
                Image icon = character.GetChild(0).GetComponent<Image>();
                icon.gameObject.SetActive(true);
                icon.sprite = ResourceManager.Instance.GetTower(towerResult.towerIndex, towerResult.towerTier);
                
                Common.Utils.ImageUtils.ImageSizeToFit(TARGET_WIDTH, TARGET_HEIGHT, ref icon);

                // DPS
                TextMeshProUGUI text = character.GetChild(1).GetComponent<TextMeshProUGUI>();
                text.gameObject.SetActive(true);
                text.text = towerResult.totalDamage.ToString();
                
                i++;
            }
        }
    }
}