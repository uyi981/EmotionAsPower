using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    [SerializeField] private float baseSpeed = 2000.0f;
    [SerializeField] private AnimationCurve heightSpeedCurve = AnimationCurve.Linear(0, 1, 100, 2);
    [SerializeField] private float maxHeight = 10000f;
    [SerializeField] private float minHeight = 500f;

    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 1000f;

    [Header("Pause Behavior")]
    [SerializeField] private bool allowMovementWhenPaused = true;

    public Vector3 offset;
    public Camera mainCamera { get; private set; }

    private void Awake()
    {
        mainCamera = GetComponentInChildren<Camera>();
    }

    private void OnEnable()
    {
        if (Singleton<InputManager>.Instance != null)
        {
            Singleton<InputManager>.Instance.OnCameraMovement += Instance_OnCameraMovement;
            Singleton<InputManager>.Instance.OnCameraScroll += Instance_OnCameraScroll;
        }
    }

    public void FocusMode(Vector3 position)
    {
        mainCamera.transform.position = position + offset;
    }

    private void OnDisable()
    {
        if (Singleton<InputManager>.Instance == null)
        {
            return;
        }
        Singleton<InputManager>.Instance.OnCameraMovement -= Instance_OnCameraMovement;
        Singleton<InputManager>.Instance.OnCameraScroll -= Instance_OnCameraScroll;
    }

    private void Instance_OnCameraMovement(Vector3 moveDir)
    {
        float height = transform.position.y;
        float speedMultiplier = heightSpeedCurve.Evaluate(height);
        float moveSpeed = baseSpeed * speedMultiplier;
        float deltaTime = GetDeltaTime();

        transform.Translate(moveDir * moveSpeed * deltaTime, Space.World);
        ClampCameraHeight();
    }

    private void Instance_OnCameraScroll(float scrollVelocity)
    {
        float height = transform.position.y;
        float speedMultiplier = heightSpeedCurve.Evaluate(height);
        float actualScrollSpeed = scrollSpeed * speedMultiplier;
        float deltaTime = GetDeltaTime();

        // Move along the camera's forward direction instead of world up
        // This maintains the camera's focus point while zooming
        Vector3 scrollMovement = mainCamera.transform.forward * scrollVelocity * actualScrollSpeed * deltaTime;
        transform.Translate(scrollMovement, Space.World);

        ClampCameraHeight();
    }

    private float GetDeltaTime()
    {
        if (allowMovementWhenPaused || !GameManager.Instance.IsPaused)
        {
            return Time.unscaledDeltaTime;
        }
        return Time.deltaTime;
    }

    private void ClampCameraHeight()
    {
        Vector3 pos = transform.position;
        if (pos.y > maxHeight)
        {
            transform.position = new Vector3(pos.x, maxHeight, pos.z);
        }
        else if (pos.y < minHeight)
        {
            transform.position = new Vector3(pos.x, minHeight, pos.z);
        }
    }
}