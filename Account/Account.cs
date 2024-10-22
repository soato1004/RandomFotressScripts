using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Functions;
using UnityEngine;
using UnityEngine.Purchasing;

namespace RandomFortress
{
    [Serializable]
    public class Account : Singleton<Account>
    {
        [SerializeField] private PlayerData _data;

        public string NicknameExplanation; // 상태 설명
        public bool IsAccountCreated = false; // 계정생성을 대기

        private FirebaseAuth auth => LoginManager.I.auth;
        private FirebaseFunctions functions => LoginManager.I.functions;
        private DatabaseReference databaseReference => LoginManager.I.databaseReference;
        private FirebaseUser user => LoginManager.I.user;

        public List<Dictionary<string, object>> MailList = new List<Dictionary<string, object>>();

        public PlayerData Data => _data;
        public string UserId => _data?.id;

        // 프로젝트 ID를 정의하고 이를 사용하여 URL을 생성
        private const string ProjectID = "randomfortress";
        // private const string CheckAccountExistsUrl = "https://us-central1-" + ProjectID + ".cloudfunctions.net/checkAccountExists";
        // private const string CheckNicknameAndCreateAccountUrl = "https://us-central1-" + ProjectID + ".cloudfunctions.net/checkNicknameAndCreateAccount";

        #region 계정

        public async Task<bool> CheckUserAccount()
        {
            try
            {
                var checkAccountFunction = FirebaseFunctions.DefaultInstance.GetHttpsCallable("checkAccountExists");
                var data = new Dictionary<string, object>
                {
                    { "userId", user.UserId }
                };
                
                var result = await checkAccountFunction.CallAsync(data);
                if (result != null && result.Data != null)
                {
                    var resultData = result.Data as Dictionary<object, object>;
                    if ((bool)resultData["exists"])
                    {
                        Dictionary<object, object> playerData = resultData["data"] as Dictionary<object, object>;
                        _data = Converter.ConvertToObject<PlayerData>(playerData);
                        Debug.Log("계정 데이터 불러오기 성공: " + _data.nickname);
                        InitAccount();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error checking user account: " + ex.Message);
                return false;
            }
        }
        
        public async Task<bool> CreateNewAccount(string nickname)
        {
            try
            {
                var createAccountFunction = FirebaseFunctions.DefaultInstance.GetHttpsCallable("checkNicknameAndCreateAccount");
                var data = new Dictionary<string, object>
                {
                    { "userId", user.UserId },
                    { "nickname", nickname }
                };

                var result = await createAccountFunction.CallAsync(data);

                if (result != null && result.Data != null)
                {
                    var resultData = result.Data as Dictionary<object, object>;
                    Debug.Log("Account created successfully: " + resultData["message"]);
                    
                    Dictionary<object, object> playerData = resultData["data"] as Dictionary<object, object>;
                    _data = Converter.ConvertToObject<PlayerData>(playerData);
                    IsAccountCreated = true;
                    InitAccount();
                    return true;
                }
                else
                {
                    Debug.LogWarning("계정 생성에 실패했습니다.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception occurred while creating account: " + ex.Message);
                IsAccountCreated = false;
                return false;
            }
        }

        // 계정 생성 또는 로드 이후 초기화작업
        public void InitAccount()
        {
            // 특정 경로의 데이터 변경을 구독
            databaseReference.Child("users").Child(UserId).Child("stamina").ValueChanged += HandleStaminaChanged;
        }

        #endregion
        
        #region 인증

        private string cachedIdToken; // 인증 id토큰
        private DateTime tokenExpiryTime;
        private const int TokenLifetimeInSeconds = 3600; // 1시간 유효
        
        // HTTP 요청 보내기
        private static readonly HttpClient client = new HttpClient();

        // Firebase 인증 토큰을 가져오는 메서드
        private async Task<string> GetIdToken()
        {
            if (string.IsNullOrEmpty(cachedIdToken) || DateTime.UtcNow > tokenExpiryTime)
            {
                try
                {
                    var tokenResult = await user.TokenAsync(true);
                    cachedIdToken = tokenResult;
                    tokenExpiryTime = DateTime.UtcNow.AddSeconds(TokenLifetimeInSeconds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to get token: {ex.Message}");
                    // 적절한 에러 처리 추가 (예: 사용자에게 알림, 재시도 로직 등)
                    throw;
                }
            }

            return cachedIdToken;
        }
        
        private async Task<HttpResponseMessage> SendHttpRequest(HttpMethod method, string url, string idToken, string jsonData = null)
        {
            try
            {
                if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != idToken)
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", idToken);
                }

                if (method == HttpMethod.Get)
                {
                    return await client.GetAsync(url);
                }
                else if (method == HttpMethod.Post)
                {
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    return await client.PostAsync(url, content);
                }
                else if (method == HttpMethod.Put)
                {
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    return await client.PutAsync(url, content);
                }
                else if (method == HttpMethod.Delete)
                {
                    return await client.DeleteAsync(url);
                }
                else
                {
                    throw new NotSupportedException("HTTP method not supported");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region 광고

        // 광고 시청 완료 시 호출되는 메서드
        public IEnumerator RecordAdView(int typeIndex)
        {
            AdRewardType adType = (AdRewardType)typeIndex;

            Task task = Task.CompletedTask;
            switch (adType)
            {
                case AdRewardType.AbilityCard:
                    task = AddAdDebuff(adType);
                    break;

                case AdRewardType.Stamina:
                    task = AddStaminaAfterAd();
                    break;
            }

            yield return new WaitUntil(() => task.IsCompleted);
        }

        // 보상형 광고디버프 요청 및 적용
        public async Task AddAdDebuff(AdRewardType adType)
        {
            try
            {
                var addAdDebuffFunction = functions.GetHttpsCallable("addAdDebuff");
                var data = new Dictionary<string, object>
                {
                    { "adType", adType.ToString() }
                };
                var result = await addAdDebuffFunction.CallAsync(data);

                LogFunctionResult("addAdDebuff", result);

                if (IsFunctionResultSuccessful(result))
                {
                    var resultData = result.Data as Dictionary<object, object>;
                    if (resultData != null && resultData.ContainsKey("endTime"))
                    {
                        string endTimeString = resultData["endTime"] as string;
                        DateTime endTime = DateTime.Parse(endTimeString, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
                        float waitTime = (float)(endTime - DateTime.UtcNow).TotalSeconds;
                        MainManager.I.SetAdDebuff(adType, waitTime);
                        StartCoroutine(AdDebuffCoroutine(adType, waitTime));
                        Debug.Log($"광고 디버프 추가 성공. 종료 시간: {endTime}");
                    }
                    else
                    {
                        Debug.LogWarning("종료 시간 정보 없음");
                    }
                }
                else
                {
                    Debug.LogWarning("광고 디버프 추가 실패");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"광고 디버프 추가 중 오류 발생: {ex.Message}");
            }
        }

        // 광고 버프 초기화
        public void InitAdDebuff()
        {
            List<AdDebuff> adDebuffsToRemove = new List<AdDebuff>(); // 삭제할 디버프 목록

            for (int i = 0; i < _data.adDebuffs.Count; ++i)
            {
                AdDebuff adDebuff = _data.adDebuffs[i];
                DateTime endTime = DateTime.Parse(adDebuff.endTime, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
                float waitTime = (float)(endTime - DateTime.UtcNow).TotalSeconds;

                if (waitTime <= 0)
                {
                    adDebuffsToRemove.Add(adDebuff); // 삭제할 디버프 추가
                }
                else
                {
                    MainManager.I.SetAdDebuff(adDebuff.type, waitTime);
                    StartCoroutine(AdDebuffCoroutine(adDebuff.type, waitTime));
                }
            }

            // 루프가 끝난 후 디버프 삭제
            foreach (var adDebuff in adDebuffsToRemove)
            {
                RemoveAdDebuff(adDebuff.type);
            }
        }

        // 광고 버프 코루틴
        private IEnumerator AdDebuffCoroutine(AdRewardType type, float waitTime)
        {
            yield return new WaitForSecondsRealtime(waitTime);
            RemoveAdDebuff(type);
            MainManager.I.UpdateLobbyUI();
        }

        // 광고 버프 제거
        private void RemoveAdDebuff(AdRewardType type)
        {
            _data.adDebuffs.RemoveAll(adDebuff => adDebuff.type == type);
        }

        #endregion

        #region 인앱결제

        /// <summary>
        /// 인앱 구매 후 서버에 해당 사실을 저장 (OnCall 함수 사용)
        /// </summary>
        public async Task<bool> SavePurchaseToServer(Product product)
        {
            try
            {
                var savePurchaseFunction = functions.GetHttpsCallable("saveInAppPurchase");
                
                // Dictionary를 사용하여 데이터 전송
                var data = new Dictionary<string, object>
                {
                    { "productId", product.definition.id },
                    { "type", product.definition.type.ToString() },
                    { "transactionId", product.transactionID },
                    { "receipt", product.receipt }
                };

                var result = await savePurchaseFunction.CallAsync(data);

                LogFunctionResult("saveInAppPurchase", result);

                if (IsFunctionResultSuccessful(result))
                {
                    var resultData = result.Data as Dictionary<object, object>;
                    bool alreadyExists = (bool)resultData["alreadyExists"];

                    if (alreadyExists)
                    {
                        Debug.Log($"구매 정보가 이미 존재합니다: {product.definition.id}");
                    }
                    else
                    {
                        Debug.Log("인앱 구매 저장 성공");
                        UpdateLocalPurchaseData(product.definition.id);
                    }

                    return true;
                }
                else
                {
                    Debug.LogWarning("인앱 구매 저장 실패");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"인앱 구매 저장 중 오류 발생: {ex.Message}");
                return false;
            }
        }

        // 인앱결제 후 게임에 적용
        public void UpdateLocalPurchaseData(string productId, bool restore = false)
        {
            MainThreadDispatcher.RunOnMainThread(() =>
            {
                switch (productId)
                {
                    case IAPManager.superPass:
                        _data.hasSuperPass = true;
                        _data.superPassExpiration = DateTime.UtcNow.AddMonths(1).ToString("o");
                        if (!restore)
                        {
                            SoundManager.I.StopSound(SoundType.BGM);
                            PopupManager.I.ClosePopup(PopupNames.SuperPassPopup);
                            MainManager.I.lobby?.ShowSuperPassEffect();
                        }

                        break;
                    default:
                        Debug.LogWarning($"알 수 없는 제품 ID: {productId}");
                        break;
                }
            });
        }

        #endregion

        #region 인벤토리

        public int[] GetTowerDeck => _data.towerDeck;

        public int TowerDeck(int index) => _data.towerDeck[index];
        public int SkillDeck(int index) => _data.skillDeck[index];

        public int ToggleTowerInDeck(int towerIndex)
        {
            int size = _data.towerDeck.Length;
            for (int i = 0; i < size; ++i)
            {
                if (_data.towerDeck[i] == towerIndex)
                {
                    _data.towerDeck[i] = 0;
                    return 0;
                }
            }

            for (int i = 0; i < size; ++i)
            {
                if (_data.towerDeck[i] == 0)
                {
                    _data.towerDeck[i] = towerIndex;
                    return towerIndex;
                }
            }

            return 0;
        }

        public int ToggleSkillInDeck(int skillIndex)
        {
            int size = _data.skillDeck.Length;
            for (int i = 0; i < size; ++i)
            {
                if (_data.skillDeck[i] == skillIndex)
                {
                    _data.skillDeck[i] = 0;
                    return 0;
                }
            }

            for (int i = 0; i < size; ++i)
            {
                if (_data.skillDeck[i] == 0)
                {
                    _data.skillDeck[i] = skillIndex;
                    return skillIndex;
                }
            }

            return 0;
        }

        #endregion

        #region 게임 결과 저장

        /// <summary>
        /// 솔로 모드 결과 저장 (OnCall 함수 사용)
        /// </summary>
        public async Task SaveGameResult(GameResult result)
        {
            try
            {
                var saveGameResultFunction = functions.GetHttpsCallable("saveGameResult");

                // GameResult를 Dictionary로 변환
                var data = result.ToDictionary();

                var functionResult = await saveGameResultFunction.CallAsync(data);

                if (IsFunctionResultSuccessful(functionResult))
                {
                    Debug.Log("게임 결과 저장 성공");

                    // 함수 결과에서 데이터 추출 및 파싱
                    if (functionResult.Data is Dictionary<object, object> resultData &&
                        resultData.ContainsKey("success") && (bool)resultData["success"])
                    {
                        var updatedUserData = resultData["updatedUserData"] as Dictionary<object, object>;
                        if (updatedUserData != null)
                        {
                            // PlayerData 업데이트
                            if (updatedUserData.ContainsKey("elo"))
                                _data.eloRating = Convert.ToInt32(updatedUserData["elo"]);
                            if (updatedUserData.ContainsKey("trophy"))
                                _data.trophy = Convert.ToInt32(updatedUserData["trophy"]);
                            if (updatedUserData.ContainsKey("soloRank"))
                                _data.soloRank = (int)Enum.Parse(typeof(GameRank),
                                    updatedUserData["soloRank"].ToString());
                            if (updatedUserData.ContainsKey("pvpRank"))
                                _data.pvpRank = Convert.ToInt32(updatedUserData["pvpRank"]);
                            
                            // 랭크 상승 및 최고 기록 확인
                            bool pvpRankUp = resultData.ContainsKey("pvpRankUp") && (bool)resultData["pvpRankUp"];
                            bool soloRankUp = resultData.ContainsKey("soloRankUp") && (bool)resultData["soloRankUp"];

                            if (pvpRankUp)
                            {
                                Debug.Log("PvP 랭크가 상승했습니다!");
                            }

                            if (soloRankUp)
                            {
                                Debug.Log("솔로 랭크가 상승했습니다!");
                                result.rank = (GameRank)_data.soloRank;
                                _data.bestGameResult = result;
                                MainManager.I.UpdateLobbyUI();
                            }
                        }
                        else
                        {
                            Debug.Log("이미 처리된 게임 결과이거나 업데이트할 데이터가 없습니다.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("게임 결과 저장 실패 또는 잘못된 응답 형식");
                    }
                }
                else
                {
                    Debug.LogWarning("게임 결과 저장 실패");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"게임 결과 저장 중 오류 발생: {ex.Message}");
            }
        }

        #endregion

        #region 스테미너

        private const int MaxStamina = 10; // 최대 스테미너 
        const float RechargeRateInMinutes = 30f; // 스테미너 회복 시간 30분

        public bool IsFullStamina => _data.stamina == _data.STAMINA_MAX;
        
        private void HandleStaminaChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.DatabaseError != null)
            {
                Debug.LogError(e.DatabaseError.Message);
                return;
            }

            // 새로운 스태미나 값 가져오기
            if (e.Snapshot.Value != null)
            {
                int newStamina = int.Parse(e.Snapshot.Value.ToString());
                _data.stamina = newStamina;
                MainManager.I.UpdateLobbyUI();
                Debug.Log("Stamina has changed to: " + newStamina);
                ScheduleStaminaUpdate();
            }
        }

        private void ScheduleStaminaUpdate()
        {
            if (_data.stamina < MaxStamina)
            {
                float waitTime = CalculateTimeUntilNextUpdate(_data.stamina, DateTime.Parse(_data.lastActivityTime, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal));
                Invoke("UpdateStamina", waitTime);
            }
        }

        // 다음 스테미너 업데이트주기
        public float CalculateTimeUntilNextUpdate(int currentStamina, DateTime lastActivityTime)
        {
            if (currentStamina >= MaxStamina)
            {
                return float.MaxValue; // 스태미나가 이미 최대치이면 업데이트 필요 없음
            }

            DateTime now = DateTime.UtcNow;
            TimeSpan timeSinceLastActivity = now - lastActivityTime;

            // 마지막 활동 이후 회복된 스태미나 계산
            int recoveredStamina = (int)(timeSinceLastActivity.TotalMinutes / RechargeRateInMinutes);
            int effectiveCurrentStamina = Math.Min(MaxStamina, currentStamina + recoveredStamina);

            if (effectiveCurrentStamina >= MaxStamina)
            {
                return 0f; // 이미 최대치로 회복되었으면 즉시 업데이트
            }

            // 다음 스태미나 회복까지 남은 시간 계산
            float minutesUntilNextRecharge = RechargeRateInMinutes -
                                             (float)(timeSinceLastActivity.TotalMinutes % RechargeRateInMinutes);

            // 초 단위로 변환
            float secondsUntilNextUpdate = minutesUntilNextRecharge * 60f;

            return Math.Max(0f, secondsUntilNextUpdate);
        }

        /// 스테미너 업데이트 (OnCall 함수 사용)
        public async Task UpdateStamina()
        {
            try
            {
                var updateStaminaFunction = functions.GetHttpsCallable("updateStamina");
                var result = await updateStaminaFunction.CallAsync(null);

                LogFunctionResult("updateStamina", result);

                if (IsFunctionResultSuccessful(result))
                {
                    Debug.Log("스테미너 업데이트 성공");
                }
                else
                {
                    Debug.LogWarning("스테미너 업데이트 실패");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"스테미너 업데이트 중 오류 발생: {ex.Message}");
            }
        }

        /// 광고를 보고 스테미너 추가 (OnCall 함수 사용)
        public async Task AddStaminaAfterAd()
        {
            try
            {
                var addStaminaFunction = functions.GetHttpsCallable("addStaminaAfterAd");
                var result = await addStaminaFunction.CallAsync(null);

                LogFunctionResult("addStaminaAfterAd", result);

                if (IsFunctionResultSuccessful(result))
                {
                    Debug.Log("광고 후 스테미너 추가 성공");
                }
                else
                {
                    Debug.LogWarning("광고 후 스테미너 추가 실패");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"광고 후 스테미너 추가 중 오류 발생: {ex.Message}");
            }
        }

        /// 게임 시작 시 스테미너 소비 (OnCall 함수 사용)
        public async Task ConsumeStaminaOnGameStart()
        {
            try
            {
                var consumeStaminaFunction = functions.GetHttpsCallable("consumeStaminaOnGameStart");
                var data = new Dictionary<string, object>
                {
                    { "gameType", (int)MainManager.I.gameType }
                };
                var result = await consumeStaminaFunction.CallAsync(data);

                LogFunctionResult("consumeStaminaOnGameStart", result);

                if (IsFunctionResultSuccessful(result))
                {
                    Debug.Log("게임 시작 시 스테미너 소비 성공");
                }
                else
                {
                    Debug.LogWarning("게임 시작 시 스테미너 소비 실패");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"게임 시작 시 스테미너 소비 중 오류 발생: {ex.Message}");
            }
        }

        #endregion

        #region 공용

        /// functions 함수 결과 로깅
        private void LogFunctionResult(string functionName, HttpsCallableResult result)
        {
            Debug.Log($"{functionName} function result:");

            // result가 null인지 확인
            if (result == null)
            {
                Debug.LogWarning("함수 결과가 null입니다.");
                return;
            }

            // result.Data가 null인지 확인
            if (result.Data == null)
            {
                Debug.Log("No data returned");
                return;
            }

            if (result.Data is IDictionary<object, object> objectData)
            {
                foreach (var item in objectData)
                {
                    Debug.Log($"{functionName}: {item.Key}, {item.Value}");
                }
            }
            // result.Data가 bool 형식인 경우 처리
            else if (result.Data is bool boolResult)
            {
                Debug.Log($"Result: {boolResult}");
            }
            // 그 외의 형식인 경우 처리
            else
            {
                Debug.LogWarning($"예상치 못한 함수 결과 형식: {result.Data.GetType()}");
            }
        }

        /// Firebase Functions 호출 결과의 성공 여부를 확인합니다.
        private bool IsFunctionResultSuccessful(HttpsCallableResult result)
        {
            // result가 null인 경우 실패로 간주
            if (result == null)
            {
                Debug.LogWarning("함수 결과가 null입니다.");
                return false;
            }

            // result.Data가 null인 경우 실패로 간주
            if (result.Data == null)
            {
                Debug.LogWarning("함수 결과 데이터가 null입니다.");
                return false;
            }

            // result.Data가 IDictionary<string, object> 형식인 경우
            if (result.Data is IDictionary<string, object> data)
            {
                // 'success' 키가 있고 그 값이 true인 경우
                if (data.TryGetValue("success", out object successValue) && successValue is bool success)
                {
                    return success;
                }

                // 'error' 키가 없는 경우도 성공으로 간주
                return !data.ContainsKey("error");
            }
            // result.Data가 Dictionary<object, object> 형식인 경우 처리
            else if (result.Data is IDictionary<object, object> objectData)
            {
                // 'success' 키가 있고 그 값이 true인 경우
                if (objectData.TryGetValue("success", out object successValue) && successValue is bool success)
                {
                    return success;
                }

                // 'error' 키가 없는 경우도 성공으로 간주
                return !objectData.ContainsKey("error");
            }
            // result.Data가 bool 형식인 경우 (일부 Firebase Functions에서 직접 bool을 반환할 수 있음)
            else if (result.Data is bool boolResult)
            {
                return boolResult;
            }

            // 예상치 못한 형식인 경우 실패로 간주하고 로그 남김
            Debug.LogWarning($"예상치 못한 함수 결과 형식: {result.Data.GetType()}");
            return false;
        }

        #endregion
    }
}