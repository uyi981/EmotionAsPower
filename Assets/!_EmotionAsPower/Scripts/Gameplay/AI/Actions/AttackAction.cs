using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack Action", menuName = "Scriptable Objects/AI/Actions/Attack")]
public class AttackAction : AIAction
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private List<InteractableType> preferredTargets = new List<InteractableType>();

    private Transform target;
    private float lastAttackTime;

    public override bool CanPerform(AIController controller)
    {
        return GetAttackTarget(controller) != null;
    }

    public override void StartAction(AIController controller)
    {
        target = GetAttackTarget(controller);
        lastAttackTime = 0f;
    }

    public override ActionState UpdateAction(AIController controller)
    {
        if (target == null)
            return ActionState.Failed;

        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth == null || targetHealth.IsDead)
            return ActionState.Success;

        float distance = Vector3.Distance(controller.transform.position, target.position);
        if (distance > attackRange)
            return ActionState.Failed;

        float timeSinceLastAttack = Time.time - lastAttackTime;
        if (timeSinceLastAttack >= 1f / attackSpeed)
        {
            targetHealth.TakeDamage(attackDamage);
            lastAttackTime = Time.time;
        }

        return ActionState.Running;
    }

    public override void StopAction(AIController controller)
    {
        target = null;
    }

    public override void OnActionComplete(AIController controller)
    {
        target = null;
    }

    public override void OnActionInterrupted(AIController controller)
    {
        target = null;
    }

    public override int GetDynamicPriority(AIController controller)
    {
        Transform nearestTarget = GetAttackTarget(controller);
        if (nearestTarget != null)
        {
            float distance = Vector3.Distance(controller.transform.position, nearestTarget.position);
            return basePriority + Mathf.RoundToInt((attackRange - distance) * 10);
        }
        return 0;
    }

    private Transform GetAttackTarget(AIController controller)
    {
        Transform bestTarget = null;
        float closestDistance = attackRange;

        foreach (var targetType in preferredTargets)
        {
            bestTarget = FindTargetOfType(controller.transform.position, targetType, closestDistance);
            if (bestTarget != null)
                break;
        }

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
}