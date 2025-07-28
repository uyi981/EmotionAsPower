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

        MoveToGridPosition(target);
    }

    public void MoveToGridPosition(Vector2Int targetGridPosition)
    {
        if (pathFinding == null || grid == null || gridMap == null)
        {
            Debug.LogError($"Required components not initialized for {gameObject.name}");
            OnMovementFailed?.Invoke();
            return;
        }

        // Stop current movement
        StopMovement();

        // Get current grid position
        Vector3Int currentGridPos = grid.WorldToCell(transform.position);
        Debug.Log(currentGridPos);
        Vector2Int startGridPosition = new Vector2Int(currentGridPos.x, currentGridPos.z);

        // Check if this is already at the target
        if (startGridPosition == targetGridPosition)
        {
            Debug.Log($"{gameObject.name} is already at target position");
            return;
        }

        int gridSize = Mathf.Max(gridMap.GetLength(0), gridMap.GetLength(1));
        currentPath = pathFinding.GetPathResult(startGridPosition, targetGridPosition, gridMap, 1);

        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogWarning($"No path found for {gameObject.name} from {startGridPosition} to {targetGridPosition}");
            OnMovementFailed?.Invoke();
            return;
        }

        currentPath.Reverse();

        currentPathIndex = 1;
        isMoving = true;
        moveCoroutine = StartCoroutine(FollowPath());
        OnMovementStarted?.Invoke();

        Debug.Log($"{gameObject.name} starting movement. Path length: {currentPath.Count}");
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

    private IEnumerator FollowPath()
    {
        while (currentPathIndex < currentPath.Count)
        {
            Vector2Int nextGridPos = currentPath[currentPathIndex];

            // Convert grid position to world position
            Vector3Int gridPos3D = new Vector3Int(nextGridPos.x, 0, nextGridPos.y);
            Vector3 target = grid.CellToWorld(gridPos3D);
            target.y = transform.position.y; 

            // Move towards the target
            while (Vector3.Distance(transform.position, target) > stopDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

                yield return null;
            }


            transform.position = target;


            OnReachedWaypoint?.Invoke(nextGridPos);

            currentPathIndex++;
        }

        isMoving = false;
        currentPath = null;
        currentPathIndex = 0;
        OnMovementCompleted?.Invoke();

        Debug.Log($"{gameObject.name} reached destination");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showPath || currentPath == null || grid == null)
            return;

        Gizmos.color = pathColor;
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Vector2Int currentGrid = currentPath[i];
            Vector2Int nextGrid = currentPath[i + 1];

            Vector3Int current3D = new Vector3Int(currentGrid.x, 0, currentGrid.y);
            Vector3Int next3D = new Vector3Int(nextGrid.x, 0, nextGrid.y);

            Vector3 currentWorld = grid.CellToWorld(current3D);
            Vector3 nextWorld = grid.CellToWorld(next3D);

            currentWorld.y += pathDrawHeight;
            nextWorld.y += pathDrawHeight;

            Gizmos.DrawLine(currentWorld, nextWorld);
            Gizmos.DrawSphere(currentWorld, 0.1f);
        }

        if (currentPath.Count > 0)
        {
            Vector2Int targetGrid = currentPath[currentPath.Count - 1];
            Vector3Int target3D = new Vector3Int(targetGrid.x, 0, targetGrid.y);
            Vector3 targetWorld = grid.CellToWorld(target3D);
            targetWorld.y += pathDrawHeight;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetWorld, 0.15f);
        }
    }
#endif
}