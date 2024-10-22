using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public enum PopupStartType
    {
        None,
        ScaleUp,
    }
    
    public class PopupBase : MonoBehaviour
    {
        [SerializeField] protected PopupNames popupName;
        [SerializeField] private PopupStartType type = PopupStartType.None;
        [SerializeField] private Image dim;
        [SerializeField] private Transform popup;
        
        protected virtual void Reset()
        {
        }
        
        public virtual void ShowPopup(params object[] values)
        {
            if (type == PopupStartType.ScaleUp && popup != null)
            {
                popup.SetActive(false);
                DOVirtual.DelayedCall(0.2f, () => popup.SetActive(true));
                dim?.DOFade(0.2f, 0.6f).From(0f);
            }
        }

        public virtual void ClosePopup()
        {
            PopupManager.I.ClosePopup(popupName);
            gameObject.SetActive(false);
        }
        
        public virtual void RemovePopup()
        {
            PopupManager.I.RemovePopup(PopupNames.NicknamePopup);
        }
    }
}