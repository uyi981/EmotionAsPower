using UnityEngine;

[System.Serializable]
public class AIActionData
{
    public NewAIController ai;
    public GameObject targetObject;
    public ActionState state = ActionState.Running;

    [System.NonSerialized]
    public AIActionExecutor currentExecutor;
}