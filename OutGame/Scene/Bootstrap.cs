using GoogleAds;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Scene
{
    /// <summary> </summary>
    public class Bootstrap : MainBase
    {
        void Start()
        {
            // (int)Screen.currentResolution.refreshRateRatio.value
            Application.targetFrameRate = 144;
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
            
            // 게임 메인 매니저
            await MainManager.InitializeAndDefaultSetting();

            GoogleAdMobController.Initialize();

            await DataManager.Instance.LoadInfoAsync();

            await ResourceManager.Instance.LoadAllResourcesAsync();

            MainManager.Instance.ChangeScene(SceneName.Lobby);
            
            // 버전 업데이트
            // await CheckForVersionUpdated();
            // 사운드매니저
            // await SoundManager.Instance.LoadContentsAudioClip(NAR, SoundManager.SoundType.Narration);
            // APIManager.Instance.InitializeFaceImageAnalysis();
            // APIManager.Instance.InitObserve();
            // LoadingManager.Instance.Show;
            //
            // 최초 테스크
            // var initTaskList = new List<UniTask>() {
            //     PlaySplashSound(),
            //     // Logo Animation Start
            //     LogoAnimationStart(),
            //     // Default Game Data Initialize
            //     DefaultGameDataInit(),
            //     LoadNetwork(),
            // };
            //
            // // init List에 있는 모든 작업을 완료 후에 다음 Scene로 이동 하도록 처리.
            // var allTask = UniTask.WhenAll(initTaskList);
            // try {
            //     await allTask;
            // } catch (Exception e) {
            //     JTDebug.LogError($"알수 없는 Error {e}");
            // }
            //
            // await DelayCallUtils.NextEndOfFrame();
            // await GameMainManager.Instance.ManagerCameraReset();
            // await NotchBgShow();
            //
            // // 작업후 다음씬 이동
            // if (allTask.Status == UniTaskStatus.Succeeded) {
            //     JTDebug.Log("All Work Complete Next Home Scene");
            //     MainManager.Instance.ChangeScene(SceneName.Menu);
            // } else if (allTask.Status == UniTaskStatus.Faulted) {
            //     JTDebug.LogError("All Work Fail");
            // } else {
            //     JTDebug.LogColor($"All Work Status {allTask.Status} ???");
            // }
        }
    }
}