using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Game
{
    public class BuildButton : ButtonBase
    {
        [SerializeField] private Button button;         // 버튼 스크립트
        [SerializeField] private TextMeshProUGUI Cost;      // 스킬 사용가능시
    }
}