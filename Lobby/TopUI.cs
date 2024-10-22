using System.Collections;
using System.Collections.Generic;
using RandomFortress;
using TMPro;
using UnityEngine;

public class TopUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI trophy;
    [SerializeField] private TextMeshProUGUI stamina;
    [SerializeField] private TextMeshProUGUI gold;
    [SerializeField] private TextMeshProUGUI gem;
    [SerializeField] private GameObject lobbyMenu;
    

    public void UpdateUI()
    {
        trophy.text = Account.I.Data.trophy.ToString();
        stamina.text = Account.I.Data.stamina + "/" + Account.I.Data.STAMINA_MAX;
        gold.text = Account.I.Data.gold.ToString();
        gem.text = Account.I.Data.gem.ToString();
    }

    public void OnTopButtonClick()
    {
        lobbyMenu.SetActive(!lobbyMenu.activeSelf);
    }

    public void OnSettingButtonClick()
    {
        PopupManager.I.ShowPopup(PopupNames.SettingPopup);
    }
    
    public void OnMailBoxButtonClick()
    {
        PopupManager.I.ShowPopup(PopupNames.MailBoxPopup);
    }
}
