using System;
using UnityEngine;

namespace RandomFortress
{
    public class SceneBase : MonoBehaviour
    {
        protected virtual void Awake()
        {
            MainManager.I.currentScene = this;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.runInBackground = false;
        }

        public virtual void StartScene() {}
    }
}