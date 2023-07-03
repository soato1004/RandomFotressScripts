using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Game
{
    public class SkillButton : ButtonBase
    {
        public GameObject frame;      // 스킬 테두리
        public GameObject icon;       // 스킬 아이콘
        public TextMeshProUGUI skillName;  // 스킬 이름
        public GameObject wait;       // 스킬 사용대기
        public TextMeshProUGUI coolTime; // 쿨타임 텍스트
    }
}