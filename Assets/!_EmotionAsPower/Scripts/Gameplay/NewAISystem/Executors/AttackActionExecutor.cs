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
        audioSource = actionData.ai.GetComponent<AudioSource>();
        if (audioSource == null && attackAction.AttackSound != null)
        {
            audioSource = actionData.ai.gameObject.AddComponent<AudioSource>();
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
    }

    public override ActionState UpdateAction()
    {
        if (actionData.targetObject == null)
            return ActionState.Failed;

        Health targetHealth = actionData.targetObject.GetComponent<Health>();
        IHealth health = actionData.targetObject.GetComponent<IHealth>();
        if (health == null)
        {
            Debug.LogWarning("Ihealth not found");
            if ((targetHealth == null || targetHealth.IsDead))
                return ActionState.Success;
            if (isPlayingAttackAnimation)
            {
                return HandleAttackAnimation(targetHealth);
            }
        }

        else
        {
            Debug.LogWarning("Attacking ihealth");
            if (health.IsDead())
                return ActionState.Success;
            if (isPlayingAttackAnimation)
            {
                return HandleAttackAnimationWithIHealth(health);
            }
        }

        Vector3 targetPos = actionData.targetObject.transform.position;
        if (attackAction.UseSmartPositioning)
        {
            CalculateOptimalAttackPosition(targetPos);
        }
        else
        {
            targetAttackPosition = targetPos;
        }

        float distanceToAttackPosition = Vector3.Distance(actionData.ai.transform.position, targetAttackPosition);
        if (distanceToAttackPosition <= attackAction.AttackRange)
        {
            // FIXED: Stop movement and stay in attack range
            if (isMovingToTarget)
            {
                UnitMover mover = actionData.ai.UnitMover;
                if (mover != null)
                {
                    mover.StopMovement();
                }
                isMovingToTarget = false;
            }

            float timeSinceLastAttack = Time.time - lastAttackTime;
            if (timeSinceLastAttack >= 1f / attackAction.AttackSpeed)
            {
                if (health == null)
                {
                    StartAttackSequence(targetHealth);
                }
                else
                {
                    StartAttackSequenceWithIHealth(health);
                }
                lastAttackTime = Time.time;
            }
            return ActionState.Running;
        }
        else
        {
            // FIXED: Only move if not in attack range
            HandleMovementToTarget();
            return ActionState.Running;
        }
    }

    private void CalculateOptimalAttackPosition(Vector3 targetPosition)
    {
        Vector3 aiPosition = actionData.ai.transform.position;
        Vector3 rightPosition = new Vector3(targetPosition.x + attackAction.PositioningOffset, targetPosition.y, targetPosition.z);
        Vector3 leftPosition = new Vector3(targetPosition.x - attackAction.PositioningOffset, targetPosition.y, targetPosition.z);
        float distanceToRight = Vector3.Distance(aiPosition, rightPosition);
        float distanceToLeft = Vector3.Distance(aiPosition, leftPosition);
        bool needsRecalculation = targetAttackPosition == Vector3.zero || Vector3.Distance(lastTargetPosition, targetPosition) > 0.5f;

        if (needsRecalculation)
        {
            targetAttackPosition = distanceToLeft < distanceToRight ? leftPosition : rightPosition;
            Vector3 directionToAttackPos = (targetAttackPosition - targetPosition).normalized;
            if (Physics.Raycast(targetPosition, directionToAttackPos, out RaycastHit hit, attackAction.PositioningOffset))
            {
                targetAttackPosition = distanceToLeft < distanceToRight ? rightPosition : leftPosition;
            }
        }
    }

    private void StartAttackSequence(Health targetHealth)
    {
        // Face the target before attacking
        FaceTarget();

        isPlayingAttackAnimation = true;
        animationStartTime = Time.time;
        hasDamageBeenApplied = false;

        if (animator != null && !string.IsNullOrEmpty(attackAction.AttackAnimationTrigger))
        {
            animator.SetTrigger(attackAction.AttackAnimationTrigger);
        }

        if (audioSource != null && attackAction.AttackSound != null)
        {
            audioSource.PlayOneShot(attackAction.AttackSound);
        }
    }

    private void StartAttackSequenceWithIHealth(IHealth targetHealth)
    {
        // Face the target before attacking
        FaceTarget();

        isPlayingAttackAnimation = true;
        animationStartTime = Time.time;
        hasDamageBeenApplied = false;

        if (animator != null && !string.IsNullOrEmpty(attackAction.AttackAnimationTrigger))
        {
            animator.SetTrigger(attackAction.AttackAnimationTrigger);
        }

        if (audioSource != null && attackAction.AttackSound != null)
        {
            audioSource.PlayOneShot(attackAction.AttackSound);
        }
    }

    private ActionState HandleAttackAnimation(Health targetHealth)
    {
        float timeSinceAnimStart = Time.time - animationStartTime;

        ApplyDamageWithEffects(targetHealth);
        hasDamageBeenApplied = true;

        if (targetHealth.IsDead)
        {
            isPlayingAttackAnimation = false; // Reset animation state
            return ActionState.Success;
        }

        // Reset animation state after animation completes to allow new attacks
        if (timeSinceAnimStart >= attackAction.AnimationDuration)
        {
            isPlayingAttackAnimation = false;
        }

        return ActionState.Running;
    }

    private ActionState HandleAttackAnimationWithIHealth(IHealth targetHealth)
    {
        float timeSinceAnimStart = Time.time - animationStartTime;

        ApplyDamageWithEffectOnIHealth(targetHealth);
        hasDamageBeenApplied = true;

        if (targetHealth.IsDead())
        {
            isPlayingAttackAnimation = false; // Reset animation state
            return ActionState.Success;
        }

        // Reset animation state after animation completes to allow new attacks
        if (timeSinceAnimStart >= attackAction.AnimationDuration)
        {
            isPlayingAttackAnimation = false;
        }

        return ActionState.Running;
    }

    private void ApplyDamageWithEffects(Health targetHealth)
    {
        targetHealth.TakeDamage(attackAction.AttackDamage);
        if (attackAction.AttackEffect != null)
        {
            Vector3 effectPosition = actionData.targetObject.transform.position;
            GameObject effect = GameObject.Instantiate(attackAction.AttackEffect, effectPosition, Quaternion.identity);
            if (attackAction.EffectDuration > 0)
            {
                GameObject.Destroy(effect, attackAction.EffectDuration);
            }
        }
    }

    private void ApplyDamageWithEffectOnIHealth(IHealth targetHealth)
    {
        targetHealth.TakeDamage(attackAction.AttackDamage);
        if (attackAction.AttackEffect != null)
        {
            Vector3 effectPosition = actionData.targetObject.transform.position;
            GameObject effect = GameObject.Instantiate(attackAction.AttackEffect, effectPosition, Quaternion.identity);
            if (attackAction.EffectDuration > 0)
            {
                GameObject.Destroy(effect, attackAction.EffectDuration);
            }
        }
    }

    private void HandleMovementToTarget()
    {
        UnitMover mover = actionData.ai.UnitMover;
        if (mover == null) return;

        Vector3 currentTargetPos = actionData.targetObject.transform.position;

        // FIXED: Check if we're already in attack range before recalculating
        float currentDistanceToTarget = Vector3.Distance(actionData.ai.transform.position, currentTargetPos);
        if (currentDistanceToTarget <= attackAction.AttackRange)
        {
            // We're in range, stop moving
            if (isMovingToTarget)
            {
                mover.StopMovement();
                isMovingToTarget = false;
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
                if (attackAction.UseSmartPositioning)
                {
                    CalculateOptimalAttackPosition(currentTargetPos);
                    mover.MoveToWorldPosition(targetAttackPosition);
                }
                else
                {
                    mover.MoveToWorldPosition(currentTargetPos);
                }
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
    }

    public override void OnActionComplete()
    {
        isMovingToTarget = false;
        isPlayingAttackAnimation = false;
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
            Debug.LogWarning("Stopping action - no target");
            actionData.state = ActionState.Failed;
            StopAction();
            return;
        }

        Health targetHealth = actionData.targetObject.GetComponent<Health>();
        IHealth health = actionData.targetObject.GetComponent<IHealth>();
        if (health != null)
        {
            if (health.IsDead())
            {
                actionData.state = ActionState.Success;
                OnActionComplete();
                return;
            }
        }
        else if ((targetHealth == null || targetHealth.IsDead))
        {
            actionData.state = ActionState.Success;
            OnActionComplete();
            return;
        }

        if (isPlayingAttackAnimation)
        {
            ActionState animResult;
            if (health != null)
            {
                animResult = HandleAttackAnimationWithIHealth(health);
            }
            else
            {
                animResult = HandleAttackAnimation(targetHealth);
            }
            actionData.state = animResult;
            if (animResult != ActionState.Running)
            {
                if (animResult == ActionState.Success)
                    OnActionComplete();
                return;
            }
        }
        else
        {
            Vector3 targetPos = actionData.targetObject.transform.position;
            if (attackAction.UseSmartPositioning)
            {
                CalculateOptimalAttackPosition(targetPos);
            }
            else
            {
                targetAttackPosition = targetPos;
            }

            float distanceToTarget = Vector3.Distance(actionData.ai.transform.position, targetAttackPosition);
            if (distanceToTarget <= attackAction.AttackRange)
            {
                // FIXED: Stop movement when in range
                if (isMovingToTarget)
                {
                    UnitMover mover = actionData.ai.UnitMover;
                    if (mover != null)
                    {
                        mover.StopMovement();
                        isMovingToTarget = false;
                    }
                }

                float timeSinceLastAttack = Time.time - lastAttackTime;
                if (timeSinceLastAttack >= 1f / attackAction.AttackSpeed && !isPlayingAttackAnimation)
                {
                    if (health == null)
                    {
                        StartAttackSequence(targetHealth);
                    }
                    else
                    {
                        StartAttackSequenceWithIHealth(health);
                    }
                    lastAttackTime = Time.time;
                }
            }
            else
            {
                // FIXED: Only handle movement if not in range
                HandleMovementToTarget();
            }
        }

        if (health != null)
        {
            if (health.IsDead())
            {
                actionData.state = ActionState.Success;
                OnActionComplete();
            }
        }
        else if (targetHealth.IsDead)
        {
            actionData.state = ActionState.Success;
            OnActionComplete();
        }
        else
        {
            actionData.state = ActionState.Running;
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