using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RandomFortress
{
    public class CommonPopup : PopupBase
    {
        [SerializeField] protected TextMeshProUGUI titleText;
        [SerializeField] protected TextMeshProUGUI bodyText;
        [SerializeField] protected TextMeshProUGUI buttonText;
        
        [SerializeField] protected Button closeButton;
        [SerializeField] protected Button oneButton;
        [SerializeField] protected Button twoButton;
        
        protected override void Reset()
        {
            
        }
        
        public override void ShowPopup(params object[] values)
        {
            closeButton?.onClick.RemoveAllListeners();
            closeButton?.onClick.AddListener(ClosePopup);

            // 첫버튼
            if (values.Length > 0 && values[0] is UnityAction primaryAction)
            {
                oneButton?.onClick.RemoveAllListeners();
                oneButton?.onClick.AddListener(primaryAction);
            }
            else
            {
                oneButton?.onClick.AddListener(ClosePopup);
            }
            
            // 두번째버튼
            if (values.Length > 1 && values[1] is UnityAction secondaryAction)
            {
                twoButton?.onClick.RemoveAllListeners();
                twoButton?.onClick.AddListener(secondaryAction);
            }

            // 타이틀
            if (values.Length > 2 && values[2] is string title)
            {
                titleText.text = title;
            }
            
            // 본문
            if (values.Length > 3 && values[3] is string body)
            {
                bodyText.text = body;
            }
            
            // 버튼
            if (values.Length > 4 && values[4] is string button)
            {
                buttonText.text = button;
            }
            
            base.ShowPopup();
        }
    }
}