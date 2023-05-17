using System.Collections;
using DG.Tweening;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Image = UnityEngine.UI.Image;

namespace RandomFortress.Scene
{
    public class LogoMain : BaseMain
    {
        [SerializeField] private CanvasGroup canvasGroup;
        
        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            Time.timeScale = 1;
            StartCoroutine(StartCor());
        }

        private IEnumerator StartCor()
        {
            canvasGroup.DOFade(1, 0.5f).From(0);
            
            yield return new WaitForSeconds(1f);

            canvasGroup.DOFade(0, 0.25f);
            
            yield return new WaitForSeconds(0.5f);
            
            SceneManager.LoadScene(2);
        }
    }
}