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
        if(other.CompareTag("Enermy"))
        {
           Health health = other.GetComponent<Health>();
           health?.TakeDamage(1f); // Assuming the enemy has a Health component
           Debug.Log("Attacked enemy: -10" + other.name);
        }
        //if(other.CompareTag("Building"))
        //{
        //   BuildingBase building = other.GetComponent<BuildingBase>();
        //   building?.TakeDamage(1f); // Assuming the building has a BuildingBase component
        //   Debug.Log("Attacked building: -10" + other.name);
        //}
    }
    private void Start()
    {
        Collider collider = GetComponent<Collider>();
        collider.enabled = false; // Ensure the collider is enabled
    }
}
