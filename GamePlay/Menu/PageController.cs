using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomFortress.Menu
{
    public class PageController : MonoBehaviour
    {
        [SerializeField] private BasePage currentPage;
        [SerializeField] private BasePage[] pages;

        public enum PageType
        {
            Shop,
            Inventory,
            Battle,
            Event,
            Clan
        };

        private PageType pageType = PageType.Battle;

        public void OnPageButtonClick(int type)
        {
            if ((int)pageType == type)
                return;

            pageType = (PageType)type;
            currentPage.gameObject.SetActive(false);
            currentPage = pages[(int)type];
            currentPage.gameObject.SetActive(true);
        }
    }
}