


using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    /// <summary>
    /// 배틀 에서 표시되는 현재 솔로모드 최고기록을 보여주는곳
    /// </summary>
    public class RankBoard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI clearTime;
        [SerializeField] private TextMeshProUGUI maxStage;
        [SerializeField] private Image rankIcon;
        [SerializeField] private TextMeshProUGUI rankName;

        public void UpdateRankBoard()
        {
            GameResult result = Account.I.Data.bestGameResult;
            if (result == null)
                return;

            clearTime.text = (result.clearTime/60).ToString("D2")+":"+(result.clearTime%60).ToString("D2");
            maxStage.text = result.maxClearStage.ToString();
            rankName.text = LocalizationManager.I.GetLocalizedString("common_rank_" + result.rank);
            
            string spriteName = "Icon_GradeBadge_" + result.rank.ToString();
            Sprite sprite = ResourceManager.I.GetSprite(spriteName);
            rankIcon.sprite = sprite;
        }
    }
}