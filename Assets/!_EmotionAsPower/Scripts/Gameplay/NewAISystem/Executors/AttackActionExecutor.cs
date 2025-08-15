using UnityEngine;
using System.Collections;

public class AttackActionExecutor : AIActionExecutor
{
    private float lastAttackTime;
    private bool isMovingToTarget;
    private AttackAction attackAction;
    private Vector3 lastTargetPosition;
    private Vector3 targetAttackPosition;
    private float pathRecalculationCooldown = 0.5f;
    private float lastPathCalculationTime;
    private float stuckCheckDistance = 0.1f;
    private float stuckCheckTime = 2f;
    private Vector3 lastPositionCheck;
    private float lastPositionTime;
    private Animator animator;
    private AudioSource audioSource;
    private bool isPlayingAttackAnimation;
    private float animationStartTime;
    private bool hasDamageBeenApplied;

    public AttackActionExecutor(AIActionData data, AttackAction action) : base(data)
    {
        attackAction = action;
        animator = actionData.ai.GetComponent<Animator>();

        // Setup AudioSource properly
        audioSource = actionData.ai.GetComponent<AudioSource>();
        if (audioSource == null && attackAction.AttackSound != null)
        {
            audioSource = actionData.ai.gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource settings to match ShootActionExecutor behavior
        if (audioSource != null)
        {
            audioSource.loop = false; // Ensure it doesn't loop
            audioSource.playOnAwake = false;
            // Optional: Configure 3D settings if you want distance-based volume
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = 50f; // Adjust as needed
            audioSource.minDistance = 1f;
        }
    }

    public override void StartAction()
    {
        lastAttackTime = 0f;
        isMovingToTarget = false;
        lastTargetPosition = Vector3.zero;
        targetAttackPosition = Vector3.zero;
        lastPathCalculationTime = 0f;
        lastPositionCheck = actionData.ai.transform.position;
        lastPositionTime = Time.time;
        isPlayingAttackAnimation = false;
        hasDamageBeenApplied = false;

        Debug.Log($"AttackAction started for {actionData.ai.name}");
    }

    public override ActionState UpdateAction()
    {
        if (actionData.targetObject == null)
        {
            Debug.Log("Attack failed: No target object");
            return ActionState.Failed;
        }

        Health targetHealth = actionData.targetObject.GetComponent<Health>();
        IHealth health = actionData.targetObject.GetComponent<IHealth>();

        // Check if target is dead
        bool targetIsDead = false;
        if (health != null)
        {
            targetIsDead = health.IsDead();
        }
        else if (targetHealth != null)
        {
            targetIsDead = targetHealth.IsDead;
        }
        else
        {
            Debug.LogWarning("Target has no Health or IHealth component!");
            return ActionState.Failed;
        }

        if (targetIsDead)
        {
            Debug.Log("Attack succeeded: Target is dead");
            return ActionState.Success;
        }

        // Handle ongoing attack animation
        if (isPlayingAttackAnimation)
        {
            return HandleAttackAnimation(targetHealth, health);
        }

        // Calculate attack position - try to find a good position around the target
        Vector3 targetPos = actionData.targetObject.transform.position;
        if (attackAction.UseSmartPositioning)
        {
            CalculateOptimalAttackPosition(targetPos);
        }
        else
        {
            targetAttackPosition = targetPos;
        }

        float distanceToTarget = Vector3.Distance(actionData.ai.transform.position, targetPos);
        float distanceToAttackPosition = Vector3.Distance(actionData.ai.transform.position, targetAttackPosition);

        Debug.Log($"Distance to target: {distanceToTarget}, Distance to attack position: {distanceToAttackPosition}, Attack range: {attackAction.AttackRange}");

        // Check if we can attack from current position (direct distance to target)
        if (distanceToTarget <= attackAction.AttackRange)
        {
            // Stop movement when in range
            if (isMovingToTarget)
            {
                UnitMover mover = actionData.ai.UnitMover;
                if (mover != null)
                {
                    mover.StopMovement();
                }
                isMovingToTarget = false;
                Debug.Log("Stopped movement - in attack range of target");
            }

            // Check if we can attack (cooldown check)
            float timeSinceLastAttack = Time.time - lastAttackTime;
            float attackCooldown = 1f / attackAction.AttackSpeed;

            Debug.Log($"Time since last attack: {timeSinceLastAttack}, Cooldown: {attackCooldown}");

            if (timeSinceLastAttack >= attackCooldown && !isPlayingAttackAnimation)
            {
                Debug.Log("Starting attack sequence");
                StartAttackSequence(targetHealth, health);
                lastAttackTime = Time.time;
            }
            return ActionState.Running; // Keep running to continue attacking
        }
        else
        {
            // Move to a good attack position
            Debug.Log("Moving to attack position");
            HandleMovementToTarget();
            return ActionState.Running;
        }
    }

    private void StartAttackSequence(Health targetHealth, IHealth health)
    {
        Debug.Log($"Attack sequence started on {actionData.targetObject.name}");

        // Face the target before attacking
        FaceTarget();

        isPlayingAttackAnimation = true;
        animationStartTime = Time.time;
        hasDamageBeenApplied = false;

        // Play animation
        if (animator != null && !string.IsNullOrEmpty(attackAction.AttackAnimationTrigger))
        {
            Debug.Log($"Playing attack animation: {attackAction.AttackAnimationTrigger}");
            animator.SetTrigger(attackAction.AttackAnimationTrigger);
        }
        else
        {
            Debug.LogWarning("No animator or animation trigger set!");
        }

        // Play sound using the same method as ShootActionExecutor
        if (attackAction.AttackSound != null)
        {
            Vector3 attackPosition = actionData.ai.transform.position;
            AudioSource.PlayClipAtPoint(attackAction.AttackSound, attackPosition);
            Debug.Log("Attack sound played at position");
        }
    }

    private ActionState HandleAttackAnimation(Health targetHealth, IHealth health)
    {
        float timeSinceAnimStart = Time.time - animationStartTime;

        // Apply damage at the correct time during animation
        if (!hasDamageBeenApplied && timeSinceAnimStart >= attackAction.DamageDelayFromAnimStart)
        {
            Debug.Log($"Applying damage: {attackAction.AttackDamage}");
            ApplyDamageWithEffects(targetHealth, health);
            hasDamageBeenApplied = true;
        }

        // Check if target died after damage
        bool targetIsDead = false;
        if (health != null)
        {
            targetIsDead = health.IsDead();
        }
        else if (targetHealth != null)
        {
            targetIsDead = targetHealth.IsDead;
        }

        if (targetIsDead)
        {
            Debug.Log("Target died during attack animation");
            isPlayingAttackAnimation = false;
            return ActionState.Success;
        }

        // Check if animation is complete - if so, reset for next attack cycle
        if (timeSinceAnimStart >= attackAction.AnimationDuration)
        {
            Debug.Log("Attack animation complete - ready for next attack");
            isPlayingAttackAnimation = false;
            // Continue running to allow for next attack cycle
        }

        return ActionState.Running;
    }

    private void ApplyDamageWithEffects(Health targetHealth, IHealth health)
    {
        Debug.Log($"Applying {attackAction.AttackDamage} damage to {actionData.targetObject.name}");

        // Apply damage to the appropriate component
        if (health != null)
        {
            health.TakeDamage(attackAction.AttackDamage);
        }
        else if (targetHealth != null)
        {
            targetHealth.TakeDamage(attackAction.AttackDamage);
        }

        // Spawn visual effect
        if (attackAction.AttackEffect != null)
        {
            Vector3 effectPosition = actionData.targetObject.transform.position;
            GameObject effect = GameObject.Instantiate(attackAction.AttackEffect, effectPosition, Quaternion.identity);
            if (attackAction.EffectDuration > 0)
            {
                GameObject.Destroy(effect, attackAction.EffectDuration);
            }
            Debug.Log("Attack effect spawned");
        }
    }

    private void CalculateOptimalAttackPosition(Vector3 targetPosition)
    {
        Vector3 aiPosition = actionData.ai.transform.position;

        // Try multiple positions around the target
        Vector3[] potentialPositions = {
            new Vector3(targetPosition.x + attackAction.PositioningOffset, targetPosition.y, targetPosition.z), // Right
            new Vector3(targetPosition.x - attackAction.PositioningOffset, targetPosition.y, targetPosition.z), // Left
            new Vector3(targetPosition.x, targetPosition.y, targetPosition.z + attackAction.PositioningOffset), // Forward
            new Vector3(targetPosition.x, targetPosition.y, targetPosition.z - attackAction.PositioningOffset), // Back
        };

        bool needsRecalculation = targetAttackPosition == Vector3.zero || Vector3.Distance(lastTargetPosition, targetPosition) > 0.5f;

        if (needsRecalculation)
        {
            Vector3 bestPosition = targetPosition; // Fallback to target position
            float bestDistance = float.MaxValue;

            foreach (Vector3 pos in potentialPositions)
            {
                // Check if the position is walkable on the grid
                if (IsPositionWalkable(pos))
                {
                    float distance = Vector3.Distance(aiPosition, pos);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestPosition = pos;
                    }
                }
            }

            targetAttackPosition = bestPosition;
            lastTargetPosition = targetPosition;
        }
    }

    private bool IsPositionWalkable(Vector3 worldPosition)
    {
        if (GridSystem.Instance == null) return true;

        Grid grid = GridSystem.Instance.grid;
        float[,] gridMap = GridSystem.Instance.gridMap;

        Vector3Int gridPos = grid.WorldToCell(worldPosition);

        // Apply grid offset (assuming 50,50 like in PlacementSystem)
        int gridX = gridPos.x + 50;
        int gridZ = gridPos.z + 50;

        // Check bounds
        if (gridX < 0 || gridZ < 0 || gridX >= gridMap.GetLength(0) || gridZ >= gridMap.GetLength(1))
            return false;

        // Check if cell is walkable (0 = walkable, 1 = occupied)
        return gridMap[gridX, gridZ] == 0;
    }

    private void HandleMovementToTarget()
    {
        UnitMover mover = actionData.ai.UnitMover;
        if (mover == null) return;

        Vector3 currentTargetPos = actionData.targetObject.transform.position;

        // Check if we're already in attack range of the actual target before recalculating
        float currentDistanceToTarget = Vector3.Distance(actionData.ai.transform.position, currentTargetPos);
        if (currentDistanceToTarget <= attackAction.AttackRange)
        {
            // We're in range of target, stop moving
            if (isMovingToTarget)
            {
                mover.StopMovement();
                isMovingToTarget = false;
                Debug.Log("In range of target, stopping movement");
            }
            return;
        }

        bool targetMoved = Vector3.Distance(lastTargetPosition, currentTargetPos) > 0.5f;
        bool canRecalculatePath = Time.time - lastPathCalculationTime > pathRecalculationCooldown;
        bool isStuck = CheckIfStuck();

        if (!isMovingToTarget || targetMoved || isStuck)
        {
            if (canRecalculatePath || !isMovingToTarget)
            {
                Vector3 moveTarget;
                if (attackAction.UseSmartPositioning)
                {
                    CalculateOptimalAttackPosition(currentTargetPos);
                    moveTarget = targetAttackPosition;
                }
                else
                {
                    moveTarget = currentTargetPos;
                }

                Debug.Log($"Moving to position: {moveTarget}");
                mover.MoveToWorldPosition(moveTarget);
                isMovingToTarget = true;
                lastTargetPosition = currentTargetPos;
                lastPathCalculationTime = Time.time;
                ResetStuckCheck();
            }
        }
    }

    private bool CheckIfStuck()
    {
        Vector3 currentPosition = actionData.ai.transform.position;
        float timeDiff = Time.time - lastPositionTime;
        if (timeDiff > stuckCheckTime)
        {
            float distanceMoved = Vector3.Distance(currentPosition, lastPositionCheck);
            bool stuck = distanceMoved < stuckCheckDistance && isMovingToTarget;
            lastPositionCheck = currentPosition;
            lastPositionTime = Time.time;
            return stuck;
        }
        return false;
    }

    private void ResetStuckCheck()
    {
        lastPositionCheck = actionData.ai.transform.position;
        lastPositionTime = Time.time;
    }

    public override void StopAction()
    {
        UnitMover mover = actionData.ai.UnitMover;
        if (mover != null)
        {
            mover.StopMovement();
        }
        isMovingToTarget = false;
        isPlayingAttackAnimation = false;

        // Stop any playing audio to prevent looping issues
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        Debug.Log("Attack action stopped");
    }

    public override void OnActionComplete()
    {
        isMovingToTarget = false;
        isPlayingAttackAnimation = false;

        // Stop any playing audio
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        Debug.Log("Attack action completed");
    }

    public override void OnActionInterrupted()
    {
        UnitMover mover = actionData.ai.UnitMover;
        if (mover != null)
        {
            mover.StopMovement();
        }
        isMovingToTarget = false;
        isPlayingAttackAnimation = false;

        // Stop any playing audio
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        Debug.Log("Attack action interrupted");
    }

    public override void Perform()
    {
        // Always call UpdateAction, don't check if state is running
        ActionState result = UpdateAction();
        actionData.state = result;

        if (result == ActionState.Success)
        {
            OnActionComplete();
        }
        else if (result == ActionState.Failed)
        {
            StopAction();
        }
    }

    private void FaceTarget()
    {
        if (actionData.targetObject == null) return;

        Vector3 targetPosition = actionData.targetObject.transform.position;
        Vector3 aiPosition = actionData.ai.transform.position;
        Vector3 directionToTarget = (targetPosition - aiPosition).normalized;

        // Use the UnitMover's flip functionality if available
        UnitMover mover = actionData.ai.UnitMover;
        if (mover != null && mover.flipable)
        {
            // Determine flip direction based on target position
            if (Mathf.Abs(directionToTarget.x) > 0.1f)
            {
                bool shouldFlip = mover.baseFlip ? (directionToTarget.x >= 0) : (directionToTarget.x < 0);
                mover.ToggleFlip(shouldFlip);
            }
        }
    }
}