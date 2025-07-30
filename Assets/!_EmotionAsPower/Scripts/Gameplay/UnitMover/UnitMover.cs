using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;
    public float stopDistance = 0.1f;

    [Header("Path Smoothing")]
    public bool enablePathSmoothing = true;
    public float smoothingRadius = 0.5f;

    private APathFinding pathFinding;
    private Grid grid;
    private float[,] gridMap;
    private List<Vector2Int> currentPath;
    private List<Vector3> smoothedPath;
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
        smoothedPath = null;
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

    public List<Vector3> GetSmoothedPath()
    {
        return smoothedPath;
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

            // Convert path to world positions
            List<Vector3> worldPath = ConvertPathToWorldPositions(path);

            // Apply path smoothing if enabled
            if (enablePathSmoothing)
            {
                smoothedPath = SmoothPath(worldPath);
            }
            else
            {
                smoothedPath = worldPath;
            }

            currentPathIndex = 0;
            isMoving = true;

            OnMovementStarted?.Invoke();
            moveCoroutine = StartCoroutine(MovingSmooth(smoothedPath, moveSpeed));
        }
        else
        {
            Debug.LogWarning($"No valid path found for {gameObject.name} to move from {startPosition} to {targetPosition}");
            OnMovementFailed?.Invoke();
        }
    }

    private List<Vector3> ConvertPathToWorldPositions(List<Vector2Int> gridPath)
    {
        List<Vector3> worldPath = new List<Vector3>();

        for (int i = gridPath.Count - 1; i >= 0; i--)
        {
            Vector2Int normalPosition = VoHauMethod.InverseNormalizeGridPosition(gridPath[i], 100, 100);
            Vector3 worldPos = new Vector3(normalPosition.x, transform.position.y, normalPosition.y);
            worldPath.Add(worldPos);
        }

        return worldPath;
    }

    private List<Vector3> SmoothPath(List<Vector3> originalPath)
    {
        if (originalPath.Count <= 2)
            return originalPath;

        List<Vector3> smoothed = new List<Vector3>();
        smoothed.Add(originalPath[0]); 

        for (int i = 1; i < originalPath.Count - 1; i++)
        {
            Vector3 prev = originalPath[i - 1];
            Vector3 current = originalPath[i];
            Vector3 next = originalPath[i + 1];

            if (CanCreateDiagonalPath(prev, next))
            {
                continue;
            }
            else
            {
                Vector3 smoothedPoint = SmoothCorner(prev, current, next);
                smoothed.Add(smoothedPoint);
            }
        }

        smoothed.Add(originalPath[originalPath.Count - 1]);

        return smoothed;
    }

    private bool CanCreateDiagonalPath(Vector3 from, Vector3 to)
    {
        // Check if the diagonal path is clear
        Vector3 direction = (to - from).normalized;
        float distance = Vector3.Distance(from, to);

        float deltaX = Mathf.Abs(to.x - from.x);
        float deltaZ = Mathf.Abs(to.z - from.z);

        return Mathf.Abs(deltaX - deltaZ) < 0.5f && distance <= 2.0f;
    }

    private Vector3 SmoothCorner(Vector3 prev, Vector3 current, Vector3 next)
    {
        Vector3 dirToPrev = (prev - current).normalized;
        Vector3 dirToNext = (next - current).normalized;
        Vector3 avgDirection = (dirToPrev + dirToNext).normalized;

        return current + avgDirection * smoothingRadius;
    }

    private IEnumerator MovingSmooth(List<Vector3> path, float speed)
    {
        for (int i = 0; i < path.Count; i++)
        {
            currentPathIndex = i;
            Vector3 targetPosition = path[i];


            while (Vector3.Distance(transform.position, targetPosition) > stopDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetPosition;

            Vector3Int gridPos = grid.WorldToCell(targetPosition);
            Vector2Int gridPos2D = new Vector2Int(gridPos.x, gridPos.z);
            OnReachedWaypoint?.Invoke(gridPos2D);
        }

        isMoving = false;
        currentPath = null;
        smoothedPath = null;
        currentPathIndex = 0;
        moveCoroutine = null;

        OnMovementCompleted?.Invoke();
    }

    public void Move(Vector2Int targetPosition, float speed)
    {
        moveSpeed = speed;
        Move(targetPosition);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (smoothedPath != null && smoothedPath.Count > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < smoothedPath.Count - 1; i++)
            {
                Gizmos.DrawLine(smoothedPath[i], smoothedPath[i + 1]);
            }

            Gizmos.color = Color.yellow;
            foreach (Vector3 point in smoothedPath)
            {
                Gizmos.DrawSphere(point, 0.1f);
            }
        }

        if (currentPath != null && currentPath.Count > 1)
        {
            Gizmos.color = Color.red;
            for (int i = currentPath.Count - 1; i > 0; i--)
            {
                Vector2Int pos1 = VoHauMethod.InverseNormalizeGridPosition(currentPath[i], 100, 100);
                Vector2Int pos2 = VoHauMethod.InverseNormalizeGridPosition(currentPath[i - 1], 100, 100);

                Vector3 world1 = new Vector3(pos1.x, transform.position.y + 0.5f, pos1.y);
                Vector3 world2 = new Vector3(pos2.x, transform.position.y + 0.5f, pos2.y);

                Gizmos.DrawLine(world1, world2);
            }
        }
    }
#endif
}