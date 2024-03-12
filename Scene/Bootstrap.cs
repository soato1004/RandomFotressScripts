using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using GoogleAds;
using RandomFortress.Common;
using RandomFortress.Constants;
using RandomFortress.Game;
using RandomFortress.Manager;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace RandomFortress.Scene
{
    /// <summary> </summary>
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private Image dim;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI loadText;
        
        void Start()
        {
            // (int)Screen.currentResolution.refreshRateRatio.value
            Application.targetFrameRate = 60;
            Debug.Log("target FPS : "+Application.targetFrameRate);
            Initialize(); 
        }

        private void Initialize()
        {
            InitializeSetting();
        }

        /// <summary> 게임 실행시 최초 실행 및 초기화 코드 </summary>
        private async void InitializeSetting()
        {
            Time.timeScale = 1f;
            
            // 게임 메인 매니저 로드 및 모든 매니저 생성
            loadText.text = "Init Manager";
            InitializeManagerAll();
            await Observable.NextFrame(FrameCountType.EndOfFrame).ToUniTask();

            // 구글 애드몹 초기화
            loadText.text = "Init GoogleAdMob";
            GoogleAdMobController.Instance.Init();
            await Observable.NextFrame(FrameCountType.EndOfFrame).ToUniTask();

            // 게임데이터 로드
            loadText.text = "Load Data";
            await DataManager.Instance.LoadInfoAsync(); // 32개

            // 게임 리소스 로드
            loadText.text = "Load Resource";
            await ResourceManager.Instance.LoadAllResourcesAsync(); // 847개
            await ResourceManager.Instance.LoadAllResourcesAsync("GameSound");
            SoundManager.Instance.Reset();
            
            // 로컬라이징
            loadText.text = "Load LocalizationSettings";
            await LocalizationManager.Instance.LoadTablesAsync();
            
            // 계정정보 로드
            loadText.text = "Load AccountSetting";
            // MainManager.Instance.InitializeAccount();
            Account.Instance.Init();
            await Task.CompletedTask;

            
            loadText.text = "Complete";
            MainManager.Instance.ChangeScene(SceneName.Lobby);
        }
        
        public void InitializeManagerAll()
        {
            InitializeManager<DataManager>();
            InitializeManager<MainManager>();
            InitializeManager<ResourceManager>();
            InitializeManager<SoundManager>();
            InitializeManager<GoogleAdMobController>();
            // InitializeNetManager<PunManager>();
        }

        /// <summary> 해당 타입의 매니저를 초기화한다. </summary>
        private void InitializeManager<T>() where T : Singleton<T>
        {
            if (null != GetComponent<T>()) 
                return;
            
            Singleton<T>.Initialize();
            Singleton<T>.Instance.Reset();
        }
        
        private void InitializeNetManager<T>() where T : SingletonPun<T>
        {
            if (null != GetComponent<T>()) 
                return;

            SingletonPun<T>.Initialize();
            SingletonPun<T>.Instance.Reset();
        }
        
        /// <summary> 해당 타입의 매니저를 마무리한다. </summary>
        private void TerminateManager<T>(GameObject manager = null) where T : Singleton<T>
        {
            Singleton<T>.Instance.Terminate();
            if (manager != null)
                Destroy(Singleton<T>.Instance);
        }
    }
}