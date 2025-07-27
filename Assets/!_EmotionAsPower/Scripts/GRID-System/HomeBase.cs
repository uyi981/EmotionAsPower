using UnityEngine;

public class HomeBase : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Item"))
        {
          Destroy(other.gameObject); // Destroy the item when it enters the home base
        }
    }
}
