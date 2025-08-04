using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(UnitMover), typeof(Detector))]
public class NewAIController : MonoBehaviour
{
    [SerializeField] private NewAIBehaviour behaviour;

    [Header("AI Settings")]
    public static float considerationInterval = 0.2f;

    [SerializeField] private NewAIAction currentAction;

    private Detector detector;
    private UnitMover unitMover;

    #region Getters
    public UnitMover UnitMover => unitMover;
    public Detector Detector => detector;   
    #endregion

    [SerializeField]
    private AIActionData actionData = new AIActionData();

    private void OnEnable()
    {
        Initialize();
    }

    private void Update()
    {
        
        if(actionData.state == ActionState.Success || actionData.state == ActionState.Failed)
        {
            ConsiderAndSetActionToPerform();
        }

        if(currentAction != null)
        {
            currentAction.Perform(actionData);
        }      
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (currentAction != null)
        {
            currentAction.StopAction(actionData);
            currentAction = null;
        }
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

        StartCoroutine(ConsiderationCoroutine(considerationInterval));
    }

    /// <summary>
    /// Evaluates all possible actions and set the action with the data to perform
    /// </summary>
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
        if(highestPriorityActionIndex < 0)
        {
            // Cancel current action
            currentAction = null;
            actionData.state =  ActionState.Failed;
            return;
        }

        // Update action data
        actionData.targetObject = GetNearestTarget(bestActionTargets);

        // Set current action to perform (Perform handled in Update)
        currentAction = availableActions[highestPriorityActionIndex];
        actionData.state = ActionState.Running;
    }

    /// <summary>
    /// Finds the nearest target from the available targets array
    /// </summary>
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

    /// <summary>
    /// Coroutine that periodically evaluates which action should be performed
    /// </summary>
    private IEnumerator ConsiderationCoroutine(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            ConsiderAndSetActionToPerform();
        }
    }

}