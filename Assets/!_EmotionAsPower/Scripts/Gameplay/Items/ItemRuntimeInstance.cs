using UnityEngine;
using System;

[Serializable]
public class ItemRuntimeInstance
{
    public string id;
    public int amount;
    public Vector3 position;

    public ItemRuntimeInstance(string id, int amount, Vector3 position)
    {
        this.id = id;
        this.amount = amount;
        this.position = position;
    }
}