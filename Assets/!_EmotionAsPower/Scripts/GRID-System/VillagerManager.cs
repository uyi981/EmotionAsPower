using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class  VillagerManager : Singleton<VillagerManager>
{
    public List<Villager> jobForWorkers = new List<Villager>();
    Stack<JobForWorker> jobForWorkerPool = new Stack<JobForWorker>();
    Stack<Vector2Int> bedPool = new Stack<Vector2Int>();
    public void SendJobInMorning()
    {
        SendJobToVillager();
    }   
    public Vector2Int GetBed()
    {
        if (bedPool.Count > 0)
        {
            Vector2Int bed = bedPool.Pop();
            if (bed != null)
            {
                return bed;
            }
        }
        return Vector2Int.zero; // Return a default value if no bed is available
    }    

    public void OnDayStageChange(DayTimeController.TimeStage timeStage)
    {
        if(timeStage == DayTimeController.TimeStage.Morning)
        {
            SendJobInMorning();
        }
        else if (timeStage == DayTimeController.TimeStage.Evening)
        {
            VillagersGoToSleep();
        }
    }
    public void AssignFreeBed(Vector2Int bed)
    {
        Debug.Log("Assigning free bed at position: " + bed);
        bedPool.Push(bed);
    }
    public void SetUp()
    {
        Singleton<DayTimeController>.Instance.OnTimeStageChanged += OnDayStageChange;
    }
    private void Start()
    {
        SetUp();
    }
    public void VillagersGoToSleep()
    {
        foreach (Villager villager in jobForWorkers)
        {
                villager.isWorking = false;
                villager.TransitionTo(villager.villagerSleepState);
        }
    }    
    public void SendJobToVillager()
    {
        foreach (Villager villager in jobForWorkers)
        {
            if (jobForWorkerPool.Count == 0)
            {
                return;
            }
            if (villager.isWorking == false)
            {
                villager.currentJob = jobForWorkerPool.Pop();
                villager.isWorking = true;
                villager.TransitionTo(villager.villagerWorkingState);
                return;
            }
        }
    }
    public void SendJobRequestToManager(JobForWorker newJob)
    {
        jobForWorkerPool.Push(newJob);
    }
}
public class Bed :Building
{
  public bool isOccupied = false;
}