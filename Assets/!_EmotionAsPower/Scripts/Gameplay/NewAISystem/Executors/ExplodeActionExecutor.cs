using UnityEngine;

public class ExplodeActionExecutor : AIActionExecutor
{
    private bool isMovingToTarget;
    private bool hasExploded;
    private bool isExploding;
    private float explosionTimer;
    private ExplodeAction explodeAction;
    private Vector3 lastTargetPosition;
    private float pathRecalculationCooldown = 0.5f;
    private float lastPathCalculationTime;
    private float stuckCheckDistance = 0.1f;
    private float stuckCheckTime = 2f;
    private Vector3 lastPositionCheck;
    private float lastPositionTime;

    public ExplodeActionExecutor(AIActionData data, ExplodeAction action) : base(data)
    {
        explodeAction = action;
    }

    public override void StartAction()
    {
        isMovingToTarget = false;
        hasExploded = false;
        isExploding = false;
        explosionTimer = 0f;
        lastTargetPosition = Vector3.zero;
        lastPathCalculationTime = 0f;
        lastPositionCheck = actionData.ai.transform.position;
        lastPositionTime = Time.time;
    }

    public override ActionState UpdateAction()
    {
        if (actionData.targetObject == null)
            return ActionState.Failed;

        // Check if target is still alive
        Health targetHealth = actionData.targetObject.GetComponent<Health>();
        if (targetHealth != null && targetHealth.IsDead)
            return ActionState.Success;

        // If already exploded, action is complete
        if (hasExploded)
            return ActionState.Success;

        float distanceToTarget = Vector3.Distance(actionData.ai.transform.position, actionData.targetObject.transform.position);

        if (distanceToTarget <= explodeAction.TriggerRange)
        {
            // Within explosion range - start exploding
            if (!isExploding)
            {
                StartExploding();
            }

            // Handle explosion timer
            explosionTimer += Time.deltaTime;
            if (explosionTimer >= explodeAction.ExplosionDelay)
            {
                Explode();
                return ActionState.Success;
            }

            return ActionState.Running;
        }
        else
        {
            // Handle movement to target with improved logic
            HandleMovementToTarget();
            return ActionState.Running;
        }
    }

    private void HandleMovementToTarget()
    {
        UnitMover mover = actionData.ai.UnitMover;
        if (mover == null) return;

        Vector3 currentTargetPos = actionData.targetObject.transform.position;
        bool targetMoved = Vector3.Distance(lastTargetPosition, currentTargetPos) > 0.5f;
        bool canRecalculatePath = Time.time - lastPathCalculationTime > pathRecalculationCooldown;

        bool isStuck = CheckIfStuck();

        if (!isMovingToTarget || targetMoved || isStuck)
        {
            if (canRecalculatePath || !isMovingToTarget)
            {
                mover.MoveToWorldPosition(currentTargetPos);
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

        if (isExploding && !hasExploded)
        {
            Explode();
        }
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

        if (!hasExploded)
        {
            isExploding = false;
            explosionTimer = 0f;
        }
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
            Debug.LogWarning("Explode action stopping - no target");
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

        // If already exploded, action is complete
        if (hasExploded)
        {
            actionData.state = ActionState.Success;
            OnActionComplete();
            return;
        }

        float distanceToTarget = Vector3.Distance(actionData.ai.transform.position, actionData.targetObject.transform.position);

        if (distanceToTarget <= explodeAction.TriggerRange)
        {
            if (isMovingToTarget)
            {
                UnitMover mover = actionData.ai.UnitMover;
                if (mover != null)
                {
                    mover.StopMovement();
                    isMovingToTarget = false;
                }
            }

            if (!isExploding)
            {
                StartExploding();
            }

            explosionTimer += Time.deltaTime;
            if (explosionTimer >= explodeAction.ExplosionDelay)
            {
                Explode();
                actionData.state = ActionState.Success;
                OnActionComplete();
                return;
            }

            actionData.state = ActionState.Running;
        }
        else
        {
            HandleMovementToTarget();
            actionData.state = ActionState.Running;
        }
    }

    private void StartExploding()
    {
        isExploding = true;
        explosionTimer = 0f;

        Debug.Log($"{actionData.ai.name} starting explosion sequence!");

        // Stop any movement
        UnitMover mover = actionData.ai.UnitMover;
        if (mover != null)
        {
            mover.StopMovement();
        }
        isMovingToTarget = false;
    }

    private void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;
        Vector3 explosionPosition = actionData.ai.transform.position;

        Debug.Log($"{actionData.ai.name} exploding at {explosionPosition}!");

        // Spawn the explosive object and configure it with enhanced scaling
        if (explodeAction.ExplosiveObjectPrefab != null)
        {
            GameObject explosiveGO = Object.Instantiate(explodeAction.ExplosiveObjectPrefab, explosionPosition, Quaternion.identity, EnemyManager.Instance.explosivesParent);
            Explosive explosive = explosiveGO.GetComponent<Explosive>();

            if (explosive != null)
            {
                // Configure basic explosion properties
                explosive.explosionRange = explodeAction.ExplosionRange;
                explosive.explosionDamage = explodeAction.ExplosionDamage;
                explosive.damageLayerMask = explodeAction.DamageLayerMask;
                explosive.explosionPrefab = explodeAction.ExplosionPrefab;
                explosive.explosionSound = explodeAction.ExplosionSound;

                // Enhanced scaling configuration
                explosive.scaleWithDistance = true;

                // Create a realistic damage falloff curve
                AnimationCurve damageCurve = new AnimationCurve();
                damageCurve.AddKey(0f, 1f);    // Full damage at center
                damageCurve.AddKey(0.3f, 0.8f); // 80% damage at 30% range
                damageCurve.AddKey(0.6f, 0.5f); // 50% damage at 60% range
                damageCurve.AddKey(1f, 0.1f);   // 10% damage at edge

                // Smooth the curve
                for (int i = 0; i < damageCurve.keys.Length; i++)
                {
                    damageCurve.SmoothTangents(i, 0.5f);
                }

                explosive.damageFalloffCurve = damageCurve;

                // Enable physics force for more realistic explosions
                explosive.usePhysicsForce = true;
                explosive.explosionForce = explodeAction.ExplosionDamage * 10f; // Scale force with damage

                explosive.TriggerExplosion(0f);
            }
        }

        // Destroy self if specified
        if (explodeAction.DestroySelfOnExplode)
        {
            Health selfHealth = actionData.ai.GetComponent<Health>();
            if (selfHealth != null)
            {
                selfHealth.Die();
            }
            else
            {
                Object.Destroy(actionData.ai.gameObject);
            }
        }
    }
}