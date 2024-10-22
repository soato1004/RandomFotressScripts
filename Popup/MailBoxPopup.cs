using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class MailBoxPopup : PopupBase
    {
        [SerializeField] protected Button closeButton;
        [SerializeField] protected Button deleteReadButton;
        [SerializeField] protected Button claimAllButton;
        [SerializeField] private GameObject noMailImage;
        
        protected override void Reset()
        {
            
        }
        
        public override void ShowPopup(params object[] values)
        {
            closeButton?.onClick.RemoveAllListeners();
            closeButton?.onClick.AddListener(Hide);
            
            deleteReadButton?.onClick.RemoveAllListeners();
            deleteReadButton?.onClick.AddListener(OnDeleteReadButtonClick);
            
            claimAllButton?.onClick.RemoveAllListeners();
            claimAllButton?.onClick.AddListener(OnClaimAllButtonClick);
            
            base.ShowPopup();
        }

        // 읽은 메일 삭제
        private void OnDeleteReadButtonClick()
        {
        }
        
        // 메일 보상 전부수령
        private void OnClaimAllButtonClick()
        {
        }

        
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}