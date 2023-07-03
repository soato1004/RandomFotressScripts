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

        private const int iconWidth = 80;
        
        public void Reset()
        {
            stageText.text = "";
            for (int i = 0; i < towerList.childCount; ++i)
            {
                Transform temps = towerList.transform.GetChild(i);
                for (int j = 0; j < temps.childCount; ++j)
                {
                    temps.GetChild(j).SetActive(false);
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
                character.gameObject.SetActive(true);
                
                Image icon = character.GetChild(0).GetComponent<Image>();
                icon.sprite = ResourceManager.Instance.GetTower(towerResult.towerIndex, towerResult.towerTier);
                ResizeImage(icon);

                TextMeshProUGUI text = character.GetChild(1).GetComponent<TextMeshProUGUI>();
                text.text = towerResult.totalDamage.ToString();
                i++;
            }
        }
        
        void ResizeImage(Image image)
        {
            // 이미지 원본 크기
            float originalWidth = image.sprite.rect.width / image.sprite.pixelsPerUnit;
            float originalHeight = image.sprite.rect.height / image.sprite.pixelsPerUnit;

            // 이미지의 가로 세로 비율 계산
            float aspectRatio = originalWidth / originalHeight;

            // 새로운 크기 계산
            float newWidth = iconWidth;
            float newHeight = newWidth / aspectRatio;

            // 이미지 컴포넌트 크기 조정
            image.rectTransform.anchorMax = image.rectTransform.anchorMin;
            image.rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
        }
    }
}