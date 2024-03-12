using System;
using System.Collections;
using RandomFortress.Constants;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Menu
{
    public class AdDebuffState
    {
        public AdDebuffType type;
        public string endTime;    
        
        public AdDebuffState() {}
    }
    
    public class AdDebuff : MonoBehaviour
    {
        [SerializeField] private AdDebuffType type;
        [SerializeField] private GameObject bg;
        [SerializeField] private Image frame;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI debuffTimeText;
        
        public void ShowAdDebuff(float time)
        {
            gameObject.SetActive(true);
            StartCoroutine(AdDebuffCoroutine(time));
        }   
        
        private IEnumerator AdDebuffCoroutine(float remainingSeconds)
        {
            float endTime = Time.realtimeSinceStartup + remainingSeconds;

            while (Time.realtimeSinceStartup < endTime)
            {
                float timeLeft = endTime - Time.realtimeSinceStartup;
                // 남은 시간을 "분:초" 형식으로 변환
                string timeString = string.Format("{0:D2}:{1:D2}", (int)timeLeft / 60, (int)timeLeft % 60);
            
                if (debuffTimeText != null)
                {
                    debuffTimeText.text = timeString; // UI 업데이트
                }

                // 실제 시간을 기준으로 1초마다 업데이트
                yield return new WaitForSecondsRealtime(1f);
            }
            
            gameObject.SetActive(false);
        }
    }
}