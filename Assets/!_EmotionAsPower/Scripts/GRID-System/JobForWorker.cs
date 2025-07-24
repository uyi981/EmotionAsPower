using UnityEngine;

public struct JobForWorker
{
    public Vector2Int Position;
    public string JobType;
    public JobForWorker(Vector2Int position, string jobType)
    {
        Position = position;
        JobType = jobType;
    }
}