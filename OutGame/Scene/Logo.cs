using System.Collections;
using DG.Tweening;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RandomFortress.Scene
{
    public class Logo : MainBase
    {
        [SerializeField] private Image dim;
        [SerializeField] private CanvasGroup loadCanvasGroup;
        [SerializeField] private TextMeshProUGUI loadText;

        private void Start()
        {
            StartCoroutine(StartCor());
            SceneManager.LoadSceneAsync(SceneName.Bootstrap.ToString(), LoadSceneMode.Additive);
        }

        private IEnumerator StartCor()
        {
            dim.DOFade(1f, 1f);
            
            yield return new WaitForSeconds(1f);

            dim.DOFade(0.5f, 0.5f);
            loadCanvasGroup.DOFade(1, 0.5f);
            
            MainManager.Instance.ChangeScene(SceneName.Bootstrap);
            
        }
    }
}