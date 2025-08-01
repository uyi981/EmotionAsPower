using UnityEngine;

public class ColliderAttack : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Resource"))
        {
           Health health = other.GetComponent<Health>();
           health?.TakeDamage(10f); // Assuming the resource has a Health component
        }
    }
}
