using RandomFortress.Common.Utils;
using RandomFortress.Data;
using RandomFortress.Game;
using UnityEngine;

namespace RandomFortress.Manager
{
    public class SpawnManager : Common.Singleton<SpawnManager>
    {
        public override void Reset()
        {
            JTDebug.LogColor("SpawnManager Reset");
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("SpawnManager Terminate");
            Destroy(Instance);
        }

        public GameObject GetFloatingText(Vector3 pos)
        {
            GameObject go = Instantiate(ResourceManager.Instance.GetPrefab("FloatingText"),
                GameUIManager.Instance.uICanvas[1].transform);
            go.transform.position = pos;
            go.name = "FloatingText";
            return go;
        }
        
        public HpBar GetHpBar(Transform parent)
        {
            GameObject go = Instantiate(ResourceManager.Instance.GetPrefab("HpBar"), 
                GameUIManager.Instance.uICanvas[1].transform);
            go.transform.position = Vector3.zero;
            go.name = "HpBar";
            
            return go.GetComponent<HpBar>();
        }
        
        public Transform SpawnMonsterTypeEff(string prefabName, Transform target)
        {
            GameObject go = Instantiate(ResourceManager.Instance.GetPrefab(prefabName), 
                GameManager.Instance.gameMode.uiParent);
            go.transform.position = Vector3.zero;
            go.name = "Speed";
            FollowObject followObject = go.AddComponent<FollowObject>();
            followObject.SetTarget(target);
            
            return go.GetComponent<Transform>();
        }
        
        public GameObject GetBullet(Vector3 pos, int bulletIndex)
        {
           //TODO: 총알부분 제대로 구현필요
            if (DataManager.Instance.bulletDataDic.ContainsKey(bulletIndex) == false)
            {
                bulletIndex = 0;
            }

            // 총알 데이터
            BulletData data = DataManager.Instance.bulletDataDic[bulletIndex];
            
            // 총알 감싸는 오브젝트
            GameObject go = ObjectPoolManager.Instance.Get(data.prefabName);
            go.transform.SetParent(GameManager.Instance.gameMode.bulletParent);
            
            go.transform.position = pos;
            return go;
        }
        
        public GameObject GetTower(Vector3 pos, int towerIndex = -1)
        {
            TowerData data = DataManager.Instance.GetTowerData(towerIndex);
            string prefabName = data.name;

            GameObject go = Instantiate(ResourceManager.Instance.GetPrefab(prefabName),
                GameManager.Instance.myPlayer.towerParent);
            go.name = prefabName;
            go.transform.position = pos;
            
            return go;
        }
        
        public GameObject GetEffect(string prefabName, Vector3 pos)
        {
            // GameObject go = ObjectPoolManager.Instance.Get(prefabName);
            // go.transform.SetParent(GameManager.Instance.gameMode.effectParent);
            
            GameObject go = Instantiate(ResourceManager.Instance.GetPrefab(prefabName), 
                GameManager.Instance.gameMode.effectParent);
            go.name = prefabName;
            go.transform.position = pos;
            
            return go;
        }
        
        public GameObject GetMonster(Vector3 pos, int index)
        {
            GameObject go = Instantiate(ResourceManager.Instance.GetMonster(index),
                GameManager.Instance.gameMode.monsterParent);
            go.name = "Monster "+index;
            go.transform.position = pos;
            
            return go;
        }
    }
}