using System;
using UnityEngine;
using UnityEngine.UI;

public class GameHudManager : MonoBehaviour
{
    public static GameHudManager Instance { get; private set; }


    [Header("UI Panells")]
    [SerializeField] private GameObject winGame;
    [SerializeField] private GameObject loseGame;
    [SerializeField] private GameObject mainMenuPanel;
    //[SerializeField] private GameObject homePanel;


    [Header("Lose Buttons")]
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button losePanelBackHome;

    [Header("Win Buttons")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button winPanelBackHome;


    private void OnEnable()
    {
        tryAgainButton.onClick.AddListener(TryAgain);
        nextLevelButton.onClick.AddListener(NextLevel);
        losePanelBackHome.onClick.AddListener(BackHome);
        winPanelBackHome.onClick.AddListener(BackHome);
    }

    private void Awake()
    {
        Instance = this;
    }


    private void BackHome()
    {
        mainMenuPanel.SetActive(true);
        // homePanel.SetActive(true);
    }

    private void NextLevel()
    {
        LevelManager.Instance.NextLevel();
    }

    private void TryAgain()
    {
        LevelManager.Instance.reloadLevel();
    }

    public void OpenWinGamePanel()
    {
        winGame.SetActive(true);
        loseGame.SetActive(false);
        //homePanel.SetActive(false);
    }

    public void OpenLoseGamePanel()
    {
        loseGame.SetActive(true);
        winGame.SetActive(false);
    }
}
