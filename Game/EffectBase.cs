using UnityEngine;
using UnityEngine.Pool;

namespace RandomFortress.Game
{
    public class EffectBase : MonoBehaviour
    {
                
        protected IObjectPool<GameObject> Pool;
        public void SetPool(IObjectPool<GameObject> pool) => Pool = pool;
    }
}