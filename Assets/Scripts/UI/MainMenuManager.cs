using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panells")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject rankPanel;
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button shopButton;
    [SerializeField] private Button rankButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;


    private void Awake()
    {
        shopButton.onClick.AddListener(ShopButton);
        rankButton.onClick.AddListener(RankButton);
        homeButton.onClick.AddListener(HomeButton);
        playButton.onClick.AddListener(PlayGameButton);
        settingsButton.onClick.AddListener(SettingsButton);
    }
    private void Start()
    {
        HomeButton();
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
        MainMenuSoundManager.Instance.PlayClick(); // Ses efektini çal
        homePanel.SetActive(true);
    }
    private void SettingsButton()
    {
        MainMenuSoundManager.Instance.PlayClick(); // Ses efektini çal
        settingsPanel.SetActive(true);
    }

    public void PlayGameButton() //ToDo: level Manager'a yönlendir ve hangi levela geçeceðini o söylesin.
    {
        //// Örn: oyun sahnesini yükle
        //UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");

        // 1. Level verisini bul
        /*Level targetLevel = LevelManager.Instance.currentLevel;

        if (targetLevel != null)
        {
            LevelManager.Instance.LoadLevel(targetLevel.LevelID);
        }
        else
        {
            Debug.LogError("Level ID "+targetLevel.LevelID+" bulunamadý!");
        }*/
        MainMenuSoundManager.Instance.PlayClick(); // Ses efektini çal
    }

    private void CloseAllPanels()
    {
        shopPanel.SetActive(false);
        rankPanel.SetActive(false);
        homePanel.SetActive(false);
    }
}
