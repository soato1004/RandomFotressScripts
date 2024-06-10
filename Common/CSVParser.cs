// using UnityEngine;
// using System.Collections.Generic;
// using System.Text.RegularExpressions;
// 
// using RandomFortress.Data;
//
// namespace RandomFortress
// {
//     public static class CSVParser
//     {
//         static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
//         static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
//         static char[] TRIM_CHARS = { '\"' };
//
//
//         public static List<int> ParseStageCSV(string filePath)
//         {
//             var list = new List<int>();
//             TextAsset data = Resources.Load(filePath) as TextAsset;
//
//             var lines = Regex.Split(data.text, LINE_SPLIT_RE);
//
//             if (lines.Length <= 1)
//                 return list;
//
//             var header = Regex.Split(lines[0], SPLIT_RE);
//             for (var i = 1; i < lines.Length; i++)
//             {
//                 var values = Regex.Split(lines[i], SPLIT_RE);
//                 if (values.Length == 0 || values[0] == "")
//                     continue;
//
//                 // 현재는 stage, hp 정보밖에없다
//                 string value = values[1];
//                 value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
//                 int hp = int.Parse(value);
//                 list.Add(hp);
//             }
//
//             return list;
//         }
//
//         public static List<TowerData> ParseTowerCSV(string filePath)
//         {
//             var list = new List<TowerData>();
//             int towerIndex = 1;
//
//             while (true)
//             {
//                 // 타워 데이터 파싱
//                 string path = filePath + towerIndex.ToString("D3");
//                 ++towerIndex;
//
//                 TextAsset data = Resources.Load(path) as TextAsset;
//                 if (data == null)
//                     break;
//
//                 var lines = Regex.Split(data.text, LINE_SPLIT_RE);
//
//                 if (lines.Length <= 1)
//                     break;
//
//                 int dataType = 0; // 파싱시에 사용
//                 int dataCount = 0; // 타워 최대 티어 갯수만큼 파싱하기 위한 변수
//                 
//                 for (var i = 0; i < lines.Length; i++)
//                 {
//                     // 한 줄 파싱
//                     var values = Regex.Split(lines[i], SPLIT_RE);
//
//                     int j = 0;
//
//                     // 줄 안의 데이터 파싱부
//                     while (j < values.Length)
//                     {
//                         if (values.Length == 0)
//                         {
//                             break;
//                         }
//
//                         if (values[j] == "" || values[j][0] == '#')
//                         {
//                             ++j;
//                             continue;
//                         }
//
//                         if (values[j] == "index")
//                         {
//                             values = Regex.Split(lines[++i], SPLIT_RE);
//                             dataType = 1;
//                             continue;
//                         }
//
//                         if (dataType == 1)
//                         {
//                             TowerData towerData = ScriptableObject.CreateInstance<TowerData>();
//                             towerData.index = int.Parse(values[j++]);
//                             towerData.name = values[j++];
//                             towerData.attackPower = int.Parse(values[j++]);
//                             towerData.attackSpeed = int.Parse(values[j++]);
//                             towerData.attackRange = int.Parse(values[j++]);
//                             towerData.attackType = int.Parse(values[j++]);
//                             towerData.bulletIndex = int.Parse(values[j++]);
//                             towerData.tier = int.Parse(values[j++]);
//                             towerData.salePrice = int.Parse(values[j++]);
//                             towerData.dynamicData[0] = int.Parse(values[j++]);
//                             towerData.dynamicData[1] = int.Parse(values[j++]);
//                             list.Add(towerData);
//                             ++dataCount;
//                             break;
//                         }
//                         else ++j;
//                     }
//                     
//                     if (dataCount >= GameConstants.TOWER_TIER_MAX)
//                         break;
//                 }
//             }
//
//
//             return list;
//         }
//
//         public static List<TowerUpgradeData> ParseTowerUpgradeCSV(string filePath)
//         {
//             var list = new List<TowerUpgradeData>();
//             int towerIndex = 1;
//
//             while (true)
//             {
//                 string path = filePath + towerIndex.ToString("D3");
//
//                 TextAsset data = Resources.Load(path) as TextAsset;
//                 if (data == null)
//                     break;
//
//                 var lines = Regex.Split(data.text, LINE_SPLIT_RE);
//
//                 if (lines.Length <= 1)
//                 {
//                     ++towerIndex;
//                     continue;
//                 }
//
//                 TowerUpgradeData towerUpData = ScriptableObject.CreateInstance<TowerUpgradeData>();
//                 towerUpData.index = towerIndex;
//                 int dataType = 0;
//                 
//                 for (var i = 0; i < lines.Length; i++)
//                 {
//                     var values = Regex.Split(lines[i], SPLIT_RE);
//
//                     int j = 0;
//                     
//                     while (j < values.Length)
//                     {
//                         if (values.Length == 0)
//                             break;
//
//                         if (values[j] == "" || values[j][0] == '#')
//                         {
//                             j++;
//                             continue;
//                         }
//
//                         if (values[j] == "CardLV")
//                         {
//                             values = Regex.Split(lines[++i], SPLIT_RE);
//                             dataType = 1;
//                         }
//                         else if (values[j] == "Upgrade")
//                         {
//                             values = Regex.Split(lines[++i], SPLIT_RE);
//                             dataType = 2;
//                         }
//
//                         if (dataType == 1)
//                         {
//                             // 타워카드 업그레이드 정보
//                             int cardLV = int.Parse(values[j++]);
//                             int dataCount = 0;
//
//                             CardUpgradeInfo cardUpgradeInfo = new CardUpgradeInfo();
//                             cardUpgradeInfo.NeedCard = int.Parse(values[j++]);
//                             while (j < values.Length)
//                             {
//                                 if (values[j] == "")
//                                     break;
//                                 cardUpgradeInfo.CardLVData.Add(int.Parse(values[j++]));
//                             }
//                             
//                             towerUpData.CardLvData.Add(cardUpgradeInfo);
//                             break;
//                         }
//                         else if (dataType == 2)
//                         {
//                             // 인게임 타워 업그레이드 정보
//                             int upgradeLV = int.Parse(values[j++]);
//                             int dataCount = 0;
//
//                             TowerUpgradeInfo towerUpgradeInfo = new TowerUpgradeInfo();
//                             while (j < values.Length)
//                             {
//                                 if (values[j] == "")
//                                     break;
//                                 towerUpgradeInfo.Data.Add(int.Parse(values[j++]));
//                             }
//
//                             towerUpData.UpgradeData.Add(towerUpgradeInfo);
//                             break;
//                         }
//                         else ++j;
//                     }
//                 }
//
//                 list.Add(towerUpData);
//                 ++towerIndex;
//             }
//
//
//             return list;
//         }
//     }
// }