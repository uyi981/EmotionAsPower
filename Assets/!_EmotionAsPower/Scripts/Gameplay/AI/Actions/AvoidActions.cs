using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Avoid Action", menuName = "Scriptable Objects/AI/Actions/Avoid")]
public class AvoidAction : AIAction
{
    [Header("Avoid Settings")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float fleeDistance = 8f;
    [SerializeField] private LayerMask threatLayers = -1;
    [SerializeField] private List<string> threatTags = new List<string> { "Enemy", "Danger" };

    public override bool CanPerform(AIController controller)
    {
        return GetNearestThreat(controller.transform.position) != null;
    }

    public override void StartAction(AIController controller)
    {
        Vector3 fleePosition = CalculateFleePosition(controller);
        controller.ActionData.targetPosition = fleePosition;

        if (debugMode)
            Debug.Log($"Fleeing to {fleePosition}");
    }

    public override ActionResult UpdateAction(AIController controller)
    {
        // Check if we're safe now
        Transform threat = GetNearestThreat(controller.transform.position);
        if (threat == null)
        {
            return ActionResult.Success;
        }

        // Check if we've reached the flee position
        float distance = Vector3.Distance(controller.transform.position, controller.ActionData.targetPosition);
        if (distance <= 1f)
        {
            // Recalculate flee position if still in danger
            Vector3 newFleePosition = CalculateFleePosition(controller);
            controller.ActionData.targetPosition = newFleePosition;
        }

        // Move to flee position
        if (!controller.unitMover.IsMoving())
        {
            controller.unitMover.MoveToWorldPosition(controller.ActionData.targetPosition);
        }

        return ActionResult.Running;
    }

    private Transform GetNearestThreat(Vector3 position)
    {
        Collider[] threats = Physics.OverlapSphere(position, detectionRange, threatLayers);

        Transform nearestThreat = null;
        float closestDistance = detectionRange;

        foreach (var threat in threats)
        {
            // Check if it's actually a threat (has threatening tag or is an enemy)
            bool isThreat = false;
            foreach (string tag in threatTags)
            {
                if (threat.CompareTag(tag))
                {
                    isThreat = true;
                    break;
                }
            }

            if (!isThreat)
                continue;

            float distance = Vector3.Distance(position, threat.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestThreat = threat.transform;
            }
        }

        return nearestThreat;
    }

    private Vector3 CalculateFleePosition(AIController controller)
    {
        Transform threat = GetNearestThreat(controller.transform.position);
        if (threat == null)
            return controller.transform.position;

        // Calculate direction away from threat
        Vector3 fleeDirection = (controller.transform.position - threat.position).normalized;
        Vector3 fleePosition = controller.transform.position + fleeDirection * fleeDistance;

        return fleePosition;
    }
}