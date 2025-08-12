using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Properties")]
    public float damage = 25f;
    public float speed = 10f;
    public float lifetime = 5f;
    public LayerMask damageLayerMask = -1;

    [Header("Visual Effects (Optional)")]
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;

    public Vector3 direction;
    public float currentLifetime { get; set; } = 0f;
    private bool hasHit = false;

    public void Initialize(Vector3 shootDirection, float bulletDamage, float bulletSpeed, LayerMask layerMask)
    {
        direction = shootDirection.normalized;
        damage = bulletDamage;
        speed = bulletSpeed;
        damageLayerMask = layerMask;
        currentLifetime = 0f;
        hasHit = false;

        // Rotate bullet to face direction
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void Update()
    {
        if (hasHit) return;

        // Move bullet forward
        transform.position += direction * speed * Time.deltaTime;

        // Update lifetime
        currentLifetime += Time.deltaTime;
        if (currentLifetime >= lifetime)
        {
            DestroyBullet();
            return;
        }

        // Check for collision using raycast
        RaycastHit hit;
        float rayDistance = speed * Time.deltaTime;

        if (Physics.Raycast(transform.position, direction, out hit, rayDistance, damageLayerMask))
        {
            OnHit(hit.collider, hit.point);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // Check if the collider is on the damage layer mask
        if (((1 << other.gameObject.layer) & damageLayerMask) != 0)
        {
            OnHit(other, transform.position);
        }
    }

    private void OnHit(Collider hitCollider, Vector3 hitPoint)
    {
        if (hasHit) return;
        hasHit = true;

        // Apply damage
        IHealth health = hitCollider.GetComponent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Debug.Log($"Bullet hit {hitCollider.name} for {damage} damage");
        }

        // Spawn hit effect
        if (hitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(-direction));
            Destroy(hitEffect, 3f);
        }

        // Play hit sound
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, hitPoint);
        }

        DestroyBullet();
    }

    private void DestroyBullet()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // Draw bullet trajectory in editor
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, direction * 2f);
    }
}