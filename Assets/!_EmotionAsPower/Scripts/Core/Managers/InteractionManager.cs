using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractionManager : Singleton<InteractionManager>
{
    [SerializeField] private CameraController cameraController;

    private void OnEnable()
    {
        InputManager.Instance.OnMouseLeftClick += Instance_OnMouseLeftClick;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnMouseLeftClick -= Instance_OnMouseLeftClick;
    }

    private void Instance_OnMouseLeftClick(Vector2 clickPosition)
    {
        StartCoroutine(HandleClickNextFrame(clickPosition));
    }

    private IEnumerator HandleClickNextFrame(Vector2 clickPosition)
    {
        yield return null;

        if (cameraController?.mainCamera == null)
        {
            Debug.LogError("Camera isn't available for interaction.");
            yield break;
        }

        Ray ray = cameraController.mainCamera.ScreenPointToRay(clickPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var obj = hit.collider.GetComponent<IInteractable>();
            if (obj != null)
            {
                IInteractable interactingObject = obj as IInteractable;
                interactingObject.OnInteract();
            }
            else
            {
                Debug.Log("Not interactable object");
            }
        }
    }
}