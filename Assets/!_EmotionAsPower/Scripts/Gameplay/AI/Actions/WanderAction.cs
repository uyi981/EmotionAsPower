using UnityEngine;

[CreateAssetMenu(fileName = "Wander Action", menuName = "Scriptable Objects/AI/Actions/Wander")]
public class WanderAction : AIAction
{
    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private Vector3 centerPoint = Vector3.zero;
    [SerializeField] private bool useSpawnPointAsCenter = true;

    private enum WanderState
    {
        Moving,
        Waiting
    }

    public override bool CanPerform(AIController controller)
    {
        // Can always wander
        return controller.unitMover != null;
    }

    public override void StartAction(AIController controller)
    {
        Vector3 center = useSpawnPointAsCenter ? controller.transform.position : centerPoint;
        controller.ActionData.SetCustomData("wanderCenter", center);
        controller.ActionData.SetCustomData("wanderState", WanderState.Moving);

        SetNewWanderTarget(controller);
    }

    public override ActionResult UpdateAction(AIController controller)
    {
        WanderState state = controller.ActionData.GetCustomData("wanderState", WanderState.Moving);

        switch (state)
        {
            case WanderState.Moving:
                return HandleMoving(controller);

            case WanderState.Waiting:
                return HandleWaiting(controller);

            default:
                return ActionResult.Failed;
        }
    }

    private ActionResult HandleMoving(AIController controller)
    {
        // Check if we've reached the target
        float distance = Vector3.Distance(controller.transform.position, controller.ActionData.targetPosition);
        if (distance <= 1f)
        {
            // Switch to waiting state
            controller.ActionData.SetCustomData("wanderState", WanderState.Waiting);
            controller.ActionData.SetCustomData("waitStartTime", Time.time);
            controller.unitMover.StopMovement();
            return ActionResult.Running;
        }

        // Continue moving
        if (!controller.unitMover.IsMoving())
        {
            controller.unitMover.MoveToWorldPosition(controller.ActionData.targetPosition);
        }

        return ActionResult.Running;
    }

    private ActionResult HandleWaiting(AIController controller)
    {
        float waitStartTime = controller.ActionData.GetCustomData("waitStartTime", 0f);

        if (Time.time - waitStartTime >= waitTime)
        {
            // Set new wander target and switch back to moving
            SetNewWanderTarget(controller);
            controller.ActionData.SetCustomData("wanderState", WanderState.Moving);
        }

        return ActionResult.Running;
    }

    private void SetNewWanderTarget(AIController controller)
    {
        Vector3 center = controller.ActionData.GetCustomData("wanderCenter", Vector3.zero);

        // Generate random point within wander radius
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        Vector3 wanderTarget = center + new Vector3(randomCircle.x, 0, randomCircle.y);

        controller.ActionData.targetPosition = wanderTarget;

        if (debugMode)
            Debug.Log($"New wander target: {wanderTarget}");
    }
}