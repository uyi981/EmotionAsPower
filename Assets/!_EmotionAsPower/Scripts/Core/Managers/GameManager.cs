using System;
using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private bool isDebugMode;
    public bool IsDebugMode => isDebugMode;

    public GameObject[] debugFeatures;

    [Header("Game State")]
    [SerializeField] private bool isPaused = false;
    [SerializeField] private float timeScaleBeforePause = 1f;

    public Action OnSetupFinished;
    public Action<bool> OnGamePaused;
    public bool IsPaused => isPaused;
    public float CurrentTimeScale => Time.timeScale;

    protected override void Awake()
    {
        base.Awake();
        PauseGame();
        StartCoroutine(SetupAll());
    }

    private IEnumerator SetupAll()
    {
        yield return ContentManager.Instance.SetupCoroutine();
        UIManager.Instance.Setup();
        ResumeGame();
        OnSetupFinished?.Invoke();
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
        foreach (GameObject debugFeature in debugFeatures) { 
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
        if(isDebugMode) ExitDebug();
        else EnterDebug();
    }
}
