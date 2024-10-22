using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace RandomFortress
{
    public abstract class EntityBase : MonoBehaviour
    {
        public GamePlayer player;
        public Action OnUnitDestroy;
        
        public int _unitID = 0;
        public static float offSet = 0f; // 로컬플레이어가 항상 하단이므로 위치를 보정해야한다

        protected bool IsDestroyed { get; set; }
        public IObjectPool<GameObject> Pool { get; set; }

        protected virtual void Awake()
        {
            IsDestroyed = false;
        }

        public abstract void Reset();
        
        protected virtual void Remove() {}
        
        public virtual void Release()
        {
            if (Pool != null)
                Pool.Release(gameObject);
            else
                Destroy(gameObject);
        }
        
        // 동기화에 필요한 정보를 직렬화
        public virtual object[] SerializeSyncData()
        {
            // 월드 좌표 기준으로 위치 전송
            return new object[]
            {
                transform.position,
                transform.rotation,
                transform.localScale
            };
        }

        // 직렬화된 정보를 바탕으로 엔티티 상태 업데이트
        public virtual void DeserializeSyncData(object[] data)
        {
            if (data.Length >= 3)
            {
                Vector3 pos = (Vector3)data[0];
                pos.y += offSet;
                
                transform.position = pos;
                transform.rotation = (Quaternion)data[1];
                transform.localScale = (Vector3)data[2];
            }
        }
    }
}

