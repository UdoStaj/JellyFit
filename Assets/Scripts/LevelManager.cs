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
    public Animator boxCloseCoverAnim;

    public static event Action<int> OnChangeLevel;


    private void Awake()
    {
        instance = this;

        LevelCompleted += CloseBoxCoverAnim;
    }

    private void OnDestroy()
    {
        LevelCompleted -= CloseBoxCoverAnim;
    }
    public static void TriggerLevelCompleted()
    {
        LevelCompleted?.Invoke();
    }
    private void CloseBoxCoverAnim()
    {
        boxCloseCoverAnim.SetBool("isOpen", false);
    }
    public void OnLevelCompleted()
    {
        Debug.Log("Level tamamlandý!");
        currentLevel++;
        if (currentLevel > levelPrefabs.Count)
        {
            currentLevel = 1; // Reset to level 1 if all levels are completed
        }
        else
        {
            nextLevelButtonText.text = "Next Level " + currentLevel;
            nextLevelPanel.SetActive(true);
            isOpenPanel = true;
        }
        OnChangeLevel?.Invoke(currentLevel);

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
        Transform box = level.transform.Find("Animation");
        boxCloseCoverAnim = box.GetComponent<Animator>();
        boxCloseCoverAnim.SetBool("isOpen", true);
    }
}
