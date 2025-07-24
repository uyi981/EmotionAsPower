using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class JobDistributionSystem : Singleton<JobDistributionSystem>
{
    Stack<JobForWorker> jobForWorkers;
    public List<Villager> villagers;
    public void ReceiveNewJob(JobForWorker job)
    {
        jobForWorkers.Push(job);
    }
    public void SendJobToWorker()
    {
      if(jobForWorkers.Count > 0 && villagers.Count > 0)
      {
         List<Villager> villagerNoJob =villagers.FindAll(v => !v.isWorking);
         foreach(Villager villager in villagerNoJob)
         {
           villager.isWorking = true;
           villager.currentJob = jobForWorkers.Pop();
         }
        } 
    }

}
