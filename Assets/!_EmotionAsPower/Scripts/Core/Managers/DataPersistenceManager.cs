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

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceList;

    public Action OnGameSaved;

    private void Start()
    {
        this.fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        this.dataPersistenceList = FindAllDataPersistences();
        this.gameData = new GameData();
    }
    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void LoadGame()
    {
        Debug.Log("Loading");
        gameData = fileDataHandler.Load();

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
        OnGameSaved?.Invoke();
        Debug.Log("Saved");
    }

    private List<IDataPersistence> FindAllDataPersistences()
    {
        IEnumerable<IDataPersistence> dataPersistences = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistences);
    }
}

