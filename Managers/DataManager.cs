using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GoogleSheetsToUnity;

using RandomFortress.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace RandomFortress
{
    // 게임 내에서 사용되는 데이터
    public class DataManager : Singleton<DataManager>
    {
        public GameData gameData;
        public StageData stageData;
        public TowerUpgradeData towerUpgradeData;
        public SerializedDictionary<int, TowerData> towerDataDic;
        public SerializedDictionary<int, MonsterData> monsterStateDic;
        public SerializedDictionary<int, SkillData> skillDataDic;
        public SerializedDictionary<int, BulletData> bulletDataDic;
        public SerializedDictionary<int, AbilityData> abilityDataDic;

        public SerializedDictionary<string, string> stringTableDic =
            new SerializedDictionary<string, string>();

        void Start()
        {
            towerDataDic = new SerializedDictionary<int, TowerData>();
            monsterStateDic = new SerializedDictionary<int, MonsterData>();
            skillDataDic = new SerializedDictionary<int, SkillData>();
            bulletDataDic = new SerializedDictionary<int, BulletData>();
            abilityDataDic = new SerializedDictionary<int, AbilityData>();
        }

        public override void Reset()
        {
            JustDebug.LogColor("DataManager Reset");
        }

        #region Addreasable

        // 인게임 시작시
        public async UniTask LoadInfoAsync()
        {
#if UNITY_EDITOR
            // 스크립터블오브젝트로 데이터 로드
            await LoadAllResourcesAsync();

            // 스프레드시트로 데이터 로드
            // await LoadDataFromGoogleSheetAsync();
#elif UNITY_ANDROID
            await LoadAllResourcesAsync();
#endif
            await Task.CompletedTask;

            JustDebug.LogColor("Data Load Complete", "green");
        }

        public async UniTask LoadAllResourcesAsync(string label = "GlobalData")
        {
            // 해당 라벨의 모든 리소스 로드
            AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(label);
            await handle.Task;

            // 로드 성공시 처리하기
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("데이터 갯수 : " + handle.Result.Count);

                foreach (IResourceLocation location in handle.Result)
                {
                    if (location.ResourceType == typeof(MonsterData))
                    {
                        await LoadResourceAsync<MonsterData>(location, monsterStateDic);
                    }
                    else if (location.ResourceType == typeof(TowerData))
                    {
                        await LoadResourceAsync<TowerData>(location, towerDataDic);
                    }
                    else if (location.ResourceType == typeof(SkillData))
                    {
                        await LoadResourceAsync<SkillData>(location, skillDataDic);
                    }
                    else if (location.ResourceType == typeof(BulletData))
                    {
                        await LoadResourceAsync<BulletData>(location, bulletDataDic);
                    }
                    // else if (location.ResourceType == typeof(PlayerData))
                    // {
                    //     await LoadResourceAsync<PlayerData>(location);
                    // }
                    else if (location.ResourceType == typeof(StageData))
                    {
                        await LoadResourceAsync<StageData>(location);
                    }
                    else if (location.ResourceType == typeof(TowerUpgradeData))
                    {
                        await LoadResourceAsync<TowerUpgradeData>(location);
                    }
                    else if (location.ResourceType == typeof(AbilityData))
                    {
                        await LoadResourceAsync<AbilityData>(location, abilityDataDic);
                    }
                    else
                    {
                        Debug.Log("unknow Type " + location.ResourceType + ", " + location.PrimaryKey);
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

        private async Task LoadResourceAsync<T>(IResourceLocation location,
            SerializedDictionary<int, T> SerializedDictionary = null) where T : UnityEngine.Object
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
                    SerializedDictionary[data.index] = handle.Result;
                }
                else if (typeof(T) == typeof(TowerData))
                {
                    TowerData data = handle.Result as TowerData;
                    SerializedDictionary[data.index] = handle.Result;
                }
                else if (typeof(T) == typeof(SkillData))
                {
                    SkillData data = handle.Result as SkillData;
                    SerializedDictionary[data.index] = handle.Result;
                }
                else if (typeof(T) == typeof(BulletData))
                {
                    BulletData data = handle.Result as BulletData;
                    SerializedDictionary[data.index] = handle.Result;
                }
                // else if (typeof(T) == typeof(PlayerData))
                // {
                //     Account.Instance.SetPlayerData(handle.Result as PlayerData);
                // }
                else if (typeof(T) == typeof(StageData))
                {
                    stageData = handle.Result as StageData;
                }
                else if (typeof(T) == typeof(TowerUpgradeData))
                {
                    towerUpgradeData = handle.Result as TowerUpgradeData;
                }
                else if (typeof(T) == typeof(AbilityData))
                {
                    AbilityData data = handle.Result as AbilityData;
                    SerializedDictionary[data.index] = handle.Result;
                }
                else Debug.Log(fullPath);
            }
            else
            {
                Debug.LogError("Failed to load " + typeof(T).Name + " at " + location.PrimaryKey);
            }
        }

        public TowerData GetTowerData(int towerIndex) => towerDataDic[towerIndex];

        //TODO: 계정생성은 서버에서 하고 받는것만 이곳에서..
//         public void SetPlayerData(ref PlayerData data)
//         {
// #if UNITY_EDITOR
//             data = ScriptableObject.CreateInstance<PlayerData>();
//             string path = "Assets/Resources_Addressable/Data/";
//             string fileNmae = "PlayerData";
//             data.Init();
//             AssetDatabase.CreateAsset(data, path + fileNmae + ".asset");
// #else
//             data = new PlayerData();
//             data.Init();
// #endif
//         }

        #endregion

        #region GoogleSheet 연동

        [Serializable]
        public class GoogleSheetInfo
        {
            public string sheetId;
            public string worksheet;

            public GoogleSheetInfo()
            {
            }

            public GoogleSheetInfo(string sheetId, string worksheet)
            {
                this.sheetId = sheetId;
                this.worksheet = worksheet;
            }
        }

        private GoogleSheetInfo[] googleSheetInfos = new GoogleSheetInfo[]
        {
            new GoogleSheetInfo { sheetId = "1ZNrFfLq_-rNeeJtlT00lhEhPdbQWqZyJKL6teFMQC1U", worksheet = "TowerData" },
            new GoogleSheetInfo
                { sheetId = "1ZNrFfLq_-rNeeJtlT00lhEhPdbQWqZyJKL6teFMQC1U", worksheet = "TowerUpgrade" },
            new GoogleSheetInfo { sheetId = "1ZNrFfLq_-rNeeJtlT00lhEhPdbQWqZyJKL6teFMQC1U", worksheet = "TowerCardLV" },
            new GoogleSheetInfo { sheetId = "1ZNrFfLq_-rNeeJtlT00lhEhPdbQWqZyJKL6teFMQC1U", worksheet = "StageData" },
            new GoogleSheetInfo { sheetId = "1ZNrFfLq_-rNeeJtlT00lhEhPdbQWqZyJKL6teFMQC1U", worksheet = "MonsterData" },
            new GoogleSheetInfo { sheetId = "1ZNrFfLq_-rNeeJtlT00lhEhPdbQWqZyJKL6teFMQC1U", worksheet = "BulletData" },
            new GoogleSheetInfo { sheetId = "1ZNrFfLq_-rNeeJtlT00lhEhPdbQWqZyJKL6teFMQC1U", worksheet = "SkillData" },
            new GoogleSheetInfo { sheetId = "1ZNrFfLq_-rNeeJtlT00lhEhPdbQWqZyJKL6teFMQC1U", worksheet = "AbilityData" },
        };

        public enum SheetType
        {
            TowerData,
            TowerUpgrade,
            TowerCardLV,
            StageData,
            MonsterData,
            BulletData,
            SkillData,
            AbilityData
        }

        private int loadCount = 0;

        // TODO: 테스트용
        public async UniTask LoadDataFromTargetSheetAsync(SheetType sheetType)
        {
            GoogleSheetInfo info = googleSheetInfos[(int)sheetType];
            loadCount = 0;

            SpreadsheetManager.Read(new GSTU_Search(info.sheetId, info.worksheet), SetAbilityData);

            // isLoadComplete all true check
            while (loadCount < 1) // 현재 총 4개 데이터 로드중
            {
                await Task.Delay(10);
            }

            await Task.CompletedTask;
        }

        // 스프레드시트 연동
        public async UniTask LoadDataFromGoogleSheetAsync()
        {
            int i = 0, loadCompleteCount = googleSheetInfos.Length;
            loadCount = 0;

            // 타워 데이터
            GoogleSheetInfo info = googleSheetInfos[i++];
            SpreadsheetManager.Read(new GSTU_Search(info.sheetId, info.worksheet), SetTowerData);

            towerUpgradeData = ScriptableObject.CreateInstance<TowerUpgradeData>();

            // 타어 업그레이드
            info = googleSheetInfos[i++];
            SpreadsheetManager.Read(new GSTU_Search(info.sheetId, info.worksheet), SetTowerUpgradeData);

            // 타워카드 업그레이드
            info = googleSheetInfos[i++];
            SpreadsheetManager.Read(new GSTU_Search(info.sheetId, info.worksheet), SetTowerCardData);

            // 스테이지 업데이트
            info = googleSheetInfos[i++];
            SpreadsheetManager.Read(new GSTU_Search(info.sheetId, info.worksheet), SetStageData);

            // 몬스터 데이터
            info = googleSheetInfos[i++];
            SpreadsheetManager.Read(new GSTU_Search(info.sheetId, info.worksheet), SetMonsterData);

            // 총알 데이터
            info = googleSheetInfos[i++];
            SpreadsheetManager.Read(new GSTU_Search(info.sheetId, info.worksheet), SetBulletData);

            // 스킬 데이터
            info = googleSheetInfos[i++];
            SpreadsheetManager.Read(new GSTU_Search(info.sheetId, info.worksheet), SetSkillData);

            // 어빌리티 데이터
            info = googleSheetInfos[i++];
            SpreadsheetManager.Read(new GSTU_Search(info.sheetId, info.worksheet), SetAbilityData);

            // isLoadComplete all true check
            while (loadCount < loadCompleteCount) // 현재 총 4개 데이터 로드중
            {
                await Task.Delay(10);
            }

            string path = "Assets/Resources_Addressable/Data/";
            string fileName = "TowerUpgradeData";
            DeleteFilesInFolder(path, fileName + ".asset");
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(towerUpgradeData, path + fileName + ".asset");
#endif
            await Task.CompletedTask;
        }

        private void SetStageData(GstuSpreadSheet ss)
        {
            bool isFirst = true;

            stageData = ScriptableObject.CreateInstance<StageData>();
            List<StageDataInfo> stageDataList = new List<StageDataInfo>();

            foreach (var row in ss.rows.secondaryKeyLink)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }

                string key = row.Key;

                StageDataInfo stageInfo = new StageDataInfo();
                stageInfo.hp = int.Parse(ss[key, "HP"].value);
                stageInfo.stageReward = int.Parse(ss[key, "Reward"].value);
                stageInfo.appearMonster = int.Parse(ss[key, "AppearMonster"].value);
                stageInfo.monsterCount = int.Parse(ss[key, "MonsterCount"].value);
                stageDataList.Add(stageInfo);
            }

            loadCount++;

            stageData.Infos = stageDataList.ToArray();

            string path = "Assets/Resources_Addressable/Data/";
            string fileName = "StageData";
            DeleteFilesInFolder(path, fileName + ".asset");
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(stageData, path + fileName + ".asset");
#endif
        }

        void SetTowerData(GstuSpreadSheet ss)
        {
            bool isFirst = true;

            string path = "Assets/Resources_Addressable/Data/Tower/";
            DeleteFilesInFolder(path);

            towerDataDic.Clear();

            foreach (var row in ss.rows.secondaryKeyLink)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }

                string key = row.Key;

                TowerInfo towerInfo = new TowerInfo
                {
                    index = int.Parse(ss[key, "index"].value),
                    towerName = ss[key, "name"].value,
                    attackPower = int.Parse(ss[key, "attackPower"].value),
                    attackSpeed = int.Parse(ss[key, "attackSpeed"].value),
                    attackRange = int.Parse(ss[key, "attackRange"].value),
                    attackType = int.Parse(ss[key, "attackType"].value),
                    bulletIndex = int.Parse(ss[key, "bulletIndex"].value),
                    tier = int.Parse(ss[key, "tier"].value),
                    salePrice = int.Parse(ss[key, "SalePrice"].value),
                };

                string json = ss[key, "Extra"].value;
                towerInfo.extraInfo = new ExtraInfo();
                towerInfo.extraInfo = JsonUtility.FromJson<ExtraInfo>(json);


                if (towerDataDic.ContainsKey(towerInfo.index))
                {
                    towerDataDic[towerInfo.index].towerInfoDic.Add(towerInfo.tier, towerInfo);
                }
                else
                {
                    TowerData towerData = ScriptableObject.CreateInstance<TowerData>();
                    towerData.index = towerInfo.index;
                    towerData.towerName = towerInfo.towerName;

                    towerDataDic.Add(towerData.index, towerData);
                    towerDataDic[towerData.index].towerInfoDic.Add(towerInfo.tier, towerInfo);
                }
            }

#if UNITY_EDITOR
            foreach (var towerData in towerDataDic)
                AssetDatabase.CreateAsset(towerData.Value, path + towerData.Value.towerName + ".asset");
#endif

            loadCount++;
        }

        void SetTowerUpgradeData(GstuSpreadSheet ss)
        {
            bool isFirst = true;

            foreach (var row in ss.rows.secondaryKeyLink)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }


                string key = row.Key;

                TowerUpgradeInfo towerUpgradeInfo = new TowerUpgradeInfo();
                towerUpgradeInfo.Data.Add(int.Parse(ss[key, "attackPower"].value));
                towerUpgradeInfo.Data.Add(int.Parse(ss[key, "Extra 1"].value));
                towerUpgradeInfo.Data.Add(int.Parse(ss[key, "Extra 2"].value));
                towerUpgradeInfo.Data.Add(int.Parse(ss[key, "Extra 3"].value));
                towerUpgradeData.UpgradeData.Add(towerUpgradeInfo);
            }

            loadCount++;
        }

        void SetTowerCardData(GstuSpreadSheet ss)
        {
            bool isFirst = true;

            foreach (var row in ss.rows.secondaryKeyLink)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }


                string key = row.Key;

                CardUpgradeInfo cardUpgradeInfo = new CardUpgradeInfo();
                cardUpgradeInfo.NeedCard = int.Parse(ss[key, "NeedCard"].value);
                cardUpgradeInfo.CardLVData.Add(int.Parse(ss[key, "attackPower"].value));
                cardUpgradeInfo.CardLVData.Add(int.Parse(ss[key, "Extra 1"].value));
                cardUpgradeInfo.CardLVData.Add(int.Parse(ss[key, "Extra 2"].value));
                cardUpgradeInfo.CardLVData.Add(int.Parse(ss[key, "Extra 3"].value));
                towerUpgradeData.CardLvData.Add(cardUpgradeInfo);
            }

            loadCount++;
        }

        void SetMonsterData(GstuSpreadSheet ss)
        {
            bool isFirst = true;
            string path = "Assets/Resources_Addressable/Data/Monster/";
            DeleteFilesInFolder(path);
            monsterStateDic.Clear();

            foreach (var row in ss.rows.secondaryKeyLink)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }


                string key = row.Key;

                MonsterData monsterData = ScriptableObject.CreateInstance<MonsterData>();

                monsterData.index = int.Parse(ss[key, "Index"].value);
                monsterData.unitName = ss[key, "Name"].value;
                monsterData.prefabName = ss[key, "Prefab"].value;
                monsterData.moveSpeed = int.Parse(ss[key, "MoveSpeed"].value);
                monsterData.monsterType = (MonsterType)Enum.Parse(typeof(MonsterType), ss[key, "MonsterType"].value);

                monsterStateDic.Add(monsterData.index, monsterData);
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(monsterData, path + monsterData.unitName + ".asset");
#endif

            }

            loadCount++;
        }

        void SetBulletData(GstuSpreadSheet ss)
        {
            bool isFirst = true;
            string path = "Assets/Resources_Addressable/Data/Bullet/";
            DeleteFilesInFolder(path);
            bulletDataDic.Clear();

            foreach (var row in ss.rows.secondaryKeyLink)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }


                string key = row.Key;

                BulletData bulletData = ScriptableObject.CreateInstance<BulletData>();
                bulletData.index = int.Parse(ss[key, "Index"].value);
                bulletData.bulletName = ss[key, "Name"].value;
                bulletData.prefabName = ss[key, "PrefabName"].value;
                bulletData.startEffName = ss[key, "StartEffect"].value;
                bulletData.hitEffName = ss[key, "HitEffect"].value;

                bulletDataDic.Add(bulletData.index, bulletData);
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(bulletData, path + bulletData.bulletName + ".asset");
#endif
            }

            loadCount++;
        }

        void SetSkillData(GstuSpreadSheet ss)
        {
            bool isFirst = true;
            string path = "Assets/Resources_Addressable/Data/Skill/";
            DeleteFilesInFolder(path);
            skillDataDic.Clear();

            foreach (var row in ss.rows.secondaryKeyLink)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }


                string key = row.Key;

                SkillData skillData = ScriptableObject.CreateInstance<SkillData>();
                skillData.index = int.Parse(ss[key, "Index"].value);
                skillData.skillName = ss[key, "Name"].value;
                skillData.coolTime = int.Parse(ss[key, "CoolTime"].value);
                skillData.dynamicData[0] = int.Parse(ss[key, "Extra 1"].value);
                skillData.dynamicData[1] = int.Parse(ss[key, "Extra 2"].value);
                skillData.dynamicData[2] = int.Parse(ss[key, "Extra 3"].value);

                skillDataDic.Add(skillData.index, skillData);
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(skillData, path + skillData.skillName + ".asset");
#endif
            }

            loadCount++;
        }

        void SetAbilityData(GstuSpreadSheet ss)
        {
            bool isFirst = true;
            string path = "Assets/Resources_Addressable/Data/AbilityCard/";
            DeleteFilesInFolder(path);
            abilityDataDic.Clear();

            foreach (var row in ss.rows.secondaryKeyLink)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }

                string key = row.Key;

                AbilityData abilityData = ScriptableObject.CreateInstance<AbilityData>();
                abilityData.index = int.Parse(ss[key, "Index"].value);
                abilityData.abilityName = ss[key, "Name"].value;
                abilityData.iconName = ss[key, "IconName"].value;
                abilityData.rarity = (Rarity)Enum.Parse(typeof(Rarity), ss[key, "Rare"].value);
                abilityData.percent = int.Parse(ss[key, "Percent"].value);
                // abilityData.dynamicData[0] = int.Parse(ss[key, "Extra"].value);
                abilityData.explan[0] = ss[key, "kr"].value;
                abilityData.explan[1] = ss[key, "en"].value;

                string json = ss[key, "Extra"].value;
                abilityData.extraInfo = JsonUtility.FromJson<ExtraInfo>(json);

                abilityDataDic.Add(abilityData.index, abilityData);
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(abilityData, path + abilityData.rarity + "_" + abilityData.abilityName + ".asset");
#endif
            }

            loadCount++;
        }

        public static void DeleteFilesInFolder(string folderPath, string fileName = "")
        {
            // 폴더 내의 모든 파일 경로를 가져옵니다.
            string[] files = Directory.GetFiles(folderPath);

            if (fileName == "")
            {
                foreach (string file in files)
                {
                    try
                    {
                        // 각 파일을 삭제합니다.
                        File.Delete(file);
                        Debug.Log($"Deleted file: {file}");
                    }
                    catch (System.Exception ex)
                    {
                        // 파일 삭제 중 오류가 발생한 경우, 로그를 출력합니다.
                        Debug.LogError($"Error deleting file {file}: {ex.Message}");
                    }
                }
            }
            else
            {
                foreach (string file in files)
                {
                    try
                    {
                        if (file == fileName)
                        {
                            // 파일을 삭제합니다.
                            File.Delete(file);
                            Debug.Log($"Deleted file: {file}");
                            break;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        // 파일 삭제 중 오류가 발생한 경우, 로그를 출력합니다.
                        Debug.LogError($"Error deleting file {file}: {ex.Message}");
                    }
                }
            }
        }

        #endregion
    }
}