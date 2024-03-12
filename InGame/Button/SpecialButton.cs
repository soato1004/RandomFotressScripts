using System.Collections;
using RandomFortress.Constants;
using RandomFortress.Game;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpecialButton : ButtonBase
{
    [SerializeField] private Button button;
    // [SerializeField] private GameObject ready;
    [SerializeField] private Image Frame;
    [SerializeField] private Image Icon;
    [SerializeField] private GameObject CooltimeBG;
    [SerializeField] private TextMeshProUGUI CooltimeText;
    [SerializeField] private TextMeshProUGUI Name;

    public void OnSkipButtonClick()
    {
        bool isSkip = GameManager.Instance.SkipWaitTime();

        if (isSkip)
        {
            // 쿨타임 적용
            button.enabled = false;
            CooltimeBG.SetActive(true);
            
            StartCoroutine(CooltimeCor(GameConstants.SkipDelayTime));
        }
    }

    private IEnumerator CooltimeCor(float time)
    {
        float waitTime = 0f;
        float OneSecond = 0f;
        int i = (int)time;

        CooltimeText.text = i.ToString();
        
        while (waitTime < time)
        {
            OneSecond += Time.deltaTime;
            waitTime += Time.deltaTime;

            if (OneSecond >= 1f)
            {
                OneSecond = 0f;
                i--;
                CooltimeText.text = i.ToString();
            }
            
            yield return null;
        }
        
        button.enabled = true;
        CooltimeBG.SetActive(false);
    }
}
