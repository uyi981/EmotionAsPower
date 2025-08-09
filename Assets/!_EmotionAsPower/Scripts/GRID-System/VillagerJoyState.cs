using UnityEngine;

public class VillagerJoyIdle : IState
{
    Villager villager;
    public VillagerJoyIdle(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {

    }

    public void ExitState()
    {

        
    }
    public void UpdateState()
    {
        
    }
}
public class VillagerAngryIdle : IState
{
    Villager villager;
    public VillagerAngryIdle(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
        throw new System.NotImplementedException();
    }

    public void ExitState()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateState()
    {
        throw new System.NotImplementedException();
    }
}
public class VillagerSadIdle : IState
{
    Villager villager;
    public VillagerSadIdle(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
        throw new System.NotImplementedException();
    }

    public void ExitState()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateState()
    {
        throw new System.NotImplementedException();
    }
}
public class VillagerBoringIdle : IState
{
    Villager villager;
    public VillagerBoringIdle(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
        throw new System.NotImplementedException();
    }

    public void ExitState()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateState()
    {
        throw new System.NotImplementedException();
    }
}
public class VillagerFearIdle : IState
{
    Villager villager;
    public VillagerFearIdle(Villager villager)
    {
        this.villager = villager;
    }
    public void OnMeetOtherVillager(Collision other)
    {
       if(other.gameObject.CompareTag("Villager"))
       {

       }
    }
    public void EnterState()
    {
        throw new System.NotImplementedException();
    }

    public void ExitState()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateState()
    {
        throw new System.NotImplementedException();
    }
}
