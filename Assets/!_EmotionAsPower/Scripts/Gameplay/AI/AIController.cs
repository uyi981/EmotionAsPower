using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Main AI Controller that handles action selection and execution
[RequireComponent(typeof(UnitMover))]
public class AIController : MonoBehaviour
{
    [Header("AI Configuration")]
    [SerializeField] private AIBehaviour currentBehaviour;
    [SerializeField] private float actionUpdateInterval = 0.1f;
    [SerializeField] private bool debugMode = false;

    [Header("Components")]
    public UnitMover unitMover;
    public Health health;

    // Current action state
    private AIAction currentAction;
    private ActionData actionData;
    private Dictionary<AIAction, float> actionCooldowns = new Dictionary<AIAction, float>();
    private float lastActionUpdate;

    public AIBehaviour CurrentBehaviour => currentBehaviour;
    public AIAction CurrentAction => currentAction;
    public ActionData ActionData => actionData;
    public bool IsPerformingAction => currentAction != null;

    public System.Action<AIAction> OnActionStarted;
    public System.Action<AIAction, ActionResult> OnActionEnded;

    private void Awake()
    {
        unitMover = GetComponent<UnitMover>();
        health = GetComponent<Health>();
        actionData = new ActionData();
    }

    private void Start()
    {
        if (currentBehaviour == null)
        {
            Debug.LogError($"No AI Behavior assigned to {gameObject.name}");
            enabled = false;
        }
    }

    private void Update()
    {
        if (Time.time - lastActionUpdate < actionUpdateInterval)
            return;

        lastActionUpdate = Time.time;
        UpdateCooldowns();
        ProcessActions();
    }

    private void ProcessActions()
    {
        // Update current action if one is running
        if (currentAction != null)
        {
            ActionResult result = currentAction.UpdateAction(this);
            
            if (result != ActionResult.Running)
            {
                EndCurrentAction(result);
            }
            else if (!currentAction.CanBeInterrupted)
            {
                return; // Don't check for new actions if current one can't be interrupted
            }
        }

        // Find the highest priority action
        AIAction bestAction = FindBestAction();
        
        if (bestAction != null && bestAction != currentAction)
        {
            // Start new action (will stop current one if needed)
            StartAction(bestAction);
        }
    }

    private AIAction FindBestAction()
    {
        if (currentBehaviour == null || currentBehaviour.Actions.Count == 0)
            return null;

        AIAction bestAction = null;
        int highestPriority = -1;

        foreach (var actionConfig in currentBehaviour.Actions)
        {
            if (actionConfig.action == null)
                continue;

            // Check cooldown
            if (IsActionOnCooldown(actionConfig.action))
                continue;

            // Check if action can be performed
            if (!actionConfig.action.CanPerform(this))
                continue;

            // Get priority (behavior can override default priority)
            int priority = actionConfig.action.GetPriority(this, actionConfig.priorityOverride);
            
            if (priority > highestPriority)
            {
                highestPriority = priority;
                bestAction = actionConfig.action;
            }
        }

        return bestAction;
    }

    private void StartAction(AIAction action)
    {
        if (action == null)
            return;

        // Stop current action if there is one
        if (currentAction != null)
        {
            currentAction.OnActionInterrupted(this);
            currentAction.StopAction(this);
        }

        // Start new action
        currentAction = action;
        actionData.startTime = Time.time;
        
        if (debugMode)
            Debug.Log($"Starting action: {action.actionName} on {gameObject.name}");

        action.StartAction(this);
        OnActionStarted?.Invoke(action);
    }

    private void EndCurrentAction(ActionResult result)
    {
        if (currentAction == null)
            return;

        AIAction finishedAction = currentAction;
        
        // Set cooldown if action completed or failed
        if (result == ActionResult.Success || result == ActionResult.Failed)
        {
            SetActionCooldown(finishedAction);
        }

        // Call appropriate completion method
        if (result == ActionResult.Success)
        {
            finishedAction.OnActionComplete(this);
        }
        else if (result == ActionResult.Cancelled)
        {
            finishedAction.OnActionInterrupted(this);
        }

        finishedAction.StopAction(this);
        
        if (debugMode)
            Debug.Log($"Ended action: {finishedAction.actionName} with result: {result}");

        OnActionEnded?.Invoke(finishedAction, result);
        currentAction = null;
    }

    private void UpdateCooldowns()
    {
        var keys = new List<AIAction>(actionCooldowns.Keys);
        foreach (var action in keys)
        {
            actionCooldowns[action] -= Time.deltaTime;
            if (actionCooldowns[action] <= 0)
            {
                actionCooldowns.Remove(action);
            }
        }
    }

    private bool IsActionOnCooldown(AIAction action)
    {
        return actionCooldowns.ContainsKey(action) && actionCooldowns[action] > 0;
    }

    private void SetActionCooldown(AIAction action)
    {
        if (action.CooldownTime > 0)
        {
            actionCooldowns[action] = action.CooldownTime;
        }
    }

    public void SetBehavior(AIBehaviour newBehavior)
    {
        if (currentAction != null && !currentAction.CanBeInterrupted)
        {
            Debug.LogWarning($"Cannot change behavior while performing non-interruptible action: {currentAction.actionName}");
            return;
        }

        EndCurrentAction(ActionResult.Cancelled);
        currentBehaviour = newBehavior;
        
        if (debugMode)
            Debug.Log($"Changed behavior to: {newBehavior?.name ?? "None"}");
    }

    public void ForceStopCurrentAction()
    {
        if (currentAction != null)
        {
            EndCurrentAction(ActionResult.Cancelled);
        }
    }

    public float GetActionCooldownRemaining(AIAction action)
    {
        if (actionCooldowns.ContainsKey(action))
            return actionCooldowns[action];
        return 0f;
    }
#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (!debugMode || currentAction == null)
            return;

        // Draw debug info about current action
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, currentAction.actionName);
        #endif
    }

#endif
}

