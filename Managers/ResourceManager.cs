using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using RandomFortress.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.U2D;


namespace RandomFortress
{
    // 게임 내에서 사용되는 리소스
    public class ResourceManager : Singleton<ResourceManager>
    {
        public bool isResourcesLoaded = false;
        
        public SerializedDictionary<string, GameObject> PrefabDic = new SerializedDictionary<string, GameObject>();
        public SerializedDictionary<string, AudioClip> SoundDic = new SerializedDictionary<string, AudioClip>();
        public SerializedDictionary<string, SpriteAtlas> SpriteAtlasDic = new SerializedDictionary<string, SpriteAtlas>();
        public SerializedDictionary<string, Material> MaterialDic = new SerializedDictionary<string, Material>();

        private IList<IResourceLocation> _locations;
        
        public override void Reset()
        {
            JustDebug.LogColor("ResourceManager Reset");
        }

        #region Load

        // public IEnumerator LoadAssetsBundlesCor(string assetBundleName)
        // {
        //     // PlayAssetBundleRequest bundleRequest = PlayAssetDelivery.RetrieveAssetBundleAsync(assetBundleName);
        //     //
        //     // while (!bundleRequest.IsDone) {
        //     //     if(bundleRequest.Status == AssetDeliveryStatus.WaitingForWifi) {
        //     //         var userConfirmationOperation = PlayAssetDelivery.ShowCellularDataConfirmation();
        //     //
        //     //         // Wait for confirmation dialog action.
        //     //         yield return userConfirmationOperation;
        //     //
        //     //         if((userConfirmationOperation.Error != AssetDeliveryErrorCode.NoError) ||
        //     //            (userConfirmationOperation.GetResult() != ConfirmationDialogResult.Accepted)) {
        //     //             // The user did not accept the confirmation - handle as needed.
        //     //         }
        //     //
        //     //         // Wait for Wi-Fi connection OR confirmation dialog acceptance before moving on.
        //     //         yield return new WaitUntil(() => bundleRequest.Status != AssetDeliveryStatus.WaitingForWifi);
        //     //     }
        //     //
        //     //     // Use bundleRequest.DownloadProgress to track download progress.
        //     //     // Use bundleRequest.Status to track the status of request.
        //     //
        //     //     yield return null;
        //     // }
        //     //
        //     // if (bundleRequest.Error != AssetDeliveryErrorCode.NoError) {
        //     //     // There was an error retrieving the bundle. For error codes NetworkError
        //     //     // and InsufficientStorage, you may prompt the user to check their
        //     //     // connection settings or check their storage space, respectively, then
        //     //     // try again.
        //     //     yield return null;
        //     // }
        //     //
        //     // Request was successful. Retrieve AssetBundle from request.AssetBundle.
        //     // AssetBundle assetBundle = bundleRequest.AssetBundle;
        //     
        //     // Loads the AssetBundle from disk, downloading the asset pack containing it if necessary.
        //     PlayAssetBundleRequest bundleRequest = PlayAssetDelivery.RetrieveAssetBundleAsync("soundassets");
        //     AssetBundle assetBundle = bundleRequest.AssetBundle;
        //     yield return null;
        // }
        
        // 인게임 시작시
        public async UniTask LoadAllResourcesAsync(string label = "InGame")
        {
            // 그외 모든 리소스 로드
            AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(label);
            await handle.Task;
            
            Debug.Log("리소스 갯수 : " + handle.Result.Count);
            
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
                        await LoadResourceAsync<AudioClip>(location, SoundDic);
                    }
                    else if (location.ResourceType == typeof(Material))
                    {
                        await LoadResourceAsync<Material>(location, MaterialDic);
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
            
            isResourcesLoaded = true;
            JustDebug.LogColor("Resource Load Complete", "green");
        }
        
        private async Task LoadResourceAsync<T>(IResourceLocation location, SerializedDictionary<string, T> SerializedDictionary) where T : UnityEngine.Object
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(location);
            await handle.Task;
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                string fullPath = location.PrimaryKey;
                string fileNmae = Path.GetFileNameWithoutExtension(fullPath);
                SerializedDictionary[fileNmae] = handle.Result;
                JustDebug.Log(fullPath);
            }
            else
            {
                JustDebug.LogError("Failed to load " + typeof(T).Name + " at " + location.PrimaryKey);
            }
        }

        #endregion

        #region Get

        public GameObject GetMonster(int index)
        {
            if (DataManager.Instance.monsterStateDic.ContainsKey(index))
            {
                string key = DataManager.Instance.monsterStateDic[index].prefabName;
                if (PrefabDic.ContainsKey(key))
                {
                    return PrefabDic[key];   
                }
                
                JustDebug.LogError("PrefabDic "+key+" Not Found");
                return null;
            }
            
            JustDebug.LogError("Monster "+index+" Not Found");
            return null;
        }

        public string GetMonsterPrefabName(int index)
        {
            if (DataManager.Instance.monsterStateDic.ContainsKey(index))
            {
                return DataManager.Instance.monsterStateDic[index].prefabName;
            }
            
            JustDebug.LogError("Monster "+index+" Not Found");
            return "";
        }

        public GameObject GetPrefab(string name)
        {
            if (PrefabDic.ContainsKey(name))
            {
                return PrefabDic[name];
            }
            
            JustDebug.LogError("Prefab "+name+" Not Found");
            return null;
        }
        
        public Sprite GetTower(int towerIndex, int tier)
        {
            TowerData target = DataManager.Instance.towerDataDic[towerIndex];
            string imgName = target.towerName + "_1"; //+ tier;
        
            return GetSprite(imgName);
        } 

        public Sprite GetBg(string imgName)
        {
            return SpriteAtlasDic["Bg_Atlas"].GetSprite(imgName);
        }
        
        public Sprite GetSprite(string imgName)
        {
            Sprite sprite = SpriteAtlasDic["Common_Atlas"].GetSprite(imgName);
            if (sprite == null)
                Debug.Log("sprite : " + imgName + ", NULL!!");
            return sprite;
        }
        
        public Material GetMaterial(string materialName)
        {
            if (MaterialDic.ContainsKey(materialName))
            {
                return MaterialDic[materialName];
            }
            
            JustDebug.LogError("Material "+name+" Not Found");
            return null;
        }
        
        // ------- Resources 폴더 말고 다른 폴더에서 데이터 불러오기 -------- 
        // public static List<T> FindAssets<T>(params string[] paths) where T : UnityEngine.Object
        // {
        //     string[] assetGUIDs = AssetDatabase.FindAssets("t:" + typeof(T), paths);
        //     List<T> assets = new List<T>();
        //     foreach (string guid in assetGUIDs)
        //     {
        //         string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        //         T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        //         assets.Add(asset);
        //     }
        //     return assets;
        // }

        #endregion
    }
}