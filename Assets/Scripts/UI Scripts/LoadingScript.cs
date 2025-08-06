using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScipt : MonoBehaviour
{
    //loading scene manage
    Slider progressBar;
    private void Awake()
    {
        progressBar = FindAnyObjectByType<Slider>();
    }
    private void Start()
    {
        LoadScene(PlayerPrefs.GetInt("LoadScene", 0));
    }

    public void LoadScene(int SceneIndex)
    {
        StartCoroutine(LoadSceneAsync(SceneIndex));
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;
            if (operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.5f);
                operation.allowSceneActivation = true;
            }
            yield return null;
        }

    }
}
