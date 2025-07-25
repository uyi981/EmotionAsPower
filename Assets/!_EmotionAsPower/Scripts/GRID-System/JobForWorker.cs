using UnityEngine;

public struct JobForWorker
{
    public Vector2Int Position;
    public JobType JobType;
    public JobForWorker(Vector2Int position, JobType jobType)
    {
        Position = position;
        JobType = jobType;
    }
}
public enum JobType
{
    None,
    Gather,
    Build,
    Repair,
    Transport
}