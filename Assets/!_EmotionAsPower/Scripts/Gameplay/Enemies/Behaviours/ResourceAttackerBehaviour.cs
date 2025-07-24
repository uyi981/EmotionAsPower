using UnityEngine;

public class ResourceAttackerBehaviour : IEnemyBehaviour
{
    private Enemy enemy;
    private float lastAttackTime;
    private Vector3 centerPosition = Vector3.zero;

    public ResourceAttackerBehaviour(Enemy enemy)
    {
        this.enemy = enemy;
        lastAttackTime = 0f;
    }

    public void Update()
    {
        Resource target = FindNearestResource();

        if (target == null)
        {
            // No resources to attack, move towards center (0,0,0)
            MoveTowardsCenter();
            return;
        }

        float distance = Vector3.Distance(enemy.transform.position, target.transform.position);

        if (distance < 1f) // Attack range
        {
            if (Time.time >= lastAttackTime + 1f / enemy.EnemySO.defaultData.attackSpeed)
            {
                Attack(target);
                lastAttackTime = Time.time;
            }
        }
        else
        {
            // Move towards the resource
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

    private Resource FindNearestResource()
    {
        Resource[] resources = Object.FindObjectsByType<Resource>(FindObjectsSortMode.None);
        Resource nearest = null;
        float minDistance = float.MaxValue;

        foreach (var resource in resources)
        {
            if (!resource.IsDepleted)
            {
                float distance = Vector3.Distance(enemy.transform.position, resource.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = resource;
                }
            }
        }

        return nearest;
    }

    private void Attack(Resource resource)
    {
        if (resource != null && !resource.IsDepleted)
        {
            resource.Health.TakeDamage(enemy.EnemySO.defaultData.attackDamage);
        }
    }
}