using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class SoloResult : ResultBase
    {
        [SerializeField] protected Transform title;
        [SerializeField] protected TextMeshProUGUI titleText;
        [SerializeField] protected ResultPlayer player; 
        [SerializeField] protected Button claimBtn;
        
        // TODO: 차우 리워드 추가
        public IEnumerator ResultCor()
        {
            GamePlayer myPlayer = GameManager.I.myPlayer;
            
            // 게임결과
            GameResult result = new GameResult(GameManager.I.IsGameClear, myPlayer.stageProcess, (int)GameManager.I.gameTime);
            result.gameType = GameType.Solo;
            result.towerList = myPlayer.Towers
                .Where(t => t != null && t.Info != null)
                .Select(t => t.Info.index)
                .ToArray();

            // 계정에 게임결과 저장
            _ = Account.I.SaveGameResult(result);
            
            //TODO: 클리어 사운드는 다르게
            SoundManager.I.PlayOneShot(SoundKey.result_win);

            // 타이틀표시
            bool isWin = GameManager.I.isWin;
            ShowTitle(isWin, title, titleText);
            
            // 플레이어 게임 수치 표시
            player.SetNickname(myPlayer.Nickname);
            player.ShowResultPlayer(myPlayer.totalDmgDic, result);
            
            // 버튼
            claimBtn.gameObject.SetActive(true);

            yield return null;
        }
    }
}