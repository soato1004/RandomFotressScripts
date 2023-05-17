using System.Collections.Generic;
using RandomFortress.Common.Extensions;
using RandomFortress.Common.Util;
using RandomFortress.Game;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

        // public ObjectPool(GameObject obj, Transform parent)
        // {
        //     this.parent = parent;
        //     prefab = obj;
        //     Pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool,
        //         OnReturnedToPool, OnDestroyPoolObject);
        // }

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
        public int maxPoolSize = 1;


        public enum ObjectType
        {
            Bullet, Monster, Tower, Effect, MonsterHP, DamegeText
        }
        
        public override void Reset()
        {
            JTDebug.LogColor("ObjectPullManager Reset");
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("ObjectPullManager Terminate");
        }

        public GameObject GetDamageText()
        {
            int key = (int)ObjectType.DamegeText;
            
            if (!_objectPoolDic.ContainsKey(key))
            {
                // ObjectPool pool = new ObjectPool(ResourceManager.Instance.GetPrefab("DamageText"), GameManager.Instance.game.effectParent);
                // _objectPoolDic[key] = pool;
                
                GameObject goInst = new GameObject();
                goInst.name = "DamageText";
                goInst.transform.parent = transform;
                ObjectPool pool = goInst.AddComponent<ObjectPool>();
                pool.Initialize(ResourceManager.Instance.GetPrefab("DamageText"), GameManager.Instance.game.effectParent);
                _objectPoolDic[key] = pool;
            }

            GameObject go = _objectPoolDic[key].Get();
            DamageText damageText = go.GetComponent<DamageText>();
            damageText.SetPool(_objectPoolDic[key].Pool);
            return go;
        }
        
        public HpBar GetHpBar(int index = 0)
        {
            int key = (int)ObjectType.MonsterHP * 10000 + index;
            
            if (!_objectPoolDic.ContainsKey(key))
            {
                GameObject goInst = new GameObject();
                goInst.name = "HpBar";
                goInst.transform.parent = transform;
                ObjectPool pool = goInst.AddComponent<ObjectPool>();
                pool.Initialize(ResourceManager.Instance.GetPrefab("HpBar"), GameManager.Instance.game.hpBarParent);
                _objectPoolDic[key] = pool;
                
                // ObjectPool pool = new ObjectPool(ResourceManager.Instance.GetPrefab("HpBar"), GameManager.Instance.game.hpBarParent);
                // _objectPoolDic[key] = pool;
            }

            GameObject go = _objectPoolDic[key].Get();
            HpBar hpBar = go.GetComponent<HpBar>();
            hpBar.SetPool(_objectPoolDic[key].Pool);
            return hpBar;
        }
        
        public GameObject GetBullet(int index = 0)
        {
            int key = (int)ObjectType.Bullet * 10000 + index;
            
            if (!_objectPoolDic.ContainsKey(key))
            {
                // TODO: 총알 종류 다르게
                // GameObject prefab = ResourceManager.Instance.GetPrefab("normalBullet");
                // ObjectPool pool = new ObjectPool(prefab, GameManager.Instance.game.bulletParent);
                // _objectPoolDic[key] = pool;
                
                GameObject goInst = new GameObject();
                goInst.name = "Bullet";
                goInst.transform.parent = transform;
                ObjectPool pool = goInst.AddComponent<ObjectPool>();
                pool.Initialize(ResourceManager.Instance.GetPrefab("normalBullet"), GameManager.Instance.game.bulletParent);
                _objectPoolDic[key] = pool;
            }

            GameObject go = _objectPoolDic[key].Get();
            
            // TODO: 임시
            go.GetComponent<SortingGroup>().sortingOrder = 50;
            
            //
            NormalBullet bullet = go.GetComponent<NormalBullet>();
            bullet.SetPool(_objectPoolDic[key].Pool);
            bullet.bulletIndex = index;
            
            return go;
        }
        
        public GameObject GetEffect(string text, int index = 0)
        {
            int key = (int)ObjectType.Effect * 10000 + index;
            
            if (!_objectPoolDic.ContainsKey(key))
            {
                // GameObject prefab = ResourceManager.Instance.PrefabDic[text+index].gameObject;
                // ObjectPool pool = new ObjectPool(prefab, GameManager.Instance.game.effectParent);
                // _objectPoolDic[key] = pool;
                
                GameObject goInst = new GameObject();
                goInst.name = "Effect";
                goInst.transform.parent = transform;
                ObjectPool pool = goInst.AddComponent<ObjectPool>();
                pool.Initialize(ResourceManager.Instance.GetPrefab(text+index), GameManager.Instance.game.effectParent);
                _objectPoolDic[key] = pool;
            }

            GameObject go = _objectPoolDic[key].Get();
            go.transform.SetGlobalScale(10f);
            
            SortingGroup order = go.GetComponent<SortingGroup>();
            order.sortingOrder = 50;
            
            EffectBase effect = go.AddComponent<EffectBase>();
            effect.SetPool(_objectPoolDic[key].Pool);
            return go;
        }
        
        public GameObject GetMonster(ObjectType type, int index = 0)
        {
            int key = (int)type * 10000 + index;
            
            if (!_objectPoolDic.ContainsKey(key))
            {
                // GameObject prefab = ResourceManager.Instance.GetMonster(index%12);
                // ObjectPool pool = new ObjectPool(prefab, GameManager.Instance.game.monsterParent);
                // _objectPoolDic[key] = pool;
                
                GameObject goInst = new GameObject();
                goInst.name = "Monster";
                goInst.transform.parent = transform;
                ObjectPool pool = goInst.AddComponent<ObjectPool>();
                pool.Initialize(ResourceManager.Instance.GetMonster(index%12), GameManager.Instance.game.monsterParent);
                _objectPoolDic[key] = pool;
            }

            GameObject go = _objectPoolDic[key].Get();
            go.ExGetOrAddComponent<Monster>().SetPool(_objectPoolDic[key].Pool);
            return go;
        }
        
        public GameObject GetTower()
        {
            int key = (int)ObjectType.Tower * 10000;
            
            if (!_objectPoolDic.ContainsKey(key))
            {
                // GameObject prefab = ResourceManager.Instance.GetPrefab("Tower");
                // ObjectPool pool = new ObjectPool(prefab, GameManager.Instance.game.towerParent);
                // _objectPoolDic[key] = pool;
                
                GameObject goInst = new GameObject();
                goInst.name = "Tower";
                goInst.transform.parent = transform;
                ObjectPool pool = goInst.AddComponent<ObjectPool>();
                pool.Initialize(ResourceManager.Instance.GetPrefab("Tower"), GameManager.Instance.game.towerParent);
                _objectPoolDic[key] = pool;
            }
            
            GameObject go = _objectPoolDic[key].Get();
            go.GetComponent<UnitBase>().SetPool(_objectPoolDic[key].Pool);
            
            return go;
        }

        public void Clear()
        {
            _objectPoolDic.Clear();
        }
    }
}