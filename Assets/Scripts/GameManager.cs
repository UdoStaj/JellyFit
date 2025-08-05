using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public enum GameState { MainMenu, Playing, Paused, Win, Lose }
    public GameState CurrentState { get; private set; }

    public static event Action<GameState> OnGameStateChanged;

    [Header("Auto Start Settings")]
    [SerializeField] private bool autoStartGame = true;
    [SerializeField] private int startLevelIndex = 0;
    private int _money;
    public int Money { get { return _money; } set { if (_money > 0) _money = value; } }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetGameState(GameState.MainMenu);

        if (autoStartGame)
        {
            StartGame();
        }
    }

    public void SetGameState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                break;

            case GameState.Win:
                Time.timeScale = 0f;
                break;

            case GameState.Lose:
                Time.timeScale = 0f;
                break;
        }
    }

    public void StartGame()
    {
        SetGameState(GameState.Playing);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(startLevelIndex);
        }
    }
    public void PauseGame()
    {
        SetGameState(GameState.Paused);
    }
    public void ResumeGame()
    {
        SetGameState(GameState.Playing);
    }
    public void WinGame()
    {
        startLevelIndex++;
        SetGameState(GameState.Win);
    }
    public void LoseGame()
    {
        SetGameState(GameState.Lose);
    }
    public void RestartLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.reloadLevel();
            SetGameState(GameState.Playing);
        }
    }

    public void LoadNextLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
            SetGameState(GameState.Playing);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
