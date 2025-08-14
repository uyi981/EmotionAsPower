using JetBrains.Annotations;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class  VillagerManager : Singleton<VillagerManager>,IDataPersistence
{
    public List<Villager> jobForWorkers = new List<Villager>();
    public VillagerManagerUI villagerManagerUI;
    public Villager prefab;
    Stack<JobForWorker> jobForWorkerPool = new Stack<JobForWorker>();
    public Stack<Vector2Int> bedPool = new Stack<Vector2Int>();
    public List<VillagerRuntimeData> villagersRuntime = new List<VillagerRuntimeData>();
    public TextMeshProUGUI villagerCountText;
    public TextMeshProUGUI foodConsumeText;
    public bool isUnableToSendJob;
    public int QuantityOfFoodConsumedPerDay
    {
        get { return caculateQuantityOfFoodConsumedPerDay(); }
    }
    public void SendJobInMorning()
    {

        //foreach (Villager villager in jobForWorkers)
        //{
        //    villager.isWorking = false;
        //    villager.isSleeping = false;
        //    villager.TransitionTo(villager.villagerIdleState);
        //}
        //SendJobToVillager();
    }   
    public void OnEnermySpawn()
    {
        isUnableToSendJob = true;
    }
    public int caculateQuantityOfFoodConsumedPerDay()
    {
        int quantity = 0;
        foreach (Villager villager in jobForWorkers)
        {
            quantity += Mathf.RoundToInt(villager.hungerModifier);
        }
        return quantity;
    }
    public void VillagerEating(int quantity)
    {
        List<ItemSO> foodItems = new List<ItemSO>();
        foreach (var item in Singleton<ContentManager>.Instance.itemLibrary.foods)
        {
            foodItems.Add(item);
        }
        foodItems.Sort((x, y) => y.useCases.Pairs[0].value.CompareTo(x.useCases.Pairs[0].value));
        for(int i=0;i<foodItems.Count && quantity > 0; i++)
        {
            ItemSO foodItem = foodItems[i];
            if (Singleton<ItemStorage>.Instance.StoragedItems.ContainsKey(foodItem.ID) && Singleton<ItemStorage>.Instance.StoragedItems[foodItem.ID] > 0)
            {
                int amountToConsume = Mathf.Min(quantity, Singleton<ItemStorage>.Instance.StoragedItems[foodItem.ID] * foodItem.useCases.Pairs[0].value);
                Singleton<ItemStorage>.Instance.TryTakeItem(foodItem.ID, amountToConsume);
                quantity -= amountToConsume;
                Debug.Log("Villager consumed " + amountToConsume + " of " + foodItem.name);
            }
        }
        if(quantity > 0)
        {
            Debug.LogWarning("Not enough food for villagers. Remaining quantity: " + quantity);
            
        }
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            VillagerEating(QuantityOfFoodConsumedPerDay);
        }
    }
    public void WakeUpVillagerWhenRaidCome()
    {
     
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
        Singleton<VillagerManagerUI>.Instance.UpdateVillagerSlots(jobForWorkers);
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
          VillagerEating(QuantityOfFoodConsumedPerDay);
        }
        else if (timeStage == DayTimeController.TimeStage.Evening)
        {
          //VillagersGoToSleep();
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
        Singleton<EnemyManager>.Instance.enemySpawned += OnEnermySpawn;
    }
    private void Start()
    {
        SetUp();
        villagerManagerUI.UpdateVillagerSlots(jobForWorkers);
        InvokeRepeating("SendJobToVillager", 5f,1);
        if (villagerCountText != null)
        {
            villagerCountText.text =""+ jobForWorkers.Count;
        }
        if (foodConsumeText != null)
        {
            foodConsumeText.text = "" + QuantityOfFoodConsumedPerDay;
        }
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
        if (isUnableToSendJob)
        {
            Debug.LogWarning("Unable to send job due to enermy spawn.");
            return;
        }
        foreach (Villager villager in jobForWorkers)
        {
            if (jobForWorkerPool.Count == 0)
            {
                return;
            }
            if (villager.isWorking == false)
            {
                 JobForWorker job = jobForWorkerPool.Pop();
                if(job.JobType.Equals(JobType.None))
                {
                    Debug.LogWarning("Job type is None, skipping assignment.");
                    continue;
                }
                else if (job.JobType.Equals(JobType.Build))
                {
                    if( job.buildingBase == null)
                    {
                        Debug.LogWarning("Building base is null for job type: " + job.JobType);
                        continue;
                    }
                    else if(job.buildingBase.isBuildingComplete)
                    {
                        Debug.LogWarning("Building base is null for job type: " + job.JobType);
                        continue;
                    }
                }
                villager.currentJob = job;
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