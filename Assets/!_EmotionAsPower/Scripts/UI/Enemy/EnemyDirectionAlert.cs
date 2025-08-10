using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDirectionAlert : MonoBehaviour
{
    [Header("Alert Settings")]
    [SerializeField] private GameObject alertIndicatorPrefab;
    [SerializeField] private Canvas alertCanvas;
    [SerializeField] private float alertDistance = 20f;
    [SerializeField] private float edgeOffset = 50f;
    [SerializeField] private bool showDistanceText = true;
    [SerializeField] private LayerMask enemyLayer = -1;

    [Header("Colors")]
    [SerializeField] private Color closeEnemyColor = Color.red;
    [SerializeField] private Color farEnemyColor = Color.yellow;
    [SerializeField] private float closeDistance = 10f;

    private Camera playerCamera;
    private RectTransform canvasRect;
    private Dictionary<Enemy, GameObject> activeAlerts = new Dictionary<Enemy, GameObject>();
    private List<Enemy> enemiesInRange = new List<Enemy>();

    private void Start()
    {
        // Get the main camera or find the player camera
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();

        // Get canvas rect for screen calculations
        if (alertCanvas == null)
            alertCanvas = FindObjectOfType<Canvas>();

        canvasRect = alertCanvas.GetComponent<RectTransform>();

        // Validate prefab
        if (alertIndicatorPrefab == null)
        {
            Debug.LogError("Alert Indicator Prefab is not assigned!");
        }
    }

    private void Update()
    {
        FindEnemiesInRange();
        UpdateAlerts();
        CleanupInactiveAlerts();
    }

    private void FindEnemiesInRange()
    {
        enemiesInRange.Clear();

        // Find all enemies within alert distance
        Collider[] enemyColliders = Physics.OverlapSphere(
            playerCamera.transform.position,
            alertDistance,
            enemyLayer
        );

        foreach (Collider col in enemyColliders)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemiesInRange.Add(enemy);
            }
        }
    }

    private void UpdateAlerts()
    {
        foreach (Enemy enemy in enemiesInRange)
        {
            if (enemy == null) continue;

            Vector3 enemyPosition = enemy.transform.position;
            Vector3 cameraPosition = playerCamera.transform.position;

            // Check if enemy is visible on screen
            Vector3 screenPoint = playerCamera.WorldToViewportPoint(enemyPosition);
            bool isVisible = screenPoint.z > 0 && screenPoint.x >= 0 && screenPoint.x <= 1
                           && screenPoint.y >= 0 && screenPoint.y <= 1;

            // Only show alert if enemy is outside screen bounds
            if (!isVisible)
            {
                CreateOrUpdateAlert(enemy, enemyPosition, cameraPosition);
            }
            else
            {
                // Remove alert if enemy is now visible
                RemoveAlert(enemy);
            }
        }
    }

    private void CreateOrUpdateAlert(Enemy enemy, Vector3 enemyPosition, Vector3 cameraPosition)
    {
        // Create alert if it doesn't exist
        if (!activeAlerts.ContainsKey(enemy))
        {
            GameObject alertObject = Instantiate(alertIndicatorPrefab, alertCanvas.transform);
            activeAlerts[enemy] = alertObject;
        }

        GameObject alert = activeAlerts[enemy];
        if (alert == null) return;

        // Calculate direction from camera to enemy
        Vector3 direction = (enemyPosition - cameraPosition).normalized;

        // Convert to screen direction
        Vector3 screenDirection = playerCamera.WorldToViewportPoint(cameraPosition + direction) -
                                 playerCamera.WorldToViewportPoint(cameraPosition);
        screenDirection = screenDirection.normalized;

        // Calculate position on screen edge
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 alertPosition = GetEdgePosition(screenDirection, screenSize);

        // Convert to canvas coordinates
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, alertPosition, alertCanvas.worldCamera, out Vector2 canvasPosition);

        // Update alert position
        RectTransform alertRect = alert.GetComponent<RectTransform>();
        alertRect.localPosition = canvasPosition;

        // Update alert rotation to point toward enemy
        float angle = Mathf.Atan2(screenDirection.y, screenDirection.x) * Mathf.Rad2Deg;
        alertRect.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Update color based on distance
        float distance = Vector3.Distance(enemyPosition, cameraPosition);
        Color alertColor = distance <= closeDistance ? closeEnemyColor : farEnemyColor;

        Image alertImage = alert.GetComponent<Image>();
        if (alertImage != null)
            alertImage.color = alertColor;

        // Update distance text if enabled
        if (showDistanceText)
        {
            Text distanceText = alert.GetComponentInChildren<Text>();
            if (distanceText != null)
                distanceText.text = $"{distance:F0}m";
        }
    }

    private Vector2 GetEdgePosition(Vector3 screenDirection, Vector2 screenSize)
    {
        // Calculate where the direction intersects the screen edges
        Vector2 edgePosition = Vector2.zero;

        float absX = Mathf.Abs(screenDirection.x);
        float absY = Mathf.Abs(screenDirection.y);

        if (absX > absY)
        {
            // Hit left or right edge
            edgePosition.x = screenDirection.x > 0 ? screenSize.x - edgeOffset : edgeOffset;
            edgePosition.y = screenSize.y * 0.5f + (screenDirection.y / absX) * (screenSize.y * 0.5f - edgeOffset);
        }
        else
        {
            // Hit top or bottom edge
            edgePosition.y = screenDirection.y > 0 ? screenSize.y - edgeOffset : edgeOffset;
            edgePosition.x = screenSize.x * 0.5f + (screenDirection.x / absY) * (screenSize.x * 0.5f - edgeOffset);
        }

        // Clamp to screen bounds
        edgePosition.x = Mathf.Clamp(edgePosition.x, edgeOffset, screenSize.x - edgeOffset);
        edgePosition.y = Mathf.Clamp(edgePosition.y, edgeOffset, screenSize.y - edgeOffset);

        return edgePosition;
    }

    private void RemoveAlert(Enemy enemy)
    {
        if (activeAlerts.ContainsKey(enemy))
        {
            if (activeAlerts[enemy] != null)
                Destroy(activeAlerts[enemy]);
            activeAlerts.Remove(enemy);
        }
    }

    private void CleanupInactiveAlerts()
    {
        // Remove alerts for destroyed enemies
        List<Enemy> keysToRemove = new List<Enemy>();

        foreach (var kvp in activeAlerts)
        {
            if (kvp.Key == null || !enemiesInRange.Contains(kvp.Key))
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (Enemy key in keysToRemove)
        {
            RemoveAlert(key);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null) return;

        // Draw alert range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerCamera.transform.position, alertDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerCamera.transform.position, closeDistance);
    }
}