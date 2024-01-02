using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RandomFortress.Data;

namespace RandomFortress.Game
{
    public static class CSVParser
    {
        static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        static char[] TRIM_CHARS = { '\"' };
        
        
        public static List<int> ParseStageCSV(string filePath)
        {
            var list = new List<int>();
            TextAsset data = Resources.Load (filePath) as TextAsset;
            
            var lines = Regex.Split (data.text, LINE_SPLIT_RE);
            
            if(lines.Length <= 1) 
                return list;
            
            var header = Regex.Split(lines[0], SPLIT_RE);
            for(var i=1; i < lines.Length; i++) {
            
                var values = Regex.Split(lines[i], SPLIT_RE);
                if(values.Length == 0 ||values[0] == "") 
                    continue;
            
                // 현재는 stage, hp 정보밖에없다
                string value = values[1];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                int hp = int.Parse(value);
                list.Add (hp);
            }
            return list;
        }
        
        public static List<TowerData> ParseTowerCSV(string filePath)
        {
            var list = new List<TowerData>();
            TextAsset data = Resources.Load (filePath) as TextAsset;
            
            var lines = Regex.Split (data.text, LINE_SPLIT_RE);
            
            if(lines.Length <= 1) 
                return list;
            
            var header = Regex.Split(lines[0], SPLIT_RE);
            for(var i=1; i < lines.Length; i++) {
            
                var values = Regex.Split(lines[i], SPLIT_RE);
                
                if(values.Length == 0 ||values[0] == "") 
                    continue;

                TowerData towerData = ScriptableObject.CreateInstance<TowerData>();
                towerData.index = int.Parse(values[0]);
                towerData.towerName = values[1];
                towerData.attackPower = int.Parse(values[2]);
                towerData.attackSpeed = int.Parse(values[3]);
                towerData.attackRange = int.Parse(values[4]);
                towerData.attackType = int.Parse(values[5]);
                towerData.bulletIndex = int.Parse(values[6]);
                towerData.tier = int.Parse(values[7]);
                towerData.sellPrice = int.Parse(values[8]);
                towerData.dynamicData[0] = int.Parse(values[9]);
                towerData.dynamicData[1] = int.Parse(values[10]);
                list.Add(towerData);

                // string value = values[1];
                // value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                // int hp = int.Parse(value);
                // list.Add (hp);
            }
            return list;
        }

        // public static List<Dictionary<string, object>> Read(string file)
        // {
        //     var list = new List<Dictionary<string, object>>();
        //     TextAsset data = Resources.Load (file) as TextAsset;
        //
        //     var lines = Regex.Split (data.text, LINE_SPLIT_RE);
        //
        //     if(lines.Length <= 1) return list;
        //
        //     var header = Regex.Split(lines[0], SPLIT_RE);
        //     for(var i=1; i < lines.Length; i++) {
        //
        //         var values = Regex.Split(lines[i], SPLIT_RE);
        //         if(values.Length == 0 ||values[0] == "") continue;
        //
        //         var entry = new Dictionary<string, object>();
        //         for(var j=0; j < header.Length && j < values.Length; j++ ) {
        //             string value = values[j];
        //             value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
        //             object finalvalue = value;
        //             int n;
        //             float f;
        //             if(int.TryParse(value, out n)) {
        //                 finalvalue = n;
        //             } else if (float.TryParse(value, out f)) {
        //                 finalvalue = f;
        //             }
        //             entry[header[j]] = finalvalue;
        //         }
        //         list.Add (entry);
        //     }
        //     return list;
        // }
    }
}