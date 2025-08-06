using UnityEngine;

[System.Serializable]
public class AIActionData
{
    public NewAIController ai;
    public GameObject targetObject;
    public ActionState state = ActionState.Failed;

    [System.NonSerialized]
    public AIActionExecutor currentExecutor;
}