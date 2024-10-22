using UnityEngine;
using System;
using System.Collections.Generic;

namespace RandomFortress
{
    
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();

        public static MainThreadDispatcher Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Update()
        {
            lock(_executionQueue) {
                while (_executionQueue.Count > 0) {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }

        public void Enqueue(Action action)
        {
            lock (_executionQueue) {
                _executionQueue.Enqueue(action);
            }
        }

        public static void RunOnMainThread(Action action)
        {
            if (Instance != null)
            {
                Instance.Enqueue(action);
            }
        }
    }
}