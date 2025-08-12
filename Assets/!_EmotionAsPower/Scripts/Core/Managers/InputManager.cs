using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    [SerializeField]
    private float cameraScrollMultiplier = 10f;

    [SerializeField]
    private float scrollSensitivity = 5f;

    [SerializeField]
    private float scrollDamping = 10f; // How quickly scroll momentum dies down

    // Camera Input Actions
    private InputAction moveForward, moveBackward, moveLeft, moveRight, moveUp, moveDown;

    // Mouse Input Actions
    public InputAction mouseLeftClick, mouseRightClick, mousePos, mouseScroll;

    // New Input Actions for game controls
    private InputAction setSpeed1x, setSpeed2x, setSpeed4x;

    // Timing variables for pause-resistant input
    private float lastInputUpdateTime;
    private const float INPUT_UPDATE_INTERVAL = 0.016f; // ~60fps

    // Smooth scrolling variables
    private float currentScrollVelocity = 0f;
    private float lastScrollTime;

    public event Action<Vector3> OnCameraMovement;
    public event Action<Vector2> OnMouseLeftClick;
    public event Action<Vector2> OnMouseRightClick;
    public event Action<float> OnCameraScroll; // New event specifically for smooth scrolling

    // UI hover camera movement
    private bool isUIHoverActive = false;
    private Vector3 currentUIDirection = Vector3.zero;

    protected override void Awake()
    {
        base.Awake();
        InitializeInputActions();
        lastInputUpdateTime = Time.realtimeSinceStartup;
        lastScrollTime = Time.realtimeSinceStartup;
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
            HandleScrollInput();
            lastInputUpdateTime = currentTime;
        }
    }

    private void InitializeInputActions()
    {
        // Camera movement actions
        moveForward = new InputAction("MoveForward", InputActionType.Value, "<Keyboard>/w");
        moveBackward = new InputAction("MoveBackward", InputActionType.Value, "<Keyboard>/s");
        moveLeft = new InputAction("MoveLeft", InputActionType.Value, "<Keyboard>/a");
        moveRight = new InputAction("MoveRight", InputActionType.Value, "<Keyboard>/d");
        moveUp = new InputAction("MoveUp", InputActionType.Value, "<Keyboard>/q");
        moveDown = new InputAction("MoveDown", InputActionType.Value, "<Keyboard>/e");

        // Mouse actions
        mouseLeftClick = new InputAction("MouseLeftClick", InputActionType.Button, "<Mouse>/leftButton");
        mouseRightClick = new InputAction("MouseRightClick", InputActionType.Button, "<Mouse>/rightButton");
        mousePos = new InputAction("MousePosition", InputActionType.Value, "<Mouse>/position");
        mouseScroll = new InputAction("MouseScroll", InputActionType.Value, "<Mouse>/scroll");

        // New game control actions
        setSpeed1x = new InputAction("SetSpeed1x", InputActionType.Button, "<Keyboard>/1");
        setSpeed2x = new InputAction("SetSpeed2x", InputActionType.Button, "<Keyboard>/2");
        setSpeed4x = new InputAction("SetSpeed4x", InputActionType.Button, "<Keyboard>/3");

        // Subscribe to events
        mouseLeftClick.performed += MouseLeftClick_performed;
        mouseRightClick.performed += MouseRightClick_performed;
        mouseScroll.performed += MouseScroll_performed; // Add scroll event handler
        setSpeed1x.performed += SetSpeed1x_performed;
        setSpeed2x.performed += SetSpeed2x_performed;
        setSpeed4x.performed += SetSpeed4x_performed;
    }

    private void CleanupInputActions()
    {
        // Unsubscribe from events and dispose
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

        if (mouseScroll != null)
        {
            mouseScroll.performed -= MouseScroll_performed;
            mouseScroll.Dispose();
        }

        if (setSpeed1x != null)
        {
            setSpeed1x.performed -= SetSpeed1x_performed;
            setSpeed1x.Dispose();
        }

        if (setSpeed2x != null)
        {
            setSpeed2x.performed -= SetSpeed2x_performed;
            setSpeed2x.Dispose();
        }

        if (setSpeed4x != null)
        {
            setSpeed4x.performed -= SetSpeed4x_performed;
            setSpeed4x.Dispose();
        }

        // Dispose camera movement actions
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

    private void MouseScroll_performed(InputAction.CallbackContext obj)
    {
        Vector2 screenPos = mousePos.ReadValue<Vector2>();
        if (IsPointerOverUI(screenPos))
        {
            return;
        }

        Vector2 scrollValue = obj.ReadValue<Vector2>();
        // Add scroll input to velocity for smooth scrolling
        currentScrollVelocity += scrollValue.y * scrollSensitivity;
        lastScrollTime = Time.realtimeSinceStartup;
    }

    private void PauseToggle_performed(InputAction.CallbackContext obj)
    {
        // Only allow pause toggle if GameManager is set up
        if (GameManager.Instance != null && GameManager.Instance.FinishedSetup)
        {
            GameManager.Instance.TogglePause();
        }
    }

    private void SetSpeed1x_performed(InputAction.CallbackContext obj)
    {
        if (GameManager.Instance != null && GameManager.Instance.FinishedSetup)
        {
            GameManager.Instance.SetGameSpeed(1.0f);
        }
    }

    private void SetSpeed2x_performed(InputAction.CallbackContext obj)
    {
        if (GameManager.Instance != null && GameManager.Instance.FinishedSetup)
        {
            GameManager.Instance.SetGameSpeed(2.0f);
        }
    }

    private void SetSpeed4x_performed(InputAction.CallbackContext obj)
    {
        if (GameManager.Instance != null && GameManager.Instance.FinishedSetup)
        {
            GameManager.Instance.SetGameSpeed(4.0f);
        }
    }

    private void EnableInputActions()
    {
        // Enable camera movement actions
        moveForward.Enable();
        moveBackward.Enable();
        moveLeft.Enable();
        moveRight.Enable();
        moveUp.Enable();
        moveDown.Enable();

        // Enable mouse actions
        mouseLeftClick.Enable();
        mouseRightClick.Enable();
        mousePos.Enable();
        mouseScroll.Enable();

        // Enable game control actions
        setSpeed1x.Enable();
        setSpeed2x.Enable();
        setSpeed4x.Enable();
    }

    private void DisableInputActions()
    {
        // Disable camera movement actions
        moveForward.Disable();
        moveBackward.Disable();
        moveLeft.Disable();
        moveRight.Disable();
        moveUp.Disable();
        moveDown.Disable();

        // Disable mouse actions
        mouseLeftClick.Disable();
        mouseRightClick.Disable();
        mousePos.Disable();
        mouseScroll.Disable();

        // Disable game control actions
        setSpeed1x.Disable();
        setSpeed2x.Disable();
        setSpeed4x.Disable();
    }

    private void HandleCameraInput()
    {
        var dir = Vector3.zero;

        // Handle UI hover input first (takes priority)
        if (isUIHoverActive)
        {
            dir = currentUIDirection;
        }
        else
        {
            // Only handle keyboard movement when UI hover is not active
            if (moveForward.ReadValue<float>() > 0) dir += Vector3.forward;
            if (moveBackward.ReadValue<float>() > 0) dir += Vector3.back;
            if (moveRight.ReadValue<float>() > 0) dir += Vector3.right;
            if (moveLeft.ReadValue<float>() > 0) dir += Vector3.left;
            if (moveUp.ReadValue<float>() > 0) dir += Vector3.up;
            if (moveDown.ReadValue<float>() > 0) dir += Vector3.down;
        }

        if (dir != Vector3.zero)
        {
            OnCameraMovement?.Invoke(dir.normalized);
        }
    }

    private void HandleScrollInput()
    {
        float currentTime = Time.realtimeSinceStartup;
        float deltaTime = currentTime - lastInputUpdateTime;

        // Apply damping to scroll velocity
        if (Mathf.Abs(currentScrollVelocity) > 0.01f)
        {
            // Trigger the scroll event with current velocity
            OnCameraScroll?.Invoke(currentScrollVelocity);

            // Apply damping
            currentScrollVelocity = Mathf.Lerp(currentScrollVelocity, 0f, scrollDamping * deltaTime);
        }
        else
        {
            currentScrollVelocity = 0f;
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

    #region UI Hover Camera Movement Methods

    /// <summary>
    /// Call this method from UI Event Triggers on Pointer Enter
    /// </summary>
    public void StartUICameraMovement(int direction)
    {
        isUIHoverActive = true;
        currentUIDirection = GetDirectionVector(direction);
    }

    /// <summary>
    /// Call this method from UI Event Triggers on Pointer Exit
    /// </summary>
    public void StopUICameraMovement()
    {
        isUIHoverActive = false;
        currentUIDirection = Vector3.zero;
    }

    /// <summary>
    /// Get direction vector based on 8-directional movement
    /// 0=Forward, 1=ForwardRight, 2=Right, 3=BackwardRight, 4=Backward, 5=BackwardLeft, 6=Left, 7=ForwardLeft
    /// </summary>
    private Vector3 GetDirectionVector(int direction)
    {
        switch (direction)
        {
            case 0: // Forward
                return Vector3.forward;
            case 1: // Forward-Right
                return (Vector3.forward + Vector3.right).normalized;
            case 2: // Right
                return Vector3.right;
            case 3: // Backward-Right
                return (Vector3.back + Vector3.right).normalized;
            case 4: // Backward
                return Vector3.back;
            case 5: // Backward-Left
                return (Vector3.back + Vector3.left).normalized;
            case 6: // Left
                return Vector3.left;
            case 7: // Forward-Left
                return (Vector3.forward + Vector3.left).normalized;
            default:
                return Vector3.zero;
        }
    }

    #endregion
}