using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    [SerializeField] private float radius = 10f;
    public float Radius => radius;

    [SerializeField] private DetectFilter filter;
    [SerializeField] private int matchedCount = 0;

    /// <summary>
    /// Detects all colliders within the detection radius
    /// </summary>
    public GameObject[] Detect()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        GameObject[] result = new GameObject[colliders.Length];

        for (int i = 0; i < colliders.Length; i++)
        {
            result[i] = colliders[i].gameObject;
        }

        return result;
    }

    /// <summary>
    /// Detects all game objects with a specific component type within the detection radius
    /// </summary>
    public GameObject[] Detect<T>() where T : MonoBehaviour
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        List<GameObject> result = new List<GameObject>();

        foreach (Collider collider in colliders)
        {
            T component = collider.gameObject.GetComponent<T>();
            if (component != null && !result.Contains(collider.gameObject))
            {
                result.Add(collider.gameObject);
            }
        }

        return result.ToArray();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void Update()
    {
        if (filter != null)
        {
            matchedCount = filter.Filter(Detect()).Length;
        }
    }
}
