using System;
using LgTyUtils;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerBaseLevel", menuName = "Scriptable Objects/PlayerBaseLevel")]
public class PlayerBaseLevel : ScriptableObject
{
    public int level;
    public LevelRequirement levelRequirement;
    public LevelUnlockedContents unlockedContents;
}

[Serializable]
public class LevelRequirement
{
    public SerializableDictionary<Emotion, int> emotionRequirements;
    public SerializableDictionary<ItemSO, int> itemRequirements;

}

[Serializable]
public class LevelUnlockedContents
{
    // TODO: Add unlocked contents: buildings, skills

    public void AddUnlockedContents(LevelUnlockedContents lockedContents)
    {
        // TODO implement logic
    }
}