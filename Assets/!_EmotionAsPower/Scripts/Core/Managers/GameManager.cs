using System;
using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private bool isDebugMode;
    public bool IsDebugMode => isDebugMode;

    [SerializeField]
    private bool finishedSetup = false;
    public bool FinishedSetup => finishedSetup;

    public GameObject[] debugFeatures;

    [Header("Game State")]
    [SerializeField] private bool isPaused = false;
    [SerializeField] private float timeScaleBeforePause = 1f;
    [SerializeField] private float currentGameSpeed = 1f;

    // FPS calculation fields
    private float deltaTime = 0.0f;
    private float currentFPS = 0.0f;
    public float CurrentFPS => currentFPS; // Public property to access FPS

    public Action OnSetupFinished;
    public Action<bool> OnGamePaused;
    public Action<float> OnGameSpeedChanged;
    public bool IsPaused => isPaused;
    public float CurrentTimeScale => Time.timeScale;
    public float CurrentGameSpeed => currentGameSpeed;

    protected override void Awake()
    {
        base.Awake();
        PauseGame();
        StartCoroutine(DelayedSetup());
        StartCoroutine(UpdateFPSCalculation()); // Start FPS calculation coroutine
    }

    private void SetupAll()
    {
        finishedSetup = false;
        ContentManager.Instance.Setup();
        UIManager.Instance.Setup();
        DataPersistenceManager.Instance.Setup();
        ResumeGame();
        OnSetupFinished?.Invoke();
        finishedSetup = true;
        PlayerBase.Instance.OnPlayerBaseDestroyed += LoseGame;
        ExitDebug();
    }

    private IEnumerator DelayedSetup()
    {
        yield return null; // Wait one frame to let other scripts initialize
        SetupAll();
    }

    // Coroutine to calculate FPS
    private IEnumerator UpdateFPSCalculation()
    {
        while (true)
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            currentFPS = 1.0f / deltaTime;
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;

        timeScaleBeforePause = Time.timeScale;
        Time.timeScale = 0f;
        isPaused = true;

        Debug.Log("Game Paused");
        OnGamePaused?.Invoke(true);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        Time.timeScale = timeScaleBeforePause;
        isPaused = false;

        Debug.Log("Game Resumed");
        OnGamePaused?.Invoke(false);
    }

    public void EnterDebug()
    {
        isDebugMode = true;
        foreach (GameObject debugFeature in debugFeatures)
        {
            debugFeature.SetActive(true);
        }
    }

    public void ExitDebug()
    {
        isDebugMode = false;
        foreach (GameObject debugFeature in debugFeatures)
        {
            debugFeature.SetActive(false);
        }
    }

    public void ToggleDebugMode()
    {
        if (isDebugMode) ExitDebug();
        else EnterDebug();
    }

    public void SetGameSpeed(float speed)
    {
        if (speed <= 0f) return;

        currentGameSpeed = speed;
        if (!isPaused)
        {
            Time.timeScale = currentGameSpeed;
        }
        timeScaleBeforePause = currentGameSpeed;

        OnGameSpeedChanged?.Invoke(currentGameSpeed);
    }

    public void LoseGame()
    {
        PauseGame();
        UIManager.Instance.ToggleLoseGamePanel(true);
    }

    public void ExitGame()
    {
        Debug.LogWarning("Exited the game");
        Application.Quit();
    }
}