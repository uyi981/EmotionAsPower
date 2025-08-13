using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class PersonalityImporter : EditorWindow
{
    [System.Serializable]
    public class EmotionVectorData
    {
        public int anger;
        public int joy;
        public int sadness;
        public int fear;
        public int apathetic;
    }

    [System.Serializable]
    public class PersonalityData
    {
        public string name;
        public string description;
        public float rateSendChat;
        public float rateAcceptChat;
        public EmotionVectorData emotionSendAfterChat;
        public EmotionVectorData emotionSensity;
        public int hungerModifier;
        public float moveSpeedModifier;
        public float workSpeedModifier;
        public float maxCarryModifier;
    }

    [System.Serializable]
    public class PersonalityDataList
    {
        public List<PersonalityData> list;
    }

    private string jsonPath;

    [MenuItem("Tools/Import Personalities from JSON")]
    public static void ShowWindow()
    {
        GetWindow(typeof(PersonalityImporter));
    }

    void OnGUI()
    {
        GUILayout.Label("Import PersonalitySO from JSON", EditorStyles.boldLabel);

        if (GUILayout.Button("Select JSON File"))
        {
            jsonPath = EditorUtility.OpenFilePanel("Select Personality JSON", "", "json");
        }

        if (!string.IsNullOrEmpty(jsonPath))
        {
            GUILayout.Label("Selected: " + jsonPath);

            if (GUILayout.Button("Import"))
            {
                ImportFromJson();
            }
        }
    }

    void ImportFromJson()
    {
        string json = File.ReadAllText(jsonPath);

        // Bọc vào list để JsonUtility đọc được mảng
        string wrappedJson = "{ \"list\": " + json + "}";
        PersonalityDataList dataList = JsonUtility.FromJson<PersonalityDataList>(wrappedJson);

        string savePath = "Assets/Personalities/";
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        foreach (var data in dataList.list)
        {
            PersonalitySO asset = ScriptableObject.CreateInstance<PersonalitySO>();

            asset.name = data.name;
            asset.description = data.description;
            asset.rateSendChat = data.rateSendChat;
            asset.rateAcceptChat = data.rateAcceptChat;

            // EmotionSendAfterChat
            asset.emotionSendAffterChat = new EmotionVector
            {
                AngerLevel = data.emotionSendAfterChat.anger,
                JoyLevel = data.emotionSendAfterChat.joy,
                SadnessLevel = data.emotionSendAfterChat.sadness,
                FearLevel = data.emotionSendAfterChat.fear,
                ApatheticLevel = data.emotionSendAfterChat.apathetic
            };

            // EmotionSensity
            asset.emotionSensity = new EmotionVector
            {
                AngerLevel = data.emotionSensity.anger,
                JoyLevel = data.emotionSensity.joy,
                SadnessLevel = data.emotionSensity.sadness,
                FearLevel = data.emotionSensity.fear,
                ApatheticLevel = data.emotionSensity.apathetic
            };

            asset.hungerModifier = data.hungerModifier;
            asset.moveSpeedModifier = data.moveSpeedModifier;
            asset.workSpeedModifier = data.workSpeedModifier;
            asset.maxCarryModifier = data.maxCarryModifier;

            AssetDatabase.CreateAsset(asset, savePath + data.name + ".asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Imported " + dataList.list.Count + " personalities!");
    }
}
