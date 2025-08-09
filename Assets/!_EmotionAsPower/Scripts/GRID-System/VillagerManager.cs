using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class  VillagerManager : Singleton<VillagerManager>,IDataPersistence
{
    public List<Villager> jobForWorkers = new List<Villager>();
    public Villager prefab;
    Stack<JobForWorker> jobForWorkerPool = new Stack<JobForWorker>();
    public Stack<Vector2Int> bedPool = new Stack<Vector2Int>();
    public List<VillagerRuntimeData> villagersRuntime = new List<VillagerRuntimeData>();
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
    public void AssginNewVillager(Villager villager)
    {
        if (villager == null)
        {
            Debug.LogWarning("Attempted to assign a null villager.");
            return;
        }
        villager.transform.SetParent(transform);
        jobForWorkers.Add(villager);
        villagersRuntime.Add(new VillagerRuntimeData()
        {
            name = villager.name,
            position = villager.transform.position,
            personalityName = villager.personality.name,
            id = villager.villagerId
        });
    }
    public void LoadingAllVillagers()
    {
        foreach(var villager in villagersRuntime)
        {
            Villager newVillager = Instantiate(prefab, villager.position, Quaternion.identity);
            newVillager.name = villager.name;
            newVillager.villagerId = villager.id;
            newVillager.personality = Singleton<PersonalitySystem>.Instance.GetPersonality(villager.personalityName);
            if(newVillager.personality == null)
            {
                newVillager.personality = Singleton<PersonalitySystem>.Instance.Breeding();
            }
            newVillager.transform.SetParent(transform);
        }    
    }    
    public void RemoveVillager(Villager villager)
    {
        if (jobForWorkers.Contains(villager))
        {
            jobForWorkers.Remove(villager);
            Debug.Log("Removed villager: " + villager.name);
        }
        VillagerRuntimeData villagerData = villagersRuntime.Find(v => v.id == villager.villagerId);
        if (villagerData != null)
        {
            villagersRuntime.Remove(villagerData);
            Debug.Log("Removed villager runtime data for: " + villager.name);
        }
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
                villager.currentJob = jobForWorkerPool.Pop();
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

    public void LoadGame(GameData gameData)
    {
      if(gameData.villagers!=null)
        {
            villagersRuntime = gameData.villagers;
        }
      LoadingAllVillagers();
    }
    public void UpdateVillagerPosition()
    {
        foreach (Villager villager in jobForWorkers)
        {
            if (villager != null)
            {
                VillagerRuntimeData villagerData = villagersRuntime.Find(v => v.id == villager.villagerId);
                if (villagerData != null)
                {
                    villagerData.position = villager.transform.position;
                }
            }
        }
    }
    public void SaveGame(ref GameData gameData)
    {
       if(villagersRuntime == null)
        {
            return;
        }
        UpdateVillagerPosition();
        Debug.Log("Saving villagers runtime data, count: " + villagersRuntime.Count);
        gameData.villagers = villagersRuntime;
    }
}
public class Bed :Building
{
  public bool isOccupied = false;
}