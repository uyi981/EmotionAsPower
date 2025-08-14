using System.Collections;
using UnityEngine;
public class VillagerBackToHomeState : IState
{
    Villager villager;
    Coroutine moveCoroutine;
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
        villager.TransitionTo(villager.villagerIdleState);   // Transition to idle state after dropping items
    }
    public void UpdateState()
    {
        // Logic for updating the villager selected stat
    }
    public void ExitState()
    {
        villager.completedGoToTarget -= DropItem;
        villager.ResetCoroutine(moveCoroutine); 
        // Reset the move coroutine reference
      //  villager.Target =  null; // Clear the target
       // villager.animator.Play("idle");
        // Logic for exiting the villager selected state
    }
}
public class VillagerLeavingState : IState
{
    Villager villager;
    Coroutine moveCoroutine;

    public VillagerLeavingState(Villager villager)
    {
        this.villager = villager;
    }

    public void EnterState()
    {
        // Move to a far position (e.g., outside the map)
        villager.Move(new Vector2Int(69,69), 1f);
        villager.completedGoToTarget += OnLeavingComplete;
        villager.isWorking = true;
    }

    private void OnLeavingComplete()
    {
        // Remove from job list and other systems if needed
        Singleton<VillagerManager>.Instance.jobForWorkers.Remove(villager);

        // Optionally play a leaving animation here
        // villager.animator.Play("Leave");

        // Destroy after a short delay for animation (optional)
        villager.StartCoroutine(DestroyAfterDelay(0.5f));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject.Destroy(villager.gameObject);
    }

    public void UpdateState()
    {
        // No update logic needed for leaving
    }

    public void ExitState()
    {
        villager.completedGoToTarget -= OnLeavingComplete;
        villager.ResetCoroutine(moveCoroutine);
    }
}
public class VillagerAttackEnermyState : IState
{
    Villager villager;
    Coroutine attack;
    Coroutine moveCoroutine;


    public VillagerAttackEnermyState(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
        villager.isWorking = true; // Set working state to false
        if (villager.Target == null)
        {
            villager.TransitionTo(villager.villagerIdleState);
            return;
        }
        Vector3Int vector3Int = Singleton<GridSystem>.Instance.grid.WorldToCell(villager.Target.transform.position);
        Vector2Int targetPosition = new Vector2Int(vector3Int.x, vector3Int.z);
        villager.Move(targetPosition,villager.speed); // Assuming home is at (0, 0)
        villager.completedGoToTarget += DropItem;
        //villager.isWorking = true; // Set working state to false
    }
    public void DropItem()
    {
        if(villager.Target == null)
        {
            Debug.LogWarning("No target to attack.");
            villager.TransitionTo(villager.villagerIdleState);
            return;
        }
        Health health = villager.Target.GetComponent<Health>();
        if(health==null)
        {
            villager.TransitionTo(villager.villagerIdleState);
            return;
        } 
        attack = villager.StartCoroutine(Attack(health)); // Start attacking the target
    }
    public IEnumerator Attack(BuildingBase health)
    {
        while (health.currentHP>0)
        {
            villager.animator.Play("Attack");
            health.TakeDamage(1f); // Assuming personality has attack damage
            yield return new WaitForSeconds(1f); // Wait for 1 second before the next attack
        }
        villager.TransitionTo(villager.villagerIdleState); // Transition to idle state after defeating the enemy
    }
    public IEnumerator Attack(Health health)
    {
        while(health !=null||health.CurrentHealth>0)
        {
            if(health==null)
            {
                break;
            }    
            villager.animator.Play("Attack");
            health.TakeDamage(1f);
            
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
        villager.isWorking = false; // Reset working state
        villager.completedGoToTarget -= DropItem;
        villager.Target = null; // Clear the target
        villager.animator.Play("idle");
        villager.ResetCoroutine(attack); // Reset the coroutine reference
        villager.ResetCoroutine(moveCoroutine); // Reset the move coroutine reference
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
        if(villager.moveCoroutine!=null)
        {
            villager.StopCoroutine(villager.moveCoroutine); // Stop any existing move coroutine
            villager.moveCoroutine = null; // Clear the reference
        }
    }

    public void ExitState()
    {

    }

    public void UpdateState()
    {
       
    }
}
