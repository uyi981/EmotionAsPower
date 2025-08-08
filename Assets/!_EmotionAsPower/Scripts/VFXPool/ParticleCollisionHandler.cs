using UnityEngine;

public class ParticleCollisionHandler : MonoBehaviour
{
    public float damage;
    void OnParticleCollision(GameObject other)
    {
       
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // Assuming the enemy has a method to take damage
            var enemy = other.GetComponent<Health>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage); // Replace with actual damage value
            }
        }
    }
}
