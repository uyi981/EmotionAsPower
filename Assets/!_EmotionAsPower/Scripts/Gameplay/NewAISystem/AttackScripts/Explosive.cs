using UnityEngine;

public class Explosive : MonoBehaviour
{
    public float explosionRange = 3f;
    public float explosionDamage = 50f;
    public LayerMask damageLayerMask = -1;
    public GameObject explosionPrefab; // Visual effect prefab
    public AudioClip explosionSound;

    public void TriggerExplosion(float delay = 0f)
    {
        if (delay <= 0f)
        {
            Explode();
        }
        else
        {
            Invoke(nameof(Explode), delay);
        }
    }

    private void Explode()
    {
        Vector3 explosionPosition = transform.position;

        Collider[] hitColliders = Physics.OverlapSphere(explosionPosition, explosionRange, damageLayerMask);

        foreach (var hitCollider in hitColliders)
        {
            IHealth health = hitCollider.GetComponent<IHealth>();
            if (health != null)
            {
                float distance = Vector3.Distance(explosionPosition, hitCollider.transform.position);
                float damageFalloff = Mathf.Clamp01(1f - (distance / explosionRange));
                float finalDamage = explosionDamage * damageFalloff;

                health.TakeDamage(finalDamage);
                Debug.Log($"Explosion damaged {hitCollider.name} for {finalDamage} damage");
            }
        }

        // Spawn explosion effect
        if (explosionPrefab != null)
        {
            GameObject explosion = Object.Instantiate(explosionPrefab, explosionPosition, Quaternion.identity);
            Object.Destroy(explosion, 5f);
        }

        // Play explosion sound
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, explosionPosition);
        }

        Destroy(gameObject);
    }
}