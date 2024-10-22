using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;

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

        public GameObject GetFloatingText(Vector3 pos, int canvasIndex = 2)
        {
            GameObject go = ObjectPoolManager.I.Get(GameConstants.PrefabFloatingText);
            go.transform.SetParent(GameUIManager.I.uICanvas[canvasIndex].transform);
            go.transform.position = pos;
            go.name = "FloatingText";
            go.SetActive(true);
            return go;
        }
        
        public HpBar GetHpBar(Vector3 startPos, int canvasIndex = 1)
        {
            GameObject go = ObjectPoolManager.I.Get(GameConstants.PrefabNameHpBar);
            go.transform.SetParent(GameUIManager.I.uICanvas[canvasIndex].transform);
            go.transform.position = startPos;
            go.name = "HpBar";
            go.SetActive(true);
            HpBar hp = go.GetComponent<HpBar>();
            return hp;
        }
        
        public Transform SpawnMonsterTypeEff(string prefabName, Transform target)
        {
            GameObject go = Instantiate(ResourceManager.I.GetPrefab(prefabName), uiParent);
            go.transform.position = Vector3.zero;
            go.name = "Speed";
            FollowObject followObject = go.AddComponent<FollowObject>();
            followObject.SetTarget(target);
            
            return go.GetComponent<Transform>();
        }
        
        public GameObject GetBullet(Vector3 pos, int bulletIndex)
        {
           //TODO: 총알부분 제대로 구현필요
            if (DataManager.I.bulletDataDic.ContainsKey(bulletIndex) == false)
            {
                bulletIndex = 0;
            }

            // 총알 데이터
            BulletData data = DataManager.I.bulletDataDic[bulletIndex];
            
            // 총알 감싸는 오브젝트
            GameObject go = ObjectPoolManager.I.Get(data.prefabName);
            go.transform.SetParent(bulletParent);
            go.transform.position = pos;
            go.SetActive(true);
            return go;
        }
        
        public GameObject GetTower(Vector3 pos, Transform parent,int towerIndex = -1)
        {
            TowerData data = DataManager.I.GetTowerData(towerIndex);
            string prefabName = data.name;

            GameObject go = Instantiate(ResourceManager.I.GetPrefab(prefabName), parent);
            go.name = prefabName;
            go.transform.position = pos;
            
            return go;
        }


        public GameObject GetBulletEffect(string prefabName, Vector3 pos)
        {
            // TODO: 하드코딩
            if (prefabName == "" || prefabName == "0")
                prefabName = "expl_02_01";
            
            GameObject go = GetEffect(prefabName, pos);
            
            if (GameManager.I.gameType == GameType.Solo)
                go.transform.localScale *= 1.333f;
                
            go.SetActive(true);
            go.GetComponent<EffectBase>().Play();
            
            return go;
        }

        public GameObject GetSkillEffect(string prefabName, Vector3 pos)
        {
            GameObject go = GetEffect(prefabName, pos);
            
            if (GameManager.I.gameType == GameType.Solo)
                go.transform.localScale *= 1.333f;
                
            go.SetActive(true);
            go.GetComponent<EffectBase>().Play();
            
            return go;
        }
        
        public GameObject GetEffect(string prefabName, Vector3 pos)
        {
            GameObject go = ObjectPoolManager.I.Get(prefabName);
            go.transform.SetParent(effectParent);
            go.transform.position = pos;
            go.name = prefabName;
            return go;
        }
        
        public GameObject GetMonster(Vector3 pos, int index)
        {
            GameObject go = Instantiate(ResourceManager.I.GetMonster(index), monsterParent);
            go.name = "Monster "+index;
            go.transform.position = pos;
            
            return go;
        }
    }
}