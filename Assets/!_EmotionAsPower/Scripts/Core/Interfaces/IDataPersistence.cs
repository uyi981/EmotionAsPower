using UnityEngine;

public interface IDataPersistence
{
    void LoadGame(GameData gameData);

    void SaveGame(ref GameData gameData);
}