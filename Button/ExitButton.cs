namespace RandomFortress
{
    public class ExitButton : ButtonBase
    {
        // 게임종료
        public void OnExitButton()
        {
            ButtonLock(1f);
            GameManager.I.EndGame();
        }

    }
}