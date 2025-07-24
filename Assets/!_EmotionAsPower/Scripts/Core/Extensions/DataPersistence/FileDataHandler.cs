using System;
using System.IO;
using UnityEngine;

public class FileDataHandler
{
    private string dataDirPath = "";

    private string dataFileName = "";

    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public GameData Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        GameData data = null;

        if (File.Exists(fullPath))
        {
            try
            {
                // Load the serialized data from filed
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // Deserialize data back to C#
                data = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error occred when trying to load data from file: {fullPath}\n{e}");
            }
        }
        return data;
    }

    public void Save(GameData gameData)
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            // Create the directory the file will be written to if it doesn't already existed
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // Serialize data to json
            string dataToStore = JsonUtility.ToJson(gameData);

            // Write serialize data to the file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    sw.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }
}