using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public static event Action LevelCompleted;
    public static event Action LevelFailed;
    public static event Action LevelStarted;

    public int currentLevel=1;
    public GameObject nextLevelPanel;
    public GameObject mainMenuPanel;
    public GameObject gameFinishedPanel;
    public TMPro.TMP_Text nextLevelButtonText;
    public List<GameObject> levelPrefabs = new List<GameObject>();
    public GameObject level;
    public Transform levelPrefabPos;
    public bool isOpenPanel;

    private void Awake()
    {
        instance = this;

        LevelCompleted += OnLevelCompleted;
    }

    private void OnDestroy()
    {
        LevelCompleted -= OnLevelCompleted;
    }
    public static void TriggerLevelCompleted()
    {
        LevelCompleted?.Invoke();
    }
    private void OnLevelCompleted()
    {
        Debug.Log("Level tamamlandý!");
        currentLevel++;
        if (currentLevel > levelPrefabs.Count)
        {
            Debug.Log("Tüm seviyeler tamamlandý!");
            gameFinishedPanel.SetActive(true);
            isOpenPanel = true;
        }
        else
        {
            nextLevelButtonText.text = "Next Level " + currentLevel;
            nextLevelPanel.SetActive(true);
            isOpenPanel = true;
        }
        
    }
    public void StartLevel()
    {
        Debug.Log("Level baþlatýldý!");
        //LevelStarted?.Invoke();
        if(currentLevel==1)
        {
            level = Instantiate(levelPrefabs[currentLevel - 1], levelPrefabPos.position, Quaternion.identity);
            mainMenuPanel.SetActive(false);
            isOpenPanel = false;
        }
        else
        {
            Destroy(level);
            level = Instantiate(levelPrefabs[currentLevel - 1], levelPrefabPos.position, Quaternion.identity);
            nextLevelPanel.SetActive(false);
            isOpenPanel = false;
        }

    }
}
