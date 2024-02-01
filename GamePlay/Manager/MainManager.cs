using Cysharp.Threading.Tasks;
using GoogleAds;
using RandomFortress.Common;
using RandomFortress.Common.Util;
using RandomFortress.Game.Netcode;
using RandomFortress.Scene;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RandomFortress.Manager
{
    /// <summary> 게임 전체에서 사용될 공통기능 매니저클래스 </summary>
    public class MainManager : Singleton<MainManager>
    {
        public SceneName preScene = SceneName.Bootstrap;
        public SceneName currentScene;
        
        public override void Reset()
        {
            JTDebug.LogColor("MainManager Reset");
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("MainManager Terminate");
        }
        
        public static async UniTask InitializeAndDefaultSetting()
        {
            Initialize();
            Instance.InitializeManagerAll();
            await Observable.NextFrame(FrameCountType.EndOfFrame).ToUniTask();
        }

        public void InitializeManagerAll()
        {
            InitializeManager<ResourceManager>();
            InitializeManager<DataManager>();
            InitializeManager<AudioManager>();
            InitializeManager<GoogleAdMobController>();
            
            InitializeNetManager<PunManager>();

            // Application.logMessageReceived += ApplicationLogMessageReceived;
            // Application.lowMemory += ApplicationLowMemory;

            // InitializeManager<APIManager>();
            // InitializeManager<AssetReferenceLoadManager>();
            // InitializeManager<AtlasBindManager>();
            // InitializeManager<GameDataManager>();
            // InitializeManager<CustomManager>();
            // InitializeManager<SceneLoadManager>();
            // InitializeManager<SoundManager>();
            // InitializeManager<VideoDownLoadManager>();
            //
            // SceneLoadState = SceneLoadManager.Instance.SceneLoadState;
            // // GameDataManager에 NetworkState가 변경이 되면 해당 Observer로 확인이 가능하다.
            // NetworkStateObserver = GameDataManager.Instance.NetworkStateObserver;
            //
            // UnityControlState = APIManager.Instance.UnityControlState;
            // UnityControlState.Subscribe(OnUnityControlState).AddTo(gameObject);
            //
            // ChangedUnityScene = APIManager.Instance.ChangedUnityScene;
            // ChangedUnityScene.Subscribe(OnChangeApplicationPause).AddTo(gameObject);
            //
            // _screenOrientationState = APIManager.Instance.ScreenOrientationState;
            // _screenOrientationState.SkipLatestValueOnSubscribe().Subscribe(OnScreenOrientationState).AddTo(gameObject);
            //
            // _appSettingsInfoData = APIManager.Instance.AppSettingsInfoData;
            // _appSettingsInfoData.Subscribe(OnChangeAppSettingsInfoData).AddTo(gameObject);
            //
            // // native에서 상태 값을 변경하면 GameDataManager에 전달한다.
            // _apiNetworkState = APIManager.Instance.NetworkState;
            // _apiNetworkState.Subscribe(OnChangeNetworkState).AddTo(gameObject);
            //
            // UIOrientation = APIManager.Instance.UIOrientation;
            // UIOrientation.Subscribe(OnUIOrientation).AddTo(gameObject);
        }

        /// <summary> 해당 타입의 매니저를 초기화한다. </summary>
        private void InitializeManager<T>() where T : Singleton<T>
        {
            if (null != GetComponent<T>()) return;
            // ReSharper disable once UnusedVariable
            // var addComponent = gameObject.AddComponent<T>();
            Singleton<T>.Initialize();
            Singleton<T>.Instance.Reset();
        }
        
        private void InitializeNetManager<T>() where T : SingletonPun<T>
        {
            if (null != GetComponent<T>()) return;
            // ReSharper disable once UnusedVariable
            // var addComponent = gameObject.AddComponent<T>();
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

        public void ChangeScene(SceneName name)
        {
            preScene = currentScene;
            currentScene = name;
            SceneManager.LoadScene(name.ToString());
        }
    }
}