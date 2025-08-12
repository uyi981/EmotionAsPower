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
        villager.DropAllItems();
    }
    public void UpdateState()
    {
        // Logic for updating the villager selected state
        if (villager.itemHandle.transform.childCount == 0)
        {
            villager.TransitionTo(villager.villagerIdleState);
            return;
        }
    }
    public void ExitState()
    {
        villager.completedGoToTarget -= DropItem;
        villager.Target =  null; // Clear the target
        villager.animator.Play("idle");
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
        if(villager.Target == null)
        {
            villager.TransitionTo(villager.villagerIdleState);
            return;
        }
        Vector3Int vector3Int = Singleton<GridSystem>.Instance.grid.WorldToCell(villager.Target.transform.position);
        Vector2Int targetPosition = new Vector2Int(vector3Int.x, vector3Int.z);
        villager.Move(targetPosition,villager.personality.moveSpeedModifier); // Assuming home is at (0, 0)
        villager.completedGoToTarget += DropItem;
        villager.isWorking = true; // Set working state to false
    }
    public void DropItem()
    {
        if(villager.Target == null)
        {
            Debug.LogWarning("No target to attack.");
            return;
        }
        Health health = villager.Target.GetComponent<Health>();
        if (health == null)
        {
            BuildingBase healthInterface = villager.Target.GetComponent<BuildingBase>();
            if(healthInterface != null)
            {
                villager.StartCoroutine(Attack(health)); // Start attacking the target
                return;
            }
            Debug.LogWarning("No health component found on the target.");
            villager.TransitionTo(villager.villagerIdleState);
            
        }
        villager.StartCoroutine(Attack(health)); // Start attacking the target
    }
    public IEnumerator Attack(BuildingBase health)
    {
        while (health.currentHP>0)
        {
            villager.animator.Play("Attack");
            yield return new WaitForSeconds(1f); // Wait for 1 second before the next attack
        }
        villager.TransitionTo(villager.villagerIdleState); // Transition to idle state after defeating the enemy
    }
    public IEnumerator Attack(Health health)
    {
        while(health.CurrentHealth>0)
        {
            villager.animator.Play("Attack");
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
public class villagerPrisonState :IState
{
    Villager villager;
    public villagerPrisonState(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
       villager.isPrisoner = true; // Set villager as a prisoner
    }

    public void ExitState()
    {

    }

    public void UpdateState()
    {
       
    }
}
