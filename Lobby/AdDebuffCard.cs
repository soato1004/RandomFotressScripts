using System;
using System.Collections;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    /// <summary>
    /// 로비에서 보여주는 디버프
    /// </summary>
    public class AdDebuffCard : MonoBehaviour
    {
        public AdRewardType type;
        [SerializeField] private GameObject bg;
        [SerializeField] private Image frame;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI debuffTimeText;
        [SerializeField] private Button rewardAdButton;
        
        
        public void ShowAdDebuff(float waitTime)
        {
            gameObject.SetActive(true);
            rewardAdButton.gameObject.SetActive(false);
            StartCoroutine(AdDebuffCoroutine(waitTime));
        }   
        
        private IEnumerator AdDebuffCoroutine(float waitTime)
        {
            float totalWaitTime = waitTime;
            while (totalWaitTime > 0)
            {
                // 남은 시간을 "분:초" 형식으로 변환
                string timeString = $"{(int)totalWaitTime / 60:D2}:{(int)totalWaitTime % 60:D2}";
                debuffTimeText.SetText(timeString);

                // 실제 시간을 기준으로 1초마다 업데이트
                yield return new WaitForSecondsRealtime(1f);

                totalWaitTime -= 1;
            }
            
            // 초기화
            debuffTimeText.SetText("00:00");
            rewardAdButton.gameObject.SetActive(true);
        }
    }
}