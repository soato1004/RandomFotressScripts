using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class WaitPlayerPopup : PopupBase
    {
        [SerializeField] private Slider waitTimeSlider;
        [SerializeField] private TextMeshProUGUI waitTimeText;

        // private float remainingTime;
        private Tween sliderTween;

        public float elapsedWaitTime = 0f;
        
        public override void ShowPopup(params object[] values)
        {
            base.ShowPopup();

            elapsedWaitTime = (float)values[0];
            
            StartCoroutine(WaitTimer());
        }

        public float CombackPlayer()
        {
            StopCoroutine(WaitTimer());
            return elapsedWaitTime;
        }
        
        public IEnumerator WaitTimer()
        {
            float remainingTime = GameConstants.PlayerWaitTime - elapsedWaitTime; // 몇초가 남아있는지
            
            waitTimeSlider.gameObject.SetActive(true);
            waitTimeSlider.maxValue = 1;

            // 슬라이더 초기값 설정 (남은 시간의 비율)
            float initialSliderValue = remainingTime / GameConstants.PlayerWaitTime;
            waitTimeSlider.value = initialSliderValue;
            
            // DOTween 애니메이션 설정
            sliderTween = DOTween.To(() => waitTimeSlider.value,
                    x => waitTimeSlider.value = x, 0f, remainingTime)
                .SetEase(Ease.Linear)
                .SetUpdate(true);

            while (remainingTime > 0)
            {
                waitTimeText.SetText(Mathf.CeilToInt(remainingTime).ToString());
                yield return new WaitForSecondsRealtime(0.1f);
                elapsedWaitTime += 0.1f;
                remainingTime = GameConstants.PlayerWaitTime - elapsedWaitTime;
            }

            if (sliderTween != null && sliderTween.IsActive())
                sliderTween.Kill();
            
            GameManager.I.SaveWinner();
            GameManager.I.PlayerOut();
        }
    }
}