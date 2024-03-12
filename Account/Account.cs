using System;
using System.Collections;
using System.IO;
using Leguar.TotalJSON;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using RandomFortress.Menu;
using UnityEngine;

namespace RandomFortress.Game
{
    /// <summary>
    /// 계정점보
    /// </summary>
    [System.Serializable]
    public class Account : Common.Singleton<Account>
    {
        [SerializeField] private PlayerData data;
        
        // private DateTime adDebuffStartTime;
        
        public PlayerData Data => data;
        public GameResult Result => data.BestGameResult;
        
        public override void Reset() { }
        
        public override void Terminate() { Destroy(Instance); }

        void OnApplicationQuit() {
            SaveGameData();
        }
        
        public void Init()
        {
            data = gameObject.AddComponent<PlayerData>();
            data.Init();
            
            //TODO: 계정 정보. 서버에서 받는것으로 바꿔야함
            LoadGameData();
        }

        public void SetPlayerData(PlayerData playerData)
        {
            data = playerData;
        }

        public void InitAdDebuff()
        {
            // 광고버프 리스트를 역순으로 순회
            for (int i = data.adDebuffList.Count - 1; i >= 0; i--)
            {
                var adDebuff = data.adDebuffList[i];
                DateTime endTime = DateTime.Parse(adDebuff.endTime);
                float waitTime = (float)endTime.Subtract(DateTime.Now).TotalSeconds;

                // 버프 시간이 지났다면 삭제
                if (waitTime <= 0)
                {
                    data.adDebuffList.RemoveAt(i);
                }
                else
                {
                    // TODO: 추후 서버에서 받는 걸로 바꿔야 한다
                    MainManager.Instance.SetAdDebuff(adDebuff.type, waitTime);
                    StartCoroutine(AdDebuffCoroutine(adDebuff.type, waitTime));
                }
            }
        }
        
        public void AddAdDebuff(AdDebuffType type)
        {
            AdDebuffState adDebuff = new AdDebuffState
            {
                type = type,
                endTime = DateTime.Now.AddMinutes(GameConstants.AdDebuffMinute).ToString()
            };
            data.adDebuffList.Add(adDebuff);
            
            DateTime endTime = DateTime.Parse(adDebuff.endTime);
            float waitTime = (float)endTime.Subtract(DateTime.Now).TotalSeconds;
            
            MainManager.Instance.SetAdDebuff(adDebuff.type, waitTime);
            StartCoroutine(AdDebuffCoroutine(adDebuff.type, waitTime));
        }
        
        private IEnumerator AdDebuffCoroutine(AdDebuffType type, float waitTime)
        {
            float timer = 0;

            while (timer < waitTime)
            {
                yield return new WaitForSecondsRealtime(1);
                timer += 1;
            }

            foreach (var adDebuff in data.adDebuffList)
            {
                if (adDebuff.type == type)
                {
                    data.adDebuffList.Remove(adDebuff);
                    break;
                }
            }

            MainManager.Instance.UpdateAdDebuff();
        }

        #region Inventory

        
        public int[] GetTowerDeck => data.towerDeck;
        
        public int TowerDeck(int index) => data.towerDeck[index];
        public int SkillDeck(int index) => data.skillDeck[index];
        
        public int GetCardLevel(int towerIndex)
        {
            if (data.towerCardDic.ContainsKey(towerIndex.ToString()))
            {
                return data.towerCardDic[towerIndex.ToString()].CardLV;
            }

            Debug.Log("Not Found TowerCard!!");
            return 0;
        }
        
        public int TowerDeckAddOrRemove(int towerIndex)
        {
            // 중복 되는 타워가 있다면 빼준다
            int size = data.towerDeck.Length;
            for (int i = 0; i < size; ++i)
            {
                if (data.towerDeck[i] == towerIndex)
                {
                    data.towerDeck[i] = 0;
                    return 0;
                }
            }
            
            // 빈슬롯이 있다면 추가한다
            for (int i = 0; i < size; ++i)
            {
                if (data.towerDeck[i] == 0)
                {
                    data.towerDeck[i] = towerIndex;
                    return towerIndex;
                }
            }
            
            // 슬롯에 자리가없다면 아무런 행동도 하지않는다
            return 0;
        }
        
        public int SkillDeckAddOrRemove(int skillIndex)
        {
            // 중복 되는 타워가 있다면 빼준다
            int size = data.skillDeck.Length;
            for (int i = 0; i < size; ++i)
            {
                if (data.skillDeck[i] == skillIndex)
                {
                    data.skillDeck[i] = 0;
                    return 0;
                }
            }
            
            // 빈슬롯이 있다면 추가한다
            for (int i = 0; i < size; ++i)
            {
                if (data.skillDeck[i] == 0)
                {
                    data.skillDeck[i] = skillIndex;
                    return skillIndex;
                }
            }
            
            // 슬롯에 자리가없다면 아무런 행동도 하지않는다
            return 0;
        }

        #endregion

        #region SaveLoad

        // 게임 결과값 저장
        public void SaveStageResult(GameResult result)
        {
            if (data.BestGameResult == null)
            {
                data.BestGameResult = result;
            }
            else
            {
                if (data.BestGameResult.rank <= result.rank)
                {
                    data.BestGameResult = result;
                }
            }
            
            data.gameResultList.Add(result);
            
            // 최대 10개까지 히스토리 저장
            if (data.gameResultList.Count > 10)
                data.gameResultList.Remove(data.gameResultList[0]);
        }
        
        private void SaveGameData()
        {
            JSON json = JSON.Serialize(data);
            string jsonString = json.CreateString();
            string filePath = Path.Combine(Application.persistentDataPath, "gameData.json");
            File.WriteAllText(filePath, jsonString);
            
            Debug.Log("Success Save GameData!! ");
            
            // string dataAsJson = JsonUtility.ToJson(data);
            // string filePath = Path.Combine(Application.persistentDataPath, "gameData.json");
            // File.WriteAllText(filePath, dataAsJson);
        }
        
        private bool LoadGameData()
        {
            string filePath = Path.Combine(Application.persistentDataPath, "gameData.json");
            if (File.Exists(filePath))
            {
                string dataAsJson = File.ReadAllText(filePath);
                if (dataAsJson.Length > 0)
                {
                    JSON json = JSON.ParseString(dataAsJson);
                    PlayerData temp = json.Deserialize<PlayerData>();
                    data.id = temp.id;
                    data.nickname = temp.nickname;
                    data.gold = temp.gold;
                    data.gem = temp.gem;
                    data.winCount = temp.winCount;
                    data.loseCount = temp.loseCount;
                    data.eloRating = temp.eloRating;
                    data.trophy = temp.trophy;
                    data.soloRank = temp.soloRank;
                    data.BestGameResult = temp.BestGameResult;
                    data.gameResultList = temp.gameResultList;
                    data.isFirstPlay = temp.isFirstPlay;
                    data.isTutorialLobby = temp.isTutorialLobby;
                    data.isTutorialGame = temp.isTutorialGame;
                    data.towerDeck = temp.towerDeck;
                    data.skillDeck = temp.skillDeck;
                    data.towerCardDic = temp.towerCardDic;
                    data.myItem = temp.myItem;
                    data.androidAppid = temp.androidAppid;
                    data.iOSAppid = temp.iOSAppid;
                    data.googleAppid = temp.googleAppid;
                    data.facebookAppid = temp.facebookAppid;
                    data.adDebuffList = temp.adDebuffList;
                    
                    Debug.Log("Success Load GameData!! " + data);
                    return true;
                }
            }
            
            Debug.Log("Fail Load GameData!!");
            
            return false;
            
            // string filePath = Path.Combine(Application.persistentDataPath, "gameData.json");
            // if (File.Exists(filePath))
            // {
            //     string dataAsJson = File.ReadAllText(filePath);
            //     if (dataAsJson.Length > 0)
            //     {
            //         JsonUtility.FromJsonOverwrite(dataAsJson, data);
            //         return true;
            //     }
            // }
            //
            // return false;
        }

        private void SetPlayerData()
        {
            
        }

        #endregion
    }
}