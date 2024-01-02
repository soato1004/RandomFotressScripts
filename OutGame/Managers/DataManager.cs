using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GamePlay.Constants;
using RandomFortress.Common.Utils;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Game;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace RandomFortress.Manager
{
    // 게임 내에서 사용되는 데이터
    [ExecuteInEditMode]
    public class DataManager : Common.Singleton<DataManager>
    {
        // public MonsterData monsterData = new MonsterData();
        public StageData[] stageDatas;
        public PlayerData playerData;
        public Dictionary<int, TowerData> TowerDataDic = new Dictionary<int, TowerData>();
        public Dictionary<int, MonsterData> MonsterStateDic = new Dictionary<int, MonsterData>();
        public Dictionary<int, SkillData> SkillDataDic = new Dictionary<int, SkillData>();
        public Dictionary<int, BulletData> BulletDataDic = new Dictionary<int, BulletData>();

        public override void Reset()
        {
            JTDebug.LogColor("DataManager Reset");
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("DataManager Terminate");
        }
        
        public void SavePlayerData()
        {
            
        }
        
        enum Lv { Low, Mid, High, Master, King } ;

        // 인게임 시작시
        public async UniTask LoadInfoAsync()
        {
            await LoadAllResourcesAsync();
            
            await LoadStageDataAsync();

            await LoadTowerDataAsync();
            
            await Task.CompletedTask;
            
            JTDebug.LogColor("Data Load Complete", "green");
        }
        
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
                // else if (typeof(T) == typeof(StageData))
                // {
                //     stageData = handle.Result as StageData;
                // }
                // Debug.Log(fullPath);
            }
            else
            {
                Debug.LogError("Failed to load " + typeof(T).Name + " at " + location.PrimaryKey);
            }
        }

        public async UniTask LoadStageDataAsync()
        {
            List<int> stageData = CSVParser.ParseStageCSV(FileFath.StageDataPath);
            
            int length = GameConstants.TotalStages;
            stageDatas = new StageData[length];
            for (int i = 0; i < length; ++i)
            {
                StageData stage = ScriptableObject.CreateInstance<StageData>();
                stage.startDelayTime = 30;
                stage.appearMonster = Enumerable.Repeat((i%5)+3, 20).ToArray();
                stage.hp = stageData[i];
                stage.appearDelay = 10;
                stage.stageReward = 100 + (10 * i);
                stageDatas[i] = stage;
            }
        }
        
        public async UniTask LoadTowerDataAsync()
        {
            List<TowerData> towerDataList = CSVParser.ParseTowerCSV(FileFath.TowerDataPath);
            foreach (TowerData towerData in towerDataList)
            {
                TowerDataDic[towerData.index + (towerData.tier-1)*10 ] = towerData;
            }
        }
        
        // public void LoadPlayerDataAsync()
        // {
        //     PlayerPrefs.SetInt("PlayerHealth", playerHealth);
        //     PlayerPrefs.SetInt("PlayerMoney", playerMoney);
        //     PlayerPrefs.Save();
        // }
        //
        // public void LoadPlayerData()
        // {
        //     // 저장된 데이터가 없으면 기본값 사용
        //     playerHealth = PlayerPrefs.GetInt("PlayerHealth", 100);
        //     playerMoney = PlayerPrefs.GetInt("PlayerMoney", 0);
        // }
    }
}