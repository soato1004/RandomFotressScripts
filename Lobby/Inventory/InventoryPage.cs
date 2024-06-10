using System.Collections.Generic;

using RandomFortress.Data;


using UnityEngine;
using UnityEngine.Rendering;

namespace RandomFortress.Menu
{
    public class InventoryPage : BasePage
    {
        [SerializeField] private TowerSlot[] TowerDeck;
        [SerializeField] private SkillSlot[] SkillDeck;
        [SerializeField] private GameObject[] Taps;
        [SerializeField] private Transform ItemScrollTrf;

        private ItemSlot[] items;
        private CardCollection SelectTap = CardCollection.Tower;
        
        public enum CardCollection { Tower, Skill };

        void Start()
        {
        }

        public override void Initialize()
        {
            // 미리 생성된 아이템슬롯
            items = ItemScrollTrf.GetComponentsInChildren<ItemSlot>();
            TowerDeck = transform.GetComponentsInChildren<TowerSlot>();
            SkillDeck = transform.GetComponentsInChildren<SkillSlot>();
            
            UpdateInventory();
        }
        
        public void UpdateInventory()
        {
            UpdatePlayerDeck();
            UpdateScroll();
        }

        // 플레이어 덱
        void UpdatePlayerDeck()
        {
            for (int i = 0; i < GameConstants.TOWER_DECK_COUNT; ++i)
            {
                TowerDeck[i].UpdateSlot(i);
            }
            
            for (int i = 0; i < GameConstants.SKILL_DECK_COUNT; ++i)
            {
                SkillDeck[i].UpdateSlot(i);
            }
        }

        void UpdateScroll()
        {
            // 초기화
            for(int j=0; j<items.Length; ++j)
                items[j].SetEmpty();
            
            // 아이템 셋업
            int i = 0;
            switch (SelectTap)
            {
                case CardCollection.Tower:
                    SerializedDictionary<int, TowerData> TowerDatas = DataManager.Instance.towerDataDic;
                    foreach (var towerData in TowerDatas)
                    {
                        TowerData data = towerData.Value; 
                            items[i++].SetTower(data);
                    }
                    break;
                
                case CardCollection.Skill:
                    SerializedDictionary<int, SkillData> SkillDatas = DataManager.Instance.skillDataDic;
                    foreach (var skillData in SkillDatas)
                    {
                        SkillData data = skillData.Value; 
                        items[i++].SetSkill(data);
                    }
                    break;
            }
        }

        public void OnTapClick(int index) // 0: tower, 1: skill
        {
            bool towerActive = index == (int)CardCollection.Tower ? true : false;
            Taps[(int)CardCollection.Tower].SetActive(towerActive);
            Taps[(int)CardCollection.Skill].SetActive(!towerActive);

            SelectTap = towerActive ? CardCollection.Tower : CardCollection.Skill;

            UpdateScroll();
        }
        
        //TODO: 스크롤의 타워 또는 스킬을 클릭
        public void OnItemClick(int index)
        {
            // switch (SelectTap)
            // {
            //     case CardCollection.Tower:
            //         int towerIndex = items[index].itemIndex;
            //         Account.Instance.TowerDeckAddOrRemove(towerIndex);
            //         UpdatePlayerDeck();
            //         break;
            //     
            //     case CardCollection.Skill:
            //         int skillIndex = items[index].itemIndex;
            //         Account.Instance.SkillDeckAddOrRemove(skillIndex);
            //         UpdatePlayerDeck();
            //         break;
            // }
        }
        
        
    }
}
