using UnityEngine;

public class Terrain : MonoBehaviour, IInteractable
{
    public InteractableType GetInteractableType() => InteractableType.Terrain;

    public void OnInteract()
    {
        Debug.Log("Interacted with Terrain");
    }

}
