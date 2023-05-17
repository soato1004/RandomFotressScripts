using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Menu
{
    public class ButtonPage : MonoBehaviour
    {
        [SerializeField] private Sprite[] buttonSprites; // 0 : select, 1 : unselect
        [SerializeField] private bool select;

        private Image bg;

        
        public void SetSelect(bool value)
        {
            select = value;
            bg.sprite = buttonSprites[select ? 0 : 1];
        }

        public void UpdateState()
        {
            
        }
    }
}

