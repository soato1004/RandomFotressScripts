using System.Collections;
using RandomFortress;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpecialButton : ButtonBase
{
    // [SerializeField] private GameObject ready;
    [SerializeField] private Image Frame;
    [SerializeField] private Image Icon;
    [SerializeField] private GameObject CooltimeBG;
    [SerializeField] private TextMeshProUGUI CooltimeText;
    [SerializeField] private TextMeshProUGUI Name;
    
    public void OnSkipButtonClick()
    {
        bool isSkip = GameManager.I.SkipStage();

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
            OneSecond += Time.deltaTime * GameManager.I.gameSpeed;
            waitTime += Time.deltaTime * GameManager.I.gameSpeed;

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
