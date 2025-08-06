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

    [Header("Buttons")]
    [SerializeField] private Button shopButton;
    [SerializeField] private Button rankButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button playButton;

    [Header("Texts")]
    public TMP_Text levelText;



    private void Awake()
    {
        shopButton.onClick.AddListener(ShopButton);
        rankButton.onClick.AddListener(RankButton);
        homeButton.onClick.AddListener(HomeButton);
        playButton.onClick.AddListener(PlayGameButton);
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
        shopPanel.SetActive(true);
    }

    private void RankButton()
    {
        CloseAllPanels();
        rankPanel.SetActive(true);
    }

    private void HomeButton()
    {
        CloseAllPanels();
        homePanel.SetActive(true);
    }

    public void PlayGameButton()
    {
        LevelManager.instance.StartLevel();
        HighlightCubes.instance.UpdateCubeList();
        SetGameHUD(false);
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
