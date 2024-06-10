using System.Collections;
using RandomFortress.Data;

using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class SoloResult : MonoBehaviour
    {
        [Header("Solo")]
        [SerializeField] private GameObject SoloTitle;
        [SerializeField] private ResultPlayer player; 
        [SerializeField] private Transform rewardList; // enegy, buff, gold, gem, item
        [SerializeField] private Button claimBtn;
        // [SerializeField] private Button continueBtn;
        
        public IEnumerator ResultCor()
        {
            GamePlayer myPlayer = GameManager.Instance.myPlayer;

            SoundManager.Instance.PlayOneShot("result_win");
            
            GameResult result = new GameResult(true, myPlayer.stageProcess, (int)GameManager.Instance.gameTime);
            Account.Instance.SaveStageResult(result);
            
            // 딤
            // yield return new WaitForSecondsRealtime(0.2f);

            // 2. 타이틀표시
            SoloTitle.SetActive(true);
            // yield return new WaitForSecondsRealtime(0.05f);
            
            // 3. 게임 수치 표시
            player.gameObject.SetActive(true);
            player.ShowResultPlayer(myPlayer.totalDmgDic, result);
            // yield return new WaitForSecondsRealtime(0.05f);
            
            // 4. 리워드 표시
            // Transform goldReward = rewardList.transform.GetChild(2);
            // goldReward.SetActive(true);
            // TextMeshProUGUI goldText = goldReward.GetChild(2).GetComponent<TextMeshProUGUI>();
            // goldText.text = "1000";
            // yield return new WaitForSecondsRealtime(0.05f);
            
            // 6. 버튼표시
            claimBtn.gameObject.SetActive(true);
            // continueBtn.ExSetActive(true);

            yield return null;
        }
    }
}