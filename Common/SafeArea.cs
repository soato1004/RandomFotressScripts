using UnityEngine;


namespace RandomFortress
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        private RectTransform rectTransform;
        private float safeAreaYOffset;
    
        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        void ApplySafeArea()
        {
            if (rectTransform == null)
                return;
        
            Rect safeArea = Screen.safeArea;
        
            Debug.Log("세이프존Y : "+safeArea.y);
        
            Vector3 pos = rectTransform.transform.position;
            pos.y = -safeArea.y;
            rectTransform.transform.position = pos;
        }

        void OnRectTransformDimensionsChange()
        {
            ApplySafeArea();
        }

        void OnEnable()
        {
            ApplySafeArea();
        }
    }
}