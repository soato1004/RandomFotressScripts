using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Functions;
using Google;
using UnityEngine;

namespace RandomFortress
{
    /// <summary>
    /// 구글or애플 로그인 후, Firebase 로그인을 진행하여 로그인 과정 수행
    /// </summary>
    public class LoginManager : Singleton<LoginManager>
    {
        public FirebaseAuth auth { get; private set; }
        public FirebaseUser user { get; private set; }
        public FirebaseFunctions functions { get; private set; }
        public DatabaseReference databaseReference { get; private set; }
        public DatabaseReference noticeRef { get; private set; }

        // Firebase Debug Token (Firebase 콘솔에서 생성한 토큰)
        private const string DebugToken = "0E012479-061D-4E0D-8697-3FE463BAD513"; // 이곳에 생성한 디버그 토큰을 넣습니다.
        
        private GoogleSignInConfiguration configuration;
        private bool isSignin = false; // 구글 및 firebase Login 성공시 true
        private bool isAutoLogin = false; // 구글 자동로그인
        public bool IsAutoLogin => isAutoLogin;
        
        string clientId = "YOUR_CLIENT_ID";
        string clientSecret = "YOUR_CLIENT_SECRET";
        private string redirectUri = "https://randomfortress.firebaseapp.com/__/auth/handler";

        public bool isLoadedData = false;
        public bool isLoadedResources = false;

        private const string FirstLoginKey = "IsFirstLogin";
        private const string LastDeviceIdKey = "LastDeviceId";
        private const string LastAppVersionKey = "LastAppVersion";
        
        public async UniTask Init()
        {
            await InitializeFirebaseAsync();
        }

        private async Task InitializeFirebaseAsync()
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebaseServicesAsync();
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        }

        private void InitializeFirebaseServicesAsync()
        {
            try
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                functions = FirebaseFunctions.DefaultInstance;
                
                // 구글 SDK를 활용한 로그인 기능 등록
                configuration = new GoogleSignInConfiguration
                {
                    RequestEmail = true,
                    RequestProfile = true,
                    RequestIdToken = true,
                    RequestAuthCode = true,
                    // must be web client ID, not android client ID
                    WebClientId = webClientId,
                };
                GoogleSignIn.Configuration = configuration;

                // 공지사항 구독
                noticeRef = FirebaseDatabase.DefaultInstance.GetReference("notices");
                SubscribeToNotices();
                
#if UNITY_EDITOR
#else
                StartCoroutine(AutoLogin());
#endif
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Firebase initialization failed: {ex.Message}");
            }
        }
        
        #region 공지사항 
        
        private void SubscribeToNotices()
        {
            noticeRef.ChildAdded += HandleNoticeAdded;
            noticeRef.ChildChanged += HandleNoticeChanged;
            noticeRef.ChildRemoved += HandleNoticeRemoved;
        }

        private void HandleNoticeAdded(object sender, ChildChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            // Add notice to local list or UI
            Debug.Log("Notice added: " + args.Snapshot.GetRawJsonValue());
        }

        private void HandleNoticeChanged(object sender, ChildChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            // Update notice in local list or UI
            Debug.Log("Notice changed: " + args.Snapshot.GetRawJsonValue());
        }

        private void HandleNoticeRemoved(object sender, ChildChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            // Remove notice from local list or UI
            Debug.Log("Notice removed: " + args.Snapshot.GetRawJsonValue());
        }
        
        #endregion
        
        #region 로그인
        
        // 구글 로그인
        public void OnGoogleSignIn()
        {
            if (isAutoLogin) return;
            
#if UNITY_EDITOR
            WebviewGoogleLogin();
            _ = LoginAsync();
#elif UNITY_ANDROID || UNITY_IOS
            StartCoroutine(SignInCoroutine());
#endif
        }
        
#if UNITY_EDITOR
        // PC 웹뷰 로그인
        private async void WebviewGoogleLogin()
        {
            // 웹뷰를 위한 화면조정
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            string authorizationCode = await WebViewManager.I.ShowWebView(webClientId, redirectUri);
            Screen.orientation = ScreenOrientation.Portrait;
            WebViewManager.I.HideWebView();

            if (!string.IsNullOrEmpty(authorizationCode))
            {
                string idToken = await ExchangeAuthorizationCodeForIdToken(authorizationCode);

                if (!string.IsNullOrEmpty(idToken))
                {
                    Debug.Log("ID Token: " + idToken);
                    FirebaseGoogleAuth(idToken);
                }
                else
                {
                    Debug.LogError("ID token is null or empty.");
                }
            }
            else
            {
                Debug.LogError("Authorization code is null or empty.");
            }
        }

        private async Task<string> ExchangeAuthorizationCodeForIdToken(string authorizationCode)
        {
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    { "code", authorizationCode },
                    { "client_id", webClientId },
                    { "client_secret", clientSecret },
                    { "redirect_uri", redirectUri },
                    { "grant_type", "authorization_code" }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = Newtonsoft.Json.Linq.JObject.Parse(responseString);
                    return responseJson["id_token"]?.ToString();
                }
                else
                {
                    Debug.LogError("Error exchanging authorization code: " + responseString);
                    return null;
                }
            }
        }
#endif

        private IEnumerator AutoLogin()
        {
            Debug.Log("자동 로그인 시도");
            // 먼저 자동 로그인 시도
            var signInTask = GoogleSignIn.DefaultInstance.SignInSilently();
            yield return new WaitUntil(() => signInTask.IsCompleted);
        
            if (signInTask.IsCompleted)
            {
                Debug.Log("자동 로그인 성공");
                isAutoLogin = true;
                FirebaseGoogleAuth(signInTask.Result.IdToken);
                _ = LoginAsync();
            }
            else
            {
                Debug.Log("자동 로그인 실패");
            }
        }
        
        private IEnumerator SignInCoroutine()
        {
            // 로그인 시도
            var signInTask = GoogleSignIn.DefaultInstance.SignIn();
            yield return new WaitUntil(() => signInTask.IsCompleted);
            
            if (signInTask.IsFaulted)
            {
                Debug.LogError("Google Sign-In 오류: " + signInTask.Exception);
            }
            else if (signInTask.IsCanceled)
            {
                Debug.Log("Google Sign-In 취소됨");
            }
            else
            {
                FirebaseGoogleAuth(signInTask.Result.IdToken);
                _ = LoginAsync();
            }
        }
        
        private void FirebaseGoogleAuth(string idToken)
        {
            Debug.Log("FirebaseGoogleAuth 진입");
            Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task => {
                if (task.IsCanceled) {
                    Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                    return;
                }

                user = task.Result;
                isSignin = true;
                
                Debug.Log("FirebaseGoogleAuth 인증성공");
            });
        }
        
        // 구글 and firebase 로그인 성공 후 firebase 계정 생성 or 정보 로드
        private async UniTask LoginAsync()
        {
            // 구글 and firebase 로그인 성공까지 대기
            await UniTask.WaitUntil(() => isSignin);

            // 계정 존재 여부 확인 및 데이터 로드
            bool hasUser = await Account.I.CheckUserAccount();

            // 데이터 및 리소스 로드 완료까지 대기
            await UniTask.WaitUntil(() => isLoadedData && isLoadedResources);
            
            // 유저 정보가 없다면
            if (!hasUser)
            {
                // 닉네임 설정 및 계정생성 팝업
                PopupManager.I.ShowPopup(PopupNames.NicknamePopup);
        
                // 계정생성 대기
                await UniTask.WaitUntil(() => Account.I.IsAccountCreated);
            }

            // 최초 로그인시 구매복원
            CheckAndRestorePurchases();
            
            // 씬 전환
            MainManager.I.ChangeScene(SceneName.Lobby);
        }

        //TODO: 복구할것이 있다면 await로 잡아놔야한다
        public void CheckAndRestorePurchases()
        {
            bool isFirstLogin = !PlayerPrefs.HasKey(FirstLoginKey);
            string currentDeviceId = SystemInfo.deviceUniqueIdentifier;
            string lastDeviceId = PlayerPrefs.GetString(LastDeviceIdKey, "");
            string currentVersion = Application.version;
            string lastVersion = PlayerPrefs.GetString(LastAppVersionKey, "");

            if (isFirstLogin || currentDeviceId != lastDeviceId || currentVersion != lastVersion)
            {
                Debug.Log("New login, device, or app version detected. Restoring purchases...");
                IAPManager.I.RestorePurchases();
            
                PlayerPrefs.SetInt(FirstLoginKey, 1);
                PlayerPrefs.SetString(LastDeviceIdKey, currentDeviceId);
                PlayerPrefs.SetString(LastAppVersionKey, currentVersion);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.Log("Familiar environment. Skipping purchase restoration.");
            }
        }
        
        #endregion
    }
}
