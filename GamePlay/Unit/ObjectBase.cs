using UnityEngine;
using UnityEngine.Pool;

namespace RandomFortress.Game
{
    public class ObjectBase : MonoBehaviour
    {
        protected bool isPooling = false;
        public IObjectPool<GameObject> pool;
        public void SetPool(IObjectPool<GameObject> value)
        {
            isPooling = true;
            pool = value;
        }
        
        protected virtual void Remove() {}
    }
}