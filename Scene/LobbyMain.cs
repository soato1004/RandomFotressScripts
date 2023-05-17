using RandomFortress.Manager;
using UnityEngine.Device;
using UnityEngine.SceneManagement;

namespace RandomFortress.Scene
{
    public class LobbyMain : BaseMain
    {
        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            
        }

        public void OnPlayButtonClick()
        {
            SceneManager.LoadScene(3);
        }

        public void OnExitButtonClick()
        {
            Application.Quit();
        }
    }
}