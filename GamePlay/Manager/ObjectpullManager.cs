using System.Collections.Generic;
using RandomFortress.Common.Extensions;
using RandomFortress.Common.Util;
using RandomFortress.Game;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using UnitBase = RandomFortress.Game.UnitBase;

namespace RandomFortress.Manager
{
    class ObjectPool : MonoBehaviour
    {
        public GameObject prefab;
        private Transform parent;
        
        public IObjectPool<GameObject> Pool { get; private set; }
    
        public void Initialize(GameObject prefab, Transform parent)
        {
            this.prefab = prefab;
            this.parent = parent;
            Pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool,
                OnReturnedToPool, OnDestroyPoolObject);
        }
    
        public GameObject Get() => Pool.Get();
    
        public void Release(GameObject go) => Pool.Release(go);
        
        private GameObject CreatePooledItem()
        {
            GameObject obj = Instantiate(prefab, parent) as GameObject;
            return obj;
        }
        
        private void OnTakeFromPool(GameObject go)
        {
            go.SetActive(true);
        }
        
        private void OnReturnedToPool(GameObject go)
        {
            go.SetActive(false);
        }
        
        private void OnDestroyPoolObject(GameObject go)
        {
            Destroy(go);
        }
    }
    
    public class ObjectPoolManager : Common.Singleton<ObjectPoolManager>
    {
        private Dictionary<int, ObjectPool> _objectPoolDic = new Dictionary<int, ObjectPool>();
        public bool collectionChecks = true;
        public int maxPoolSize = 10;

        public enum ObjectType
        {
            Bullet, Monster, Tower, Effect, MonsterHP, DamegeText
        }
        
        public override void Reset()
        {
            JTDebug.LogColor("ObjectPullManager Reset");
            _objectPoolDic.Clear();
            _objectPoolDic = null;
            _objectPoolDic = new Dictionary<int, ObjectPool>();
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("ObjectPullManager Terminate");
            Destroy(Instance);
        }

        public GameObject GetDamageText(Vector3 pos)
        {
            int key = (int)ObjectType.DamegeText;
            
            if (!_objectPoolDic.ContainsKey(key))
            {
                GameObject goInst = new GameObject();
                goInst.name = "DamageTexts";
                goInst.transform.SetParent(GameManager.Instance.game.uiEffectParent);
                
                ObjectPool pool = goInst.AddComponent<ObjectPool>();
                pool.Initialize(ResourceManager.Instance.GetPrefab("DamageText"), goInst.transform);
                _objectPoolDic[key] = pool;
            }
            
            GameObject go = _objectPoolDic[key].Get();
            go.transform.position = pos;

            DamageText damageText = go.GetComponent<DamageText>();
            damageText.SetPool(_objectPoolDic[key].Pool);
            return go;
        }
        
        public GameObject GetBullet(Vector3 pos, int bulletIndex)
        {
            int key = (int)ObjectType.Bullet * 10000 + bulletIndex;

            if (!_objectPoolDic.ContainsKey(key))
            {
                GameObject goInst = new GameObject();
                goInst.name = "Bullet "+bulletIndex;
                goInst.transform.SetParent(GameManager.Instance.game.bulletParent);
            
                ObjectPool pool = goInst.AddComponent<ObjectPool>();
                pool.Initialize(ResourceManager.Instance.GetPrefab("normalBullet"), goInst.transform);
                _objectPoolDic[key] = pool;
            }
            
            GameObject go = _objectPoolDic[key].Get();
            go.transform.position = pos;
            
            NormalBullet bullet = go.GetComponent<NormalBullet>();
            bullet.SetPool(_objectPoolDic[key].Pool);
            return go;
        }
        
        public GameObject GetTower(Vector3 pos)
        {
            int key = (int)ObjectType.Tower * 10000;
            
            if (!_objectPoolDic.ContainsKey(key))
            {
                GameObject goInst = new GameObject();
                goInst.name = "Tower " + key;
                goInst.transform.SetParent(GameManager.Instance.myPlayer.towerParent);
            
                ObjectPool pool = goInst.AddComponent<ObjectPool>();
                pool.Initialize(ResourceManager.Instance.GetPrefab("Tower"), goInst.transform);
                _objectPoolDic[key] = pool;
            }
            
            GameObject go = _objectPoolDic[key].Get();
            go.transform.position = pos;
            
            TowerBase tower = go.GetComponent<TowerBase>();
            tower.SetPool(_objectPoolDic[key].Pool);
            return go;
        }
        
        public GameObject GetEffect(string text, Vector3 pos, int index)
        {
            int key = (int)ObjectType.Effect * 10000 + index;
            
            if (!_objectPoolDic.ContainsKey(key))
            {
                GameObject goInst = new GameObject();
                goInst.name = "Effect"+index;
                goInst.transform.SetParent(GameManager.Instance.game.effectParent);
            
                ObjectPool pool = goInst.AddComponent<ObjectPool>();
                pool.Initialize(ResourceManager.Instance.GetPrefab(text+index), goInst.transform);
                _objectPoolDic[key] = pool;
            }
            
            GameObject go = _objectPoolDic[key].Get();
            go.transform.position = pos;
        
            ObjectBase effect = go.AddComponent<ObjectBase>();
            effect.SetPool(_objectPoolDic[key].Pool);
            return go;
        }
        
        public GameObject GetMonster(Vector3 pos, int index)
        {
            int key = (int)ObjectType.Monster * 10000 + index;
            
            if (!_objectPoolDic.ContainsKey(key))
            {
                GameObject goInst = new GameObject();
                goInst.name = "Monster "+index;
                goInst.transform.SetParent(GameManager.Instance.game.monsterParent);
        
                ObjectPool pool = goInst.AddComponent<ObjectPool>();
                pool.Initialize(ResourceManager.Instance.GetMonster(index%12), goInst.transform);
                _objectPoolDic[key] = pool;
            }
            
            GameObject go = _objectPoolDic[key].Get();
            go.transform.position = pos;
            
            MonsterBase monster = go.GetComponent<MonsterBase>();
            monster.SetPool(_objectPoolDic[key].Pool);
            return go;
        }
    }
}