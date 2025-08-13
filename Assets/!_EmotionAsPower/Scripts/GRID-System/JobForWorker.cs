using NUnit.Framework;
using UnityEngine;

[System.Serializable]
public struct JobForWorker
{
    public Vector2Int Position;
    public JobType JobType;
    public BuildingBase buildingBase; // Optional: reference to a building if the job is related to it
    public JobForWorker(Vector2Int position, JobType jobType, BuildingBase buildingBase)
    {
        Position = position;
        JobType = jobType;
        this.buildingBase = buildingBase; // Initialize the building reference
    }
}
public enum JobType
{
    None,
    Gather,
    Build,
    Repair,
    Transport,
    Produce,
    Sleep,
    Defend, // Added for combat-related jobs
}
