using UnityEngine;

public class Land : MonoBehaviour, IInteractable
{
    public InteractableType GetInteractableType() => InteractableType.Land;

    public void OnInteract()
    {
        Debug.Log("Interacted with Terrain");
    }

}
