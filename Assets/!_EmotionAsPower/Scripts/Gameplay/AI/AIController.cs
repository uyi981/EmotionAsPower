using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(UnitMover))]
public class AIController : MonoBehaviour
{
    public static float aiCoroutine = 0.5f;
    private AIBehaviour behaviour;
    private UnitMover unitMover;
    [SerializeField]
    private AIAction currentAction;
    private PerformingAction performingAction;

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

            if (result == ActionState.Success || result == ActionState.Failed)
            {
                if (result == ActionState.Success)
                    currentAction.OnActionComplete(this);
                else
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
        AIAction bestAction = null;
        int highestPriority = -1;

        foreach (var config in behaviour.Actions)
        {
            if (!config.isEnabled || !config.action.CanPerform(this))
                continue;

            int priority = config.priorityOverride != -1 ?
                config.priorityOverride :
                config.action.GetDynamicPriority(this);

            if (priority > highestPriority)
            {
                highestPriority = priority;
                bestAction = config.action;
            }
        }

        if (bestAction != null)
        {
            currentAction = bestAction;
            performingAction = new PerformingAction
            {
                actionName = bestAction.actionName,
                state = ActionState.Running,
                isInteruptable = true
            };
            currentAction.StartAction(this);
        }
    }

    public UnitMover GetUnitMover() => unitMover;
    public PerformingAction GetCurrentAction() => performingAction;
}
