using System.Collections;
using Photon.Pun;
using RandomFortress.Game.Netcode;
using TMPro;
using UnityEngine;

namespace RandomFortress.Scene
{
    public class Match : MonoBehaviour
    {
        public TextMeshProUGUI PlayerCount;
        public TextMeshProUGUI WaitTime;
        
        private void Start()
        {
            PunManager.Instance.Connect();
            StartCoroutine(MatchCor());
        }

        public void OnCancleButtonClick()
        {
            PunManager.Instance.LeaveRoom();
        }

        private IEnumerator MatchCor()
        {
            while (true)
            {
                if (PhotonNetwork.CurrentRoom != null)
                {
                    PlayerCount.text = "Player ( " + PhotonNetwork.CurrentRoom.PlayerCount + " )";
                    WaitTime.text = "Wait " + (int)(3 - PunManager.Instance.waitTime);
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}