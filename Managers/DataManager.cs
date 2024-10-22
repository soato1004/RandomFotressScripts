using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using GoogleSheetsToUnity;

using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Serialization;

namespace RandomFortress
{
    // 게임 내에서 사용되는 데이터를 관리하는 클래스
    public class DataManager : Singleton<DataManager>
    {
        [SerializeField] private bool isNewLoad = false; // 주석: 'shouldLoadFromGoogleSheet'로 이름 변경 권장

        public GameData gameData;
        public StageData stageData;
        public TowerUpgradeData towerUpgradeData;
        public SerializedDictionary<int, TowerData> towerDataDic;
        public SerializedDictionary<int, MonsterData> monsterDataDic;
        public SerializedDictionary<int, SkillData> skillDataDic;
        public SerializedDictionary<int, BulletData> bulletDataDic;
        public SerializedDictionary<int, AbilityData> abilityDataDic;

        public SerializedDictionary<string, string> stringTableDic;

        // 로거 인스턴스 추가
        private ILogger logger;

        void Start()
        {
            // 데이터 딕셔너리 초기화
            InitializeDictionaries();
            logger = Debug.unityLogger;
        }

        // 데이터 딕셔너리 초기화 메서드
        private void InitializeDictionaries()
        {
            towerDataDic = new SerializedDictionary<int, TowerData>();
            monsterDataDic = new SerializedDictionary<int, MonsterData>();
            skillDataDic = new SerializedDictionary<int, SkillData>();
            bulletDataDic = new SerializedDictionary<int, BulletData>();
            abilityDataDic = new SerializedDictionary<int, AbilityData>();
            stringTableDic = new SerializedDictionary<string, string>();
        }

        #region Addressable

        // 인게임 시작시 데이터 로드
        public async UniTask LoadInfoAsync()
        {
#if UNITY_EDITOR
            if (isNewLoad)
                // 스프레드시트에서 데이터 로드
                await LoadDataFromGoogleSheetAsync();
            else
                // 스크립터블 오브젝트에서 데이터 로드
                await LoadAllResourcesAsync();
#else
            await LoadAllResourcesAsync();
#endif
            LoginManager.I.isLoadedData = true;
        }

        // Addressable을 사용하여 모든 리소스 로드
        public async UniTask LoadAllResourcesAsync(string label = "GlobalData")
        {
            // logger.Log("DataManager", $"Loading all resources with label: {label}");

            try
            {
                AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(label);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    logger.Log("DataManager", $"데이터 갯수: {handle.Result.Count}");

                    foreach (IResourceLocation location in handle.Result)
                    {
                        await LoadResourceByType(location);
                    }
                }
                else
                {
                    logger.LogError("DataManager", $"Failed to load resource locations: {label}");
                }
            }
            catch (Exception e)
            {
                logger.LogError("DataManager", $"Error loading resources: {e.Message}");
            }

            logger.Log("DataManager", "Data Load Complete");
        }

        // 리소스 타입에 따라 로드 처리
        private async UniTask LoadResourceByType(IResourceLocation location)
        {
            if (location.ResourceType == typeof(MonsterData))
                await LoadResourceAsync<MonsterData>(location, monsterDataDic);
            else if (location.ResourceType == typeof(TowerData))
                await LoadResourceAsync<TowerData>(location, towerDataDic);
            else if (location.ResourceType == typeof(SkillData))
                await LoadResourceAsync<SkillData>(location, skillDataDic);
            else if (location.ResourceType == typeof(BulletData))
                await LoadResourceAsync<BulletData>(location, bulletDataDic);
            else if (location.ResourceType == typeof(StageData))
                await LoadResourceAsync<StageData>(location);
            else if (location.ResourceType == typeof(TowerUpgradeData))
                await LoadResourceAsync<TowerUpgradeData>(location);
            else if (location.ResourceType == typeof(AbilityData))
                await LoadResourceAsync<AbilityData>(location, abilityDataDic);
            else
                logger.Log("DataManager", $"Unknown Type: {location.ResourceType}, {location.PrimaryKey}");
        }

        // 개별 리소스 비동기 로드
        private async UniTask LoadResourceAsync<T>(IResourceLocation location,
            SerializedDictionary<int, T> serializedDictionary = null) where T : UnityEngine.Object
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(location);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                ProcessLoadedResource(handle.Result, serializedDictionary, location.PrimaryKey);
            }
            else
            {
                logger.LogError("DataManager", $"Failed to load {typeof(T).Name} at {location.PrimaryKey}");
            }
        }

        // 로드된 리소스 처리
        private void ProcessLoadedResource<T>(T resource, SerializedDictionary<int, T> serializedDictionary,
            string primaryKey) where T : UnityEngine.Object
        {
            string fileName = Path.GetFileNameWithoutExtension(primaryKey);

            if (resource is MonsterData monsterData)
                serializedDictionary[monsterData.index] = resource;
            else if (resource is TowerData towerData)
                serializedDictionary[towerData.index] = resource;
            else if (resource is SkillData skillData)
                serializedDictionary[skillData.index] = resource;
            else if (resource is BulletData bulletData)
                serializedDictionary[bulletData.index] = resource;
            else if (resource is StageData stageDataResource)
                stageData = stageDataResource;
            else if (resource is TowerUpgradeData towerUpgradeDataResource)
                towerUpgradeData = towerUpgradeDataResource;
            else if (resource is AbilityData abilityData)
                serializedDictionary[abilityData.index] = resource;
            else
                logger.Log("DataManager", $"Unhandled resource type: {resource.GetType().Name}");

            // logger.Log("DataManager", $"Loaded {resource.GetType().Name}: {fileName}");
        }

        // 타워 데이터 가져오기
        public TowerData GetTowerData(int towerIndex) => towerDataDic[towerIndex];

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

        // 구글 시트 정보 배열
        private GoogleSheetInfo[] googleSheetInfos = new GoogleSheetInfo[]
        {
            new GoogleSheetInfo { sheetId = "1ZNrFfLq_-rNeeJtlT00lhEhPdbQWqZyJKL6teFMQC1U", worksheet = "TowerData" },
            new GoogleSheetInfo { sheetId = "1ZNrFfLq_-rNeeJtlT00lhEhPdbQWqZyJKL6teFMQC1U", worksheet = "TowerUpgrade" },
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

        // 구글 스프레드시트에서 데이터 로드
        public async UniTask LoadDataFromGoogleSheetAsync()
        {
            int loadCompleteCount = googleSheetInfos.Length;
            loadCount = 0;

            // 각 시트 데이터 로드
            for (int i = 0; i < googleSheetInfos.Length; i++)
            {
                GoogleSheetInfo info = googleSheetInfos[i];
                SpreadsheetManager.Read(new GSTU_Search(info.sheetId, info.worksheet), GetSetDataAction((SheetType)i));
            }

            // 모든 데이터 로드 완료 대기
            while (loadCount < loadCompleteCount)
            {
                await UniTask.Delay(10);
            }

            // TowerUpgradeData 에셋 생성 및 저장
            SaveTowerUpgradeData();

            await UniTask.CompletedTask;
        }

        // 시트 타입에 따른 데이터 설정 액션 반환
        private UnityAction<GstuSpreadSheet> GetSetDataAction(SheetType sheetType)
        {
            switch (sheetType)
            {
                case SheetType.TowerData: return new UnityAction<GstuSpreadSheet>(SetTowerData);
                case SheetType.TowerUpgrade: return new UnityAction<GstuSpreadSheet>(SetTowerUpgradeData);
                case SheetType.TowerCardLV: return new UnityAction<GstuSpreadSheet>(SetTowerCardData);
                case SheetType.StageData: return new UnityAction<GstuSpreadSheet>(SetStageData);
                case SheetType.MonsterData: return new UnityAction<GstuSpreadSheet>(SetMonsterData);
                case SheetType.BulletData: return new UnityAction<GstuSpreadSheet>(SetBulletData);
                case SheetType.SkillData: return new UnityAction<GstuSpreadSheet>(SetSkillData);
                case SheetType.AbilityData: return new UnityAction<GstuSpreadSheet>(SetAbilityData);
                default: throw new ArgumentOutOfRangeException(nameof(sheetType), sheetType, null);
            }
        }

        // TowerUpgradeData 에셋 저장
        private void SaveTowerUpgradeData()
        {
            string path = "Assets/Resources_Addressable/Local/Data/";
            string fileName = "TowerUpgradeData";
            DeleteFilesInFolder(path, fileName + ".asset");
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(towerUpgradeData, path + fileName + ".asset");
#endif
        }

        // 타워 데이터 설정
        void SetTowerData(GstuSpreadSheet ss)
        {
            string path = "Assets/Resources_Addressable/Local/Data/Tower/";
            DeleteFilesInFolder(path);
            towerDataDic.Clear();

            foreach (var row in ss.rows.secondaryKeyLink.Skip(1)) // 첫 번째 행 스킵
            {
                string key = row.Key;
                TowerInfo towerInfo = CreateTowerInfo(ss, key);

                if (towerDataDic.TryGetValue(towerInfo.index, out TowerData towerData))
                {
                    towerData.towerInfoDic.Add(towerInfo.tier, towerInfo);
                }
                else
                {
                    towerData = ScriptableObject.CreateInstance<TowerData>();
                    towerData.index = towerInfo.index;
                    towerData.towerName = towerInfo.towerName;
                    towerDataDic.Add(towerData.index, towerData);
                    towerData.towerInfoDic.Add(towerInfo.tier, towerInfo);
                }
            }

#if UNITY_EDITOR
            foreach (var towerData in towerDataDic.Values)
                AssetDatabase.CreateAsset(towerData, path + towerData.towerName + ".asset");
#endif

            loadCount++;
        }

        // TowerInfo 객체 생성
        private TowerInfo CreateTowerInfo(GstuSpreadSheet ss, string key)
        {
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
                // price = int.Parse(ss[key, "Price"].value),
                salePrice = int.Parse(ss[key, "SalePrice"].value),
            };

            string json = ss[key, "Extra"].value;
            towerInfo.extraInfo = JsonUtility.FromJson<ExtraInfo>(json);

            return towerInfo;
        }

        void SetTowerUpgradeData(GstuSpreadSheet ss)
        {
            if (towerUpgradeData == null)
                towerUpgradeData = ScriptableObject.CreateInstance<TowerUpgradeData>();
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
            if (towerUpgradeData == null)
                towerUpgradeData = ScriptableObject.CreateInstance<TowerUpgradeData>();
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
            string path = "Assets/Resources_Addressable/Local/Data/Monster/";
            DeleteFilesInFolder(path);
            monsterDataDic.Clear();

            foreach (var row in ss.rows.secondaryKeyLink.Skip(1))
            {
                string key = row.Key;

                MonsterData monsterData = ScriptableObject.CreateInstance<MonsterData>();

                monsterData.index = int.Parse(ss[key, "Index"].value);
                monsterData.unitName = ss[key, "Name"].value;
                monsterData.prefabName = ss[key, "Prefab"].value;
                monsterData.moveSpeed = int.Parse(ss[key, "MoveSpeed"].value);
                monsterData.monsterType = (MonsterType)Enum.Parse(typeof(MonsterType), ss[key, "MonsterType"].value);

                monsterDataDic.Add(monsterData.index, monsterData);
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(monsterData, path + monsterData.unitName + ".asset");
#endif
            }

            loadCount++;
        }

        void SetBulletData(GstuSpreadSheet ss)
        {
            string path = "Assets/Resources_Addressable/Local/Data/Bullet/";
            DeleteFilesInFolder(path);
            bulletDataDic.Clear();

            foreach (var row in ss.rows.secondaryKeyLink.Skip(1))
            {
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
            string path = "Assets/Resources_Addressable/Local/Data/Skill/";
            DeleteFilesInFolder(path);
            skillDataDic.Clear();

            foreach (var row in ss.rows.secondaryKeyLink.Skip(1))
            {
                string key = row.Key;

                SkillData skillData = ScriptableObject.CreateInstance<SkillData>();
                skillData.index = int.Parse(ss[key, "Index"].value);
                skillData.skillName = ss[key, "Name"].value;
                skillData.coolTime = int.Parse(ss[key, "CoolTime"].value);
                skillData.dynamicData[0] = int.Parse(ss[key, "Extra 1"].value);
                skillData.dynamicData[1] = int.Parse(ss[key, "Extra 2"].value);
                skillData.dynamicData[2] = int.Parse(ss[key, "Extra 3"].value);
                skillData.skillType = (SkillType)Enum.Parse(typeof(SkillType), ss[key, "SkillType"].value);
                skillData.skillUseType = (SkillUseType)Enum.Parse(typeof(SkillUseType), ss[key, "SkillUseType"].value);

                skillDataDic.Add(skillData.index, skillData);
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(skillData, path + skillData.skillName + ".asset");
#endif
            }

            loadCount++;
        }

        void SetAbilityData(GstuSpreadSheet ss)
        {
            string path = "Assets/Resources_Addressable/Local/Data/AbilityCard/";
            DeleteFilesInFolder(path);
            abilityDataDic.Clear();

            foreach (var row in ss.rows.secondaryKeyLink.Skip(1))
            {
                string key = row.Key;

                AbilityData abilityData = ScriptableObject.CreateInstance<AbilityData>();
                abilityData.index = int.Parse(ss[key, "Index"].value);
                abilityData.abilityName = ss[key, "Name"].value;
                abilityData.iconName = ss[key, "IconName"].value;
                abilityData.rarity = (Rarity)Enum.Parse(typeof(Rarity), ss[key, "Rare"].value);
                abilityData.percent = int.Parse(ss[key, "Percent"].value);
                abilityData.explan[0] = ss[key, "kr"].value;
                abilityData.explan[1] = ss[key, "en"].value;

                string json = ss[key, "Extra"].value;
                abilityData.extraInfo = JsonUtility.FromJson<ExtraInfo>(json);

                abilityDataDic.Add(abilityData.index, abilityData);
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(abilityData,
                    path + abilityData.rarity + "_" + abilityData.abilityName + ".asset");
#endif
            }

            loadCount++;
        }
        
        private void SetStageData(GstuSpreadSheet ss)
        {
            stageData = ScriptableObject.CreateInstance<StageData>();
            List<StageDataInfo> stageDataList = new List<StageDataInfo>();

            foreach (var row in ss.rows.secondaryKeyLink.Skip(1))
            {
                string key = row.Key;
                StageDataInfo stageInfo = new StageDataInfo
                {
                    hp = int.Parse(ss[key, "HP"].value),
                    stageReward = int.Parse(ss[key, "Reward"].value),
                    appearMonster = int.Parse(ss[key, "AppearMonster"].value),
                    monsterCount = int.Parse(ss[key, "MonsterCount"].value)
                };
                stageDataList.Add(stageInfo);
            }

            stageData.Infos = stageDataList.ToArray();
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(stageData, "Assets/Resources_Addressable/Local/Data/StageData.asset");
#endif
            loadCount++;
        }

        // 폴더 내 파일 삭제
        public static void DeleteFilesInFolder(string folderPath, string fileName = "")
        {
            string[] files = Directory.GetFiles(folderPath);

            if (string.IsNullOrEmpty(fileName))
            {
                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                        Debug.Log($"Deleted file: {file}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Error deleting file {file}: {ex.Message}");
                    }
                }
            }
            else
            {
                string targetFile = Path.Combine(folderPath, fileName);
                if (File.Exists(targetFile))
                {
                    try
                    {
                        File.Delete(targetFile);
                        Debug.Log($"Deleted file: {targetFile}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Error deleting file {targetFile}: {ex.Message}");
                    }
                }
            }
        }

        #endregion
    }
}