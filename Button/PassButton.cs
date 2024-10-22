using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    // 로비에서 슈퍼패스, 광고제거 버튼에서 사용됨
    public class PassButton : ButtonBase
    {
        public PassType type;
        [SerializeField] private Image bg;
        [SerializeField] private Image applyPassBg; // 구입했거나 등등 이유로 구입불가능한 버튼상태
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI buttonText;

        // 
        public void SetApplyPass(bool active)
        {
            button.interactable = !active;
            bg.gameObject.SetActive(!active);
            applyPassBg.gameObject.SetActive(active);
            buttonText.color = active ? Color.white : Color.black;
        }
    }
}