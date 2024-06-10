

using RandomFortress.Data;

using UnityEngine;

namespace RandomFortress
{
    public class SpawnManager : Singleton<SpawnManager>
    {
        public Transform bulletParent;
        public Transform monsterParent;
        public Transform effectParent;
        public Transform skillParent;
        public Transform uiParent;
        
        public override void Reset()
        {
            JustDebug.LogColor("SpawnManager Reset");
        }

        public GameObject GetFloatingText(Vector3 pos, int canvasIndex = 2)
        {
            GameObject go = Instantiate(ResourceManager.Instance.GetPrefab("FloatingText"),
                GameUIManager.Instance.uICanvas[canvasIndex].transform);
            go.transform.position = pos;
            go.name = "FloatingText";
            return go;
        }
        
        public HpBar GetHpBar(Vector3 startPos, int canvasIndex = 1)
        {
            string prefabName = GameConstants.PrefabNameHpBar;
            GameObject go = ObjectPoolManager.Instance.Get(prefabName);
            if (go != null)
            {
                go.transform.SetParent(GameUIManager.Instance.uICanvas[canvasIndex].transform);
                go.transform.position = startPos;
                go.name = "HpBar";
                HpBar hp = go.GetComponent<HpBar>();
                return hp;
            }
            
            return null;
        }
        
        public Transform SpawnMonsterTypeEff(string prefabName, Transform target)
        {
            GameObject go = Instantiate(ResourceManager.Instance.GetPrefab(prefabName), uiParent);
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
            go.transform.SetParent(bulletParent);
            
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
            // TODO: 하드코딩
            if (prefabName == "" || prefabName == "0")
                prefabName = "expl_02_01";
            
            // GameObject go = ObjectPoolManager.Instance.Get(prefabName);
            // go.transform.SetParent(GameManager.Instance.gameMode.effectParent);
            
            GameObject go = Instantiate(ResourceManager.Instance.GetPrefab(prefabName), effectParent);
            go.name = prefabName;
            go.transform.position = pos;

            float scale = GameManager.Instance.gameType == GameType.Solo ? 8 : 4;
            go.transform.localScale = new Vector3(scale, scale);
            
            return go;
        }
        
        public GameObject GetMonster(Vector3 pos, int index)
        {
            GameObject go = Instantiate(ResourceManager.Instance.GetMonster(index), monsterParent);
            go.name = "Monster "+index;
            go.transform.position = pos;
            
            return go;
        }
    }
}