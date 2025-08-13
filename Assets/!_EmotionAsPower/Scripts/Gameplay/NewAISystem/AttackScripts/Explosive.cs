using UnityEngine;

public class Explosive : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRange = 3f;
    [Header("Using explode action damage instead of this")]
    public float explosionDamage = 50f;
    public LayerMask damageLayerMask = -1;
    public bool usePhysicsForce = true;
    public float explosionForce = 500f;

    [Header("Scaling")]
    public bool scaleWithDistance = true;
    public AnimationCurve damageFalloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.1f);

    [Header("Visual & Audio")]
    public GameObject explosionPrefab;
    public AudioClip explosionSound;

    [Header("Debug")]
    public bool showDebugGizmos = true;
    public bool logDamageDealt = true;

    public float currentTimer { get; set; } = 0f;

    public void TriggerExplosion(float delay = 0f)
    {
        if (delay <= 0f)
        {
            Explode();
        }
        else
        {
            currentTimer = delay;
        }
    }

    private void Update()
    {
        if (currentTimer > 0f)
        {
            currentTimer -= Time.deltaTime;
            if (currentTimer <= 0f)
            {
                Explode();
            }
        }
    }

    private void Explode()
    {
        Vector3 explosionPosition = transform.position;

        // Use OverlapSphere to find all colliders in range
        Collider[] hitColliders = Physics.OverlapSphere(explosionPosition, explosionRange, damageLayerMask);

        if (logDamageDealt)
        {
            Debug.Log($"Explosion found {hitColliders.Length} potential targets");
        }

        foreach (var hitCollider in hitColliders)
        {
            // Skip if it's the explosive itself
            if (hitCollider.gameObject == gameObject)
                continue;

            // Try multiple ways to find health component
            IHealth health = hitCollider.GetComponent<IHealth>();
            if (health == null)
            {
                // Try parent objects
                health = hitCollider.GetComponentInParent<IHealth>();
            }
            if (health == null)
            {
                // Try children
                health = hitCollider.GetComponentInChildren<IHealth>();
            }

            if (health != null)
            {

                float finalDamage = explosionDamage;

                // Apply damage
                health.TakeDamage(finalDamage);
            }
        }

        // Spawn explosion effect
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, explosionPosition, Quaternion.identity);
            Destroy(explosion, 5f);
        }

        // Play explosion sound
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, explosionPosition);
        }

        // Destroy the explosive object
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (showDebugGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRange);

            // Draw damage falloff visualization
            Gizmos.color = Color.yellow;
            for (int i = 1; i <= 5; i++)
            {
                float radius = (explosionRange / 5f) * i;
                float alpha = scaleWithDistance ? damageFalloffCurve.Evaluate(radius / explosionRange) : 1f;
                Gizmos.color = new Color(1f, 1f, 0f, alpha * 0.3f);
                Gizmos.DrawSphere(transform.position, radius);
            }
        }
    }
}