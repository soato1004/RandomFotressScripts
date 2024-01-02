// using UnityEditor;
// using UnityEngine;
//
// namespace Common
// {
//     [InitializeOnLoad]
//     public static class EditorPlayButtonListener
//     {
//         static EditorPlayButtonListener()
//         {
//             EditorApplication.playModeStateChanged += PlayModeStateChanged;
//         }
//
//         private static void PlayModeStateChanged(PlayModeStateChange state)
//         {
//             if (state == PlayModeStateChange.EnteredPlayMode)
//             {
//                 // 플레이 모드로 전환될 때의 동작을 수행
//                 Debug.Log("Entered Play Mode");
//
//                 // SceneManager.LoadScene(0);
//             }
//             else if (state == PlayModeStateChange.ExitingPlayMode)
//             {
//                 // 플레이 모드에서 나갈 때의 동작을 수행
//                 Debug.Log("Exiting Play Mode");
//             }
//         }
//     }
// }