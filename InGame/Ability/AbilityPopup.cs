using System.Collections;
using System.Linq;
using Photon.Pun;

using RandomFortress.Data;

using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace RandomFortress
{
    public class AbilityPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI explaneText;
        [SerializeField] private Transform ChociePannel;
        [SerializeField] private AbilityCard[] abilityCards; // 화면에 보여지는 선택가능한 어빌카드
        [SerializeField] private Transform abilityTrf; // 플레이어에게 보여줄 어빌카드
        private AbilityCard[] cards;

        private bool isSelect = false;
        private bool isChoice = false;
        private AbilityCard selectCard = null;
        // private int selectCount = 0;
        private bool isAllReady = false;

        private void Awake()
        {
            int i = 0;
            cards = new AbilityCard[abilityTrf.childCount];
            foreach (var child in abilityTrf.GetChildren())
            {
                cards[i++] = child.GetComponent<AbilityCard>();
            }
        }

        private void Reset()
        {
            //TODO: 문자열 하드코딩
            explaneText.text = "Choice Ability !!";
            ChociePannel.SetActive(true);
        }
        
        public IEnumerator ShowAbilityCard(int loopCount = 1)
        {
            Reset();
            GameManager.Instance.PauseGame();

            for(int j=0; j<loopCount; ++j)
            {
                isChoice = false;
                selectCard = null;
                isSelect = false;

                float timer = 0f;

                int stageProcess = GameManager.Instance.myPlayer.stageProcess;
                int index = stageProcess % 10;
                bool isLock = !GameManager.Instance.myPlayer.IsAbilityBuff;

                for (int i = 0; i < GameConstants.AbilityCardCount; ++i)
                {
                    // 랜덤으로 등급을 선택
                    Rarity rarity;
                    if (i < 3)
                        rarity = (Rarity)Utils.ChooseWithProbabilities(GameConstants.RarityParcent);
                    else
                        rarity = (Rarity)Utils.ChooseWithProbabilities(GameConstants.LastRarityParcent);

                    // 등급 내에서 랜덤 선택
                    AbilityData data = GetRandomAbilityByRarity(rarity);
                    abilityCards[i].SetCard(data, isLock && i == 3); // 마지막 카드는 잠근다
                }

                gameObject.SetActive(true);

                while (isChoice == false)
                {
                    timer += Time.deltaTime;
                    if (timer >= 10f)
                    {
                        isChoice = true;
                        selectCard = abilityCards[0];
                        break;
                    }

                    yield return null;
                }

                GameManager.Instance.SelectAbilityCard(selectCard.data);
            }

            if (GameManager.Instance.gameType != GameType.Solo)
            {
                ChociePannel.SetActive(false);
                //TODO: 문자열 하드코딩
                explaneText.text = "Wait...";

                isAllReady = false;
                GameManager.Instance.ChangeReadyState(true);

                float timer = 0f;
                while (!isAllReady)
                {
                    timer += Time.deltaTime;
                    if (timer >= 10f)
                    {
                        Debug.Log("Long Wait AbilityTime!!");
                        PhotonManager.Instance.LeaveRoom();
                        break;
                    }
                    yield return null;
                }
            }
            
            GameManager.Instance.ResumeGame();
            gameObject.SetActive(false);
        }
        
        public IEnumerator WaitForGameStart(double startTime)
        {
            // 현재 Photon 네트워크 시각과 게임 시작 시각의 차이 계산
            double timeToWait = startTime - PhotonNetwork.Time;

            // 지정된 시간만큼 대기
            yield return new WaitForSeconds((float)timeToWait);

            // 모든 클라이언트에서 동시에 게임 시작
            isAllReady = true;
        }
        
        public AbilityData GetRandomAbilityByRarity(Rarity rarity)
        {
            SerializedDictionary<int, AbilityData> abilities = DataManager.Instance.abilityDataDic;
            
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
            if (isSelect)
            {
                if (selectCard == abilityCards[i])
                {
                    isChoice = true;
                    selectCard.SetSelect(false);   
                }
                else
                {
                    selectCard.SetSelect(false);
                    selectCard = abilityCards[i];
                    selectCard.SetSelect(true);
                }
            }
            else
            {
                isSelect = true;
                selectCard = abilityCards[i];
                selectCard.SetSelect(true);
            }
        }
    }
}