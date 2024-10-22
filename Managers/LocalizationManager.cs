using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace RandomFortress
{
    public class LocalizationManager : Singleton<LocalizationManager>
    {
        public enum Language { kr, en }
        public Language selectLanguage = Language.kr;

        private Dictionary<string, Action<string, string>> tableProcessors;

        private void Awake()
        {
            InitializeTableProcessors();
        }

        private void InitializeTableProcessors()
        {
            tableProcessors = new Dictionary<string, Action<string, string>>
            {
                { "SoundTable", (key, value) => SoundManager.I.audioFileNames[key] = value },
                { "StringTable", (key, value) => DataManager.I.stringTableDic[key] = value },
                { "Names", (key, value) => DataManager.I.stringTableDic[key] = value }
            };
        }

        // 로컬라이제이션 테이블을 비동기로 로드
        public async UniTask LoadTablesAsync()
        {
            try
            {
                SetGameLanguage(selectLanguage);

                await LoadTableAsync("StringTable");
                await LoadTableAsync("SoundTable");
                await LoadTableAsync("Names");

                Debug.Log("All localization tables loaded successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading localization tables: {e.Message}");
            }
        }
        
        // 게임의 언어를 설정
        public void SetGameLanguage(Language language)
        {
            selectLanguage = language;
            var localeCode = language == Language.kr ? "ko" : "en";
            var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
            if (locale != null)
            {
                LocalizationSettings.SelectedLocale = locale;
                Debug.Log($"Current Language: {locale.Identifier.Code}");
            }
            else
            {
                Debug.LogError($"Locale not found for language: {language}");
            }
        }

        // 특정 테이블을 로드하는 비동기 메서드
        private async UniTask LoadTableAsync(string tableName)
        {
            try
            {
                await LocalizationSettings.InitializationOperation;

                var tableOp = LocalizationSettings.StringDatabase.GetTableAsync(tableName);
                await tableOp;

                var table = tableOp.Result;
                if (table == null)
                {
                    throw new Exception($"Table {tableName} could not be loaded.");
                }

                foreach (var entry in table)
                {
                    var keyName = table.SharedData.GetEntry(entry.Key)?.Key;
                    if (!string.IsNullOrEmpty(keyName))
                    {
                        if (tableProcessors.TryGetValue(tableName, out var processor))
                        {
                            processor(keyName, entry.Value.GetLocalizedString());
                        }
                        else
                        {
                            Debug.LogWarning($"No processor found for table: {tableName}");
                            // 기본 처리: StringTable에 저장
                            DataManager.I.stringTableDic[keyName] = entry.Value.GetLocalizedString();
                        }
                    }
                }

                // Debug.Log($"Table {tableName} loaded successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading table {tableName}: {e.Message}");
                throw; // 상위 메서드에서 처리할 수 있도록 예외를 다시 던짐
            }
        }

        // 특정 키로 로컬라이즈된 문자열을 가져오는 함수
        public string GetLocalizedString(string key)
        {
            if (DataManager.I.stringTableDic.TryGetValue(key, out var localizedString))
            {
                return localizedString;
            }

            // 캐시에 없으면 직접 로드
            var entry = LocalizationSettings.StringDatabase.GetLocalizedString(key);
            if (!string.IsNullOrEmpty(entry))
            {
                DataManager.I.stringTableDic[key] = entry; // 캐시에 추가
                return entry;
            }

            Debug.LogWarning($"Key {key} not found in localization data.");
            return key; // 키가 없을 경우 기본적으로 키를 반환
        }

        // 언어 변경 메서드
        public async UniTask ChangeLanguage(Language newLanguage)
        {
            SetGameLanguage(newLanguage);
            await LoadTablesAsync(); // 언어 변경 후 테이블 다시 로드
        }
    }
}