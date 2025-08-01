using System.Collections;
using UnityEngine;
public class VillagerBackToHomeState : IState
{
    Villager villager;
    public VillagerBackToHomeState(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
        villager.Move(new Vector2Int(0, 0), 1f); // Assuming home is at (0, 0)
        villager.completedGoToTarget += DropItem;
        villager.isWorking = true; // Set working state to false
    }
    public void DropItem()
    {
        if (villager.itemHandle.transform.childCount == 0)
        {
            Debug.LogWarning("No items to drop.");
            return;
        }
        Item item = villager.itemHandle.transform.GetChild(0).gameObject.GetComponent<Item>();
        if (item == null)
        {
            Debug.LogWarning("No item found in the item handle.");
            return;
        }
        Singleton<ItemStorage>.Instance.AddItem(item.ItemSO, 1); // Add item to storage
        villager.itemHandle.transform.DetachChildren(); // Detach all children from the item handle
        GameObject.Destroy(item.gameObject); // Destroy the item game object
        villager.TransitionTo(villager.villagerIdleState);
    }
    public void UpdateState()
    {
        // Logic for updating the villager selected state
    }
    public void ExitState()
    {
        villager.completedGoToTarget -= DropItem;
        // Logic for exiting the villager selected state
    }
}
public class VillagerAttackEnermyState : IState
{
    Villager villager;
    
    public VillagerAttackEnermyState(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
        Vector3Int vector3Int = Singleton<GridSystem>.Instance.grid.WorldToCell(villager.Target.transform.position);
        Vector2Int targetPosition = new Vector2Int(vector3Int.x, vector3Int.z);
        villager.Move(targetPosition,villager.personality.moveSpeedModifier); // Assuming home is at (0, 0)
        villager.completedGoToTarget += DropItem;
        villager.isWorking = true; // Set working state to false
    }
    public void DropItem()
    {
        Health health = villager.Target.GetComponent<Health>();
        if (health == null)
        {
            Debug.LogWarning("No health component found on the target.");
        }
        villager.StartCoroutine(Attack(health)); // Start attacking the target
    }
    public IEnumerator Attack(Health health)
    {
        while(health.CurrentHealth>0)
        {
            villager.animator.Play("Attack");
            health.TakeDamage(-5);
            yield return new WaitForSeconds(1f); // Wait for 1 second before the next attack
        }
        villager.TransitionTo(villager.villagerIdleState); // Transition to idle state after defeating the enemy
    }
    public void UpdateState()
    {
        // Logic for updating the villager selected state
    }
    public void ExitState()
    {
        villager.completedGoToTarget -= DropItem;
        // Logic for exiting the villager selected state
    }
}
