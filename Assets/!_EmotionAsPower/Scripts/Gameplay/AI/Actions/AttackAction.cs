using UnityEngine;

[CreateAssetMenu(fileName = "Attack Action", menuName = "Scriptable Objects/AI/Actions/Attack")]
public class AttackAction : NewAIAction
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private DetectableType[] targetTypes;

    private float lastAttackTime;
    private bool isMovingToTarget;

    public override DetectableType[] TargetTypes()
    {
        return targetTypes;
    }

    public override bool Interruptible()
    {
        return false;
    }

    public override void StartAction(AIActionData actionData)
    {
        lastAttackTime = 0f;
        isMovingToTarget = false;
    }

    public override ActionState UpdateAction(AIActionData actionData)
    {
        if (actionData.targetObject == null)
            return ActionState.Failed;

        Health targetHealth = actionData.targetObject.GetComponent<Health>();
        if (targetHealth == null || targetHealth.IsDead)
            return ActionState.Success;

        float distanceToTarget = Vector3.Distance(actionData.ai.transform.position, actionData.targetObject.transform.position);
        if (distanceToTarget <= attackRange)
        {
            isMovingToTarget = false;
            float timeSinceLastAttack = Time.time - lastAttackTime;
            if (timeSinceLastAttack >= 1f / attackSpeed)
            {
                Debug.LogWarning("Damaging");
                targetHealth.TakeDamage(attackDamage);
                lastAttackTime = Time.time;
            }
            return ActionState.Running;
        }
        else
        {
            if (!isMovingToTarget)
            {
                UnitMover mover = actionData.ai.UnitMover;
                if (mover != null)
                {
                    mover.MoveToWorldPosition(actionData.targetObject.transform.position);
                    isMovingToTarget = true;
                }
            }
            UnitMover unitMover = actionData.ai.UnitMover;
            if (unitMover != null && !unitMover.IsMoving())
            {
                unitMover.MoveToWorldPosition(actionData.targetObject.transform.position);
                isMovingToTarget = true;
            }
            return ActionState.Running;
        }
    }

    public override void StopAction(AIActionData actionData)
    {
        UnitMover mover = actionData.ai.UnitMover;
        if (mover != null)
        {
            mover.StopMovement();
        }
        isMovingToTarget = false;
    }

    public override void OnActionComplete(AIActionData actionData)
    {
        isMovingToTarget = false;
    }

    public override void OnActionInterrupted(AIActionData actionData)
    {
        UnitMover mover = actionData.ai.UnitMover;
        if (mover != null)
        {
            mover.StopMovement();
        }
        isMovingToTarget = false;
    }

    public override void Perform(AIActionData actionData)
    {
        if (actionData.state != ActionState.Running)
        {
            StartAction(actionData);
            actionData.state = ActionState.Running;
        }

        if (actionData.targetObject == null)
        {
            Debug.LogWarning("Stopping action");
            actionData.state = ActionState.Failed;
            StopAction(actionData);
            return;
        }

        Health targetHealth = actionData.targetObject.GetComponent<Health>();
        if (targetHealth == null || targetHealth.IsDead)
        {
            actionData.state = ActionState.Success;
            OnActionComplete(actionData);
            return;
        }

        float distanceToTarget = Vector3.Distance(actionData.ai.transform.position, actionData.targetObject.transform.position);
        if (distanceToTarget <= attackRange)
        {
            // Stop moving if within range
            if (isMovingToTarget)
            {
                UnitMover mover = actionData.ai.UnitMover;
                if (mover != null)
                {
                    mover.StopMovement();
                    isMovingToTarget = false;
                }
            }

            // Attack logic
            float timeSinceLastAttack = Time.time - lastAttackTime;
            if (timeSinceLastAttack >= 1f / attackSpeed)
            {
                targetHealth.TakeDamage(attackDamage);
                lastAttackTime = Time.time;
            }
        }
        else
        {
            // Move towards the target
            if (!isMovingToTarget)
            {
                UnitMover mover = actionData.ai.UnitMover;
                if (mover != null)
                {
                    mover.MoveToWorldPosition(actionData.targetObject.transform.position);
                    isMovingToTarget = true;
                }
            }
        }

        // Check if the target is still valid
        if (targetHealth.IsDead)
        {
            actionData.state = ActionState.Success;
            OnActionComplete(actionData);
        }
        else
        {
            actionData.state = ActionState.Running;
        }
    }
}