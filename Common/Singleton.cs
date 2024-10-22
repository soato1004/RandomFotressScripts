using Photon.Pun;
using UnityEngine;

namespace RandomFortress
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();

        [SerializeField] private bool dontDestroyOnLoad = true;

        public static T I
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
            _instance = FindAnyObjectByType<T>();

            if (_instance == null)
            {
                GameObject singletonObject = new GameObject(typeof(T).Name, typeof(T));
                _instance = singletonObject.AddComponent<T>();
            }

            Singleton<T> singletonComponent = _instance as Singleton<T>;
            if (singletonComponent != null && singletonComponent.dontDestroyOnLoad)
            {
                DontDestroyOnLoad(_instance.gameObject);
                Debug.Log("DontDestroy : "+_instance.gameObject.name);
            }
        }
    }

    public abstract class SingletonPun<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacks
    {
        private static T _instance;
        private static readonly object _lock = new object();

        [SerializeField] private bool dontDestroyOnLoad = true;

        public static T I
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
            _instance = FindAnyObjectByType<T>();

            if (_instance == null)
            {
                GameObject singletonObject = new GameObject(typeof(T).Name, typeof(T));
                _instance = singletonObject.AddComponent<T>();
            }

            SingletonPun<T> singletonComponent = _instance as SingletonPun<T>;
            if (singletonComponent != null && singletonComponent.dontDestroyOnLoad)
            {
                DontDestroyOnLoad(_instance.gameObject);
                Debug.Log("DontDestroy : "+_instance.gameObject.name);
            }
        }
    }
}
