using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;
    public float stopDistance = 0.1f;

    [Header("Flip Settings")]
    public bool flipable = true;
    public bool useBoneFlip = false;
    public bool baseFlip;
    private SpriteRenderer sprite;
    private Transform boneTransform;

    [Header("Path Smoothing")]
    public bool enablePathSmoothing = true;
    public float smoothingRadius = 0.5f;

    [Header("Movement Debugging")]
    public float minDistanceForNewPath = 1f;
    public float pathValidationDistance = 0.5f;

    private APathFinding pathFinding;
    private Grid grid;
    private float[,] gridMap;
    private List<Vector2Int> currentPath;
    private List<Vector3> smoothedPath;
    private int currentPathIndex;
    private bool isMoving;
    private Coroutine moveCoroutine;
    private Vector3 currentTarget;

    public System.Action OnMovementStarted;
    public System.Action OnMovementCompleted;
    public System.Action OnMovementFailed;
    public System.Action<Vector2Int> OnReachedWaypoint;

    private void Start()
    {
        pathFinding = new APathFinding();

        InitializeFlipComponents();

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

    private void InitializeFlipComponents()
    {
        if (!flipable) return;

        if (useBoneFlip)
        {
            boneTransform = FindBoneTransform();

            if (boneTransform == null)
            {
                Debug.LogWarning($"No bone transform found on {gameObject.name} for bone flipping. Falling back to sprite flip.");
                useBoneFlip = false;
            }
            else
            {
                SetBoneFlip(baseFlip);
            }
        }

        if (!useBoneFlip)
        {
            sprite = GetComponentInChildren<SpriteRenderer>();
            if (sprite == null)
            {
                Debug.LogWarning($"No SpriteRenderer found on {gameObject.name}. Flipping disabled.");
                flipable = false;
            }
            else
            {
                sprite.flipX = baseFlip;
            }
        }
    }

    private Transform FindBoneTransform()
    {
        string[] boneNames = { "Body", "Bone", "Root", "Armature", "Model", "Character" };

        foreach (string boneName in boneNames)
        {
            Transform found = transform.Find(boneName);
            if (found != null) return found;
        }

        if (transform.childCount > 0)
        {
            return transform.GetChild(0);
        }

        return null;
    }

    public void MoveToWorldPosition(Vector3 worldPosition)
    {
        if (grid == null || gridMap == null)
        {
            Debug.LogError($"Grid system not initialized for {gameObject.name}");
            OnMovementFailed?.Invoke();
            return;
        }

        if (isMoving && Vector3.Distance(currentTarget, worldPosition) < minDistanceForNewPath)
        {
            return;
        }

        Vector3Int startGridPos = grid.WorldToCell(transform.position);
        Vector3Int targetGridPos = grid.WorldToCell(worldPosition);

        Vector2Int start = new Vector2Int(startGridPos.x, startGridPos.z);
        Vector2Int target = new Vector2Int(targetGridPos.x, targetGridPos.z);

        if (Vector3.Distance(transform.position, worldPosition) <= stopDistance)
        {
            return;
        }

        currentTarget = worldPosition;
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
        currentTarget = Vector3.zero;

        SetFlipState(baseFlip);
    }

    public bool IsMoving()
    {
        return isMoving && moveCoroutine != null;
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

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        Vector3Int unitGridPos = grid.WorldToCell(transform.position);
        Vector2Int startPosition = new Vector2Int(unitGridPos.x, unitGridPos.z);

        if (startPosition == targetPosition)
        {
            return;
        }

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

            List<Vector3> worldPath = ConvertPathToWorldPositions(path);

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
        Vector3 currentPos = transform.position;

        List<Vector3> rawWorldPath = new List<Vector3>();
        for (int i = gridPath.Count - 1; i >= 0; i--)
        {
            Vector2Int normalPosition = VoHauMethod.InverseNormalizeGridPosition(gridPath[i], 100, 100);
            Vector3 worldPos = new Vector3(normalPosition.x, transform.position.y, normalPosition.y);
            rawWorldPath.Add(worldPos);
        }

        if (rawWorldPath.Count == 0) return worldPath;

        worldPath = OptimizePathStart(rawWorldPath, currentPos);

        // Ensure the final point is exactly the target world position
        if (worldPath.Count > 0 && currentTarget != Vector3.zero)
        {
            worldPath[worldPath.Count - 1] = new Vector3(currentTarget.x, transform.position.y, currentTarget.z);
        }

        return worldPath;
    }

    private List<Vector3> OptimizePathStart(List<Vector3> rawPath, Vector3 currentPosition)
    {
        List<Vector3> optimizedPath = new List<Vector3>();

        if (rawPath.Count == 0) return optimizedPath;

        int startIndex = 0;
        float minBacktrackDistance = float.MaxValue;

        for (int i = 0; i < Mathf.Min(3, rawPath.Count); i++)
        {
            Vector3 waypoint = rawPath[i];
            Vector3 directionToCurrent = (currentPosition - waypoint).normalized;

            if (i + 1 < rawPath.Count)
            {
                Vector3 directionToNext = (rawPath[i + 1] - waypoint).normalized;
                float backtrackAmount = Vector3.Dot(directionToCurrent, directionToNext);

                if (backtrackAmount < minBacktrackDistance)
                {
                    minBacktrackDistance = backtrackAmount;
                    startIndex = i;
                }
            }

            float distanceToWaypoint = Vector3.Distance(currentPosition, waypoint);
            if (distanceToWaypoint < pathValidationDistance && i + 1 < rawPath.Count)
            {
                startIndex = i + 1;
                break;
            }
        }

        for (int i = startIndex; i < rawPath.Count; i++)
        {
            optimizedPath.Add(rawPath[i]);
        }

        if (startIndex > 0 && optimizedPath.Count > 0)
        {
            Vector3 firstWaypoint = optimizedPath[0];
            Vector3 directionToFirst = (firstWaypoint - currentPosition).normalized;
            float distanceToFirst = Vector3.Distance(currentPosition, firstWaypoint);

            if (distanceToFirst > pathValidationDistance * 2)
            {
                Vector3 intermediatePoint = currentPosition + directionToFirst * (distanceToFirst * 0.5f);
                optimizedPath.Insert(0, intermediatePoint);
            }
        }

        return optimizedPath;
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
                if (smoothed.Count == 0 || Vector3.Distance(smoothed[smoothed.Count - 1], current) > pathValidationDistance)
                {
                    Vector3 smoothedPoint = SmoothCorner(prev, current, next);
                    smoothed.Add(smoothedPoint);
                }
            }
        }

        Vector3 lastPoint = originalPath[originalPath.Count - 1];
        if (smoothed.Count == 0 || Vector3.Distance(smoothed[smoothed.Count - 1], lastPoint) > pathValidationDistance * 0.5f)
        {
            smoothed.Add(lastPoint);
        }

        return smoothed;
    }

    private bool CanCreateDiagonalPath(Vector3 from, Vector3 to)
    {
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
        Vector3 previousPosition = transform.position;

        for (int i = 0; i < path.Count; i++)
        {
            currentPathIndex = i;
            Vector3 targetPosition = path[i];

            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            if (distanceToTarget < pathValidationDistance)
            {
                continue;
            }

            if (flipable)
            {
                Vector3 movementDirection = targetPosition - transform.position;
                if (Mathf.Abs(movementDirection.x) > 0.1f)
                {
                    bool shouldFlip = baseFlip ? (movementDirection.x >= 0) : (movementDirection.x < 0);
                    SetFlipState(shouldFlip);
                }
            }

            while (Vector3.Distance(transform.position, targetPosition) > stopDistance)
            {
                Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

                Vector3 directionToTarget = (targetPosition - transform.position).normalized;
                Vector3 movementDirection = (newPosition - transform.position).normalized;

                if (Vector3.Dot(directionToTarget, movementDirection) > 0.1f || Vector3.Distance(transform.position, targetPosition) > stopDistance * 2)
                {
                    transform.position = newPosition;
                }
                else
                {
                    transform.position = targetPosition;
                    break;
                }

                yield return null;

                if (!isMoving)
                {
                    yield break;
                }
            }

            transform.position = targetPosition;
            previousPosition = targetPosition;

            Vector3Int gridPos = grid.WorldToCell(targetPosition);
            Vector2Int gridPos2D = new Vector2Int(gridPos.x, gridPos.z);
            OnReachedWaypoint?.Invoke(gridPos2D);
        }

        CompleteMovement();
    }

    private void CompleteMovement()
    {
        isMoving = false;
        currentPath = null;
        smoothedPath = null;
        currentPathIndex = 0;
        moveCoroutine = null;
        currentTarget = Vector3.zero;

        SetFlipState(baseFlip);

        OnMovementCompleted?.Invoke();
    }

    public void Move(Vector2Int targetPosition, float speed)
    {
        moveSpeed = speed;
        Move(targetPosition);
    }

    public void ToggleFlip(bool direction)
    {
        SetFlipState(direction);
    }

    private void SetFlipState(bool flipState)
    {
        if (!flipable) return;

        if (useBoneFlip && boneTransform != null)
        {
            SetBoneFlip(flipState);
        }
        else if (!useBoneFlip && sprite != null)
        {
            sprite.flipX = flipState;
        }
    }

    private void SetBoneFlip(bool flipState)
    {
        if (boneTransform == null) return;

        Vector3 scale = boneTransform.localScale;
        scale.z = flipState ? -Mathf.Abs(scale.z) : Mathf.Abs(scale.z);
        boneTransform.localScale = scale;
    }

    public bool ShouldRecalculatePath(Vector3 newTarget)
    {
        if (!isMoving) return true;
        return Vector3.Distance(currentTarget, newTarget) > minDistanceForNewPath;
    }

    public void SetFlipMode(bool useBone)
    {
        if (useBone != useBoneFlip)
        {
            SetFlipState(baseFlip);

            useBoneFlip = useBone;
            InitializeFlipComponents();
        }
    }

    public void SetBoneTransform(Transform bone)
    {
        boneTransform = bone;
        if (useBoneFlip && boneTransform != null)
        {
            SetBoneFlip(baseFlip);
        }
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

            if (currentTarget != Vector3.zero)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(currentTarget, Vector3.one * 0.5f);
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