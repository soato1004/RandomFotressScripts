using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;

namespace RandomFortress
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        // 오브젝트풀들을 관리할 딕셔너리
        [SerializeField] private SerializedDictionary<string, IObjectPool<GameObject>> objectPoolDic = new SerializedDictionary<string, IObjectPool<GameObject>>();

        // 오브젝트풀에서 오브젝트를 새로 생성할때 사용할 딕셔너리
        [SerializeField] private SerializedDictionary<string, GameObject> goDic = new SerializedDictionary<string, GameObject>();

        private const int PoolingCount = 15;
        
        public void Init()
        {
            objectPoolDic.Clear();
            goDic.Clear();
        }

        public void InitObjectPool(GameObject go, string objectName, int count = PoolingCount)
        {
            // 등록체크
            if (goDic.ContainsKey(objectName))
            {
                Debug.LogFormat("{0} 이미 등록된 오브젝트입니다.", objectName);
                return;
            }

            // 오브젝트 풀 생성 시, 각 오브젝트 이름에 대응하는 람다 함수를 사용
            IObjectPool<GameObject> pool = new UnityEngine.Pool.ObjectPool<GameObject>(
                () => CreatePooledItem(objectName), // 여기서 objectName을 직접 참조
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                true, count, count);
    
            goDic.Add(objectName, go);
            objectPoolDic.Add(objectName, pool);
        }

        // 생성 함수에 objectName 매개변수 추가
        private GameObject CreatePooledItem(string objectName)
        {
            GameObject poolGo = Instantiate(goDic[objectName]);
            poolGo.GetComponent<EntityBase>().Pool = objectPoolDic[objectName];
            return poolGo;
        }

        // 대여
        private void OnTakeFromPool(GameObject poolGo)
        {
            if (poolGo == null)
            {
                Debug.LogWarning("OnTakeFromPool: poolGo is null");
                return;
            }
            // poolGo.SetActive(true);
        }

        // 반환
        private void OnReturnedToPool(GameObject poolGo)
        {
            if (poolGo == null)
            {
                Debug.LogWarning("OnReturnedToPool: poolGo is null");
                return;
            }
            poolGo.SetActive(false);
        }

        // 삭제
        private void OnDestroyPoolObject(GameObject poolGo)
        {
            if (poolGo != null)
            {
                Destroy(poolGo);
            }
        }

        public GameObject Get(string goName)
        {
            if (!goDic.ContainsKey(goName))
            {
                GameObject go = ResourceManager.I.GetPrefab(goName);
                InitObjectPool(go, goName);
            }

            return objectPoolDic[goName].Get();
        }
    }
}
