using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Game
{
    public class BuildButton : ButtonBase
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI Cost;
    }
}