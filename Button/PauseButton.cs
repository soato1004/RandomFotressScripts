namespace RandomFortress
{
    public class PauseButton : ButtonBase
    {
        // 일시정지
        public void OnPauseButton()
        {
            ButtonLock(0.3f);
            if (GameManager.I.isPaused)
            {
                GameManager.I.ResumeGame();
            }
            else
            {
                GameManager.I.PauseGame();
            }
            
            GameUIManager.I.SetDim(GameManager.I.isPaused);
        }
    }
}