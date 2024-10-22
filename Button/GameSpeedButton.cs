using TMPro;

namespace RandomFortress
{
    public class GameSpeedButton : ButtonBase
    {
        public TextMeshProUGUI speedText;

        void Start()
        {
            // 슈퍼패스를 구입해야만 배속할수있다
            button.interactable = Account.I.Data.hasSuperPass;
            speedText.alpha = Account.I.Data.hasSuperPass ? 1 : 0.5f;
        }
        
        public void OnGameSpeedButtonClick()
        {
            ButtonLock(2);
            GameManager.I.ChangeGameSpeed();
        }

        public void UpdateUI()
        {
            speedText.text = "x" + GameManager.I.gameSpeed;
        }
    }
}