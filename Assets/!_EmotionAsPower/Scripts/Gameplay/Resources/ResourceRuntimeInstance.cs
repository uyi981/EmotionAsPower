using System;
using UnityEngine;

[Serializable]
public class ResourceRuntimeInstance
{
    public string id;
    public Vector3 position;
    public float health;
    public float lastRegularDropTime;
    public bool isForHarvest;

    public ResourceRuntimeInstance(string id, Vector3 position, float health, float lastRegularDropTime = 0f, bool isForHarvest = false)
    {
        this.id = id;
        this.position = position;
        this.health = health;
        this.lastRegularDropTime = lastRegularDropTime;
        this.isForHarvest = isForHarvest;
    }
}