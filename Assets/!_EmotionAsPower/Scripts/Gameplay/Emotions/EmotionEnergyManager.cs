using System;
using System.Collections;
using LgTyUtils;
using UnityEngine;

public class EmotionEnergyManager : Singleton<EmotionEnergyManager>, IDataPersistence
{
    [SerializeField]
    private SerializableDictionary<EmotionType, int> emotionEnergy;

    public SerializableDictionary<EmotionType, int> EmotionEnergy => emotionEnergy;

    public Action<EmotionType, int> OnEmotionEnergyChange;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Initialize()
    {
        emotionEnergy = new SerializableDictionary<EmotionType, int>();
        foreach(EmotionType emotion in Enum.GetValues(typeof(EmotionType)))
        {
            emotionEnergy.Add(emotion, 0);
            OnEmotionEnergyChange?.Invoke(emotion, 0);
        }


    }
    public void AddEnergy(EmotionType type, int amount)
    {
        emotionEnergy[type] += amount;
        OnEmotionEnergyChange?.Invoke(type, emotionEnergy[type]);
    }

    public int TryTakeEnergy(EmotionType type, int amount) { 
        var targetEnergyAmount = emotionEnergy[type];
        if (targetEnergyAmount < amount) {
            emotionEnergy[type] = 0;
            OnEmotionEnergyChange?.Invoke(type, emotionEnergy[type]);
            return targetEnergyAmount;
        }
        emotionEnergy[type] -= amount;
        OnEmotionEnergyChange?.Invoke(type, emotionEnergy[type]);
        return amount;
    }


    public void LoadGame(GameData gameData)
    {
        this.emotionEnergy = gameData.emotionEnergy;
        foreach (EmotionType emotion in emotionEnergy.Keys) { 
            OnEmotionEnergyChange?.Invoke(emotion, emotionEnergy[emotion]);
        }
    }

    public void SaveGame(ref GameData gameData)
    {
        gameData.emotionEnergy = this.emotionEnergy;
    }
}