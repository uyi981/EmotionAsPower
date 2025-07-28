using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack Action", menuName = "Scriptable Objects/AI/Actions/Attack")]
public class AttackAction : AIAction
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackSpeed = 1f;

    [Header("Target Types")]
    [SerializeField] private List<InteractableType> preferredTargets = new List<InteractableType>();

    public override bool CanPerform(AIController controller)
    {
        Transform target = GetAttackTarget(controller);
        return target != null;
    }

    public override void StartAction(AIController controller)
    {
        Transform target = GetAttackTarget(controller);
        if (target != null)
        {
            controller.ActionData.target = target;
            controller.ActionData.lastActionTime = 0f; // Allow immediately first attack
        }

        if (debugMode)
            Debug.Log($"Starting attack on {target?.name}");
    }

    public override ActionResult UpdateAction(AIController controller)
    {
        // Check if target is still valid
        if (controller.ActionData.target == null)
        {
            return ActionResult.Failed;
        }

        Health targetHealth = controller.ActionData.target.GetComponent<Health>();
        if (targetHealth == null || targetHealth.IsDead)
        {
            return ActionResult.Success; // Target destroyed
        }

        // Check if target is still in range
        float distance = Vector3.Distance(controller.transform.position, controller.ActionData.target.position);
        if (distance > attackRange)
        {
            return ActionResult.Failed; // Target moved out of range
        }

        // Check if we can attack (based on attack speed)
        float timeSinceLastAttack = Time.time - controller.ActionData.lastActionTime;
        if (timeSinceLastAttack >= 1f / attackSpeed)
        {
            PerformAttack(controller, targetHealth);
            controller.ActionData.lastActionTime = Time.time;
        }

        return ActionResult.Running;
    }

    private Transform GetAttackTarget(AIController controller)
    {
        Transform bestTarget = null;
        float closestDistance = attackRange;

        // Try each preferred target type in order
        foreach (var targetType in preferredTargets)
        {
            bestTarget = FindTargetOfType(controller.transform.position, targetType, closestDistance);
            if (bestTarget != null)
                break;
        }

        // If no preferred targets found and Any is included, find any valid target
        if (bestTarget == null && preferredTargets.Contains(InteractableType.Any))
        {
            bestTarget = TargetFinder.FindNearestTarget<Health>(controller.transform.position, attackRange)?.transform;
        }

        return bestTarget;
    }

    private Transform FindTargetOfType(Vector3 position, InteractableType targetType, float maxRange)
    {
        switch (targetType)
        {
            case InteractableType.Building:
                return FindTargetWithComponent<BuildingBase>(position, maxRange);

            case InteractableType.Villager:
                return FindTargetWithComponent<Villager>(position, maxRange);

            case InteractableType.Resource:
                return FindTargetWithComponent<Resource>(position, maxRange);

            case InteractableType.Enemy:
                return FindTargetWithComponent<Enemy>(position, maxRange);

            case InteractableType.Any:
                return TargetFinder.FindNearestTarget<Health>(position, maxRange)?.transform;

            default:
                return null;
        }
    }

    private Transform FindTargetWithComponent<T>(Vector3 position, float maxRange) where T : MonoBehaviour, IInteractable
    {
        T target = TargetFinder.FindNearestTarget<T>(position, maxRange);
        return target?.transform;
    }

    private void PerformAttack(AIController controller, Health targetHealth)
    {
        targetHealth.TakeDamage(attackDamage);

        if (debugMode)
            Debug.Log($"{controller.name} attacked {targetHealth.name} for {attackDamage} damage");
    }
}
