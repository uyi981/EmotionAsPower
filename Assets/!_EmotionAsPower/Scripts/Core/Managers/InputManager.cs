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
    private InputAction mouseLeftClick, mousePos;
    // Input Events
    public event Action<Vector3> OnCameraMovement;
    public event Action<Vector2> OnMouseLeftClick;
    protected override void Awake()
    {
        base.Awake();
        InitializeInputActions();
    }

    protected override void OnDestroy()
    {
        //mouseLeftClick.performed -= OnMouseClickPerformed;
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
        HandleCameraInput();
    }
    private void InitializeInputActions()
    {
        moveForward = new InputAction("MoveForward", InputActionType.Value, "<keyboard>/w");
        moveBackward = new InputAction("MoveBackward", InputActionType.Value, "<keyboard>/s");
        moveLeft = new InputAction("MoveLeft", InputActionType.Value, "<keyboard>/a");
        moveRight = new InputAction("MoveRight", InputActionType.Value, "<keyboard>/d");
        moveUp = new InputAction("MoveUp", InputActionType.Value, "<keyboard>/q");
        moveDown = new InputAction("MoveDown", InputActionType.Value, "<keyboard>/e");
        mouseLeftClick = new InputAction("MouseLeftClick", InputActionType.Button, "<mouse>/leftButton");
        mousePos = new InputAction("MousePosition", InputActionType.Value, "<mouse>/position");
        mouseLeftClick.performed += OnMouseClickPerformed;
    }
    private void EnableInputActions()
    {
        moveForward.Enable(); moveBackward.Enable();
        moveLeft.Enable(); moveRight.Enable();
        moveUp.Enable(); moveDown.Enable();
        mouseLeftClick.Enable();
        mousePos.Enable();
    }
    private void DisableInputActions()
    {
        moveForward.Disable(); moveBackward.Disable();
        moveLeft.Disable(); moveRight.Disable();
        moveUp.Disable(); moveDown.Disable();
        mouseLeftClick.Disable();
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
            OnCameraMovement?.Invoke(dir.normalized);
    }
    private void OnMouseClickPerformed(InputAction.CallbackContext ctx)
    {
        Vector2 screenPos = mousePos.ReadValue<Vector2>();
        StartCoroutine(PerformClickNextFrame(screenPos));
    }
    private IEnumerator PerformClickNextFrame(Vector2 screenPosition)
    {
        yield return null;
        if (IsPointerOverUI(screenPosition))
            yield break;
        OnMouseLeftClick?.Invoke(screenPosition);
    }
    private bool IsPointerOverUI(Vector2 screenPos)
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}