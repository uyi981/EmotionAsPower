using UnityEngine;

public class ShootActionExecutor : AIActionExecutor
{
    private ShootAction shootAction;
    private float lastShotTime;
    private bool wasMoving;
    private Vector3 lastTargetPosition;

    public ShootActionExecutor(AIActionData data, ShootAction action) : base(data)
    {
        shootAction = action;
    }

    public override void StartAction()
    {
        lastShotTime = 0f;
        wasMoving = false;
        lastTargetPosition = Vector3.zero;
    }

    public override ActionState UpdateAction()
    {
        if (actionData.targetObject == null)
            return ActionState.Failed;

        // Check if target is still alive
        Health targetHealth = actionData.targetObject.GetComponent<Health>();
        if (targetHealth != null && targetHealth.IsDead)
            return ActionState.Success;

        Vector3 targetPosition = actionData.targetObject.transform.position;
        float distanceToTarget = Vector3.Distance(actionData.ai.transform.position, targetPosition);

        // If target is out of range, move closer
        if (distanceToTarget > shootAction.AttackRange)
        {
            HandleMovementToTarget();
            return ActionState.Running;
        }

        // Target is in range - check line of sight if required
        if (shootAction.RequiresLineOfSight && !HasLineOfSight(targetPosition))
        {
            HandleMovementToTarget();
            return ActionState.Running;
        }

        // Stop moving if required
        if (shootAction.StopMovingWhileShooting && wasMoving)
        {
            UnitMover mover = actionData.ai.UnitMover;
            if (mover != null)
            {
                mover.StopMovement();
            }
            wasMoving = false;
        }

        // Face the target
        FaceTarget(targetPosition);

        // Check if we can shoot (fire rate cooldown)
        float timeSinceLastShot = Time.time - lastShotTime;
        float shootCooldown = 1f / shootAction.FireRate;

        if (timeSinceLastShot >= shootCooldown)
        {
            Shoot(targetPosition);
        }

        return ActionState.Running;
    }

    private void HandleMovementToTarget()
    {
        UnitMover mover = actionData.ai.UnitMover;
        if (mover == null) return;

        Vector3 currentTargetPos = actionData.targetObject.transform.position;

        // Move to optimal attack position (85% of max range for grid-based map to account for pathfinding)
        Vector3 directionToTarget = (currentTargetPos - actionData.ai.transform.position).normalized;
        Vector3 optimalPosition = currentTargetPos - directionToTarget * (shootAction.AttackRange * 0.85f);

        mover.MoveToWorldPosition(optimalPosition);
        wasMoving = true;
        lastTargetPosition = currentTargetPos;
    }

    private bool HasLineOfSight(Vector3 targetPosition)
    {
        Vector3 shootPosition = actionData.ai.transform.position + shootAction.ShootOffset;
        Vector3 directionToTarget = (targetPosition - shootPosition).normalized;
        float distanceToTarget = Vector3.Distance(shootPosition, targetPosition);

        // Raycast to check for obstacles
        RaycastHit hit;
        if (Physics.Raycast(shootPosition, directionToTarget, out hit, distanceToTarget))
        {
            // If we hit something other than the target, there's no line of sight
            return hit.collider.gameObject == actionData.targetObject;
        }

        return true;
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        UnitMover mover = actionData.ai.UnitMover;
        if (mover == null || !mover.flipable) return;

        Vector3 directionToTarget = (targetPosition - actionData.ai.transform.position).normalized;

        // Determine flip direction based on target position
        if (Mathf.Abs(directionToTarget.x) > 0.1f)
        {
            bool shouldFlip = mover.baseFlip ? (directionToTarget.x >= 0) : (directionToTarget.x < 0);
            mover.ToggleFlip(shouldFlip);
        }
    }

    private void Shoot(Vector3 targetPosition)
    {
        lastShotTime = Time.time;

        Vector3 shootPosition = actionData.ai.transform.position + shootAction.ShootOffset;
        Vector3 shootDirection = (targetPosition - shootPosition).normalized;

        Debug.Log($"{actionData.ai.name} shooting at {actionData.targetObject.name}!");

        // Spawn bullet
        if (shootAction.BulletPrefab != null)
        {
            GameObject bulletGO = Object.Instantiate(shootAction.BulletPrefab, shootPosition, Quaternion.LookRotation(shootDirection), EnemyManager.Instance.bulletsParent);
            Bullet bullet = bulletGO.GetComponent<Bullet>();

            if (bullet != null)
            {
                bullet.Initialize(shootDirection, shootAction.BulletDamage, shootAction.BulletSpeed, shootAction.BulletLifetime, shootAction.DamageLayerMask);
            }
        }

        // Spawn muzzle flash effect
        if (shootAction.MuzzleFlashPrefab != null)
        {
            GameObject muzzleFlash = Object.Instantiate(shootAction.MuzzleFlashPrefab, shootPosition, Quaternion.LookRotation(shootDirection));
            Object.Destroy(muzzleFlash, 0.5f);
        }

        // Play shoot sound
        if (shootAction.ShootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootAction.ShootSound, shootPosition);
        }
    }

    public override void StopAction()
    {
        UnitMover mover = actionData.ai.UnitMover;
        if (mover != null)
        {
            mover.StopMovement();
        }
        wasMoving = false;
    }

    public override void OnActionComplete()
    {
        wasMoving = false;
    }

    public override void OnActionInterrupted()
    {
        UnitMover mover = actionData.ai.UnitMover;
        if (mover != null)
        {
            mover.StopMovement();
        }
        wasMoving = false;
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
            Debug.LogWarning("Shoot action stopping - no target");
            actionData.state = ActionState.Failed;
            StopAction();
            return;
        }

        Health targetHealth = actionData.targetObject.GetComponent<Health>();
        if (targetHealth != null && targetHealth.IsDead)
        {
            actionData.state = ActionState.Success;
            OnActionComplete();
            return;
        }

        Vector3 targetPosition = actionData.targetObject.transform.position;
        float distanceToTarget = Vector3.Distance(actionData.ai.transform.position, targetPosition);

        // If target is out of range, move closer
        if (distanceToTarget > shootAction.AttackRange)
        {
            HandleMovementToTarget();
            actionData.state = ActionState.Running;
            return;
        }

        // Check line of sight if required
        if (shootAction.RequiresLineOfSight && !HasLineOfSight(targetPosition))
        {
            HandleMovementToTarget();
            actionData.state = ActionState.Running;
            return;
        }

        // Stop moving if required
        if (shootAction.StopMovingWhileShooting && wasMoving)
        {
            UnitMover mover = actionData.ai.UnitMover;
            if (mover != null)
            {
                mover.StopMovement();
            }
            wasMoving = false;
        }

        // Face the target
        FaceTarget(targetPosition);

        // Check if we can shoot (fire rate cooldown)
        float timeSinceLastShot = Time.time - lastShotTime;
        float shootCooldown = 1f / shootAction.FireRate;

        if (timeSinceLastShot >= shootCooldown)
        {
            Shoot(targetPosition);
        }

        actionData.state = ActionState.Running;
    }
}