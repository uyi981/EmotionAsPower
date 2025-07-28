using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem.XR;

// Base ScriptableObject for all AI actions
public abstract class AIAction : ScriptableObject
{
    [Header("Action Settings")]
    public string actionName;
    [SerializeField] protected int defaultPriority = 50;
    [SerializeField] protected bool canBeInterrupted = true;
    [SerializeField] protected float cooldownTime = 0f;

    [Header("Debug")]
    public bool debugMode = false;

    public int DefaultPriority => defaultPriority;
    public bool CanBeInterrupted => canBeInterrupted;
    public float CooldownTime => cooldownTime;

    public abstract bool CanPerform(AIController controller);

    public virtual int GetPriority(AIController controller, int behaviorPriorityOverride = -1)
    {
        return behaviorPriorityOverride >= 0 ? behaviorPriorityOverride : defaultPriority;
    }

    public abstract void StartAction(AIController controller);

    public abstract ActionResult UpdateAction(AIController controller);

    public virtual void StopAction(AIController controller)
    {
        if (debugMode)
            Debug.Log($"Stopping action: {actionName} on {controller.name}");
    }

    public virtual void OnActionComplete(AIController controller)
    {
        if (debugMode)
            Debug.Log($"Completed action: {actionName} on {controller.name}");
    }
    public virtual void OnActionInterrupted(AIController controller)
    {
        if (debugMode)
            Debug.Log($"Interrupted action: {actionName} on {controller.name}");
    }
}