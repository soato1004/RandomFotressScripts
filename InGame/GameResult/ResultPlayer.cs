using System.Collections.Generic;
using RandomFortress.Common.Extensions;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Game
{
    public class TowerResultData
    {
        public int towerIndex;
        public int towerTier;
        public int totalDamage;
    }
    
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
        
        /// <summary>
        /// 게임 내에서 사용된 타워 종류별로 1개씩만 표시하고 데미지를 통합. 가장 티어가 높았던 이미지로
        /// </summary>
        /// <param name="towers"></param>
        /// <param name="stage"></param>
        public void ShowTower(Dictionary<int, TowerResultData> towers, int stage)
        {
            // 스테이지 진행상황 표시
            stageText.text = "Stage Progress    <#FFDC30>" + stage + "</color>";
            
            int i = 0;
            foreach(KeyValuePair<int, TowerResultData> tower in towers)
            {
                int towerIndex = tower.Value.towerIndex;
                int towerDamage = tower.Value.totalDamage;
                int towerTier = tower.Value.towerTier;
                
                Transform character = towerList.GetChild(i/4).GetChild(i%4);

                // 아이콘
                Image icon = character.GetChild(0).GetComponent<Image>();
                icon.gameObject.SetActive(true);
                icon.sprite = ResourceManager.Instance.GetTower(towerIndex, towerTier);

                if (icon.sprite == null)
                {
                    Debug.Log("Not Found Image : " + towerIndex + " " + towerTier);
                    continue;
                }

                Common.Utils.ImageUtils.ImageSizeToFit(TARGET_WIDTH, TARGET_HEIGHT, ref icon);

                // DPS
                TextMeshProUGUI text = character.GetChild(1).GetComponent<TextMeshProUGUI>();
                text.gameObject.SetActive(true);
                text.text = towerDamage.ToString();
                
                i++;
            }
        }
    }
}