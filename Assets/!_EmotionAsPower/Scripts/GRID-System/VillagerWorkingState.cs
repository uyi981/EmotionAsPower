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
        // Implement logic for entering the working state
    }
    public void OnWork()
    {

    }
    public void UpdateState()
    {
        // Implement logic for updating the working state
        Debug.Log("Villager is working...");
    }
    public void ExitState()
    {
        Debug.Log("Villager has finished working.");
        villager.completedGoToTarget -= OnWork;
        if (villager.moveCoroutine != null)
        {
            villager.StopCoroutine(villager.moveCoroutine);
            villager.moveCoroutine = null;
        }
        // Implement logic for exiting the working state
    }
}
