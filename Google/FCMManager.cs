using Firebase.Messaging;
using UnityEngine;

namespace Google
{
    /// <summary>
    /// 푸시알림
    /// </summary>
    public class FCMManager : MonoBehaviour
    {
        void Start()
        {
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.MessageReceived += OnMessageReceived;
        }

        // token 수신
        private void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
        {
            Debug.Log("FCM Token: " + token.Token);
        }

        // 메시지 수신
        private void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
        {
            Debug.Log("Received a new message from: " + e.Message.From);
            Debug.Log("Message data: " + e.Message.Data);
            // Display the message or notification in the UI
        }
    }
}