using UnityEngine;

public class AttackActionExecutor : AIActionExecutor
{
    private float lastAttackTime;
    private bool isMovingToTarget;
    private AttackAction attackAction;

    public AttackActionExecutor(AIActionData data, AttackAction action) : base(data)
    {
        attackAction = action;
    }

    public override void StartAction()
    {
        lastAttackTime = 0f;
        isMovingToTarget = false;
    }

    public override ActionState UpdateAction()
    {
        if (actionData.targetObject == null)
            return ActionState.Failed;

        Health targetHealth = actionData.targetObject.GetComponent<Health>();
        if (targetHealth == null || targetHealth.IsDead)
            return ActionState.Success;

        float distanceToTarget = Vector3.Distance(actionData.ai.transform.position, actionData.targetObject.transform.position);
        if (distanceToTarget <= attackAction.AttackRange)
        {
            isMovingToTarget = false;
            float timeSinceLastAttack = Time.time - lastAttackTime;
            if (timeSinceLastAttack >= 1f / attackAction.AttackSpeed)
            {
                Debug.LogWarning("Damaging");
                targetHealth.TakeDamage(attackAction.AttackDamage);
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

    public override void StopAction()
    {
        UnitMover mover = actionData.ai.UnitMover;
        if (mover != null)
        {
            mover.StopMovement();
        }
        isMovingToTarget = false;
    }

    public override void OnActionComplete()
    {
        isMovingToTarget = false;
    }

    public override void OnActionInterrupted()
    {
        UnitMover mover = actionData.ai.UnitMover;
        if (mover != null)
        {
            mover.StopMovement();
        }
        isMovingToTarget = false;
    }

    public override void Perform()
    {
        if (actionData.state != ActionState.Running)
        {
            StartAction();
            actionData.state = ActionState.Running;
        }

        if (actionData.targetObject == null)
        {
            Debug.LogWarning("Stopping action");
            actionData.state = ActionState.Failed;
            StopAction();
            return;
        }

        Health targetHealth = actionData.targetObject.GetComponent<Health>();
        if (targetHealth == null || targetHealth.IsDead)
        {
            actionData.state = ActionState.Success;
            OnActionComplete();
            return;
        }

        float distanceToTarget = Vector3.Distance(actionData.ai.transform.position, actionData.targetObject.transform.position);
        if (distanceToTarget <= attackAction.AttackRange)
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
            if (timeSinceLastAttack >= 1f / attackAction.AttackSpeed)
            {
                targetHealth.TakeDamage(attackAction.AttackDamage);
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
            OnActionComplete();
        }
        else
        {
            actionData.state = ActionState.Running;
        }
    }
}