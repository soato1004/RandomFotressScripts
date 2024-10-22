using System.Collections;
using System.Linq;
using DG.Tweening;

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace RandomFortress
{
    public class AbilityPopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform choicePanel;
        [SerializeField] private AbilityCard[] abilityCards; // 화면에 보여지는 선택가능한 어빌카드
        [SerializeField] private Slider waitTimeSlider;
        [SerializeField] private TextMeshProUGUI waitTimeText;

        private bool isChoice = false;
        private AbilityCard selectCard = null;

        private float remainingTime;
        private Tween sliderTween;
        
        private const float ChoiceWaitTime = 10f;
        private const float MaxWaitTime = 20f;
        
        // 어빌리티 카드선택 완료 후 게임레디
        public IEnumerator ShowAbilityCard(int loopCount = 1)
        {
            gameObject.SetActive(true);
            choicePanel.SetActive(true);


            // 어빌리티 카드 선택루프
            for (int j = 0; j < loopCount; j++)
            {
                yield return ChoiceAbilityCor();
            }

            // 멀티 게임일경우, 둘다 선택 완료시까지 대기
            if (GameManager.I.gameType != GameType.Solo)
            {
                // Debug.Log($"Ability choice for player {GameManager.I.myPlayer.ActorNumber} Ready");
                choicePanel.SetActive(false);
                waitTimeSlider.gameObject.SetActive(false);
                titleText.text = LocalizationManager.I.GetLocalizedString("game_ability_explan");
            
                GameManager.I.GameReady();
            }
        }
        
        // 어빌리티 카드선택 코루틴
        private IEnumerator ChoiceAbilityCor()
        {
            isChoice = false;
            selectCard = null;
            waitTimeSlider.value = 1f;
            
            bool isAbilityBuffActive = GameManager.I.myPlayer.IsAbilityBuff;

            for (int i = 0; i < GameConstants.AbilityCardCount; i++)
            {
                Rarity rarity = ChooseRarity(i);
                AbilityData data = GetRandomAbilityByRarity(rarity);
                bool isLocked = !isAbilityBuffActive && i == GameConstants.AbilityCardCount - 1;
                abilityCards[i].SetCard(data, isLocked);
            }
            
            yield return RunChoiceTimer();

            if (!isChoice)
            {
                int availableCardCount = GameManager.I.myPlayer.IsAbilityBuff ? 3 : 2;
                selectCard = abilityCards[Random.Range(0, availableCardCount)];
                selectCard.SetSelect(true);
                isChoice = true;
            }

            GameManager.I.SelectAbilityCard(selectCard.data);
        }

        private Rarity ChooseRarity(int cardIndex)
        {
            float[] rarityProbabilities = cardIndex < 3 ? GameConstants.RarityParcent : GameConstants.LastRarityParcent;
            return (Rarity)Utils.ChooseWithProbabilities(rarityProbabilities);
        }

        public IEnumerator RunChoiceTimer()
        {
            float timer = 0f;
            waitTimeSlider.value = 100f;
            waitTimeSlider.gameObject.SetActive(true);

            sliderTween = DOTween.To(() => waitTimeSlider.value, x => waitTimeSlider.value = x, 0f, ChoiceWaitTime)
                .SetEase(Ease.Linear);

            while (!isChoice && timer < ChoiceWaitTime)
            {
                waitTimeText.text = ((int)(ChoiceWaitTime - timer)).ToString();
                yield return new WaitForSeconds(1f);
                timer += 1f;
            }

            if (sliderTween != null && sliderTween.IsActive())
                sliderTween.Kill();
        }

        private AbilityData GetRandomAbilityByRarity(Rarity rarity)
        {
            var abilitiesOfRarity = DataManager.I.abilityDataDic.Values
                .Where(a => a.rarity == rarity)
                .ToList();
            return abilitiesOfRarity[Random.Range(0, abilitiesOfRarity.Count)];
        }

        public void OnCardClick(int i)
        {
            if (isChoice) return;

            isChoice = true;
            selectCard = abilityCards[i];
            selectCard.SetSelect(true);
            StopCoroutine(ChoiceAbilityCor());
        }
    }
}