using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Google;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RandomFortress
{
     // iOS      675648994797-qlmso70d1bo2h2vp8upus1lj39ufllkb.apps.googleusercontent.com
     // Android  675648994797-vifcun6rkfo3jdn310fvqllu060kc4lt.apps.googleusercontent.com
     // Web      675648994797-8mlctt7gcj626pb9mmrtoegl09fj73iq.apps.googleusercontent.com

    public class GoogleFirebaseManager : Singleton<GoogleFirebaseManager>
    {
        [SerializeField] private TextMeshProUGUI GoogleStatusText;
        
        private FirebaseAuth auth;
        private DatabaseReference databaseReference;
        
        private readonly string googleWebAPI = "675648994797-8mlctt7gcj626pb9mmrtoegl09fj73iq.apps.googleusercontent.com";
        private GoogleSignInConfiguration configuration;
        private bool isSignin = false;
        
        public void Init()
        {
             // Firebase 초기화
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    FirebaseApp app = FirebaseApp.DefaultInstance;
                    auth = FirebaseAuth.DefaultInstance; 
                    databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                    Debug.Log("Firebase initialization completed.");
                    
                     // 구글 SDK를 활용한 로그인 기능 등록
                    configuration = new GoogleSignInConfiguration
                    {
                        WebClientId = googleWebAPI, 
                        RequestIdToken = true
                    };
                }
                else
                {
                    Debug.Log("Firebase initialization failed: " + task.Result);
                }
            });
        }

        public override void Reset()
        {
        }

        public void OnGoogleSignIn()
        {
#if UNITY_EDITOR
            MainManager.Instance.ChangeScene(SceneName.Lobby);
#elif UNITY_ANDROID
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthFinished);
#elif UNITY_IOS
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthFinished);
#endif
        }

        void OnGoogleAuthFinished(Task<GoogleSignInUser> task)
        {
            if (task.IsFaulted)
            {
                 Debug.Log("Google Sign-In 오류: " + task.Exception);
            }
            else if (task.IsCanceled)
            {
                 Debug.Log("Google Sign-In 취소됨");
            }
            else
            {
                 Debug.Log("Google Sign-In 성공: " + task.Result.IdToken);
                FirebaseGoogleAuth(task.Result.IdToken);
            }
        }

        void FirebaseGoogleAuth(string idToken)
        {
            Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
            auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                     Debug.Log("Firebase 인증 취소됨");
                    return;
                }

                if (task.IsFaulted)
                {
                     Debug.Log("Firebase 인증 오류: " + task.Exception);
                    return;
                }

                FirebaseUser newUser = task.Result;
                 Debug.Log("Firebase 사용자 로그인: " + newUser.DisplayName + ", "+ newUser.UserId);
                CheckUserAccount(newUser);
            });
        }

        void CheckUserAccount(FirebaseUser user)
        {
            databaseReference.Child("users").Child(user.UserId).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.Log("데이터 불러오기 오류: " + task.Exception);
                    return;
                }

                if (!task.Result.Exists)
                {
                     // 계정이 없으므로 생성
                    CreateNewAccount(user);
                }
                else
                {
                     // 계정이 존재하므로 불러오기
                    LoadUserData(task.Result);
                }
                
                Debug.Log("Scene 로드 " + SceneName.Lobby.ToString());
                MainManager.Instance.ChangeScene(SceneName.Lobby);
                SceneManager.LoadScene(SceneName.Lobby.ToString());
            });
        }

        void CreateNewAccount(FirebaseUser user)
        {
            PlayerData newPlayerData = new PlayerData
            {
                id = user.UserId,
                nickname = user.DisplayName,
                isFirstPlay = true,
                isFirstAccountCreation = true
            };

            string json = JsonUtility.ToJson(newPlayerData);
            databaseReference.Child("users").Child(user.UserId).SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                     Debug.Log("계정 생성 오류: " + task.Exception);
                }
                else
                {
                     Debug.Log("새 계정이 성공적으로 생성되었습니다.");
                    Account.Instance.SetPlayerData(newPlayerData);
                    MainManager.Instance.ChangeScene(SceneName.Lobby);
                }
            });
        }

        void LoadUserData(DataSnapshot snapshot)
        {
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(snapshot.GetRawJsonValue());
            Debug.Log("계정 데이터 불러오기 성공: " + playerData.nickname);
            Account.Instance.SetPlayerData(playerData);
        }
    }
}