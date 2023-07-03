using UnityEngine;
using Screen = UnityEngine.Device.Screen;

namespace RandomFortress.Common
{
    public class SafeAreaAdapter : MonoBehaviour
    {
        [SerializeField] private RectTransform TopUI;
        [SerializeField] private RectTransform BottomUI;

        void Start()
        {
            Rect safeArea = Screen.safeArea;

            Vector2 p = TopUI.anchoredPosition;
            p.y = -safeArea.y;
            TopUI.anchoredPosition = p;
        }
    }
}