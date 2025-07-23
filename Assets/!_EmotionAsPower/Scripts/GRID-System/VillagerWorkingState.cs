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
        // Implement logic for entering the working state
    }
    public void UpdateState()
    {
        // Implement logic for updating the working state
        Debug.Log("Villager is working...");
    }
    public void ExitState()
    {
        Debug.Log("Villager has finished working.");
        // Implement logic for exiting the working state
    }
}
