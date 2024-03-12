using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using RandomFortress.Common;
using RandomFortress.Common.Utils;
using RandomFortress.Manager;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace DefaultNamespace
{
    public class LocalizationManager : Singleton<LocalizationManager>
    {
        public override void Reset()
        {
            JTDebug.LogColor("LocalizationManager Reset");
        }

        public override void Terminate()
        {
            JTDebug.LogColor("LocalizationManager Terminate");
        }

        // public async UniTask LoadTableAsync(string tableName)
        // {
        //     await LocalizationSettings.InitializationOperation;
        //     
        //     // If the translations are all in 1 table then its quicker to just get the table, we wont need to do anymore waiting
        //     var tableOp = LocalizationSettings.StringDatabase.GetTableAsync("StringTable");
        //     await tableOp;
        //     
        //     // E.G dict["AccusedCard_DisplayName"] = "Karl"
        //     // dict["AccusedCard_DisplayName"] = tableOp.Result.GetEntry("AccusedCard_DisplayName")?.GetLocalizedString();
        //
        //
        //     // SerializableDictionaryBase<string, string> stringTableDic = DataManager.Instance.stringTableDic;
        //     
        //     // We can add them all if we need to
        //     foreach(var entry in tableOp.Result)
        //     {
        //         // Get the key name
        //         var keyName = tableOp.Result.SharedData.GetEntry(entry.Key)?.Key;
        //
        //         // If its a smart string then we may need to pass some arguments here
        //         DataManager.Instance.stringTableDic[keyName] = entry.Value.GetLocalizedString();
        //     }
        //
        //     // Now get the main entry
        //     // var smartFormatEntry = tableOp.Result.GetEntry("My String");
        //
        //     // Translate it using the dictionary
        //     // E.G "My name is {AccusedCard_DisplayName} => My name is Karl
        //     // var translatedString = smartFormatEntry.GetLocalizedString(dict);
        //
        //     // Debug.Log(translatedString);
        //     
        //     await Task.CompletedTask;
        // }


        public async UniTask LoadTablesAsync()
        {
            await LoadTableAsync("StringTable");
            await LoadTableAsync("SoundTable");
        }
        
        private async UniTask LoadTableAsync(string tableName)
        {
            await LocalizationSettings.InitializationOperation;
            
            var tableOp = LocalizationSettings.StringDatabase.GetTableAsync(tableName);
            await tableOp;
            
            foreach(var entry in tableOp.Result)
            {
                var keyName = tableOp.Result.SharedData.GetEntry(entry.Key)?.Key;
                DataManager.Instance.stringTableDic[keyName] = entry.Value.GetLocalizedString();
            }
            
            await Task.CompletedTask;
        }

        public void InitLocail()
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            string twoLetterISOLanguageName = cultureInfo.TwoLetterISOLanguageName;
            
            SetGameLanguage(twoLetterISOLanguageName);
        }
        
        void SetGameLanguage(string twoLetterISOLanguageName = "en")
        {
            switch (twoLetterISOLanguageName)
            {
                case "en":
                    // 영어 설정 적용
                    break;
                case "fr":
                    // 프랑스어 설정 적용
                    break;
                // 추가 언어에 대한 케이스 처리
                default:
                    // 기본 언어 설정 적용
                    break;
            }
        
            Debug.Log("Current Language: " + twoLetterISOLanguageName);
        }
    }
}