using UnityEngine;

public class ItemGroupHandler : MonoBehaviour
{
    public static int maxAmountPerItemInstance = 99;
    [SerializeField]
    private Item item;
    public Item Item => item;

    private bool isProcessingMerge = false; // Prevent multiple merge operations

    [Header("Merge Settings")]
    [SerializeField] private float mergeRadius = 1.5f; // Distance to spread multiple items
    [SerializeField] private LayerMask groundLayer = 1; // For ground detection

    private void OnTriggerEnter(Collider other)
    {
        // Skip if already processing a merge to prevent duplicate operations
        if (isProcessingMerge) return;

        ItemGroupHandler otherHandler = other.gameObject.GetComponent<ItemGroupHandler>();
        if (otherHandler != null && !otherHandler.isProcessingMerge)
        {
            // Check if items are the same type
            if (otherHandler.Item.ItemSO.ID == this.item.ItemSO.ID)
            {
                //Debug.Log("Detect same item - attempting to merge");
                MergeItems(otherHandler);
            }
        }
    }

    private void MergeItems(ItemGroupHandler otherHandler)
    {
        // Set both handlers as processing to prevent duplicate merges
        this.isProcessingMerge = true;
        otherHandler.isProcessingMerge = true;

        // Calculate total amount
        int totalAmount = this.item.Amount + otherHandler.Item.Amount;

        // Calculate center position between the two items
        Vector3 centerPosition = (this.transform.position + otherHandler.transform.position) / 2f;

        // Handle amount overflow
        if (totalAmount <= maxAmountPerItemInstance)
        {
            // Can merge into single item - spawn at center
            SpawnMergedItem(this.item.ItemSO, totalAmount, FindValidSpawnPosition(centerPosition) + Vector3.up);

            // Destroy both original items
            DestroyItems(this.item, otherHandler.Item);
        }
        else
        {
            // Need to split into multiple items with proper spacing
            HandleOverflow(this.item.ItemSO, totalAmount, centerPosition, this.item, otherHandler.Item);
        }
    }

    private void SpawnMergedItem(ItemSO itemSO, int amount, Vector3 position)
    {
        ItemManager.Instance.SpawnItem(itemSO, amount, position);
    }

    private void HandleOverflow(ItemSO itemSO, int totalAmount, Vector3 centerPosition, Item item1, Item item2)
    {
        // Calculate how many full stacks we need
        int fullStacks = totalAmount / maxAmountPerItemInstance;
        int remainder = totalAmount % maxAmountPerItemInstance;
        int totalItems = fullStacks + (remainder > 0 ? 1 : 0);

        // Find valid positions for all items
        Vector3[] spawnPositions = FindCircularSpawnPositions(centerPosition, totalItems);

        int positionIndex = 0;

        // Spawn full stacks
        for (int i = 0; i < fullStacks; i++)
        {
            if (positionIndex < spawnPositions.Length)
            {
                SpawnMergedItem(itemSO, maxAmountPerItemInstance, spawnPositions[positionIndex]);
                positionIndex++;
            }
        }

        // Spawn remainder if exists
        if (remainder > 0 && positionIndex < spawnPositions.Length)
        {
            SpawnMergedItem(itemSO, remainder, spawnPositions[positionIndex]);
        }

        // Destroy original items
        DestroyItems(item1, item2);
    }

    private Vector3[] FindCircularSpawnPositions(Vector3 centerPosition, int itemCount)
    {
        Vector3[] positions = new Vector3[itemCount];

        if (itemCount == 1)
        {
            positions[0] = FindValidSpawnPosition(centerPosition);
            return positions;
        }

        // Arrange items in a circle around the center
        float angleStep = 360f / itemCount;

        for (int i = 0; i < itemCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * mergeRadius,
                0f,
                Mathf.Sin(angle) * mergeRadius
            );

            Vector3 targetPosition = centerPosition + offset;
            positions[i] = FindValidSpawnPosition(targetPosition);
        }

        return positions;
    }

    private Vector3 FindValidSpawnPosition(Vector3 preferredPosition)
    {
        // First, try to find ground level at the preferred position
        Vector3 finalPosition = preferredPosition;

        // Raycast down to find ground
        if (Physics.Raycast(preferredPosition + Vector3.up * 5f, Vector3.down, out RaycastHit groundHit, 10f, groundLayer))
        {
            finalPosition.y = groundHit.point.y + 0.1f; // Slight offset above ground
        }

        // Check if position is clear of other colliders (except ground)
        float checkRadius = 0.5f; // Adjust based on your item collider size
        Collider[] overlapping = Physics.OverlapSphere(finalPosition, checkRadius);

        // Filter out ground and trigger colliders
        bool positionClear = true;
        foreach (var collider in overlapping)
        {
            if (!collider.isTrigger && !IsGroundCollider(collider))
            {
                positionClear = false;
                break;
            }
        }

        // If position is not clear, try to find a nearby clear position
        if (!positionClear)
        {
            finalPosition = FindNearestClearPosition(preferredPosition, checkRadius);
        }

        return finalPosition;
    }

    private Vector3 FindNearestClearPosition(Vector3 startPosition, float itemRadius)
    {
        // Try positions in expanding spiral pattern
        int maxAttempts = 16;
        float searchRadius = itemRadius * 2f;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float angle = attempt * 45f * Mathf.Deg2Rad; // 45 degree increments
            float distance = searchRadius + (attempt * itemRadius * 0.5f);

            Vector3 testPosition = startPosition + new Vector3(
                Mathf.Cos(angle) * distance,
                0f,
                Mathf.Sin(angle) * distance
            );

            // Adjust for ground height
            if (Physics.Raycast(testPosition + Vector3.up * 5f, Vector3.down, out RaycastHit groundHit, 10f, groundLayer))
            {
                testPosition.y = groundHit.point.y + 0.1f;
            }

            // Check if this position is clear
            Collider[] overlapping = Physics.OverlapSphere(testPosition, itemRadius);
            bool isClear = true;

            foreach (var collider in overlapping)
            {
                if (!collider.isTrigger && !IsGroundCollider(collider))
                {
                    isClear = false;
                    break;
                }
            }

            if (isClear)
            {
                return testPosition;
            }
        }

        // Fallback: return original position slightly elevated
        return startPosition + Vector3.up * 0.5f;
    }

    private bool IsGroundCollider(Collider collider)
    {
        // Check if collider is on ground layer
        return ((1 << collider.gameObject.layer) & groundLayer) != 0;
    }

    private void DestroyItems(Item item1, Item item2)
    {
        // Use a small delay to ensure the merge operation completes properly
        StartCoroutine(DestroyItemsDelayed(item1, item2));
    }

    private System.Collections.IEnumerator DestroyItemsDelayed(Item item1, Item item2)
    {
        yield return new WaitForEndOfFrame();

        if (item1 != null) item1.Clear();
        if (item2 != null) item2.Clear();
    }

    public void SetItem(Item item)
    {
        this.item = item;
    }

    // Optional: Visual feedback when items can be merged
    private void OnTriggerStay(Collider other)
    {
        if (isProcessingMerge) return;

        ItemGroupHandler otherHandler = other.gameObject.GetComponent<ItemGroupHandler>();
        if (otherHandler != null && !otherHandler.isProcessingMerge)
        {
            if (otherHandler.Item.ItemSO.ID == this.item.ItemSO.ID)
            {
                // Optional: Add visual feedback here (e.g., highlight items)
                // This could be useful for showing players which items can merge
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Optional: Remove visual feedback when items separate
        ItemGroupHandler otherHandler = other.gameObject.GetComponent<ItemGroupHandler>();
        if (otherHandler != null)
        {
            if (otherHandler.Item.ItemSO.ID == this.item.ItemSO.ID)
            {
                // Remove any visual feedback here
            }
        }
    }

    // Debug visualization in Scene view
    private void OnDrawGizmosSelected()
    {
        if (item != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, mergeRadius);

            // Show trigger range
            SphereCollider sphereCollider = GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, sphereCollider.radius);
            }
        }
    }
}