using UnityEngine;

public class ColliderAttack : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Resource"))
        {
           Health health = other.GetComponent<Health>();
           health?.TakeDamage(1f); // Assuming the resource has a Health component
           Debug.Log("Attacked resource: -10" + other.name);
        }
    }
}
