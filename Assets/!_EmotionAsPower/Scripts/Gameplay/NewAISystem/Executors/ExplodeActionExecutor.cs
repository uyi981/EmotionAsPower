using UnityEngine;

public class ExplodeActionExecutor : AIActionExecutor
{
    private bool isMovingToTarget;
    private bool hasExploded;
    private bool isExploding;
    private float explosionTimer;
    private ExplodeAction explodeAction;

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

            // Check if unit stopped moving and restart movement if needed
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

        // If we're in the middle of exploding, complete the explosion
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

        // Reset explosion state if interrupted before exploding
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

        // Check if target is still alive
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
            // Stop moving if within trigger range
            if (isMovingToTarget)
            {
                UnitMover mover = actionData.ai.UnitMover;
                if (mover != null)
                {
                    mover.StopMovement();
                    isMovingToTarget = false;
                }
            }

            // Start explosion sequence
            if (!isExploding)
            {
                StartExploding();
            }

            // Handle explosion timer
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

        // Find all colliders within explosion range
        Collider[] hitColliders = Physics.OverlapSphere(explosionPosition, explodeAction.ExplosionRange, explodeAction.DamageLayerMask);

        foreach (Collider hitCollider in hitColliders)
        {
            // Don't damage self unless specified
            if (hitCollider.gameObject == actionData.ai.gameObject && !explodeAction.DestroySelfOnExplode)
                continue;

            Health health = hitCollider.GetComponent<Health>();
            if (health != null)
            {
                // Calculate damage based on distance (optional falloff)
                float distance = Vector3.Distance(explosionPosition, hitCollider.transform.position);
                float damageFalloff = Mathf.Clamp01(1 - (distance / explodeAction.ExplosionRange));
                float finalDamage = explodeAction.ExplosionDamage * damageFalloff;

                health.TakeDamage(finalDamage);
                Debug.Log($"Explosion damaged {hitCollider.name} for {finalDamage} damage");
            }
        }

        // Spawn explosion effect
        if (explodeAction.ExplosionPrefab != null)
        {
            GameObject explosion = Object.Instantiate(explodeAction.ExplosionPrefab, explosionPosition, Quaternion.identity);
            // Auto-destroy explosion effect after some time
            Object.Destroy(explosion, 5f);
        }

        // Play explosion sound
        if (explodeAction.ExplosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explodeAction.ExplosionSound, explosionPosition);
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
