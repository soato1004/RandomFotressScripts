using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RandomFortress.Common.Util;
using RandomFortress.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RandomFortress.Scene
{
    /// <summary> </summary>
    public class BootstrapMain : MonoBehaviour
    {
        void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
            InitializeSetting();
        }
        
        /// <summary> 게임 실행시 최초 실행 및 초기화 코드 </summary>
        private async void InitializeSetting()
        {
            Time.timeScale = 1f;
            
            // 게임 메인 매니저
            await MainManager.InitializeAndDefaultSetting();
            
            // 버전 업데이트
            // await CheckForVersionUpdated();
            
            // 사운드매니저
            // await SoundManager.Instance.LoadContentsAudioClip(NAR, SoundManager.SoundType.Narration);
            
            // 
            // APIManager.Instance.InitializeFaceImageAnalysis();

            // 
            // APIManager.Instance.InitObserve();

            
            // 최초 테스크
            var initTaskList = new List<UniTask>() {
                // PlaySplashSound(),
                // Logo Animation Start
                // LogoAnimationStart(),
                // Default Game Data Initialize
                // DefaultGameDataInit(),
                
                DataManager.Instance.LoadInfo(),
                ResourceManager.Instance.LoadAllResourcesAsync(),
            };
            
            // init List에 있는 모든 작업을 완료 후에 다음 Scene로 이동 하도록 처리.
            var allTask = UniTask.WhenAll(initTaskList);
            try {
                await allTask;
            } catch (Exception e) {
                JTDebug.LogError($"알수 없는 Error {e}");
            }
            
            
            // await DelayCallUtils.NextEndOfFrame();
            // await GameMainManager.Instance.ManagerCameraReset();
            // await NotchBgShow();
            
            
            // 작업후 다음씬 이동
            if (allTask.Status == UniTaskStatus.Succeeded) {
                JTDebug.Log("All Work Complete Next Home Scene");
                GoNextScene();
            } else if (allTask.Status == UniTaskStatus.Faulted) {
                JTDebug.LogError("All Work Fail");
            } else {
                JTDebug.LogColor($"All Work Status {allTask.Status} ???");
            }
        }

        private void GoNextScene()
        {
            // TODO: Scene 이동전 데이터 로드를 따로한다.
            
            // TODO: Addreable로드가 필요한 Scene인지 체크 후 다운로드

            // TODO: 로딩 화면을 구현한다

            // TODO: Scene 이동
#if UNITY_EDITOR
            SceneManager.LoadScene(3);
#elif UNITY_ANDROID
            SceneManager.LoadScene(1);
#endif
        }
    }
}