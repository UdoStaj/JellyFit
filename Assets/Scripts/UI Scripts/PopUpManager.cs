using Ricimi;
using System;
using UnityEngine;
using UnityEngine.UI;
public class PopUpManager : MonoBehaviour
{
    [Header("UI Panells")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject coinsPanel;

    [Header("Buttons")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button coinsButton;

    private PopupOpener settingsPopupOpener;


    private void OnEnable()
    {
        settingsButton.onClick.AddListener(SettingsButton);
       // coinsButton.onClick.AddListener(CoinsButton);
        settingsPopupOpener = settingsButton.GetComponent<PopupOpener>();
    }

    private void SettingsButton()
    {
        CloseAllPanels();
       settingsPopupOpener.OpenPopup();
    }

    private void CoinsButton() // TODO: burasý settingsButtonunki gibi yaptýrýlacak.
    {
        CloseAllPanels();
        coinsPanel.SetActive(true);
    }


    private void CloseAllPanels()
    {
        settingsPanel.SetActive(false);
        coinsPanel.SetActive(false);
    }

}
