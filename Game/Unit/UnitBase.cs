using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace RandomFortress.Game
{
    public abstract class UnitBase : MonoBehaviour
    {
        public float timeScale = 1;
        
        protected virtual void Awake()
        {
        }
        
        protected IObjectPool<GameObject> Pool;
        public void SetPool(IObjectPool<GameObject> pool) => Pool = pool;
        
        
        public Action OnUnitDestroy;
    }
}

