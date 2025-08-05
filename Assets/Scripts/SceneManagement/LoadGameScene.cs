using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGameScene : MonoBehaviour
{
    public string sceneName = "Game";

    private void Start()
    {
        StartCoroutine(LoadSceneWithDelay());
    }

    private IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }
}