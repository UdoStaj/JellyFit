using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panells")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject rankPanel;
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject navigationPanel;
    [SerializeField] private GameObject backgroundPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button shopButton;
    [SerializeField] private Button rankButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;

    [Header("Will Hide")]
    [SerializeField] private Button replayButton;

    [Header("Texts")]
    public TMP_Text levelText;



    private void Awake()
    {
        shopButton.onClick.AddListener(ShopButton);
        rankButton.onClick.AddListener(RankButton);
        homeButton.onClick.AddListener(HomeButton);
        playButton.onClick.AddListener(PlayGameButton);
        settingsButton.onClick.AddListener(SettingsButton);
        replayButton.onClick.AddListener(ReplayButton);
    }
    private void Start()
    {
        HomeButton();
        LevelManager.OnChangeLevel += UpdateLevelText;
        UpdateLevelText(LevelManager.instance.currentLevel);
    }

    private void UpdateLevelText(int obj)
    {
        levelText.text = obj.ToString();
    }

    private void ShopButton()
    {
        CloseAllPanels();
        MainMenuSoundManager.Instance.PlayClick(); // Ses efektini çal
        shopPanel.SetActive(true);
    }

    private void RankButton()
    {
        CloseAllPanels();
        MainMenuSoundManager.Instance.PlayClick(); // Ses efektini çal
        rankPanel.SetActive(true);
    }

    private void HomeButton()
    {
        CloseAllPanels();
        replayButton.gameObject.SetActive(false); // Tekrar oynatma butonunu gizle
        MainMenuSoundManager.Instance.PlayClick(); // Ses efektini çal
        homePanel.SetActive(true);
    }
    private void SettingsButton()
    {
        MainMenuSoundManager.Instance.PlayClick(); // Ses efektini çal
        settingsPanel.SetActive(true);
    }

    private void ReplayButton()
    {
        LevelManager.instance.ReplayButton();
        MainMenuSoundManager.Instance.PlayClick(); // Ses efektini çal
    }

    public void PlayGameButton()
    {
        LevelManager.instance.StartLevel();
        replayButton.gameObject.SetActive(true);
        HighlightCubes.instance.UpdateCubeList();
        SetGameHUD(false);
        MainMenuSoundManager.Instance.PlayClick(); // Ses efektini çal
    }

    private void CloseAllPanels()
    {
        shopPanel.SetActive(false);
        rankPanel.SetActive(false);
        homePanel.SetActive(false);
    }

    public void SetGameHUD(bool isActive)
    {
        CloseAllPanels();
        navigationPanel.SetActive(isActive);
        backgroundPanel.SetActive(isActive);
        homePanel.SetActive(isActive);
    }
}
