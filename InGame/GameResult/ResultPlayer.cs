using System.Collections.Generic;

using RandomFortress.Data;

using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace RandomFortress
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
        public TextMeshProUGUI gameText;
        public TextMeshProUGUI stageText;
        public Image rankIcon;
        public TextMeshProUGUI rankText;
        
        private const int TARGET_WIDTH = 100;
        private const int TARGET_HEIGHT = 100;
        
        public void Reset()
        {
            gameText.text = "";
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

        public void ShowTowerList(SerializedDictionary<int, TowerResultData> towers)
        {
            // 타워 DPS
            int i = 0;
            foreach(KeyValuePair<int, TowerResultData> tower in towers)
            {
                int towerIndex = tower.Value.towerIndex;
                int towerDamage = tower.Value.totalDamage;
                int towerTier = tower.Value.towerTier;
                
                Transform character = towerList.GetChild(i/4).GetChild(i%4);

                // 타워 아이콘
                Image icon = character.GetChild(0).GetComponent<Image>();
                icon.gameObject.SetActive(true);
                icon.sprite = ResourceManager.Instance.GetTower(towerIndex, towerTier);

                if (icon.sprite == null)
                {
                    Debug.Log("Not Found Image : " + towerIndex + " " + towerTier);
                    continue;
                }

                Utils.ImageSizeToFit(TARGET_WIDTH, TARGET_HEIGHT, ref icon);

                // DPS
                TextMeshProUGUI text = character.GetChild(1).GetComponent<TextMeshProUGUI>();
                text.gameObject.SetActive(true);
                text.text = towerDamage.ToString();
                
                i++;
            }
        }
        
        /// <summary>
        /// 게임 내에서 사용된 타워 종류별로 1개씩만 표시하고 데미지를 통합. 가장 티어가 높았던 이미지로
        /// </summary>
        /// <param name="towers"></param>
        /// <param name="stage"></param>
        public void ShowResultPlayer(SerializedDictionary<int, TowerResultData> towers, GameResult result)
        {
            // 스테이지 진행상황 표시
            stageText.text = "Stage Progress    <#FFDC30>" + result.maxClearStage + "</color>";
            
            // 게임 시각
            int minute = (int)GameManager.Instance.gameTime / 60;
            string time = minute + " : " + ((int)GameManager.Instance.gameTime % 60).ToString("D2");
            gameText.text = "Game Time    <#FFDC30>" + time + "</color>";
            
            //TODO: 랭크티어
            string spriteName = "Icon_GradeBadge_" + result.rank.ToString();
            Sprite sprite = ResourceManager.Instance.GetSprite(spriteName);
            if (sprite != null)
                rankIcon.sprite = sprite;
            else 
                Debug.Log("Not Found Sprite : "+spriteName);

            rankText.text = result.rank.ToString();
            
            // 타워리스트
            ShowTowerList(towers);
        }
    }
}