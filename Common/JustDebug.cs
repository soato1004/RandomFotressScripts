namespace RandomFortress
{
    using UnityEngine;
    using System;
    using System.IO;

    public static class JustDebug
    {
        private static bool isLoggingEnabled = true;
        private static bool isSavingToFile = false;
        private static string logFilePath = "Assets/Logs/debug_log.txt";

        // 로깅 활성화/비활성화
        public static void SetLoggingEnabled(bool enabled)
        {
            isLoggingEnabled = enabled;
        }

        // 파일 저장 활성화/비활성화
        public static void SetSavingToFile(bool enabled)
        {
            isSavingToFile = enabled;
        }

        // Log 메서드
        public static void Log(object message)
        {
            if (!isLoggingEnabled) return;
            
            Debug.Log(message);

            if (isSavingToFile)
            {
                SaveToFile("" + message);
            }
        }

        // LogWarning 메서드
        public static void LogWarning(object message)
        {
            if (!isLoggingEnabled) return;
            
            Debug.LogWarning(message);

            if (isSavingToFile)
            {
                SaveToFile("WARNING: " + message);
            }
        }

        // LogError 메서드
        public static void LogError(object message)
        {
            if (!isLoggingEnabled) return;
            
            Debug.LogError(message);

            if (isSavingToFile)
            {
                SaveToFile("ERROR: " + message);
            }
        }
        // 파일에 저장
        private static void SaveToFile(string message)
        {
            try
            {
                File.AppendAllText(logFilePath, message + Environment.NewLine);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save log to file: {e.Message}");
            }
        }
    }
}