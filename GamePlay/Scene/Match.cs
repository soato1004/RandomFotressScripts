using RandomFortress.Game.Netcode;

namespace RandomFortress.Scene
{
    public class Match : MainBase
    {

        private void Start()
        {
            PunManager.Instance.Connect();
        }

        public void OnCancleButtonClick()
        {
            PunManager.Instance.LeaveRoom();
        }
    }
}