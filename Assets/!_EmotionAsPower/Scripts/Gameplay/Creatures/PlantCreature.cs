using UnityEngine.EventSystems;
using UnityEngine;

public class PlantCreature : MonoBehaviour
{
    [Header("Creature Settings")]
    [SerializeField] private AIBehaviour peacefulBehaviour;
    [SerializeField] private AIBehaviour alertBehaviour;
    [SerializeField] private float alertRadius = 5f;

    private AIController aiController;
    private bool isAlert = false;

    private void Awake()
    {
        aiController = GetComponent<AIController>();
    }

    private void Start()
    {
        if (peacefulBehaviour != null)
        {
            aiController.SetBehavior(peacefulBehaviour);
        }
    }

    private void Update()
    {
        CheckForThreats();
    }

    private void CheckForThreats()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        bool threatNearby = false;

        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= alertRadius)
            {
                threatNearby = true;
                break;
            }
        }

        if (threatNearby && !isAlert)
        {
            isAlert = true;
            if (alertBehaviour != null)
            {
                aiController.SetBehavior(alertBehaviour);
            }
        }
        else if (!threatNearby && isAlert)
        {
            isAlert = false;
            if (peacefulBehaviour != null)
            {
                aiController.SetBehavior(peacefulBehaviour);
            }
        }
    }
}
