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
        nextLevelButtonText.text = "Next Level:"+currentLevel;
        nextLevelPanel.SetActive(true);
        isOpenPanel = true;
        
    }
    public void ReplayButton()
    {
        Debug.Log("Level tekrar oynatýlýyor!");
        //LevelFailed?.Invoke();
        StartLevel();
    }
    public void StartLevel()
    {
        Debug.Log("Level baþlatýldý!");
        //LevelStarted?.Invoke();

        int prefabIndex = (currentLevel - 1) % levelPrefabs.Count;

        if (level != null)
        {
            Destroy(level);
        }

        level = Instantiate(levelPrefabs[prefabIndex], levelPrefabPos.position, Quaternion.identity);

        if (currentLevel == 1)
        {
            mainMenuPanel.SetActive(false);
        }
        else
        {
            nextLevelPanel.SetActive(false);
        }

        isOpenPanel = false;

        Transform box = level.transform.Find("Animation");
        boxCloseCoverAnim = box.GetComponent<Animator>();
        boxCloseCoverAnim.SetBool("isOpen", true);
    }
}
