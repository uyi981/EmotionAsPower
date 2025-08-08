using UnityEngine;

public abstract class AIAction : ScriptableObject
{
    [Header("Action Settings")]
    public string actionName;
    public int basePriority = 1;

    public abstract bool CanPerform(AIController controller);
    public abstract void StartAction(AIController controller);
    public abstract ActionState UpdateAction(AIController controller);
    public abstract void StopAction(AIController controller);
    public abstract void OnActionComplete(AIController controller);
    public abstract void OnActionInterrupted(AIController controller);
    public abstract int GetDynamicPriority(AIController controller);
}