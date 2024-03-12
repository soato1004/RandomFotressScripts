using System.Collections.Generic;
using RandomFortress.Common;
using RandomFortress.Common.Utils;
using RandomFortress.Game;
using UnityEngine;
using UnityEngine.Pool;

namespace RandomFortress.Manager
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        // 생성할 오브젝트의 key값지정을 위한 변수
        private string objectName;

        // 오브젝트풀들을 관리할 딕셔너리
        private Dictionary<string, IObjectPool<GameObject>> ojbectPoolDic = new Dictionary<string, IObjectPool<GameObject>>();

        // 오브젝트풀에서 오브젝트를 새로 생성할때 사용할 딕셔너리
        private Dictionary<string, GameObject> goDic = new Dictionary<string, GameObject>();

        public override void Reset()
        {
            JTDebug.LogColor("GameUIManager Reset");
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("GameUIManager Terminate");
            Destroy(Instance);
        }

        public void InitObjectPool(GameObject go, string objectName, int count = 10)
        {
            // 등록체크
            if (goDic.ContainsKey(objectName))
            {
                Debug.LogFormat("{0} 이미 등록된 오브젝트입니다.", objectName);
                return;
            }

            // 오브젝트 풀 생성 시, 각 오브젝트 이름에 대응하는 람다 함수를 사용
            IObjectPool<GameObject> pool = new ObjectPool<GameObject>(
                () => CreatePooledItem(objectName), // 여기서 objectName을 직접 참조
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                true, count, count);
    
            goDic.Add(objectName, go);
            ojbectPoolDic.Add(objectName, pool);
        }

        // 생성 함수에 objectName 매개변수 추가
        private GameObject CreatePooledItem(string objectName)
        {
            GameObject poolGo = Instantiate(goDic[objectName]);
            poolGo.GetComponent<EntityBase>().Pool = ojbectPoolDic[objectName];
            return poolGo;
        }

        // 대여
        private void OnTakeFromPool(GameObject poolGo)
        {
            poolGo.SetActive(true);
        }

        // 반환
        private void OnReturnedToPool(GameObject poolGo)
        {
            poolGo.SetActive(false);
        }

        // 삭제
        private void OnDestroyPoolObject(GameObject poolGo)
        {
            Destroy(poolGo);
        }
        
        public bool ContantKey(string goName)
        {
            return goDic.ContainsKey(goName);
        }

        public GameObject Get(string goName)
        {
            if (goDic.ContainsKey(goName) == false)
            {
                GameObject go =  ResourceManager.Instance.GetPrefab(goName);
                if (go == null)
                {
                    return null;
                }
                
                InitObjectPool(go, goName);
            }

            return ojbectPoolDic[goName].Get();
        }
    }
}