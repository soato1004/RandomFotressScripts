using System;
using System.Collections;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.Serialization;
using Vuplex.WebView;

namespace RandomFortress
{
    public class WebViewManager : Singleton<WebViewManager>
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasWebViewPrefab webViewPrefab;
        // [SerializeField] private Keyboard keyboard;
        
        private string authUrl;
        
        void Awake()
        {
            // User-Agent 설정
#if UNITY_STANDALONE || UNITY_EDITOR
            // On Windows and macOS, change the User-Agent to mobile:
            Web.SetUserAgent(true);
#elif UNITY_IOS
            // On iOS, change the User-Agent to desktop:
            Web.SetUserAgent(false);
#else
            // Otherwise, change the User-Agent to a recent version of FireFox (Google blocks older versions).
            var firefox100ReleaseDate = DateTime.Parse("2022-05-03");
            var currentVersion = 100 + ((DateTime.Now.Year - firefox100ReleaseDate.Year) * 12) + DateTime.Now.Month - firefox100ReleaseDate.Month;
            Web.SetUserAgent($"Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:{currentVersion}.0) Gecko/20100101 Firefox/{currentVersion}.0");
#endif
        }

        public async Task<string> ShowWebView(string clientId, string redirectUri)
        {
            string authorizationCode = null;
            
            // WebViewPrefab 초기화 및 설정
            webViewPrefab = CanvasWebViewPrefab.Instantiate();
            webViewPrefab.Resolution = 1.5f;
            webViewPrefab.PixelDensity = 2;
            webViewPrefab.Native2DModeEnabled = true;
            webViewPrefab.transform.SetParent(canvas.transform, false);

            // Google OAuth 2.0 URL 구성
            authUrl = $"https://accounts.google.com/o/oauth2/auth?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope=openid%20email%20profile";
            
            var rectTransform = webViewPrefab.transform as RectTransform;
            rectTransform.anchoredPosition3D = Vector3.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            webViewPrefab.transform.localScale = Vector3.one;
            
            await webViewPrefab.WaitUntilInitialized();
            
            webViewPrefab.gameObject.SetActive(true);
            
            // Keybord 설정
            webViewPrefab.KeyboardEnabled = true;
            webViewPrefab.WebView.SetFocused(true);
            
            // URL 열기
            webViewPrefab.WebView.LoadUrl(authUrl);

            // URL 변경 이벤트 처리
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            webViewPrefab.WebView.UrlChanged += (sender, eventArgs) => {
                if (eventArgs.Url.StartsWith(redirectUri))
                {
                    authorizationCode = ExtractAuthorizationCode(eventArgs.Url);
                    tcs.SetResult(authorizationCode);
                }
            };

            return await tcs.Task;
        }
        
        public async Task LoadUrlInBackground(string url)
        {
            // 임시 WebView를 사용하여 URL 로드
            var tempWebView = CanvasWebViewPrefab.Instantiate();
            tempWebView.Resolution = 1.5f;
            tempWebView.PixelDensity = 2;
            tempWebView.Native2DModeEnabled = true;
            tempWebView.transform.SetParent(canvas.transform, false);
            tempWebView.transform.localScale = Vector3.one;

            await tempWebView.WaitUntilInitialized();

            tempWebView.gameObject.SetActive(true);
            tempWebView.WebView.LoadUrl(url);

            await Task.Delay(2000); // 로그아웃 페이지 로드 대기 시간
            Destroy(tempWebView.gameObject); // 임시 WebView 제거
        }

        
        private string ExtractAuthorizationCode(string url)
        {
            // URL에서 인증 코드를 추출
            var queryParams = new Uri(url).Query;
            var queryDict = System.Web.HttpUtility.ParseQueryString(queryParams);
            return queryDict["code"];
        }
        
        public void HideWebView() => webViewPrefab?.gameObject.SetActive(false);

        const string NOT_SUPPORTED_HTML = @"
            <body>
                <style>
                    body {
                        font-family: sans-serif;
                        display: flex;
                        justify-content: center;
                        align-items: center;
                        line-height: 1.25;
                    }
                    div {
                        max-width: 80%;
                    }
                    li {
                        margin: 10px 0;
                    }
                </style>
                <div>
                    <p>
                        Sorry, but this 3D WebView package doesn't support yet the <a href='https://developer.vuplex.com/webview/IWithPopups'>IWithPopups</a> interface. Current packages that support popups:
                    </p>
                    <ul>
                        <li>
                            <a href='https://developer.vuplex.com/webview/StandaloneWebView'>3D WebView for Windows and macOS</a>
                        </li>
                        <li>
                            <a href='https://developer.vuplex.com/webview/AndroidWebView'>3D WebView for Android</a>
                        </li>
                        <li>
                            <a href='https://developer.vuplex.com/webview/AndroidGeckoWebView'>3D WebView for Android with Gecko Engine</a>
                        </li>
                    </ul>
                </div>
            </body>
        ";
    }
}
