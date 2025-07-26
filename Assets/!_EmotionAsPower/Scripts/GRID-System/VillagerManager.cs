using System.Collections.Generic;

public class  VillagerManager : Singleton<VillagerManager>
{
    List<Villager> jobForWorkers = new List<Villager>();
    Stack<JobForWorker> jobForWorkerPool = new Stack<JobForWorker>();
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