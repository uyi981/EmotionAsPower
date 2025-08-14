using UnityEngine;

public class ShootActionExecutor : AIActionExecutor
{
    private ShootAction shootAction;
    private float lastShotTime;
    private bool wasMoving;
    private Vector3 lastTargetPosition;

    // Status properties for external access
    public bool IsInRange { get; private set; }
    public bool HasClearLineOfSight { get; private set; }
    public bool IsShooting { get; private set; }
    public bool IsMovingToTarget { get; private set; }
    public float DistanceToTarget { get; private set; }
    public bool CanShoot { get; private set; } // Based on cooldown

    public ShootActionExecutor(AIActionData data, ShootAction action) : base(data)
    {
        shootAction = action;
        ResetStatusProperties();
    }

    private void ResetStatusProperties()
    {
        IsInRange = false;
        HasClearLineOfSight = false;
        IsShooting = false;
        IsMovingToTarget = false;
        DistanceToTarget = float.MaxValue;
        CanShoot = false;
    }

    public override void StartAction()
    {
        lastShotTime = 0f;
        wasMoving = false;
        lastTargetPosition = Vector3.zero;
        ResetStatusProperties();
    }

    public override ActionState UpdateAction()
    {
        if (actionData.targetObject == null)
        {
            ResetStatusProperties();
            return ActionState.Failed;
        }

        // Check if target is still alive
        Health targetHealth = actionData.targetObject.GetComponent<Health>();
        if (targetHealth != null && targetHealth.IsDead)
        {
            ResetStatusProperties();
            return ActionState.Success;
        }

        UpdateStatusProperties();

        // If target is out of range, move closer
        if (!IsInRange)
        {
            HandleMovementToTarget();
            return ActionState.Running;
        }

        // Target is in range - stop moving and always shoot
        StopMovementIfNeeded();

        // Face the target
        FaceTarget();

        // Always shoot when in range (ignore line of sight once stopped)
        if (CanShoot)
        {
            Shoot();
        }

        return ActionState.Running;
    }

    private void UpdateStatusProperties()
    {
        if (actionData.targetObject == null)
        {
            ResetStatusProperties();
            return;
        }

        Vector3 targetPosition = actionData.targetObject.transform.position;
        DistanceToTarget = Vector3.Distance(actionData.ai.transform.position, targetPosition);
        IsInRange = DistanceToTarget <= shootAction.AttackRange;
        HasClearLineOfSight = !shootAction.RequiresLineOfSight || HasLineOfSight(targetPosition);

        // Check if we can shoot based on cooldown
        float timeSinceLastShot = Time.time - lastShotTime;
        float shootCooldown = 1f / shootAction.FireRate;
        CanShoot = timeSinceLastShot >= shootCooldown;
    }

    private void HandleMovementToTarget()
    {
        UnitMover mover = actionData.ai.UnitMover;
        if (mover == null)
        {
            IsMovingToTarget = false;
            return;
        }

        Vector3 currentTargetPos = actionData.targetObject.transform.position;

        // Move to optimal attack position (85% of max range for grid-based map to account for pathfinding)
        Vector3 directionToTarget = (currentTargetPos - actionData.ai.transform.position).normalized;
        Vector3 optimalPosition = currentTargetPos - directionToTarget * (shootAction.AttackRange * 0.85f);

        mover.MoveToWorldPosition(optimalPosition);
        IsMovingToTarget = true;
        wasMoving = true;
        lastTargetPosition = currentTargetPos;
    }

    private void StopMovementIfNeeded()
    {
        if (shootAction.StopMovingWhileShooting && wasMoving)
        {
            UnitMover mover = actionData.ai.UnitMover;
            if (mover != null)
            {
                mover.StopMovement();
            }
            wasMoving = false;
        }
        IsMovingToTarget = false;
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

    private void FaceTarget()
    {
        if (actionData.targetObject == null) return;

        UnitMover mover = actionData.ai.UnitMover;
        if (mover == null || !mover.flipable) return;

        Vector3 targetPosition = actionData.targetObject.transform.position;
        Vector3 directionToTarget = (targetPosition - actionData.ai.transform.position).normalized;

        // Determine flip direction based on target position
        if (Mathf.Abs(directionToTarget.x) > 0.1f)
        {
            bool shouldFlip = mover.baseFlip ? (directionToTarget.x >= 0) : (directionToTarget.x < 0);
            mover.ToggleFlip(shouldFlip);
        }
    }

    private void Shoot()
    {
        if (actionData.targetObject == null) return;

        lastShotTime = Time.time;
        IsShooting = true;

        Vector3 targetPosition = actionData.targetObject.transform.position;
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

        // Reset shooting flag after a brief moment (will be set to false on next frame update)
        IsShooting = false;
    }

    public override void StopAction()
    {
        UnitMover mover = actionData.ai.UnitMover;
        if (mover != null)
        {
            mover.StopMovement();
        }
        wasMoving = false;
        IsMovingToTarget = false;
        IsShooting = false;
    }

    public override void OnActionComplete()
    {
        wasMoving = false;
        IsMovingToTarget = false;
        IsShooting = false;
    }

    public override void OnActionInterrupted()
    {
        UnitMover mover = actionData.ai.UnitMover;
        if (mover != null)
        {
            mover.StopMovement();
        }
        wasMoving = false;
        IsMovingToTarget = false;
        IsShooting = false;
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

        // Check if target is still alive
        Health targetHealth = actionData.targetObject.GetComponent<Health>();
        if (targetHealth != null && targetHealth.IsDead)
        {
            actionData.state = ActionState.Success;
            OnActionComplete();
            return;
        }

        // Update all status properties
        UpdateStatusProperties();

        // If target is out of range, move closer
        if (!IsInRange)
        {
            HandleMovementToTarget();
            actionData.state = ActionState.Running;
            return;
        }

        // We're in range - stop moving and always shoot
        StopMovementIfNeeded();

        // Face the target
        FaceTarget();

        // Always shoot when in range (ignore line of sight once stopped)
        if (CanShoot)
        {
            Shoot();
        }

        actionData.state = ActionState.Running;
    }
}