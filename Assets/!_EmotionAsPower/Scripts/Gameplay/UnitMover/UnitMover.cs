using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;
    public float stopDistance = 0.1f;

    private APathFinding pathFinding;
    private Grid grid;
    private float[,] gridMap;
    private List<Vector2Int> currentPath;
    private int currentPathIndex;
    private bool isMoving;
    private Coroutine moveCoroutine;

    public System.Action OnMovementStarted;
    public System.Action OnMovementCompleted;
    public System.Action OnMovementFailed;
    public System.Action<Vector2Int> OnReachedWaypoint;

    private void Start()
    {
        pathFinding = new APathFinding();

        if (GridSystem.Instance != null)
        {
            grid = GridSystem.Instance.grid;
            gridMap = GridSystem.Instance.gridMap;
        }
        else
        {
            Debug.LogError($"GridSystem instance not found! {gameObject.name} cannot move.");
        }
    }

    public void MoveToWorldPosition(Vector3 worldPosition)
    {
        if (grid == null || gridMap == null)
        {
            Debug.LogError($"Grid system not initialized for {gameObject.name}");
            OnMovementFailed?.Invoke();
            return;
        }

        // Convert world positions to grid positions
        Vector3Int startGridPos = grid.WorldToCell(transform.position);
        Vector3Int targetGridPos = grid.WorldToCell(worldPosition);

        Vector2Int start = new Vector2Int(startGridPos.x, startGridPos.z);
        Vector2Int target = new Vector2Int(targetGridPos.x, targetGridPos.z);

        Move(target);
    }

    public void StopMovement()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        isMoving = false;
        currentPath = null;
        currentPathIndex = 0;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public List<Vector2Int> GetCurrentPath()
    {
        return currentPath;
    }

    public void Move(Vector2Int targetPosition)
    {
        if (grid == null || gridMap == null)
        {
            Debug.LogError($"Grid system not initialized for {gameObject.name}");
            OnMovementFailed?.Invoke();
            return;
        }

        // Stop any existing movement
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        Vector3Int unitGridPos = grid.WorldToCell(transform.position);
        Vector2Int startPosition = new Vector2Int(unitGridPos.x, unitGridPos.z);

        Debug.Log($"Moving from {startPosition} to {targetPosition}");

        List<Vector2Int> path = pathFinding.GetPathResult(
            VoHauMethod.NormalizeGridPosition(startPosition, 100, 100),
            VoHauMethod.NormalizeGridPosition(targetPosition, 100, 100),
            gridMap,
            1
        );

        if (path != null && path.Count > 0)
        {
            currentPath = path;
            currentPathIndex = 0;
            isMoving = true;

            OnMovementStarted?.Invoke();
            moveCoroutine = StartCoroutine(Moving(path, moveSpeed));
        }
        else
        {
            Debug.LogWarning($"No valid path found for {gameObject.name} to move from {startPosition} to {targetPosition}");
            OnMovementFailed?.Invoke();
        }
    }

    private IEnumerator Moving(List<Vector2Int> path, float speed)
    {
        for (int i = path.Count - 1; i >= 0; i--)
        {
            currentPathIndex = path.Count - 1 - i;

            // Convert normalized grid position back to world position
            Vector2Int normalPosition = VoHauMethod.InverseNormalizeGridPosition(path[i], 100, 100);
            Vector3 targetPosition = new Vector3(normalPosition.x, transform.position.y, normalPosition.y);

            while (Vector3.Distance(transform.position, targetPosition) > stopDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

                yield return null;
            }

            transform.position = targetPosition;

            OnReachedWaypoint?.Invoke(normalPosition);
        }

        isMoving = false;
        currentPath = null;
        currentPathIndex = 0;
        moveCoroutine = null;

        OnMovementCompleted?.Invoke();
    }

    public void Move(Vector2Int targetPosition, float speed)
    {
        moveSpeed = speed;
        Move(targetPosition);
    }

}