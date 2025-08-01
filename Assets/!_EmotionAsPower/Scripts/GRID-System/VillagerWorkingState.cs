using Unity.VisualScripting;
using UnityEngine;

public class VillagerWorkingState : IState
{
    private Villager villager;
    public VillagerWorkingState(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
        Debug.Log("Villager is now working.");
        villager.Move(villager.currentJob.Position, 1f);
        villager.completedGoToTarget += OnWork;
        villager.collisionTrigger += OnCollisionEnter; // Subscribe to collision events
        // Implement logic for entering the working state
    }
    public void OnWork()
    {
        if (villager.currentJob.JobType.Equals(JobType.Transport))
        {
            villager.TransitionTo(villager.villagerIdleState);
        }
        else
        {
            villager.currentJob.buildingBase.OnWorkerCome(villager);

        }
    }
    public void OnCollisionEnter(Collision collision)
    {
         if (collision.gameObject.CompareTag("Item"))
        {
            collision.transform.SetParent(villager.itemHandle.transform); // Set the collided object as a child of the villager
            collision.transform.localPosition = Vector3.zero; // Reset position to the villager's position
            collision.collider.enabled = false; // Disable the collider to prevent further collisions
            collision.rigidbody.useGravity = false; // Disable gravity for the collided object
        }
    }
    public void UpdateState()
    {

    }
    public void ExitState()
    {
        Debug.Log("Villager has finished working.");
        villager.completedGoToTarget -= OnWork;
        villager.collisionTrigger -= OnCollisionEnter; // Unsubscribe from collision events
        if (villager.moveCoroutine != null)
        {
            villager.StopCoroutine(villager.moveCoroutine);
            villager.moveCoroutine = null;
        }
        // Implement logic for exiting the working state
    }
}
public class VillagerAttackState
{

}