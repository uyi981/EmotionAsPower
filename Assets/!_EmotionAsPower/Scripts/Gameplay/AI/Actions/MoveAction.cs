using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Move Action", menuName = "Scriptable Objects/AI/Actions/Move")]
public class MoveAction : AIAction
{
    [Header("Move Settings")]
    [SerializeField] private float acceptableDistance = 0.5f;
    [SerializeField] private float targetUpdateInterval = 0.5f;
    [SerializeField] private bool followMovingTarget = true;

    private float lastTargetUpdate;

    public override bool CanPerform(AIController controller)
    {
        return controller.unitMover != null &&
               (controller.ActionData.target != null || controller.ActionData.targetPosition != Vector3.zero);
    }

    public override void StartAction(AIController controller)
    {
        lastTargetUpdate = Time.time;
        UpdateTargetPosition(controller);

        if (debugMode)
            Debug.Log($"Starting move to {controller.ActionData.targetPosition}");
    }

    public override ActionResult UpdateAction(AIController controller)
    {
        if (followMovingTarget && controller.ActionData.target != null &&
            Time.time - lastTargetUpdate > targetUpdateInterval)
        {
            UpdateTargetPosition(controller);
            lastTargetUpdate = Time.time;
        }

        float distance = Vector3.Distance(controller.transform.position, controller.ActionData.targetPosition);
        if (distance <= acceptableDistance)
        {
            return ActionResult.Success;
        }

        if (controller.unitMover.IsMoving())
        {
            return ActionResult.Running;
        }

        controller.unitMover.MoveToWorldPosition(controller.ActionData.targetPosition);
        return ActionResult.Running;
    }

    public override void StopAction(AIController controller)
    {
        controller.unitMover.StopMovement();
        base.StopAction(controller);
    }

    private void UpdateTargetPosition(AIController controller)
    {
        if (controller.ActionData.target != null)
        {
            controller.ActionData.targetPosition = controller.ActionData.target.position;
        }
    }
}