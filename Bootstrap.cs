using System.Threading.Tasks;
using DG.Tweening;
using GoogleAds;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class Bootstrap : SceneBase
    {
        [SerializeField] private TextMeshProUGUI loadText;
        [SerializeField] private TextMeshProUGUI visionText;
        [SerializeField] private GameObject loginButtons;
        [SerializeField] private Image dim;
        
        void Start()
        {
            visionText.SetText("Ver. "+Application.version);
            InitializeSetting(); 
        }

        /// <summary> 게임 실행시 최초 실행 및 초기화 코드 </summary>
        private async void InitializeSetting()
        {
            // 모든 매니저 리셋
            _ = IAPManager.I;
            _ = LocalizationManager.I;
            _ = LoginManager.I;
            _ = DataManager.I;
            _ = ResourceManager.I;
            
            _ = MainManager.I;
            _ = PopupManager.I;
            _ = GoogleAdManager.I;
            _ = PhotonManager.I;
            _ = IAPManager.I;
            
            IAPManager.I.InitUnityIAP();
            
            // 로컬라이징
            await LocalizationManager.I.LoadTablesAsync();
            
            // 구글 Firebase 초기화
            loadText.text = LocalizationManager.I.GetLocalizedString("bootstrap_step_01");
            await LoginManager.I.Init();

            // 게임데이터 로드
            loadText.text = LocalizationManager.I.GetLocalizedString("bootstrap_step_02");
            _ = DataManager.I.LoadInfoAsync();

            // 게임 리소스 로드
            loadText.text = LocalizationManager.I.GetLocalizedString("bootstrap_step_03");
            _ = ResourceManager.I.LoadAllResourcesAsync();

#if UNITY_ANDROID || UNITY_IOS
            await Task.Delay(1000);
#endif

            loadText.text = LocalizationManager.I.GetLocalizedString("bootstrap_step_04");
            
            if (!LoginManager.I.IsAutoLogin)
            {
                // 로긴버튼 등장
                loginButtons.SetActive(true);
            }
        }

        public void OnGoogleLoginClick()
        {
            dim.gameObject.SetActive(true);
            dim.DOFade(0.8f, 0.25f);
            LoginManager.I.OnGoogleSignIn();
        }
    }
}