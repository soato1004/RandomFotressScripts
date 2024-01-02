using System;
using Photon.Pun;
using RandomFortress.Common.Utils;
using UnityEngine;

namespace RandomFortress.Common
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        
        [SerializeField] private static bool dontDestroyObject = true;
        
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                return null;
                // throw new Exception($"MonoSingleton {typeof(T)} Initialize");
            }
        }
        
        public static void Initialize()
        {
            // 모노싱글톤 찾기.
            _instance = FindObjectOfType(typeof(T)) as T;

            // 찾아도 없다면 인스턴스 생성.
            if (null == _instance) {
                GameObject goSingleton = new GameObject(typeof(T).Name, typeof(T));
                _instance = goSingleton.GetComponent<T>();
                JTDebug.LogColor($"MonoSingleton {typeof(T)} :: Create {goSingleton}", $"{LogColor.Singleton}");
            } else {
                JTDebug.LogColor($"MonoSingleton :: Has MonoSingleton Please Use {_instance.gameObject.name} / {typeof(T)}", $"{LogColor.Singleton}");
            }
            
            if(dontDestroyObject)
                DontDestroyOnLoad(_instance.gameObject);
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
    
    public abstract class SingletonNetwork<T> : MonoBehaviour where T : Component
    {
        private static T _instance;
        // private static Transform _parent;
        
        public virtual void Awake()
        {
            Initialize();
        }
        
        public static T Instance
        {
            get
            {
                if (_instance != null) 
                    return _instance;
                else
                {
                    Initialize();
                    
                    if (_instance != null) 
                        return _instance;
                    
                    throw new Exception($"MonoSingleton {typeof(T)} Initialize");
                }
            }
        }
        
        public static void Initialize()
        {
            // 모노싱글톤 찾기.
            _instance = UnityEngine.Object.FindObjectOfType(typeof(T)) as T;
        
            // 찾아도 없다면 인스턴스 생성.
            if (null == _instance) {
                GameObject goSingleton = new GameObject(typeof(T).Name, typeof(T));
                _instance = goSingleton.GetComponent<T>();
                UnityEngine.Object.DontDestroyOnLoad(goSingleton);
                JTDebug.LogColor($"MonoSingleton {typeof(T)} :: Create {goSingleton}", $"{LogColor.Singleton}");
            } else {
                UnityEngine.Object.DontDestroyOnLoad(_instance.gameObject);
                JTDebug.LogColor($"MonoSingleton :: Has MonoSingleton Please Use {_instance.gameObject.name} / {typeof(T)}", $"{LogColor.Singleton}");
            }
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
    
    public abstract class SingletonPun<T> : MonoBehaviourPunCallbacks where T : MonoBehaviour
    {
        private static T _instance;
        
        [SerializeField] private static bool dontDestroyObject = true;
        
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
            // 모노싱글톤 찾기.
            _instance = FindObjectOfType(typeof(T)) as T;

            // 찾아도 없다면 인스턴스 생성.
            if (null == _instance) {
                GameObject goSingleton = new GameObject(typeof(T).Name, typeof(T));
                _instance = goSingleton.GetComponent<T>();
                JTDebug.LogColor($"MonoSingleton {typeof(T)} :: Create {goSingleton}", $"{LogColor.Singleton}");
            } else {
                JTDebug.LogColor($"MonoSingleton :: Has MonoSingleton Please Use {_instance.gameObject.name} / {typeof(T)}", $"{LogColor.Singleton}");
            }
            
            if(dontDestroyObject)
                DontDestroyOnLoad(_instance.gameObject);
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
