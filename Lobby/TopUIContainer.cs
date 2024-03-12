using System;
using RandomFortress.Game;
using RandomFortress.Manager;
using TMPro;
using UnityEngine;

namespace RandomFortress.Menu
{
    public class TopUIContainer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI maxStageText;
        [SerializeField] private TextMeshProUGUI ClearTimeText;
        [SerializeField] private TextMeshProUGUI RankText;

        void Start()
        {
        }

        private void OnEnable()
        {
            UpdateUI();
        }

        public void UpdateUI()
        {
            maxStageText.text = Account.Instance.Result.maxClearStage.ToString();
            int minute = Account.Instance.Result.clearTime / 60;
            int second = Account.Instance.Result.clearTime % 60;
            ClearTimeText.text = minute + " : " + second.ToString("D2");
        }
    }
}