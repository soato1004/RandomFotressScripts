using UnityEngine;
using UnityEngine.Rendering;

namespace RandomFortress
{
    public class PopupManager : Singleton<PopupManager>
    {
        [SerializeField] private SerializedDictionary<PopupNames, PopupBase> popupDic = new (); 
        [SerializeField] private Canvas canvas;
        
        private int popupOrder = 0;

        public bool isOpenPopup => popupOrder > 0;
        
        public PopupBase ShowPopup(PopupNames popupName, params object[] values)
        {
            PopupBase popup;
            if (popupDic.TryGetValue(popupName, out var value))
            {
                popup = value;
                popup.GetComponent<SortingGroup>().sortingOrder = ++popupOrder;
                popup.gameObject.SetActive(true);
            }
            else
            {
                GameObject go = Instantiate(ResourceManager.I.GetPrefab(popupName.ToString()), canvas.transform);
                popup = go.GetComponent<PopupBase>();
                go.ExGetOrAddComponent<SortingGroup>().sortingOrder = ++popupOrder;
                popupDic.Add(popupName, popup);
            }
            
            popup.ShowPopup(values);
            return popup;
        }

        public PopupBase GetPopup(PopupNames popupName) => popupDic[popupName];

        // 팝업 닫기
        public void ClosePopup(PopupNames popupName)
        {
            if (popupDic.ContainsKey(popupName))
            {
                --popupOrder;
            }
            else
            {
                Debug.LogWarning($"{popupName} 팝업이 존재하지 않습니다.");
            }
            
            // Debug.Log("ClosePopup : "+popupName+", order : "+popupOrder);
        }
        
        // 팝업 전부닫기
        public void CloseAllPopup()
        {
            foreach (var pair in popupDic)
            {
                PopupBase popup = pair.Value;
                popup.ClosePopup();
            }

            popupOrder = 0;
        }
        
        // 팝업 삭제 함수
        public void RemovePopup(PopupNames popupName)
        {
            if (popupDic.ContainsKey(popupName))
            {
                --popupOrder;
                PopupBase popup = popupDic[popupName];
                popupDic.Remove(popupName);
                Destroy(popup.gameObject);
                Debug.Log($"{popupName} 팝업이 삭제되었습니다.");
            }
            else
            {
                Debug.LogWarning($"{popupName} 팝업이 존재하지 않습니다.");
            }
        }
    }
}