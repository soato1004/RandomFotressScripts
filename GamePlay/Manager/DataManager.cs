using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using RandomFortress.Common.Util;
using RandomFortress.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.U2D;

namespace RandomFortress.Manager
{
    [ExecuteInEditMode]
    public class DataManager : Common.Singleton<DataManager>
    {
        // public MonsterData monsterData = new MonsterData();
        public StageData stageData;
        public PlayerData playerData;
        public Dictionary<int, TowerData> TowerDataDic = new Dictionary<int, TowerData>();
        public Dictionary<int, MonsterData> MonsterStateDic = new Dictionary<int, MonsterData>();
        public Dictionary<int, SkillData> SkillDataDic = new Dictionary<int, SkillData>();
        public Dictionary<int, BulletData> BulletDataDic = new Dictionary<int, BulletData>();

        
        public Dictionary<string, GameObject> PrefabDic = new Dictionary<string, GameObject>();
        public Dictionary<string, AudioClip> SoundDic = new Dictionary<string, AudioClip>();
        public Dictionary<string, SpriteAtlas> SpriteAtlasDic = new Dictionary<string, SpriteAtlas>();
        
        // private string SAVE_DATA_DIRECTORY;  // 저장할 폴더 경로
        // private string SAVE_FILENAME = "/MonsterData.txt"; // 파일 이름


        public override void Reset()
        {
            JTDebug.LogColor("DataManager Reset");
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("DataManager Terminate");
        }
        
        #region Save

        // TODO: 차후에는 외부에서 불러와야한다
        public void SaveInfo()
        {
            
        }

        #endregion

        #region Load
        
        enum Lv { Low, Mid, High, Master, King } ;

        // 인게임 시작시
        public async UniTask LoadAllResourcesAsync(string label = "GlobalData")
        {
            // 해당 라벨의 모든 리소스 로드
            AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(label);
            await handle.Task;
            
            // 로드 성공시 처리하기
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (IResourceLocation location in handle.Result)
                {
                    if (location.ResourceType == typeof(MonsterData))
                    {
                        await LoadResourceAsync<MonsterData>(location, MonsterStateDic);
                    }
                    else if (location.ResourceType == typeof(TowerData))
                    {
                        await LoadResourceAsync<TowerData>(location, TowerDataDic);
                    }
                    else if (location.ResourceType == typeof(SkillData))
                    {
                        await LoadResourceAsync<SkillData>(location, SkillDataDic);
                    }
                    else if (location.ResourceType == typeof(BulletData))
                    {
                        await LoadResourceAsync<BulletData>(location, BulletDataDic);
                    }
                    else if (location.ResourceType == typeof(PlayerData))
                    {
                        await LoadResourceAsync<PlayerData>(location);
                    }
                    else if (location.ResourceType == typeof(StageData))
                    {
                        await LoadResourceAsync<StageData>(location);
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
        
        private async Task LoadResourceAsync<T>(IResourceLocation location, Dictionary<int, T> dictionary = null) where T : UnityEngine.Object
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(location);
            await handle.Task;
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                string fullPath = location.PrimaryKey;
                string fileNmae = Path.GetFileNameWithoutExtension(fullPath);
                if (typeof(T) == typeof(MonsterData))
                {
                    MonsterData data = handle.Result as MonsterData;
                    dictionary[data.index] = handle.Result;
                }
                else if (typeof(T) == typeof(TowerData))
                {
                    TowerData data = handle.Result as TowerData;
                    dictionary[data.index] = handle.Result;
                }
                else if (typeof(T) == typeof(SkillData))
                {
                    SkillData data = handle.Result as SkillData;
                    dictionary[data.index] = handle.Result;
                }
                else if (typeof(T) == typeof(BulletData))
                {
                    BulletData data = handle.Result as BulletData;
                    dictionary[data.index] = handle.Result;
                }
                else if (typeof(T) == typeof(PlayerData))
                {
                    playerData = handle.Result as PlayerData;
                }
                else if (typeof(T) == typeof(StageData))
                {
                    stageData = handle.Result as StageData;
                }
                // Debug.Log(fullPath);
            }
            else
            {
                Debug.LogError("Failed to load " + typeof(T).Name + " at " + location.PrimaryKey);
            }
        }

        public async UniTask LoadInfo()
        {
            await LoadAllResourcesAsync();

            // 더미 스테이지 데이터
            {
                stageData = ScriptableObject.CreateInstance<StageData>();//new StageData();
                stageData.infos = new StageInfo[100];
                stageData.StartDelayTime = 30;

                for (int i = 0; i < 100; ++i)
                {
                    StageInfo info = new StageInfo();
                    info.appearMonster = Enumerable.Repeat((i%5)+3, 10).ToArray();
                    info.buffHP = 1 + (0.1f * (float)i);
                    info.appearDelay = 5;
                    info.stageReward = 100 + (10 * i);
                    stageData.infos[i] = info;
                }
            }

            await Task.CompletedTask;
            JTDebug.LogColor("Data Load Complete", "green");
        }

        #endregion
    }
}