using System;
using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace RandomFortress
{
    public class Match : SceneBase
    {
        public TextMeshProUGUI explanText;
        public TextMeshProUGUI WaitTime;
        
        public override void StartScene()
        {
            PhotonManager.I.StartMatching();
            StartCoroutine(UpdateWaitTimeCor());
        }

        private IEnumerator UpdateWaitTimeCor()
        {
            float timer = 0;

            while (true)
            {
                int minutes = Mathf.FloorToInt(timer / 60f);
                int seconds = Mathf.FloorToInt(timer % 60f);
                WaitTime.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                yield return new WaitForSeconds(1f);
                timer += 1f;
            }
        }
        
        public void OnCancleButtonClick()
        {
            StopAllCoroutines();
            // 룸에 있다면 룸을 떠나는 함수 호출
            PhotonManager.I.GameOver();
            MainManager.I.ChangeScene(SceneName.Lobby);
        }

        public void OnDisable()
        {
            StopAllCoroutines();
        }
    }
}