using System.Threading.Tasks;
using GoogleAds;
using TMPro;
using UnityEngine;

namespace RandomFortress
{
    public class Bootstrap : MonoBehaviour
    {
        // [SerializeField] private bool offlineMode = false;
        [SerializeField] private TextMeshProUGUI loadText;
        [SerializeField] private GameObject loginButtons;
        
        void Start()
        {
            Application.targetFrameRate = 60;
            // 화면이 자동으로 꺼지지 않도록 설정
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            JustDebug.Log("target FPS : "+Application.targetFrameRate);
            InitializeSetting(); 
        }

        /// <summary> 게임 실행시 최초 실행 및 초기화 코드 </summary>
        private async void InitializeSetting()
        {
            Time.timeScale = 1f;
            
            // 구글 Firebase 초기화
            loadText.text = "Init GoogleFirebaseManager";
            GoogleFirebaseManager.Instance.Init();
            await Task.CompletedTask;
            
            // 구글 애드몹 초기화
            loadText.text = "Init GoogleAdMob";
            GoogleAdMobController.Instance.Init();
            await Task.CompletedTask;

            // 게임데이터 로드
            loadText.text = "Load Data";
            await DataManager.Instance.LoadInfoAsync();

            // 게임 리소스 로드
            loadText.text = "Load Resource";
            await ResourceManager.Instance.LoadAllResourcesAsync();
            SoundManager.Instance.Reset();
            
            // 로컬라이징
            loadText.text = "Load LocalizationSettings";
            await LocalizationManager.Instance.LoadTablesAsync();
            
            // 계정정보 로드
            loadText.text = "Load AccountSetting";
            Account.Instance.Init();
            await Task.CompletedTask;

            
            loadText.text = "Complete"; 
            loginButtons.SetActive(true);
        }

        public void OnGoogleLoginClick()
        {
            GoogleFirebaseManager.Instance.OnGoogleSignIn();
        }
    }
}