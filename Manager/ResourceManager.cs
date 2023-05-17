using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using RandomFortress.Common;
using RandomFortress.Common.Util;
using RandomFortress.Data;
using RandomFortress.Game;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Serialization;
using UnityEngine.U2D;
using Object = UnityEngine.Object;
using Task = System.Threading.Tasks.Task;

namespace RandomFortress.Manager
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        public Dictionary<string, GameObject> PrefabDic = new Dictionary<string, GameObject>();
        public Dictionary<string, AudioClip> SoundDic = new Dictionary<string, AudioClip>();
        public Dictionary<string, SpriteAtlas> SpriteAtlasDic = new Dictionary<string, SpriteAtlas>();
        // public Dictionary<string, ParticleSystem> EffectDic = new Dictionary<string, ParticleSystem>();
        // public Dictionary<string, Asset> AssetDic = new Dictionary<string, Asset>();

        //단일 에셋
        // Addressables.LoadAssetAsync<TObject>('key' 또는 'IResourceLocation') : 에셋을 메모리에 로드.
        
        // Addressables.InstantiateAsync('key' 또는 'IResourceLocation') : 에셋을 게임오브젝트로 생성한다.
        
        // Addressables.LoadSceneAsync('key'  또는 'IResourceLocation') : 씬을 로드한다.
        
        // AssetReference.LoadAssetAsync<TObject>() : 에셋 레퍼런스로 에셋을 메모리에 로드한다.
        
        // AssetReference.InstantiateAsync() : 에셋 레퍼런스로 게임오브젝트를 생성한다.
        
        // AssetReference.LoadSceneAsync() : 에셋 레퍼런스로 씬을 로드한다.
        
        // 다수 에셋로드
        // Addressables.LoadAssetsAsync<TObject>('key' 또는 'IResourceLocation', 'Action<TObject> callback')

        private IList<IResourceLocation> _locations;
        
        
        public override void Reset()
        {
            JTDebug.LogColor("ResourceManager Reset");
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("ResourceManager Terminate");
            ScriptableObject testaa = null;
        }

        // 인게임 시작시
        public async UniTask LoadAllResourcesAsync(string label = "InGame")
        {
            // 그외 모든 리소스 로드
            AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(label);
            await handle.Task;

            
            // 로드 성공시 처리하기
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (IResourceLocation location in handle.Result)
                {
                    if (location.ResourceType == typeof(SpriteAtlas))
                    {
                        await LoadResourceAsync<SpriteAtlas>(location, SpriteAtlasDic);
                    }
                    else if (location.ResourceType == typeof(GameObject))
                    {
                        await LoadResourceAsync<GameObject>(location, PrefabDic);
                    }
                    else if (location.ResourceType == typeof(AudioClip))
                    {
                        // await LoadResourceAsync<AudioClip>(location, SoundDic);
                    }
                    else
                    {
                        // Debug.Log("unknow Type " + location.ResourceType + ", "+location.PrimaryKey);
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to load resource locations" + label);
            }

            await Task.CompletedTask;
            Debug.Log("Resource Load Complete");
        }
        
        private async Task LoadResourceAsync<T>(IResourceLocation location, Dictionary<string, T> dictionary) where T : UnityEngine.Object
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(location);
            await handle.Task;
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                string fullPath = location.PrimaryKey;
                string fileNmae = Path.GetFileNameWithoutExtension(fullPath);
                dictionary[fileNmae] = handle.Result;
                Debug.Log(fullPath);
            }
            else
            {
                Debug.LogError("Failed to load " + typeof(T).Name + " at " + location.PrimaryKey);
            }
        }

        public GameObject GetMonster(int i)
        {
            if (DataManager.Instance.MonsterStateDic.ContainsKey(i))
            {
                string key = DataManager.Instance.MonsterStateDic[i].unitName;
                if (PrefabDic.ContainsKey(key))
                {
                    return PrefabDic[key];   
                }
                JTDebug.Log("Monster "+i+" Not Found");
                return null;
            }
            
            JTDebug.Log("ScriptableObject Monster "+i+" Not Found");
            return null;
        }

        public GameObject GetPrefab(string name)
        {
            if (PrefabDic.ContainsKey(name))
            {
                return PrefabDic[name];
            }
            JTDebug.Log("Prefab "+name+" Not Found");
            return null;
        }

        public Sprite GetTower(string imgName) => GetSprite("Tower_Atlas" , imgName);
        
        public Sprite GetBg(string imgName) => GetSprite("BG_Atlas" , imgName);
        
        public Sprite GetSprite(string atlasName, string imgName)
        {
            if (SpriteAtlasDic.ContainsKey(atlasName))
            {
                return SpriteAtlasDic[atlasName].GetSprite(imgName);
            }
            JTDebug.Log("atlasName "+atlasName+" Not Found");
            return null;
        }
    }
}