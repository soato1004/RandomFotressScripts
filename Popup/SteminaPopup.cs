using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RandomFortress
{
    public class SteminaPopup : PopupBase
    {
        [SerializeField] protected TextMeshProUGUI titleText;
        [SerializeField] protected TextMeshProUGUI bodyText;
        [SerializeField] protected TextMeshProUGUI buttonText;
        
        [SerializeField] protected Button closeButton;
        [SerializeField] protected Button oneButton;
        
        protected override void Reset()
        {
            
        }
        
        public override void ShowPopup(params object[] values)
        {
            closeButton?.onClick.RemoveAllListeners();
            closeButton?.onClick.AddListener(ClosePopup);

            if (values.Length > 0 && values[0] is UnityAction primaryAction)
            {
                oneButton?.onClick.RemoveAllListeners();
                oneButton?.onClick.AddListener(primaryAction);
                oneButton?.onClick.AddListener(ClosePopup);
            }
            
            base.ShowPopup();
        }
    }
}