using System.Collections;
using System.Linq;
using RandomFortress.Common.Utils;
using RandomFortress.Constants;
using RandomFortress.Data;
using RandomFortress.Manager;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

namespace RandomFortress.Game
{
    public class AbilityController : MonoBehaviour
    {
        [SerializeField] private AbilityCard[] _abilityCards;
        [SerializeField] private AbilityCard[] _bottomCards;

        private bool isClick = false;
        private AbilityCard selectCard = null;
        private int selectCount = 0;
        
        public IEnumerator ShowAbilityCard()
        {
            GameManager.Instance.PauseGame();

            isClick = false;
            selectCard = null;
            
            int stageProcess = GameManager.Instance.myPlayer.stageProcess;
            int index = stageProcess % 10;
            bool isLock = !GameManager.Instance.myPlayer.IsAbilityBuff;
            
            for (int i = 0; i < GameConstants.AbilityCardCount; ++i)
            {
                // 랜덤으로 등급을 선택
                Rarity rarity;
                if (i < 3)
                    rarity = (Rarity)RandomUtil.ChooseWithProbabilities(GameConstants.RarityParcent);
                else
                    rarity = (Rarity)RandomUtil.ChooseWithProbabilities(GameConstants.LastRarityParcent);
                
                // 등급 내에서 랜덤 선택
                AbilityData data = GetRandomAbilityByRarity(rarity);
                _abilityCards[i].SetCard(data, isLock && i == 3); // 마지막 카드는 잠근다
            }

            gameObject.SetActive(true);
            
            while (isClick == false)
            {
                yield return null;
            }

            _bottomCards[selectCount++].SetCard(selectCard);
            
            gameObject.SetActive(false);
            
            GameManager.Instance.SelectAbilityCard(selectCard.data);
            GameManager.Instance.ResumeGame();
            
        }
        
        public AbilityData GetRandomAbilityByRarity(Rarity rarity)
        {
            SerializableDictionaryBase<int, AbilityData> abilities = DataManager.Instance.abilityDataDic;
            
            var filteredAbilities = abilities.Values.Where(a => a.rarity == rarity).ToList();
            if (filteredAbilities.Count == 0) return null;
            
            // 확률 값에 따라 어빌리티를 선택하기 위해 누적 확률 계산
            int totalPercent = filteredAbilities.Sum(a => a.percent);
            int randomPoint = Random.Range(0, totalPercent);
            int currentPoint = 0;

            foreach (var ability in filteredAbilities)
            {
                currentPoint += ability.percent;
                if (randomPoint <= currentPoint)
                {
                    return ability;
                }
            }

            return null;
        }
        
        public void OnCardClick(int i)
        {
            isClick = true;
            selectCard = _abilityCards[i-1];
        }
    }
}