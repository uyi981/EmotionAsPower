using UnityEngine;

[CreateAssetMenu(fileName = "GameDataView", menuName = "EmotionAsPower/GameDataView", order = 1)]
public class GameDataView : ScriptableObject
{
    public bool shouldLoad;
    public GameData gameData;
}