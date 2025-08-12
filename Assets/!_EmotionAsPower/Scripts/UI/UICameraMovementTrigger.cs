using UnityEngine;
using UnityEngine.EventSystems;

public class UICameraMovementTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Movement Direction")]
    [SerializeField] private CameraDirection direction = CameraDirection.Forward;

    [Header("Optional - Manual Direction Override")]
    [SerializeField] private bool useCustomDirection = false;
    [SerializeField] private Vector3 customDirection = Vector3.forward;

    public enum CameraDirection
    {
        Forward = 0,
        ForwardRight = 1,
        Right = 2,
        BackwardRight = 3,
        Backward = 4,
        BackwardLeft = 5,
        Left = 6,
        ForwardLeft = 7
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InputManager.Instance != null)
        {
            if (useCustomDirection)
            {
                // For custom directions, we'll use the existing system but override
                InputManager.Instance.StartUICameraMovement((int)direction);
            }
            else
            {
                InputManager.Instance.StartUICameraMovement((int)direction);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.StopUICameraMovement();
        }
    }

    // Alternative methods for Unity Events if you prefer that approach
    public void StartCameraMovement()
    {
        OnPointerEnter(null);
    }

    public void StopCameraMovement()
    {
        OnPointerExit(null);
    }

    private void OnValidate()
    {
        // Helper to visualize direction in inspector
        if (useCustomDirection && customDirection != Vector3.zero)
        {
            customDirection = customDirection.normalized;
        }
    }
}