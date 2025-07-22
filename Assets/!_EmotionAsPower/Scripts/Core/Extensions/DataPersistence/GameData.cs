using System;
using System.Collections.Generic;
using LgTyUtils;
using UnityEngine;

[Serializable]
public class GameData
{
    public SerializableDictionary<EmotionType, int> emotionEnergy;
    public ItemRuntimeInstance[] items;
}
