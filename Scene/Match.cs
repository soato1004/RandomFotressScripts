using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace RandomFortress
{
    public class Match : MonoBehaviour
    {
        public TextMeshProUGUI PlayerCount;
        public TextMeshProUGUI WaitTime;
        
        private void Start()
        {
            Application.targetFrameRate = 60;
            // 화면이 자동으로 꺼지지 않도록 설정
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            PhotonManager.Instance.Connect();
            StartCoroutine(MatchCor());
        }

        public void OnCancleButtonClick()
        {
            // 플레이어가 룸에 있는지 확인
            if (PhotonNetwork.InRoom)
            {
                // 룸에 있다면 룸을 떠나는 함수 호출
                PhotonManager.Instance.LeaveRoom();
            }
            else
            {
                // 룸에 없다면 로비 씬으로 이동
                MainManager.Instance.ChangeScene(SceneName.Lobby);
            }
        }

        private IEnumerator MatchCor()
        {
            while (true)
            {
                if (PhotonNetwork.CurrentRoom != null)
                {
                    PlayerCount.text = "RoomPlayer " + PhotonNetwork.CurrentRoom.PlayerCount;
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}