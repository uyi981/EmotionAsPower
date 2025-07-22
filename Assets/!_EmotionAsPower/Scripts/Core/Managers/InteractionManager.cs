using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractionManager : Singleton<InteractionManager>
{
    [SerializeField] private CameraController cameraController;

    private void OnEnable()
    {
       Singleton<InputManager>.Instance.OnMouseLeftClick += Instance_OnMouseLeftClick;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null)
        {
            return;
        }
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