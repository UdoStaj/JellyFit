using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public Slider loadingSlider;
    public TMP_Text progressText;
    public string sceneToLoad = "PlayScene";

    void Start()
    {
        StartCoroutine(LoadAsyncScene());
    }

    IEnumerator LoadAsyncScene()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingSlider.value = progress;
            progressText.text = (progress * 100f).ToString("F0") + "%";

            if (operation.progress >= 0.8f)
            {
                yield return new WaitForSeconds(1.5f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
