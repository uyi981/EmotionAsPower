using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractionManager : Singleton<InteractionManager>
{
    [SerializeField] private CameraController cameraController;
    [SerializeField] private LayerMask interactionLayers = -1; // Layers to check for interaction (Resource/Item)

    private void OnEnable()
    {
        Singleton<InputManager>.Instance.OnMouseLeftClick += Instance_OnMouseLeftClick;
        InputManager.Instance.OnMouseRightClick += Instance_OnMouseRightClick;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null)
        {
            return;
        }
        InputManager.Instance.OnMouseLeftClick -= Instance_OnMouseLeftClick;
        InputManager.Instance.OnMouseRightClick -= Instance_OnMouseRightClick;
    }

    private void Instance_OnMouseLeftClick(Vector2 clickPosition)
    {
        Debug.Log("Left Clicked");

        // Check if clicking on UI element
        if (IsPointerOverUI())
        {
            return;
        }

        if (cameraController?.mainCamera == null)
        {
            Debug.LogError("Camera isn't available for interaction.");
            return;
        }

        // Convert screen position to world position for 2D raycast
        Vector3 worldPosition = cameraController.mainCamera.ScreenToWorldPoint(new Vector3(clickPosition.x, clickPosition.y, cameraController.mainCamera.nearClipPlane));
        Vector2 raycastPosition = new Vector2(worldPosition.x, worldPosition.y);

        // Perform 2D raycast
        RaycastHit2D hit = Physics2D.Raycast(raycastPosition, Vector2.zero, Mathf.Infinity, interactionLayers);

        if (hit.collider != null)
        {
            // Get the parent object to find Resource or Item class
            Transform parentTransform = hit.collider.transform.parent;
            GameObject targetObject = parentTransform != null ? parentTransform.gameObject : hit.collider.gameObject;

            var obj = targetObject.GetComponent<IInteractable>();
            if (obj != null)
            {
                IInteractable interactingObject = obj as IInteractable;

                // Check if it's a resource and handle info panel
                if (interactingObject.GetInteractableType() == InteractableType.Resource)
                {
                    Resource resource = targetObject.GetComponent<Resource>();
                    if (resource != null && UIManager.Instance != null)
                    {
                        // Hide item info panel if showing
                        UIManager.Instance.HideItemInfoPanel();
                        UIManager.Instance.ShowResourceInfoPanel(resource, clickPosition);
                        return; // Don't call OnInteract for resources on left click
                    }
                }
                // Check if it's an item and handle info panel
                else if (interactingObject.GetInteractableType() == InteractableType.Item)
                {
                    Item item = targetObject.GetComponent<Item>();
                    if (item != null && UIManager.Instance != null)
                    {
                        // Hide resource info panel if showing
                        UIManager.Instance.HideResourceInfoPanel();
                        UIManager.Instance.ShowItemInfoPanel(item, clickPosition);
                        return; // Don't call OnInteract for items on left click
                    }
                }
                else
                {
                    // Hide both info panels when clicking other objects
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.HideResourceInfoPanel();
                        UIManager.Instance.HideItemInfoPanel();
                    }

                    interactingObject.OnInteract();
                }
            }
            else
            {
                Debug.Log("Not interactable object");
                // Hide both info panels when clicking non-interactable objects
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.HideResourceInfoPanel();
                    UIManager.Instance.HideItemInfoPanel();
                }
            }
        }
        else
        {
            Debug.Log("No 2D collider hit");
            // Hide both info panels when clicking empty space
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideResourceInfoPanel();
                UIManager.Instance.HideItemInfoPanel();
            }
        }
    }

    private void Instance_OnMouseRightClick(Vector2 clickPosition)
    {
        Debug.Log("Right Clicked");

        // Check if clicking on UI element
        if (IsPointerOverUI())
        {
            return;
        }

        if (cameraController?.mainCamera == null)
        {
            Debug.LogError("Camera isn't available for interaction.");
            return;
        }

        // Convert screen position to world position for 2D raycast
        Vector3 worldPosition = cameraController.mainCamera.ScreenToWorldPoint(new Vector3(clickPosition.x, clickPosition.y, cameraController.mainCamera.nearClipPlane));
        Vector2 raycastPosition = new Vector2(worldPosition.x, worldPosition.y);

        // Perform 2D raycast
        RaycastHit2D hit = Physics2D.Raycast(raycastPosition, Vector2.zero, Mathf.Infinity, interactionLayers);

        if (hit.collider != null)
        {
            // Get the parent object to find Resource or Item class
            Transform parentTransform = hit.collider.transform.parent;
            GameObject targetObject = parentTransform != null ? parentTransform.gameObject : hit.collider.gameObject;

            var obj = targetObject.GetComponent<IInteractable>();
            if (obj != null)
            {
                IInteractable interactingObject = obj as IInteractable;

                // Right click performs the actual interaction for both resources and items
                interactingObject.OnInteract();

                // Hide both info panels after interaction
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.HideResourceInfoPanel();
                    UIManager.Instance.HideItemInfoPanel();
                }
            }
            else
            {
                Debug.Log("Not interactable object");
            }
        }
        else
        {
            Debug.Log("No 2D collider hit");
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}