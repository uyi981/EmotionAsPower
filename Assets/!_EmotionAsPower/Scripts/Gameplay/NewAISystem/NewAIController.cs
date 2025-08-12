using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(UnitMover), typeof(Detector))]
public class NewAIController : MonoBehaviour
{
    [SerializeField] private NewAIBehaviour behaviour;

    [Header("AI Settings")]
    public static float considerationInterval = 0.5f;

    [SerializeField] private NewAIAction currentAction;

    private Detector detector;
    private UnitMover unitMover;
    private float lastConsiderationTime;

    #region Getters
    public UnitMover UnitMover => unitMover;
    public Detector Detector => detector;
    #endregion

    [SerializeField]
    private AIActionData actionData = new AIActionData();

    public void Update()
    {
        PerformAction();
    }

    public void PerformAction()
    {
        // Always allow reconsideration if enough time has passed
        bool shouldReconsider = actionData.state == ActionState.Success ||
                               actionData.state == ActionState.Failed ||
                               Time.time - lastConsiderationTime >= considerationInterval;

        if (shouldReconsider)
        {
            ConsiderAndSetActionToPerform();
            lastConsiderationTime = Time.time;
        }

        // Use the executor instead of the action directly
        if (actionData.currentExecutor != null)
        {
            actionData.currentExecutor.Perform();
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (actionData.currentExecutor != null)
        {
            actionData.currentExecutor.StopAction();
            actionData.currentExecutor = null;
        }
        currentAction = null;
    }

    public void Initialize()
    {
        detector = GetComponent<Detector>();
        unitMover = GetComponent<UnitMover>();

        if (behaviour == null)
        {
            Debug.LogError($"No AI Behaviour assigned to {gameObject.name}!");
            return;
        }

        actionData.ai = this;
        lastConsiderationTime = 0f;

        // Remove the coroutine since we're handling consideration in Update now
        // StartCoroutine(ConsiderationCoroutine(considerationInterval));
    }

    public void ConsiderAndSetActionToPerform()
    {
        // Considering
        if (behaviour == null || behaviour.actions == null) return;

        GameObject[] detectedTargets = detector.Detect();
        NewAIAction[] availableActions = behaviour.actions.Keys.ToArray();

        int highestPriorityActionIndex = -1;
        int highestPriority = -1;
        GameObject[] bestActionTargets = null;

        for (int i = 0; i < availableActions.Length; i++)
        {
            NewAIAction action = availableActions[i];
            DetectFilter detectFilter = new DetectFilter(action.TargetTypes());
            GameObject[] validTargets = detectFilter.Filter(detectedTargets);

            // Check if this action has valid targets or doesn't require specific targets
            bool canPerformAction = validTargets.Length > 0 ||
                                  action.TargetTypes().Contains(DetectableType.Any);

            if (canPerformAction)
            {
                int priority = behaviour.actions[action];
                if (priority > highestPriority)
                {
                    highestPriorityActionIndex = i;
                    highestPriority = priority;
                    bestActionTargets = validTargets;
                }
            }
        }

        // Exit if there isn't performable action with current informations
        if (highestPriorityActionIndex < 0)
        {
            // Cancel current action
            if (actionData.currentExecutor != null)
            {
                actionData.currentExecutor.StopAction();
            }
            currentAction = null;
            actionData.currentExecutor = null;
            actionData.targetObject = null;
            actionData.state = ActionState.Failed;
            return;
        }

        NewAIAction newAction = availableActions[highestPriorityActionIndex];
        GameObject newTarget = GetNearestTarget(bestActionTargets);

        // Check if we need to change action or target
        bool needsNewAction = currentAction != newAction;
        bool needsNewTarget = actionData.targetObject != newTarget;
        bool currentActionComplete = actionData.state == ActionState.Success || actionData.state == ActionState.Failed;

        if (needsNewAction || needsNewTarget || currentActionComplete)
        {
            // Stop current action if it exists
            if (actionData.currentExecutor != null && (needsNewAction || currentActionComplete))
            {
                actionData.currentExecutor.StopAction();
                actionData.currentExecutor = null;
            }

            // Update action data
            actionData.targetObject = newTarget;

            // Set current action and create new executor if needed
            if (needsNewAction || actionData.currentExecutor == null)
            {
                currentAction = newAction;
                actionData.currentExecutor = currentAction.CreateExecutor(actionData);
            }

            actionData.state = ActionState.Running;
        }
    }

    public GameObject GetNearestTarget(GameObject[] targets)
    {
        if (targets == null || targets.Length == 0)
            return null;

        GameObject nearestTarget = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject target in targets)
        {
            // Skip self
            if (target == gameObject) continue;

            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = target;
            }
        }

        return nearestTarget;
    }

    // Remove the coroutine method since we're not using it anymore
    // private IEnumerator ConsiderationCoroutine(float interval) { ... }
}