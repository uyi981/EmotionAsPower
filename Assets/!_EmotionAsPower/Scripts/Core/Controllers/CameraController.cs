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

    public Camera mainCamera { get; private set; }

    private void Awake()
    {
        mainCamera = GetComponentInChildren<Camera>();
    }

    private void OnEnable()
    {
        Singleton<InputManager>.Instance.OnCameraMovement += Instance_OnCameraMovement;
    }

    private void OnDisable()
    {
        if (Singleton<InputManager>.Instance == null)
        {
            return;
        }
        Singleton<InputManager>.Instance.OnCameraMovement -= Instance_OnCameraMovement;
    }

    private void Instance_OnCameraMovement(Vector3 moveDir)
    {
        float height = transform.position.y;
        float speedMultiplier = heightSpeedCurve.Evaluate(height);
        float moveSpeed = baseSpeed * speedMultiplier;

        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

        if (transform.position.y > maxHeight)
        {
            transform.position = new Vector3(transform.position.x, maxHeight, transform.position.z);
        }
        else if (transform.position.y < minHeight)
        {
            transform.position = new Vector3(transform.position.x, minHeight, transform.position.z);
        }
    }
}
