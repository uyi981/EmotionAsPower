using UnityEngine;

public class TestingEnemy : MonoBehaviour, IInteractable
{
    public InteractableType GetInteractableType() => InteractableType.Enemy;

    public void OnInteract()
    {
        Debug.Log("Interacted with enemy");
    }
}