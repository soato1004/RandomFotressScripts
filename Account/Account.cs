using System;
using System.Collections;
using System.IO;
using Firebase.Auth;
using Firebase.Database;
using Leguar.TotalJSON;
using UnityEngine;

namespace RandomFortress
{
    /// <summary>
    /// 계정 정보
    /// </summary>
    [System.Serializable]
    public class Account : Singleton<Account>
    {
        private PlayerData data;
        public PlayerData Data => data;
        public GameResult Result => data.bestGameResult;
        private DatabaseReference databaseReference;

        public override void Reset() { }

        void OnApplicationQuit()
        {
            SaveGameData();
        }

        public void Init()
        {
            data = new PlayerData();
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

            try
            {
                LoadGameData();
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading game data: " + e.Message);
            }
        }

        public void SetPlayerData(PlayerData playerData)
        {
            data = playerData;
        }

        public void CheckUserAccount(FirebaseUser user)
        {
            databaseReference.Child("users").Child(user.UserId).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("데이터 불러오기 오류: " + task.Exception);
                    return;
                }

                if (!task.Result.Exists)
                {
                    CreateNewAccount(user);
                }
                else
                {
                    LoadUserData(task.Result);
                }
            });
        }

        void CreateNewAccount(FirebaseUser user)
        {
            data = new PlayerData
            {
                id = user.UserId,
                nickname = user.DisplayName,
                isFirstPlay = true,
                isFirstAccountCreation = true // 최초 계정 생성 여부 설정
            };

            SavePlayerData(user.UserId);
        }

        void LoadUserData(DataSnapshot snapshot)
        {
            data = JsonUtility.FromJson<PlayerData>(snapshot.GetRawJsonValue());
            data.isFirstAccountCreation = false; // 계정이 이미 존재함을 설정
            Debug.Log("계정 데이터 불러오기 성공: " + data.nickname);
        }

        public void SavePlayerData(string userId)
        {
            string json = JsonUtility.ToJson(data);
            databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("계정 저장 오류: " + task.Exception);
                }
                else
                {
                    Debug.Log("계정 데이터 저장 성공");
                }
            });
        }

        // 광고버프 적용부분
        public void InitAdDebuff()
        {
            for (int i = data.adDebuffList.Count - 1; i >= 0; i--)
            {
                var adDebuff = data.adDebuffList[i];
                DateTime endTime = DateTime.Parse(adDebuff.endTime);
                float waitTime = (float)endTime.Subtract(DateTime.Now).TotalSeconds;

                if (waitTime <= 0)
                {
                    data.adDebuffList.RemoveAt(i);
                }
                else
                {
                    MainManager.Instance.SetAdDebuff(adDebuff.type, waitTime);
                    StartCoroutine(AdDebuffCoroutine(adDebuff.type, waitTime));
                }
            }
        }

        public void AddAdDebuff(AdDebuffType type)
        {
            Debug.Log("AddAdDebuff 접근");
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
            Debug.Log("AdDebuffCoroutine 접근");
            float timer = 0;

            while (timer < waitTime)
            {
                yield return new WaitForSecondsRealtime(1);
                timer += 1;
            }

            for (int i = 0; i < data.adDebuffList.Count; i++)
            {
                if (data.adDebuffList[i].type == type)
                {
                    data.adDebuffList.RemoveAt(i);
                    break;
                }
            }

            MainManager.Instance.UpdateAdDebuff();
        }

        #region Inventory

        public int[] GetTowerDeck => data.towerDeck;

        public int TowerDeck(int index) => data.towerDeck[index];
        public int SkillDeck(int index) => data.skillDeck[index];

        public int TowerDeckAddOrRemove(int towerIndex)
        {
            int size = data.towerDeck.Length;
            for (int i = 0; i < size; ++i)
            {
                if (data.towerDeck[i] == towerIndex)
                {
                    data.towerDeck[i] = 0;
                    return 0;
                }
            }

            for (int i = 0; i < size; ++i)
            {
                if (data.towerDeck[i] == 0)
                {
                    data.towerDeck[i] = towerIndex;
                    return towerIndex;
                }
            }

            return 0;
        }

        public int SkillDeckAddOrRemove(int skillIndex)
        {
            int size = data.skillDeck.Length;
            for (int i = 0; i < size; ++i)
            {
                if (data.skillDeck[i] == skillIndex)
                {
                    data.skillDeck[i] = 0;
                    return 0;
                }
            }

            for (int i = 0; i < size; ++i)
            {
                if (data.skillDeck[i] == 0)
                {
                    data.skillDeck[i] = skillIndex;
                    return skillIndex;
                }
            }

            return 0;
        }

        #endregion

        #region SaveLoad

        public void SaveStageResult(GameResult result)
        {
            if (data.bestGameResult == null)
            {
                data.bestGameResult = result;
            }
            else
            {
                if (data.bestGameResult.rank <= result.rank)
                {
                    data.bestGameResult = result;
                }
            }

            data.gameResultList.Add(result);

            if (data.gameResultList.Count > 10)
                data.gameResultList.Remove(data.gameResultList[0]);
        }

        private void SaveGameData()
        {
            string json = JsonUtility.ToJson(data);
            string filePath = Path.Combine(Application.persistentDataPath, "gameData.json");
            File.WriteAllText(filePath, json);

            Debug.Log("Success Save GameData!!");
        }

        private bool LoadGameData()
        {
            string filePath = Path.Combine(Application.persistentDataPath, "gameData.json");
            if (File.Exists(filePath))
            {
                string dataAsJson = File.ReadAllText(filePath);
                if (dataAsJson.Length > 0)
                {
                    PlayerData temp = JsonUtility.FromJson<PlayerData>(dataAsJson);
                    data = temp;

                    Debug.Log("Success Load GameData!! " + data);
                    return true;
                }
            }

            Debug.Log("Fail Load GameData!!");
            return false;
        }

        #endregion
    }
}
