using UnityEngine;
using UnityEditor;
using System.IO;

public class PersonalitySOImporter : EditorWindow
{
    private TextAsset jsonFile;
    private string savePath = "Assets/GeneratedPersonalities";

    [MenuItem("Tools/Import PersonalitySO from JSON")]
    public static void ShowWindow()
    {
        GetWindow<PersonalitySOImporter>("Import PersonalitySO");
    }

    void OnGUI()
    {
        GUILayout.Label("Import PersonalitySO from JSON", EditorStyles.boldLabel);

        jsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON File", jsonFile, typeof(TextAsset), false);
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        if (GUILayout.Button("Import"))
        {
            if (jsonFile == null)
            {
                Debug.LogError("Please assign a JSON file.");
                return;
            }

            ImportFromJson();
        }
    }

    void ImportFromJson()
    {
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        string json = jsonFile.text;
        var dataList = JsonHelper.FromJson<PersonalitySOData>(json);

        foreach (var data in dataList)
        {
            var asset = ScriptableObject.CreateInstance<PersonalitySO>();

            // Gán dữ liệu
            asset.name = data.name;
            asset.description = data.description;
            asset.rateSendChat = data.rateSendChat;
            asset.rateAcceptChat = data.rateAcceptChat;
            asset.emotionSendAffterChat = data.emotionSendAffterChat;
            asset.emotionSensity = data.emotionSensity;
            asset.hungerModifier = data.hungerModifier;
            asset.thirstModifier = data.thirstModifier;
            asset.tiredModifier = data.tiredModifier;

            string assetPath = $"{savePath}/{asset.name}.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ Imported all PersonalitySO successfully.");
    }

    // Class dùng để parse JSON
    [System.Serializable]
    public class PersonalitySOData
    {
        public string name;
        public string description;
        public float rateSendChat;
        public float rateAcceptChat;
        public EmotionVector emotionSendAffterChat;
        public EmotionVector emotionSensity;
        public float hungerModifier;
        public float thirstModifier;
        public float tiredModifier;
    }

    // Wrapper cho JsonUtility
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string wrappedJson = "{\"Items\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
            return wrapper.Items;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
}
