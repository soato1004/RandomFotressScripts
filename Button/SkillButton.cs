
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class SkillButton : ButtonBase
    {
        [SerializeField] private GameObject ready;      // 스킬 사용가능시
        public GameObject frame;      // 스킬 테두리
        public GameObject icon;       // 스킬 아이콘
        public TextMeshProUGUI skillName;  // 스킬 이름
        public GameObject wait;       // 스킬 사용대기
        public TextMeshProUGUI coolTime; // 쿨타임 텍스트
        
        // 스킬버튼
        public void OnSkillButtonClick(int index)
        {
            GameUIManager.I.OnSkillButtonClick(index);
        }
    }
}