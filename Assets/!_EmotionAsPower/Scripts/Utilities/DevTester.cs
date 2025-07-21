using System;
using LgTyUtils;
using UnityEngine;
using Random = UnityEngine.Random;
public class DevTester : Singleton<DevTester>, IDataPersistence
{
    public string savedTime = string.Empty;
    public SerializableDictionary<int, int> testDict;
    public void LoadGame(GameData gameData)
    {
        this.savedTime = gameData.savedTime;
        this.testDict = gameData.testDict;
    }

    public void SaveGame(ref GameData gameData)
    {
        gameData.savedTime = DateTime.Now.ToString();
        gameData.testDict = this.testDict;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int randomCount = Random.Range(0, 100);
        for (int i = 0; i < randomCount; i++) { 
            testDict.Add(i, Random.Range(0, 100));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
