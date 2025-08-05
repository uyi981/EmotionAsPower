using LgTyUtils;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAIBehaviour", menuName = "Scriptable Objects/NewAI/AIBehaviour")]
public class NewAIBehaviour : ScriptableObject
{
    [Header("Actions and their base priority (higher number = higher priority)")]
    public SerializableDictionary<NewAIAction, int> actions;
}
