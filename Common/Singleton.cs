using System;
using Photon.Pun;
using UnityEngine;

namespace RandomFortress.Common
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static object _lock = new object();
        private static bool applicationIsQuitting = false;

        [SerializeField] private static bool dontDestroyObject = true;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        Initialize();

                        if (_instance == null)
                            throw new Exception($"MonoSingleton {typeof(T)} failed to Initialize.");
                    }

                    return _instance;
                }
            }
        }

        public static void Initialize()
        {
            if (_instance != null)
            {
                Debug.LogWarning($"MonoSingleton {typeof(T)} :: Instance already exists. Using existing instance.");
                return;
            }

            _instance = FindObjectOfType<T>();

            if (_instance == null)
            {
                GameObject goSingleton = new GameObject(typeof(T).Name, typeof(T));
                _instance = goSingleton.GetComponent<T>();
                Debug.Log($"MonoSingleton {typeof(T)} :: Created {goSingleton.name}");
            }
            else
            {
                Debug.Log($"MonoSingleton :: Found existing instance of {typeof(T)} using {_instance.gameObject.name}");
            }

            if (dontDestroyObject && _instance.transform.parent == null)
            {
                DontDestroyOnLoad(_instance.gameObject);
            }
            
            
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (dontDestroyObject)
                    DontDestroyOnLoad(this.gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                applicationIsQuitting = true;
            }
        }

        public abstract void Reset();
        public abstract void Terminate();
    }
    
        public abstract class SingletonPun<T> : MonoBehaviourPunCallbacks where T : MonoBehaviour
    {
        private static T _instance;
        private static object _lock = new object();
        private static bool applicationIsQuitting = false;

        [SerializeField] private static bool dontDestroyObject = true;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        Initialize();

                        if (_instance == null)
                            throw new Exception($"MonoSingleton {typeof(T)} failed to Initialize.");
                    }

                    return _instance;
                }
            }
        }

        public static void Initialize()
        {
            if (_instance != null)
            {
                Debug.LogWarning($"MonoSingleton {typeof(T)} :: Instance already exists. Using existing instance.");
                return;
            }

            _instance = FindObjectOfType<T>();

            if (_instance == null)
            {
                GameObject goSingleton = new GameObject(typeof(T).Name, typeof(T));
                _instance = goSingleton.GetComponent<T>();
                Debug.Log($"MonoSingleton {typeof(T)} :: Created {goSingleton.name}");
            }
            else
            {
                Debug.Log($"MonoSingleton :: Found existing instance of {typeof(T)} using {_instance.gameObject.name}");
            }

            if (dontDestroyObject && _instance.transform.parent == null)
            {
                DontDestroyOnLoad(_instance.gameObject);
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (dontDestroyObject)
                    DontDestroyOnLoad(this.gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                applicationIsQuitting = true;
            }
        }

        public abstract void Reset();
        public abstract void Terminate();
    }
}