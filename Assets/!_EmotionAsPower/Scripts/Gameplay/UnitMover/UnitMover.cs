using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;
    public float stopDistance = 0.1f;

    [Header("Path Visualization")]
    public bool showPath = true;
    public Color pathColor = Color.green;
    public float pathDrawHeight = 0.5f;

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

        // Get current position in grid coordinates
        Vector3Int unitGridPos = grid.WorldToCell(transform.position);
        Vector2Int startPosition = new Vector2Int(unitGridPos.x, unitGridPos.z);

        Debug.Log($"Moving from {startPosition} to {targetPosition}");

        // Find path using A* pathfinding
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
        // Move through path in reverse order (A* returns path from end to start)
        for (int i = path.Count - 1; i >= 0; i--)
        {
            currentPathIndex = path.Count - 1 - i;

            // Convert normalized grid position back to world position
            Vector2Int normalPosition = VoHauMethod.InverseNormalizeGridPosition(path[i], 100, 100);
            Vector3 targetPosition = new Vector3(normalPosition.x, transform.position.y, normalPosition.y);

            // Move towards target position
            while (Vector3.Distance(transform.position, targetPosition) > stopDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

                // Optional: Rotate towards movement direction
                if (rotationSpeed > 0)
                {
                    Vector3 direction = (targetPosition - transform.position).normalized;
                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    }
                }

                yield return null;
            }

            // Snap to exact position
            transform.position = targetPosition;

            // Notify waypoint reached
            OnReachedWaypoint?.Invoke(normalPosition);
        }

        // Movement completed
        isMoving = false;
        currentPath = null;
        currentPathIndex = 0;
        moveCoroutine = null;

        OnMovementCompleted?.Invoke();
    }

    // Overloaded method for backward compatibility
    public void Move(Vector2Int targetPosition, float speed)
    {
        moveSpeed = speed; // Update the speed
        Move(targetPosition);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showPath || currentPath == null || grid == null)
            return;

        Gizmos.color = pathColor;

        // Draw path lines and waypoints
        for (int i = currentPath.Count - 1; i > 0; i--)
        {
            Vector2Int currentGrid = VoHauMethod.InverseNormalizeGridPosition(currentPath[i], 100, 100);
            Vector2Int nextGrid = VoHauMethod.InverseNormalizeGridPosition(currentPath[i - 1], 100, 100);

            Vector3 currentWorld = new Vector3(currentGrid.x, pathDrawHeight, currentGrid.y);
            Vector3 nextWorld = new Vector3(nextGrid.x, pathDrawHeight, nextGrid.y);

            Gizmos.DrawLine(currentWorld, nextWorld);
            Gizmos.DrawSphere(currentWorld, 0.1f);
        }

        // Draw target position
        if (currentPath.Count > 0)
        {
            Vector2Int targetGrid = VoHauMethod.InverseNormalizeGridPosition(currentPath[0], 100, 100);
            Vector3 targetWorld = new Vector3(targetGrid.x, pathDrawHeight, targetGrid.y);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetWorld, 0.15f);
        }

        // Draw current target waypoint
        if (isMoving && currentPath != null && currentPathIndex < currentPath.Count)
        {
            int pathIndex = currentPath.Count - 1 - currentPathIndex;
            if (pathIndex >= 0 && pathIndex < currentPath.Count)
            {
                Vector2Int currentTarget = VoHauMethod.InverseNormalizeGridPosition(currentPath[pathIndex], 100, 100);
                Vector3 currentTargetWorld = new Vector3(currentTarget.x, pathDrawHeight, currentTarget.y);

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(currentTargetWorld, 0.12f);
            }
        }
    }
#endif
}