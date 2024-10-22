using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.U2D;

namespace RandomFortress
{
    // 게임 내에서 사용되는 리소스를 관리하는 클래스
    public class ResourceManager : Singleton<ResourceManager>
    {
        public SerializedDictionary<string, GameObject> prefabCache;
        public SerializedDictionary<string, SpriteAtlas> spriteAtlasCache;
        public SerializedDictionary<string, Material> materialCache;

        private IList<IResourceLocation> _locations;
        
        // 로그 시스템
        private ILogger logger;

        private void Awake()
        {
            logger = Debug.unityLogger;
            prefabCache = new ();
            spriteAtlasCache = new ();
            materialCache = new ();
        }

        // 리소스 매니저 초기화
        public async UniTask InitializeAsync()
        {
            // logger.Log("ResourceManager", "Initializing ResourceManager");
            await LoadAllResourcesAsync();
        }

        // 모든 리소스 로드
        public async UniTask LoadAllResourcesAsync(string label = "InGame")
        {
            // logger.Log("ResourceManager", $"Loading all resources with label: {label}");

            try
            {
                AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(label);
                await handle.Task;
                
                logger.Log("ResourceManager", $"리소스 갯수 : {handle.Result.Count}");
                
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    foreach (IResourceLocation location in handle.Result)
                    {
                        if (location.ResourceType == typeof(SpriteAtlas))
                            await LoadResourceAsync<SpriteAtlas>(location, spriteAtlasCache);
                        else if (location.ResourceType == typeof(GameObject))
                            await LoadResourceAsync<GameObject>(location, prefabCache);
                        else if (location.ResourceType == typeof(AudioClip))
                            await LoadResourceAsync<AudioClip>(location, null);
                        else if (location.ResourceType == typeof(Material))
                            await LoadResourceAsync<Material>(location, materialCache);
                    }
                }
                else
                {
                    logger.LogError("ResourceManager", $"Failed to load resource locations: {label}");
                }
            }
            catch (Exception e)
            {
                logger.LogError("ResourceManager", $"Error loading resources: {e.Message}");
                throw;
            }

            logger.Log("ResourceManager", "Resource Load Complete");
            LoginManager.I.isLoadedResources = true;
        }

        // 개별 리소스 비동기 로드
        private async UniTask LoadResourceAsync<T>(IResourceLocation location, SerializedDictionary<string, T> serializedDictionary) where T : UnityEngine.Object
        {
            try
            {
                AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(location);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    string fullPath = location.PrimaryKey;
                    string fileName = Path.GetFileNameWithoutExtension(fullPath);
                
                    if (typeof(T) == typeof(AudioClip))
                    {
                        ProcessAudioClip(location, handle.Result as AudioClip);
                    }
                    else
                    {
                        serializedDictionary[fileName] = handle.Result;
                    }
                
                    // logger.Log("ResourceManager", $"Loaded {typeof(T).Name}: {fullPath}");
                }
                else
                {
                    logger.LogError("ResourceManager", $"Failed to load {typeof(T).Name} at {location.PrimaryKey}");
                }
            }
            catch (Exception e)
            {
                logger.LogError("ResourceManager", $"Error loading {typeof(T).Name} at {location.PrimaryKey}: {e.Message}");
                throw;
            }
        }

        // AudioClip 처리 함수
        private void ProcessAudioClip(IResourceLocation location, AudioClip audioClip)
        {
            if (audioClip == null)
            {
                logger.LogError("ResourceManager", $"Failed to load AudioClip at {location.PrimaryKey}");
                return;
            }

            string fullPath = location.PrimaryKey;
            string fileName = Path.GetFileNameWithoutExtension(fullPath);

            // soundNameDic에서 key 찾기
            string soundKey = SoundManager.I.audioFileNames.FirstOrDefault(x => x.Value == fileName).Key;

            if (string.IsNullOrEmpty(soundKey))
            {
                logger.LogWarning("ResourceManager", $"No matching key found in soundNameDic for file: {fileName}");
                return;
            }

            // SoundKey enum으로 변환
            if (Enum.TryParse<SoundKey>(soundKey, out SoundKey enumKey))
            {
                SoundManager.I.audioClips[enumKey] = audioClip;
                // logger.Log("ResourceManager", $"Audio clip added to SoundManager: {enumKey}");
            }
            else
            {
                logger.LogWarning("ResourceManager", $"Failed to parse SoundKey enum for key: {soundKey}");
            }
        }

        // 프리팹 가져오기
        public GameObject GetPrefab(string name)
        {
            if (prefabCache.TryGetValue(name, out GameObject prefab))
                return prefab;

            logger.LogWarning("ResourceManager", $"Prefab not found: {name}");
            return null;
        }

        // 스프라이트 가져오기 (기본 아틀라스: Common_Atlas)
        public Sprite GetSprite(string imgName, string atlasName = "Common_Atlas")
        {
            if (spriteAtlasCache.TryGetValue(atlasName, out SpriteAtlas atlas))
            {
                Sprite sprite = atlas.GetSprite(imgName);
                if (sprite != null)
                    return sprite;
            }

            logger.LogWarning("ResourceManager", $"Sprite not found: {atlasName}/{imgName}");
            return null;
        }

        // 재질 가져오기
        public Material GetMaterial(string name)
        {
            if (materialCache.TryGetValue(name, out Material material))
                return material;

            logger.LogWarning("ResourceManager", $"Material not found: {name}");
            return null;
        }

        // 몬스터 프리팹 가져오기
        public GameObject GetMonster(int index)
        {
            if (DataManager.I.monsterDataDic.TryGetValue(index, out MonsterData monsterData))
            {
                return GetPrefab(monsterData.prefabName);
            }

            logger.LogError("ResourceManager", $"Monster data not found for index: {index}");
            return null;
        }

        // 몬스터 프리팹 이름 가져오기
        public string GetMonsterPrefabName(int index)
        {
            if (DataManager.I.monsterDataDic.TryGetValue(index, out MonsterData monsterData))
            {
                return monsterData.prefabName;
            }
            
            logger.LogError("ResourceManager", $"Monster {index} Not Found");
            return "";
        }

        // 타워 스프라이트 가져오기
        public Sprite GetTower(int towerIndex, int tier)
        {
            if (DataManager.I.towerDataDic.TryGetValue(towerIndex, out TowerData towerData))
            {
                string imgName = $"{towerData.towerName}_1"; // + tier;
                return GetSprite(imgName);
            }

            logger.LogError("ResourceManager", $"Tower data not found for index: {towerIndex}");
            return null;
        }

        // 배경 스프라이트 가져오기
        public Sprite GetBg(string imgName)
        {
            return GetSprite(imgName, "Bg_Atlas");
        }

        // 리소스 해제
        public void ReleaseResource<T>(string key) where T : UnityEngine.Object
        {
            SerializedDictionary<string, T> cache = GetCacheForType<T>();
            if (cache.TryGetValue(key, out T resource))
            {
                Addressables.Release(resource);
                cache.Remove(key);
                logger.Log("ResourceManager", $"Released resource: {typeof(T).Name}/{key}");
            }
        }

        // 모든 리소스 해제
        public void ReleaseAllResources()
        {
            ReleaseAllFromCache(prefabCache);
            ReleaseAllFromCache(spriteAtlasCache);
            ReleaseAllFromCache(materialCache);

            // logger.Log("ResourceManager", "All resources released");
        }

        private void ReleaseAllFromCache<T>(SerializedDictionary<string, T> cache) where T : UnityEngine.Object
        {
            foreach (var resource in cache.Values)
            {
                Addressables.Release(resource);
            }
            cache.Clear();
        }

        private SerializedDictionary<string, T> GetCacheForType<T>() where T : UnityEngine.Object
        {
            if (typeof(T) == typeof(GameObject)) return prefabCache as SerializedDictionary<string, T>;
            if (typeof(T) == typeof(SpriteAtlas)) return spriteAtlasCache as SerializedDictionary<string, T>;
            if (typeof(T) == typeof(Material)) return materialCache as SerializedDictionary<string, T>;

            throw new ArgumentException($"Unsupported type: {typeof(T)}");
        }
    }
}