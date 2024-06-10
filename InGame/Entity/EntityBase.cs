using System;
using UnityEngine;
using UnityEngine.Pool;

namespace RandomFortress
{
    public abstract class EntityBase : MonoBehaviour
    {
        public GamePlayer player;
        public Action OnUnitDestroy;

        protected bool IsDestroyed { get; set; }
        public IObjectPool<GameObject> Pool { get; set; }
        
        protected void Release()
        {
            Pool.Release(gameObject);
        }

        protected virtual void Awake()
        {
            IsDestroyed = false;
        }
        
        protected virtual void Remove() {}
    }
}

