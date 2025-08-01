using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AIBehaviour;

[RequireComponent(typeof(UnitMover))]
public class AIController : MonoBehaviour
{
    public static float aiCoroutine = 0.5f;
    private AIBehaviour behaviour;
    private UnitMover unitMover;
    [SerializeField]
    private AIAction currentAction;
    private PerformingAction performingAction;
    private Dictionary<AIAction, float> lastFailureTimes = new Dictionary<AIAction, float>();
    [SerializeField] private float actionCooldownTime = 5f;

    private void Awake()
    {
        unitMover = GetComponent<UnitMover>();
    }

    public void Initialize(AIBehaviour behaviour)
    {
        this.behaviour = behaviour;
        InvokeRepeating(nameof(UpdateAction), 0f, aiCoroutine);
    }

    public void UpdateAction()
    {
        if (currentAction != null)
        {
            ActionState result = currentAction.UpdateAction(this);
            performingAction.state = result;
            if (result == ActionState.Success)
            {
                currentAction.OnActionComplete(this);
                currentAction = null;
                performingAction = null;
            }
            else if (result == ActionState.Failed)
            {
                lastFailureTimes[currentAction] = Time.time;
                currentAction.OnActionInterrupted(this);
                currentAction = null;
                performingAction = null;
            }
        }
        if (currentAction == null)
        {
            SelectBestAction();
        }
    }

    private void SelectBestAction()
    {
        var possibleActions = behaviour.Actions
            .Where(config => config.isEnabled && config.action.CanPerform(this))
            .ToList();

        if (possibleActions.Count == 0)
        {
            currentAction = null;
            return;
        }

        // Find actions that are not on cooldown
        var availableActions = possibleActions
            .Where(config => !lastFailureTimes.ContainsKey(config.action) || Time.time - lastFailureTimes[config.action] > actionCooldownTime)
            .ToList();

        ActionConfiguration bestConfig;
        if (availableActions.Count > 0)
        {
            bestConfig = availableActions
                .OrderByDescending(config => config.priorityOverride != -1 ? config.priorityOverride : config.action.GetDynamicPriority(this))
                .First();
        }
        else
        {
            bestConfig = possibleActions
                .OrderByDescending(config => config.priorityOverride != -1 ? config.priorityOverride : config.action.GetDynamicPriority(this))
                .First();
        }

        currentAction = bestConfig.action;
        performingAction = new PerformingAction
        {
            actionName = currentAction.actionName,
            state = ActionState.Running,
            isInteruptable = true
        };
        currentAction.StartAction(this);
    }

    public UnitMover GetUnitMover() => unitMover;
    public PerformingAction GetCurrentAction() => performingAction;
}