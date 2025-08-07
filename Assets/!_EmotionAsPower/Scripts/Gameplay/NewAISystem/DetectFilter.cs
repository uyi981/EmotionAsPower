using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DetectFilter
{
    [SerializeField] private List<DetectableType> targets;
    public List<DetectableType> Targets => targets;

    public DetectFilter(DetectableType[] targets)
    {
        this.targets = targets.ToList();
    }

    public DetectFilter(List<DetectableType> targets)
    {
        this.targets = targets;
    }

    /// <summary>
    /// Filters a single game object based on the target types
    /// </summary>
    public GameObject Filter(GameObject gameObject)
    {
        if (gameObject == null) return null;

        if (targets == null || targets.Count == 0)
        {
            return null;
        }

        // If "Any" is specified, return the object
        if (targets.Contains(DetectableType.Any))
            return gameObject;

        // Check for specific component types
        if (targets.Contains(DetectableType.Villager) && gameObject.GetComponent<Villager>() != null)
        {
            return gameObject;
        }

        if (targets.Contains(DetectableType.Item) && gameObject.GetComponent<Item>() != null)
        {
            return gameObject;
        }

        if (targets.Contains(DetectableType.Building) && gameObject.GetComponent<BuildingBase>() != null)
        {
            return gameObject;
        }

        if (targets.Contains(DetectableType.Resource) && gameObject.GetComponent<Resource>() != null)
        {
            return gameObject;
        }

        if(targets.Contains(DetectableType.PlayerBase) && gameObject.GetComponent<PlayerBase>() != null){
            return gameObject;
        }

        return null;
    }

    /// <summary>
    /// Filters an array of game objects based on the target types
    /// </summary>
    public GameObject[] Filter(GameObject[] objects)
    {
        List<GameObject> result = new List<GameObject>();

        foreach (GameObject obj in objects)
        {
            GameObject filteredObj = Filter(obj);
            if (filteredObj != null)
            {
                result.Add(filteredObj);
            }
        }

        return result.ToArray();
    }
}

