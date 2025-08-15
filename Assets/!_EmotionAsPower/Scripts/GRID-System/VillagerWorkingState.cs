using Unity.VisualScripting;
using UnityEngine;

public class VillagerWorkingState : IState
{
    private Villager villager;
    private Coroutine moveCoroutine;
    public VillagerWorkingState(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
        Debug.Log("Villager is now working.");
        villager.Move(villager.currentJob.Position,villager.speed);
        villager.completedGoToTarget += OnWork;
        villager.collisionTrigger += OnCollisionEnter; // Subscribe to collision events
        villager.isWorking = true;
        // Implement logic for entering the working state
    }
    public void OnWork()
    {
        if (villager.currentJob.JobType.Equals(JobType.Transport))
        {
            Collider[] colliders = Physics.OverlapSphere(villager.transform.position, 1f,LayerMask.NameToLayer("Item")); // Adjust the radius as needed
            for (int i = colliders.Length - 1; i >= 0; i--)
            {
                if (colliders[i].gameObject.CompareTag("Item"))
                {
                    villager.PickupItem(colliders[i].gameObject);
                }
            }
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
          villager.PickupItem(collision.gameObject);
        }
    }
    public void UpdateState()
    {
        if(villager.currentJob.JobType.Equals(JobType.Produce))
        {
           if(villager.currentJob.buildingBase==null)
            {
                villager.TransitionTo(villager.villagerIdleState);
                return;
            }
        }
    }
    public void ExitState()
    {
        villager.isWorking = false; // Set working state to false
        if (!villager.currentJob.JobType.Equals(JobType.Build)&&!villager.currentJob.JobType.Equals(JobType.Transport))
        {
            villager.currentJob.buildingBase.OnWorkerLeave(villager);
        }   
        Debug.Log("Villager has finished working.");
        villager.completedGoToTarget -= OnWork;
        villager.collisionTrigger -= OnCollisionEnter; // Unsubscribe from collision events
        villager.ResetCoroutine(moveCoroutine); // Reset the move coroutine
        villager.currentJob.JobType = JobType.None; // Reset the job type after exiting the working state
        // Implement logic for exiting the working state
    }

}
public class VillagerAttackState
{

}