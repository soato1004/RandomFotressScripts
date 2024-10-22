using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class ButtonBase : MonoBehaviour
    {
        public Button button;
        
        private void Awake()
        {
            button = GetComponent<Button>();
        }

        protected void ButtonLock(float time)
        {
            button.interactable = false;
            StartCoroutine(ButtonLockCor(time));
        }
        
        protected IEnumerator ButtonLockCor(float time)
        {
            yield return new WaitForSecondsRealtime(time);
            button.interactable = true;
        }
    }
}