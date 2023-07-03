using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Photon
{
    public class CustomPrefabPool : MonoBehaviour, IPunPrefabPool
    {
        private Dictionary<string, Queue<GameObject>> prefabPools = new Dictionary<string, Queue<GameObject>>();
        
        public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
        {
            if (prefabPools.ContainsKey(prefabId) && prefabPools[prefabId].Count > 0)
            {
                GameObject obj = prefabPools[prefabId].Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
                return obj;
            }
            else
            {
                Debug.LogWarning($"Prefab Pool for {prefabId} is empty or not found. Instantiating a new object.");
                return PhotonNetwork.InstantiateRoomObject(prefabId, position, rotation);
            }
        }
        
        public void Destroy(GameObject gameObject)
        {
            string prefabId = gameObject.name;
        
            if (!prefabPools.ContainsKey(prefabId))
            {
                prefabPools[prefabId] = new Queue<GameObject>();
            }
        
            gameObject.SetActive(false);
            prefabPools[prefabId].Enqueue(gameObject);
        }
    }
}