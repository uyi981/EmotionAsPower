using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    // Camera Input Actions
    private InputAction moveForward, moveBackward, moveLeft, moveRight, moveUp, moveDown;

    // Mouse Input Actions
    private InputAction mouseLeftClick, mouseRightClick, mousePos;

    // Timing variables for pause-resistant input
    private float lastInputUpdateTime;
    private const float INPUT_UPDATE_INTERVAL = 0.016f; // ~60fps

    public event Action<Vector3> OnCameraMovement;
    public event Action<Vector2> OnMouseLeftClick;
    public event Action<Vector2> OnMouseRightClick;

    protected override void Awake()
    {
        base.Awake();
        InitializeInputActions();
        lastInputUpdateTime = Time.realtimeSinceStartup;
    }

    protected override void OnDestroy()
    {
        CleanupInputActions();
        base.OnDestroy();
    }

    private void OnEnable()
    {
        EnableInputActions();
    }

    private void OnDisable()
    {
        DisableInputActions();
    }

    private void Update()
    {
        float currentTime = Time.realtimeSinceStartup;

        if (currentTime - lastInputUpdateTime >= INPUT_UPDATE_INTERVAL)
        {
            HandleCameraInput();
            lastInputUpdateTime = currentTime;
        }
    }

    private void InitializeInputActions()
    {
        moveForward = new InputAction("MoveForward", InputActionType.Value, "<Keyboard>/w");
        moveBackward = new InputAction("MoveBackward", InputActionType.Value, "<Keyboard>/s");
        moveLeft = new InputAction("MoveLeft", InputActionType.Value, "<Keyboard>/a");
        moveRight = new InputAction("MoveRight", InputActionType.Value, "<Keyboard>/d");
        moveUp = new InputAction("MoveUp", InputActionType.Value, "<Keyboard>/q");
        moveDown = new InputAction("MoveDown", InputActionType.Value, "<Keyboard>/e");

        mouseLeftClick = new InputAction("MouseLeftClick", InputActionType.Button, "<Mouse>/leftButton");
        mouseRightClick = new InputAction("MouseRightClick", InputActionType.Button, "<Mouse>/rightButton");
        mousePos = new InputAction("MousePosition", InputActionType.Value, "<Mouse>/position");

        mouseLeftClick.performed += MouseLeftClick_performed;
        mouseRightClick.performed += MouseRightClick_performed;
    }

    private void CleanupInputActions()
    {
        if (mouseLeftClick != null)
        {
            mouseLeftClick.performed -= MouseLeftClick_performed;
            mouseLeftClick.Dispose();
        }

        if (mouseRightClick != null)
        {
            mouseRightClick.performed -= MouseRightClick_performed;
            mouseRightClick.Dispose();
        }

        // Dispose other actions
        moveForward?.Dispose();
        moveBackward?.Dispose();
        moveLeft?.Dispose();
        moveRight?.Dispose();
        moveUp?.Dispose();
        moveDown?.Dispose();
        mousePos?.Dispose();
    }

    private void MouseRightClick_performed(InputAction.CallbackContext obj)
    {
        Vector2 screenPos = mousePos.ReadValue<Vector2>();
        if (IsPointerOverUI(screenPos))
        {
            return;
        }
        OnMouseRightClick?.Invoke(screenPos);
    }

    private void EnableInputActions()
    {
        moveForward.Enable();
        moveBackward.Enable();
        moveLeft.Enable();
        moveRight.Enable();
        moveUp.Enable();
        moveDown.Enable();
        mouseLeftClick.Enable();
        mouseRightClick.Enable();
        mousePos.Enable();
    }

    private void DisableInputActions()
    {
        moveForward.Disable();
        moveBackward.Disable();
        moveLeft.Disable();
        moveRight.Disable();
        moveUp.Disable();
        moveDown.Disable();
        mouseLeftClick.Disable();
        mouseRightClick.Disable();
        mousePos.Disable();
    }

    private void HandleCameraInput()
    {
        var dir = Vector3.zero;

        if (moveForward.ReadValue<float>() > 0) dir += Vector3.forward;
        if (moveBackward.ReadValue<float>() > 0) dir += Vector3.back;
        if (moveRight.ReadValue<float>() > 0) dir += Vector3.right;
        if (moveLeft.ReadValue<float>() > 0) dir += Vector3.left;
        if (moveUp.ReadValue<float>() > 0) dir += Vector3.up;
        if (moveDown.ReadValue<float>() > 0) dir += Vector3.down;

        if (dir != Vector3.zero)
        {
            OnCameraMovement?.Invoke(dir.normalized);
        }
    }

    private void MouseLeftClick_performed(InputAction.CallbackContext ctx)
    {
        Vector2 screenPos = mousePos.ReadValue<Vector2>();
        StartCoroutine(PerformClickNextFrame(screenPos));
    }

    private IEnumerator PerformClickNextFrame(Vector2 screenPosition)
    {
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startTime < 0.016f) 
        {
            yield return null;
        }

        if (IsPointerOverUI(screenPosition))
            yield break;

        OnMouseLeftClick?.Invoke(screenPosition);
    }

    private bool IsPointerOverUI(Vector2 screenPos)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    public bool HasRealTimeElapsed(float seconds, ref float lastTime)
    {
        float currentTime = Time.realtimeSinceStartup;
        if (currentTime - lastTime >= seconds)
        {
            lastTime = currentTime;
            return true;
        }
        return false;
    }
}