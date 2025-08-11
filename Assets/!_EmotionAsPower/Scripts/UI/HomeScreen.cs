using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal.Execution;
using UnityEngine;
using UnityEngine.Rendering;

public class HomeScreen : MonoBehaviour
{
    [Header("StartMenu elements")]
    [SerializeField] private string gamePlaySceneName;
    [SerializeField] private ASyncLoader aSyncLoader;
    [SerializeField] private GameObject startMenuScreen;
    [SerializeField] private string backgroundMusic;

    private GameObject currentPanel;
    private void Awake()
    {
        //Set up data
        //DataPersistanceManager.Instance.SetUp();

        //Prepare UI
        if (startMenuScreen == null)
        {
            Debug.Log("Missing StartMenuScreen for HomeScreen");
        }


        if (aSyncLoader == null)
        {
            Debug.Log("Missing ASyncLoader for HomeScreen");
        }

        startMenuScreen.SetActive(true);

        //Playing background song
        if (backgroundMusic != null && backgroundMusic.Length > 0)
        {
            AudioManager.Instance.PlayMusicFromLibrary(backgroundMusic);
        }

    }

    public void ShowPanel(GameObject panel)
    {
        currentPanel?.SetActive(false);
        panel?.SetActive(true);
        currentPanel = panel;
    }

    public void DebugFunction()
    {
        Debug.LogError("Called DebugFunction. Function to call hasn't been assigned");
    }

    public void ShowPlayerCreationScreen()
    {
        //Singleton<DataPersistanceManager>.Instance.NewGame();
        startMenuScreen?.SetActive(false);

    }

    public void HidePlayerCreationScreen()
    {
        startMenuScreen?.SetActive(true);
    }

    public void ContinueGame()
    {
        //DataPersistenceManager.Instance.LoadGame();
        DataPersistenceManager.Instance.SetShouldLoad();
        aSyncLoader.LoadScene(gamePlaySceneName);
        //AudioManager.Instance.StopSound(backgroundMusic);
        
    }

    public void ExitGame()
    {
        Debug.LogWarning("Exited the game");
        Application.Quit();
        
    }

    public void StartNewGame()
    {
        DataPersistenceManager.Instance.NewGame();
        aSyncLoader.LoadScene(gamePlaySceneName);
       // AudioManager.Instance.StopAllSounds();
    }
}
