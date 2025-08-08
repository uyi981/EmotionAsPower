using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Application = UnityEngine.Application;
public class DataPersistenceManager : Singleton<DataPersistenceManager>
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName = "EmotionAsPowerSaveFile";
    private FileDataHandler fileDataHandler;
    public GameDataView gameDataView;

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceList;

    public Action OnGameSaved;

    private void Start()
    {
        this.fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        this.dataPersistenceList = FindAllDataPersistences();
        if(gameDataView!= null )
            this.gameData = gameDataView.gameData;
        if (gameDataView.shouldLoad)
        {
            LoadGame(); // Load game data if shouldLoad is true
        }
         // Load game data at the start
        //DontDestroyOnLoad(this.gameObject);
    }
    public void NewGame()
    {
        this.gameData = new GameData();
        gameDataView.shouldLoad = false; // Set shouldLoad to false to indicate a new game
        gameDataView.gameData = gameData; // Update the GameDataView with the new data
    }

    public void LoadGame()
    {
        Debug.Log("Loading");
        gameData = fileDataHandler.Load();
        gameDataView.shouldLoad = true; // Set shouldLoad to true to indicate loading existing game data
        gameDataView.gameData = gameData; // Update the GameDataView with the loaded data

        if (this.gameData == null)
        {
            Debug.Log("No data was found");
        }


        foreach (IDataPersistence dataPersistence in dataPersistenceList)
        {
            dataPersistence.LoadGame(gameData);
        }
        Debug.Log("Loaded");
    }

    public void SaveGame()
    {
        Debug.Log("Saving");

        foreach (IDataPersistence dataPersistence in dataPersistenceList)
        {
            dataPersistence.SaveGame(ref gameData);
        }

        //Save using FileDataHandler
        fileDataHandler.Save(gameData);
        gameDataView.gameData = gameData; // Update the GameDataView with the saved data
        OnGameSaved?.Invoke();
        Debug.Log("Saved");
    }

    private List<IDataPersistence> FindAllDataPersistences()
    {
        IEnumerable<IDataPersistence> dataPersistences = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistences);
    }
}

