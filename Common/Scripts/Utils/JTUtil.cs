using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace RandomFortress.Common.Util
{
    /// <summary> </summary>
    public static class JTUtil
    {
        /// <summary> 딜레이 코루틴 </summary>
        public static Coroutine DelayCallCoroutine(MonoBehaviour mono, float seconds, Action action)
        {
            if (mono == null || !mono.gameObject.activeInHierarchy)
                return null;
            
            return mono.StartCoroutine(DelayCallCoroutine(seconds, action));
        }
        
        /// <summary> 코루틴 지정 시간 딜레이 후 콜백 </summary>
        private static IEnumerator DelayCallCoroutine(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action?.Invoke();
        }
        
        /// <summary> 지정 시간 딜레이 </summary>
        public static IObservable<long> Delay(float second)
        {
            return Observable.Timer(TimeSpan.FromSeconds(second));
        }

        /// <summary> 1 프레임 딜레이</summary>
        public static IObservable<Unit> NextEndOfFrame()
        {
            return Observable.NextFrame(FrameCountType.EndOfFrame);
        }
        
        public static T DeepCopy<T>(T other) where T : ScriptableObject
        {
            // // 새로운 인스턴스를 만듭니다.
            // T clone = ScriptableObject.CreateInstance<T>();
            //
            // // 원본 ScriptableObject의 필드를 복사합니다.
            // System.Reflection.FieldInfo[] fields = typeof(T).GetFields();
            // foreach (var field in fields)
            // {
            //     field.SetValue(clone, field.GetValue(other));
            // }
            
            // 원본 ScriptableObject를 JSON으로 직렬화합니다.
            string json = JsonUtility.ToJson(other);

            // 새로운 인스턴스를 만듭니다.
            T clone = ScriptableObject.CreateInstance<T>();

            // JSON 데이터를 새로운 인스턴스에 역직렬화합니다.
            JsonUtility.FromJsonOverwrite(json, clone);

            return clone;
        }
    }

    public static class JTDebug
    {
        public static void LogColor(object message, string color = "red")
        {
            Debug.Log($"<color={color}>{message}</color>");
        }

        public static void Log(object message)
        {
            Debug.Log(message);
        }

        public static void LogError(object message)
        {
            Debug.LogError(message);
        }
    }
}