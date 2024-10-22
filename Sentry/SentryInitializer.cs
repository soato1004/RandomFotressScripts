using System;
using Sentry;

namespace RandomFortress
{
    using UnityEngine;
    using Sentry;

    public class SentryInitializer : MonoBehaviour
    {
        void Awake()
        {
            SentryOptions options = new SentryOptions
            {
                Dsn = "YOUR_SENTRY_DSN_HERE",
                Debug = true,
                AutoSessionTracking = true,
                AutoSessionTrackingInterval = TimeSpan.FromSeconds(30),
                TracesSampleRate = 1.0f
            };

            SentrySdk.Init(options);
        
            // 유니티 로그를 센트리로 전송
            Application.logMessageReceived += OnLogMessageReceived;
        }

        void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                SentrySdk.CaptureException(new Exception(condition));
            }
            else
            {
                SentrySdk.CaptureMessage(condition);
            }
        }

        void OnApplicationQuit()
        {
            SentrySdk.Close();
        }
    }
}