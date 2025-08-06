using UnityEngine;

public abstract class NewAIAction : ScriptableObject
{
    /// <summary>
    /// Returns the types of targets this action can work with
    /// </summary>
    public abstract DetectableType[] TargetTypes();

    /// <summary>
    /// Returns whether this action can be interrupted by higher priority actions
    /// </summary>
    public abstract bool Interruptible();

    /// <summary>
    /// Creates a new executor instance for this action
    /// </summary>
    public abstract AIActionExecutor CreateExecutor(AIActionData actionData);
}