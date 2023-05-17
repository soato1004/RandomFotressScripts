using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using RandomFortress.Common.Util;
using UnityEngine;

namespace RandomFortress.Common
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static Transform _parent;
        
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                throw new Exception($"MonoSingleton {typeof(T)} Initialize");
            }
        }
        
        public static void Initialize()
        {
            // 
            if (_parent == null)
            {
                var go = GameObject.Find("Managers");
                if (go == null)
                    go = new GameObject("Managers");

                _parent = go.transform;
            }
            
            // 모노싱글톤 찾기.
            _instance = UnityEngine.Object.FindObjectOfType(typeof(T)) as T;

            // 찾아도 없다면 인스턴스 생성.
            if (null == _instance) {
                GameObject goSingleton = new GameObject(typeof(T).Name, typeof(T));
                _instance = goSingleton.GetComponent<T>();
                // UnityEngine.Object.DontDestroyOnLoad(goSingleton);
                JTDebug.LogColor($"MonoSingleton {typeof(T)} :: Create {goSingleton}", $"{LogColor.Singleton}");
            } else {
                // UnityEngine.Object.DontDestroyOnLoad(_instance.gameObject);
                JTDebug.LogColor($"MonoSingleton :: Has MonoSingleton Please Use {_instance.gameObject.name} / {typeof(T)}", $"{LogColor.Singleton}");
            }

            _instance.transform.parent = _parent;
        }

        /// <summary>
        /// 매니저 클래스 초기화
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// 매니저 클래스 마무리
        /// </summary>
        public abstract void Terminate();
    }
}
