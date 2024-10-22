using TMPro;
using UnityEngine;

namespace RandomFortress
{
    public class ResultBase : MonoBehaviour
    {
        protected void ShowTitle(bool isWin, Transform title, TextMeshProUGUI titleText)
        {
            // 타이틀표시
            foreach (var child in title.GetChildren())
                child.SetActive(false);
            
            if (GameManager.I.IsGameClear)
            {
                title.GetChild(0).SetActive(true);
                SoundManager.I.PlayOneShot(SoundKey.result_clear);
                titleText.SetText(LocalizationManager.I.GetLocalizedString("common_clear"));
            }
            else if (isWin == true)
            {
                title.GetChild(1).SetActive(true);
                SoundManager.I.PlayOneShot(SoundKey.result_win);
                titleText.SetText(LocalizationManager.I.GetLocalizedString("common_win"));
            }
            else if (isWin == false)
            {
                title.GetChild(2).SetActive(true);
                SoundManager.I.PlayOneShot(SoundKey.result_lose);
                titleText.SetText(LocalizationManager.I.GetLocalizedString("common_lose"));
            }
        }
    }
}