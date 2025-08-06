using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NextLevelPanel : MonoBehaviour
{
    public Button nextLevelButton;
    public Button homeButton;
    public TMP_Text levelText;


    private void Start()
    {
        nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
        homeButton.onClick.AddListener(OnHomeButtonClicked);

        UpdateLevelText(LevelManager.instance.currentLevel);
        LevelManager.OnChangeLevel += UpdateLevelText;
    }

    private void OnHomeButtonClicked()
    {
        CanvasUIManager._instance.mainMenuManager.SetGameHUD(true);
        gameObject.SetActive(false);
    }

    private void UpdateLevelText(int obj)
    {
        levelText.text = $"LEVEL {obj-1}\n<color=white>COMPLETED</color>";
    }

    private void OnNextLevelButtonClicked()
    {
        LevelManager.instance.StartLevel();
        HighlightCubes.instance.UpdateCubeList();
        gameObject.SetActive(false);
    }
}
