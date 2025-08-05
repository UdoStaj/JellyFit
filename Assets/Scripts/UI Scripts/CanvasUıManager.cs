using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasUIManager : MonoBehaviour
{
    public static CanvasUIManager _instance;

    [Header("UiManagers")]
    public MainMenuManager mainMenuManager;
    public PopUpManager popUpManager;

   
    private void Awake()
    {
        // Singleton koruması ve DontDestroyOnLoad
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }


   
}
