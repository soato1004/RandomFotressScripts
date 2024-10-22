
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class BuildButton : ButtonBase
    {
        [SerializeField] private TextMeshProUGUI Cost;
        
        // 영웅타워를 랜덤으로 짓는다
        public void OnBuildButtonClick()
        {
            GameManager.I.BuildRandomTower();
        }
    }
}