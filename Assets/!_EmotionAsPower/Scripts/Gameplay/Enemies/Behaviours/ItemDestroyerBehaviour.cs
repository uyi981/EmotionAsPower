using UnityEngine;

public class ItemDestroyerBehaviour : IEnemyBehaviour
{
    private Enemy enemy;
    private Vector3 centerPosition = Vector3.zero;

    public ItemDestroyerBehaviour(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Update()
    {
        Item target = FindNearestItem();

        if (target == null)
        {
            // No items to destroy, move towards center (0,0,0)
            MoveTowardsCenter();
            return;
        }

        float distance = Vector3.Distance(enemy.transform.position, target.transform.position);

        if (distance < 1f) // Destroy range
        {
            DestroyItem(target);
        }
        else
        {
            // Move towards the item
            Vector3 direction = (target.transform.position - enemy.transform.position).normalized;
            enemy.transform.position += direction * enemy.EnemySO.defaultData.moveSpeed * Time.deltaTime;
        }
    }

    private void MoveTowardsCenter()
    {
        float distanceToCenter = Vector3.Distance(enemy.transform.position, centerPosition);

        // Only move if not already at center
        if (distanceToCenter > 0.1f)
        {
            Vector3 direction = (centerPosition - enemy.transform.position).normalized;
            enemy.transform.position += direction * enemy.EnemySO.defaultData.moveSpeed * Time.deltaTime;
        }
    }

    private Item FindNearestItem()
    {
        Item[] items = Object.FindObjectsByType<Item>(FindObjectsSortMode.None);
        if (items.Length == 0) return null;

        Item nearest = null;
        float minDistance = float.MaxValue;

        foreach (var item in items)
        {
            float distance = Vector3.Distance(enemy.transform.position, item.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = item;
            }
        }

        return nearest;
    }

    private void DestroyItem(Item item)
    {
        if (item != null)
        {
            item.Clear(); // Calls Item's Clear method to destroy it
        }
    }
}