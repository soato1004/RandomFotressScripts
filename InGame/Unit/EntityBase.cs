using System;
using UnityEngine;
using UnityEngine.Pool;

namespace RandomFortress.Game
{
    public abstract class EntityBase : MonoBehaviour
    {
        public GamePlayer player;
        
        public IObjectPool<GameObject> Pool { get; set; }
        
        protected void Release()
        {
            Pool.Release(gameObject);
        }
        
        public bool isDestroyed { get; protected set; }
        
        // public PhotonView photonView;

        protected virtual void Awake()
        {
            // photonView = transform.GetComponent<PhotonView>();
            isDestroyed = false;
        }

        public Action OnUnitDestroy;

        protected virtual void Remove() {}
    }
}

