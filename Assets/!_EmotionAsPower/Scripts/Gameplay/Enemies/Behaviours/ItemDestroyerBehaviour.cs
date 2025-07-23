using UnityEngine;

public class ItemDestroyerBehaviour : IEnemyBehaviour
{
    private Enemy enemy;

    public ItemDestroyerBehaviour(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Update()
    {
        Item target = FindNearestItem();
        if (target == null)
        {
            return; // No items to destroy, remain idle
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

    private Item FindNearestItem()
    {
        Item[] items = Object.FindObjectsOfType<Item>();
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