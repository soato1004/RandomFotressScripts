using UnityEngine;
using UnityEngine.Pool;

namespace RandomFortress.Game
{
    public abstract class BaseBullet : MonoBehaviour
    {
        public int bulletIndex = 0;
        public float timeScale = 1f;
        public MonsterBase Target = null;
        
        public int damage = 1;
        
        protected IObjectPool<GameObject> Pool;
        public void SetPool(IObjectPool<GameObject> pool) => Pool = pool;
        
        public virtual void Init()
        {
            // 몬스터가 제거될경우 총알의 타겟을 없앤다
            Target.OnUnitDestroy += () =>
            {
                Target = null;
            };
        }
    }
}