using System.Collections;
using System.Collections.Generic;
using RandomFortress.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Menu
{
    public class PageController : MonoBehaviour
    {
        [SerializeField] private BasePage currentPage;
        [SerializeField] private BasePage[] pages;
        [SerializeField] private Button[] pageButtons;
        
        public enum PageType
        {
            Shop,
            Inventory,
            Battle,
            Event,
            Clan
        };

        private PageType pageType = PageType.Battle;
        
        public BattlePage GetBattlePage => pages[(int)PageType.Battle] as BattlePage;

        void Start()
        {
            foreach (var page in pages)
            {
                page.Initialize();   
                // page.gameObject.SetActive(false);
            }

            // 최초 페이지
            //pageType = PageType.Battle;
            //currentPage = pages[(int)pageType];
            //currentPage.gameObject.SetActive(true);

            MainManager.Instance.PageController = this;
        }
        
        public void OnPageButtonClick(int type)
        {
            if ((int)pageType == type)
                return;

            // 이전버튼 비활성
            pageButtons[(int)pageType*2].gameObject.SetActive(true);
            pageButtons[(int)pageType*2+1].gameObject.SetActive(false);
            
            // 신규버튼 포커스
            pageButtons[type*2].gameObject.SetActive(false);
            pageButtons[type*2+1].gameObject.SetActive(true);

            pageType = (PageType)type;
            currentPage.gameObject.SetActive(false);
            currentPage = pages[(int)type];
            currentPage.UpdateUI();
            currentPage.gameObject.SetActive(true);
        }
    }
}