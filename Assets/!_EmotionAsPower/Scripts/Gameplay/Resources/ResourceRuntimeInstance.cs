using System;
using UnityEngine;

[Serializable]
public class ResourceRuntimeInstance
{
    public string id;
    public Vector3 position;
    public float health;

    public ResourceRuntimeInstance(string id, Vector3 position, float health)
    {
        this.id = id;
        this.position = position;
        this.health = health;
    }
}