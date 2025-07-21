using UnityEngine;

public interface IInteractable
{
    //void OnInteract(InteractionType interactionType);

    void OnInteract();

    InteractableType GetInteractableType();
}
