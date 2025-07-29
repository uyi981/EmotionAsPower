using UnityEngine;

[CreateAssetMenu(fileName = "Attack Player Base Action", menuName = "Scriptable Objects/AI/Actions/AttackPlayerBase")]
public class AttackPlayerBaseAction : AIAction
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 50f;
    [SerializeField] private float attackSpeed = 0.5f;

    private Transform target;
    private float lastAttackTime;

    public override bool CanPerform(AIController controller)
    {
        GameObject playerBase = GameObject.FindGameObjectWithTag("PlayerBase");
        if (playerBase == null) return false;

        float distance = Vector3.Distance(controller.transform.position, playerBase.transform.position);
        return distance <= attackRange;
    }

    public override void StartAction(AIController controller)
    {
        GameObject playerBase = GameObject.FindGameObjectWithTag("PlayerBase");
        target = playerBase?.transform;
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
        GameObject playerBase = GameObject.FindGameObjectWithTag("PlayerBase");
        if (playerBase != null)
        {
            float distance = Vector3.Distance(controller.transform.position, playerBase.transform.position);
            if (distance <= attackRange)
                return basePriority + 50;
        }
        return 0;
    }
}