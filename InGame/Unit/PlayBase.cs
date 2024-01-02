using System;
using Photon.Pun;

using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

namespace RandomFortress.Game
{
    public abstract class PlayBase : MonoBehaviour
    {
        public GamePlayer player;
        public float timeScale = 1;
        
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

