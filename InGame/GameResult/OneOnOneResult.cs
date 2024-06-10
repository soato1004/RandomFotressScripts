using System.Collections;
using RandomFortress.Data;

using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class OneOnOneResult : MonoBehaviour
    {
        [SerializeField] private ResultPlayer other;
        [SerializeField] private ResultPlayer player; 
        [SerializeField] private Transform rewardList; // enegy, buff, gold, gem, item
        [SerializeField] private GameObject WinTitle;
        [SerializeField] private GameObject DefeatTitle;
        [SerializeField] private Button claimBtn;
        
        public IEnumerator ResultCor()
        {
            GamePlayer myPlayer = GameManager.Instance.myPlayer;
            GamePlayer otherPlayer = GameManager.Instance.otherPlayer;
            bool isWin = GameManager.Instance.isWin;
            
            if (isWin) 
                SoundManager.Instance.PlayOneShot("result_win");
            else
                SoundManager.Instance.PlayOneShot("result_lose");
            
            
            GameResult myResult = new GameResult(isWin, myPlayer.stageProcess, (int)GameManager.Instance.gameTime);
            Account.Instance.SaveStageResult(myResult);
            
            // yield return new WaitForSecondsRealtime(0.2f);

            // 2. 타이틀표시
            WinTitle.SetActive(isWin);
            DefeatTitle.SetActive(!isWin);
            // yield return new WaitForSecondsRealtime(0.05f);
            
            // 3. 게임 수치 표시
            other.gameObject.SetActive(true);
            player.gameObject.SetActive(true);
            
            GameResult otherResult = new GameResult(isWin, otherPlayer.stageProcess, (int)GameManager.Instance.gameTime);
            
            other.ShowTowerList(otherPlayer.totalDmgDic);
            player.ShowResultPlayer(myPlayer.totalDmgDic, myResult);
            // yield return new WaitForSecondsRealtime(0.05f);
            
            // 4. 리워드 표시
            // Transform goldReward = rewardList.transform.GetChild(2);
            // goldReward.SetActive(true);
            // TextMeshProUGUI goldText = goldReward.GetChild(2).GetComponent<TextMeshProUGUI>();
            // goldText.text = "1000";
            // yield return new WaitForSecondsRealtime(0.05f);
            
            // 6. 버튼표시
            claimBtn.gameObject.SetActive(true);
            yield return null;
        }
    }
}