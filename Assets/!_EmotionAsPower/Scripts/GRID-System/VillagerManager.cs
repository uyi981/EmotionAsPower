using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class  VillagerManager : Singleton<VillagerManager>
{
    public List<Villager> jobForWorkers = new List<Villager>();
    Stack<JobForWorker> jobForWorkerPool = new Stack<JobForWorker>();
    public Stack<Vector2Int> bedPool = new Stack<Vector2Int>();
    public void SendJobInMorning()
    {

        foreach (Villager villager in jobForWorkers)
        {
            villager.isWorking = false;
            villager.isSleeping = false;
            villager.TransitionTo(villager.villagerIdleState);
        }
        SendJobToVillager();
    }   
    public Vector2Int GetBed()
    {
        if (bedPool.Count > 0)
        {
            Debug.Log(bedPool.Count + " beds available in pool.");
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
        foreach(Transform villager in transform)
        {
            Villager villagerComponent = villager.gameObject.GetComponent<Villager>();
            if (villagerComponent != null)
            {
                jobForWorkers.Add(villagerComponent);
            }
        }
    }
    private void Start()
    {
        SetUp();
        InvokeRepeating("SendJobToVillager", 5f,1);
    }
    public void VillagersGoToSleep()
    {
        int i = 0;
        foreach (Villager villager in jobForWorkers)
        {
            Debug.Log(i++);
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
                //JobForWorker job = jobForWorkerPool.Peek();
                //if(villager.currentEmotion.Equals((ProductionBuilding)job.buildingBase))
                //{
                   
                //}

                villager.currentJob = jobForWorkerPool.Pop();
                villager.isWorking = true;
                Debug.Log("Assigning job to villager: " + villager.name + " with job type: " + villager.currentJob.JobType);
                villager.TransitionTo(villager.villagerWorkingState);
                return;
            }
        }
    }
    public void SendJobRequestToManager(JobForWorker newJob)
    {
        Debug.Log("Received job request for type: " + newJob.JobType+newJob.Position);
        jobForWorkerPool.Push(newJob);
        SendJobToVillager();
    }
}
public class Bed :Building
{
  public bool isOccupied = false;
}