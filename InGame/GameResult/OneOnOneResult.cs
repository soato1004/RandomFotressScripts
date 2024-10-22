using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class OneOnOneResult : ResultBase
    {
        [SerializeField] protected Transform title;
        [SerializeField] protected TextMeshProUGUI titleText;
        [SerializeField] protected ResultPlayer other;
        [SerializeField] protected ResultPlayer player; 
        [SerializeField] protected Button claimBtn;
        
        //TODO: 리워드 추가
        public IEnumerator ResultCor()
        {
            GamePlayer myPlayer = GameManager.I.myPlayer;
            GamePlayer otherPlayer = GameManager.I.otherPlayer;

            bool isWin = GameManager.I.isWin;
            
            // 게임결과 반영
            GameResult result = new GameResult(isWin, myPlayer.stageProcess, (int)GameManager.I.gameTime);
            result.gameType = GameType.OneOnOne;
            result.towerList = myPlayer.Towers
                .Where(t => t != null && t.Info != null)
                .Select(t => t.Info.index)
                .ToArray();
            result.roomName = GameManager.I.RoomName;
            result.otherUserid = otherPlayer.Userid;
            result.otherTowerList = otherPlayer.Towers
                .Where(t => t != null && t.Info != null)
                .Select(t => t.Info.index)
                .ToArray();
            
            Debug.Log("상대방 UserID: "+otherPlayer.Userid);
            
            _ = Account.I.SaveGameResult(result);
            
            // 타이틀 표시 및 게임결과별 사운드
            ShowTitle(isWin, title, titleText);
            
            // 게임 수치 표시
            other.SetNickname(otherPlayer.Nickname);
            player.SetNickname(myPlayer.Nickname);
            
            other.ShowTowerList(otherPlayer.totalDmgDic);
            player.ShowResultPlayer(myPlayer.totalDmgDic, result);
            
            // 버튼표시
            claimBtn.gameObject.SetActive(true);
            yield return null;
        }
    }
}