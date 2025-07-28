using System.Collections.Generic;
using UnityEngine;

public static class TargetFinder
{
    public static T FindNearestTarget<T>(Vector3 position, float maxRange = float.MaxValue) where T : MonoBehaviour, IInteractable
    {
        T[] targets = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        T nearest = null;
        float minDistance = maxRange;

        foreach (var target in targets)
        {
            if (target == null) continue;

            float distance = Vector3.Distance(position, target.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = target;
            }
        }

        return nearest;
    }

    public static List<T> FindTargetsInRange<T>(Vector3 position, float range) where T : MonoBehaviour, IInteractable
    {
        T[] allTargets = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        List<T> targetsInRange = new List<T>();

        foreach (var target in allTargets)
        {
            if (target == null) continue;

            float distance = Vector3.Distance(position, target.transform.position);
            if (distance <= range)
            {
                targetsInRange.Add(target);
            }
        }

        return targetsInRange;
    }
}