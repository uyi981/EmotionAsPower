using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [SerializeField]
    private DropableItem[] dropableItems;

    [Header("Drop Settings")]
    [SerializeField]
    private float dropForce = 5f;
    [SerializeField]
    private float dropRadius = 2f;
    [SerializeField]
    private int minDropAmount = 1;
    [SerializeField]
    private int maxDropAmount = 3;

    private bool isInitialized = false;

    public void Initialize(DropableItem[] items)
    {
        dropableItems = items;
        isInitialized = true;

        // Validate drop chances
        ValidateDropChances();
    }

    public void DropFunction(Vector3 dropPosition, bool forceAllDrop = false)
    {
        if (!isInitialized || dropableItems == null || dropableItems.Length == 0)
        {
            Debug.LogWarning("ItemDropper is not properly initialized or has no dropable items!");
            return;
        }

        Dictionary<ItemSO, int> itemsToDrop = DetermineItemsToDrop(forceAllDrop);

        foreach (var kvp in itemsToDrop)
        {
            DropSingleItem(kvp.Key, dropPosition, kvp.Value);
        }
    }

    public void DropSpecificItem(ItemSO itemSO, Vector3 dropPosition, int amount = -1)
    {
        if (itemSO == null) return;

        if (amount == -1)
        {
            amount = UnityEngine.Random.Range(minDropAmount, maxDropAmount + 1);
        }

        DropSingleItem(itemSO, dropPosition, amount);
    }

    public void DropItems()
    {
        DropFunction(transform.position, false);
    }

    public void DropMultipleItems(Dictionary<ItemSO, int> items, Vector3 dropPosition)
    {
        foreach (var kvp in items)
        {
            DropSingleItem(kvp.Key, dropPosition, kvp.Value);
        }
    }

    public ItemSO[] GetAllDropableItems()
    {
        if (dropableItems == null) return new ItemSO[0];

        ItemSO[] items = new ItemSO[dropableItems.Length];
        for (int i = 0; i < dropableItems.Length; i++)
        {
            items[i] = dropableItems[i].item;
        }
        return items;
    }

    public void SetDropParameters(float force, float radius, int minAmount, int maxAmount)
    {
        dropForce = force;
        dropRadius = radius;
        minDropAmount = minAmount;
        maxDropAmount = maxAmount;
    }

    #region Private Methods
    private Dictionary<ItemSO, int> DetermineItemsToDrop(bool forceAllDrop)
    {
        Dictionary<ItemSO, int> itemsToDrop = new Dictionary<ItemSO, int>();

        foreach (DropableItem dropableItem in dropableItems)
        {
            if (dropableItem.item == null || dropableItem.amountChances == null) continue;

            int amountToDrop = DetermineDropAmount(dropableItem.amountChances, forceAllDrop);

            if (amountToDrop > 0)
            {
                itemsToDrop[dropableItem.item] = amountToDrop;
            }
        }

        return itemsToDrop;
    }

    private int DetermineDropAmount(AmountChance[] amountChances, bool forceAllDrop)
    {
        if (amountChances == null || amountChances.Length == 0) return 0;

        if (forceAllDrop)
        {
            // Find the highest amount when forcing drop
            int maxAmount = 0;
            foreach (AmountChance amountChance in amountChances)
            {
                if (amountChance.amount > maxAmount)
                {
                    maxAmount = amountChance.amount;
                }
            }
            return maxAmount;
        }

        // Check each amount chance in order
        foreach (AmountChance amountChance in amountChances)
        {
            if (UnityEngine.Random.Range(0f, 100f) <= amountChance.chance)
            {
                return amountChance.amount;
            }
        }

        return 0; // Nothing drops
    }

    private void DropSingleItem(ItemSO itemSO, Vector3 dropPosition, int amount)
    {
        if (amount <= 0) return;

        // Spawn individual items with amount = 1
        for (int i = 0; i < amount; i++)
        {
            // Calculate random position within drop radius for each item
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * dropRadius;
            Vector3 finalDropPosition = dropPosition + new Vector3(randomCircle.x, 0, randomCircle.y);

            // Spawn the item using ItemManager with amount = 1
            GameObject droppedItem = ItemManager.Instance.SpawnItem(itemSO, 1, finalDropPosition);

            // Add some physics force if the item has a Rigidbody
            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 randomForce = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(0.5f, 1f),
                    UnityEngine.Random.Range(-1f, 1f)
                ).normalized * dropForce;

                rb.AddForce(randomForce, ForceMode.Impulse);
            }
        }
    }

    private void ValidateDropChances()
    {
        if (dropableItems == null) return;

        foreach (DropableItem dropableItem in dropableItems)
        {
            if (dropableItem.item == null)
            {
                Debug.LogWarning("DropableItem contains null ItemSO!");
                continue;
            }

            if (dropableItem.amountChances == null || dropableItem.amountChances.Length == 0)
            {
                Debug.LogWarning($"DropableItem for {dropableItem.item.name} has no amount chances!");
                continue;
            }

            foreach (AmountChance amountChance in dropableItems.amountChances)
            {
                if (amountChance.chance < 0f || amountChance.chance > 100f)
                {
                    Debug.LogWarning($"Amount chance for item {dropableItem.item.name} (amount: {amountChance.amount}) is outside valid range (0-100): {amountChance.chance}");
                }

                if (amountChance.amount < 0)
                {
                    Debug.LogWarning($"Negative amount ({amountChance.amount}) found for item {dropableItem.item.name}");
                }
            }
        }
    }

    #endregion

    #region Unity Methods

    private void Start()
    {
        // Auto-initialize if dropableItems are set in inspector
        if (dropableItems != null && dropableItems.Length > 0 && !isInitialized)
        {
            Initialize(dropableItems);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Drop radius in scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dropRadius);
    }

    #endregion
}