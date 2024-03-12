using RandomFortress.Data;
using RandomFortress.Game;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Menu
{
    public class RankBoard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI clearTime;
        [SerializeField] private TextMeshProUGUI maxStage;
        [SerializeField] private Image rankIcon;
        [SerializeField] private TextMeshProUGUI rankName;

        public void UpdateRankBoard()
        {
            if (Account.Instance.Data.BestGameResult == null)
                return;

            GameResult result = Account.Instance.Data.BestGameResult;
            clearTime.text = (result.clearTime/60).ToString("D2")+":"+(result.clearTime%60).ToString("D2");
            maxStage.text = result.maxClearStage.ToString();
            
            //TODO: 랭크 아이콘 경로 하드코딩
            string spriteName = "Icon_GradeBadge_" + result.rank.ToString();
            Sprite sprite = ResourceManager.Instance.GetSprite(spriteName);
            if (sprite != null)
                rankIcon.sprite = sprite;
            else Debug.Log("Not Found Sprite : "+spriteName);
        }
    }
}