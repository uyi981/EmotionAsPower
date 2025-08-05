using UnityEngine;

public abstract class NewAIAction : ScriptableObject
{
    /// <summary>
    /// Returns the types of targets this action can work with
    /// </summary>
    public abstract DetectableType[] TargetTypes();

    /// <summary>
    /// Called when the action starts executing
    /// </summary>
    public abstract void StartAction(AIActionData actionData);

    /// <summary>
    /// Called every frame while the action is running. Returns the current state of the action.
    /// </summary>
    public abstract ActionState UpdateAction(AIActionData actionData);

    /// <summary>
    /// Called when the action is manually stopped
    /// </summary>
    public abstract void StopAction(AIActionData actionData);

    /// <summary>
    /// Called when the action completes successfully
    /// </summary>
    public abstract void OnActionComplete(AIActionData actionData);

    /// <summary>
    /// Called when the action is interrupted by a higher priority action
    /// </summary>
    public abstract void OnActionInterrupted(AIActionData actionData);

    /// <summary>
    /// Returns whether this action can be interrupted by higher priority actions
    /// </summary>
    public abstract bool Interruptible();

    /// <summary>
    /// Just perform the action. This function automatically handle the action state
    /// </summary>
    /// <param name="actionData"></param>
    public abstract void Perform(AIActionData actionData);  
}
