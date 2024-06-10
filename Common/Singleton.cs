using System;
using Photon.Pun;
using UnityEngine;

namespace RandomFortress
{

    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();

        [SerializeField] private bool dontDestroyOnLoad = true; // 이제 인스턴스 변수입니다.

        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        Initialize();
                    }
                    return _instance;
                }
            }
        }

        private static void Initialize()
        {
            if (_instance != null)
                return;

            _instance = FindAnyObjectByType<T>();

            if (_instance == null)
            {
                GameObject singletonObject = new GameObject(typeof(T).Name, typeof(T));
                _instance = singletonObject.AddComponent<T>();
            }

            Singleton<T> singletonComponent = _instance as Singleton<T>;
            if (singletonComponent != null && singletonComponent.dontDestroyOnLoad && _instance.transform.parent == null)
            {
                DontDestroyOnLoad(_instance.gameObject);
            }
        }

        public abstract void Reset();
    }

    public abstract class SingletonPun<T> : MonoBehaviourPunCallbacks where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();

        [SerializeField] private bool dontDestroyOnLoad = true;

        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        Initialize();
                    }
                    return _instance;
                }
            }
        }

        private static void Initialize()
        {
            if (_instance != null)
                return;

            _instance = FindAnyObjectByType<T>();

            if (_instance == null)
            {
                GameObject singletonObject = new GameObject(typeof(T).Name, typeof(T));
                _instance = singletonObject.GetComponent<T>();
            }

            SingletonPun<T> singletonComponent = _instance as SingletonPun<T>;
            if (singletonComponent != null && singletonComponent.dontDestroyOnLoad && _instance.transform.parent == null)
            {
                DontDestroyOnLoad(_instance.gameObject);
            }
        }

        public abstract void Reset();
    }
}