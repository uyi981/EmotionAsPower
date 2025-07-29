using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AIController), typeof(Health))]
public class Enemy : MonoBehaviour, IInteractable
{
    [Header("Enemy Configuration")]
    [SerializeField] private EnemySO enemySO;

    private AIController aiController;
    private Health health;
    private float existingTimer;
    private bool isInitialized = false;
    private EnemyState currentState = EnemyState.Spawning;

    public EnemySO EnemySO => enemySO;
    public Health Health => health;
    public AIController AIController => aiController;
    public float ExistingTimer => existingTimer;
    public EnemyState CurrentState => currentState;
    public bool IsInitialized => isInitialized;

    public System.Action<Enemy> OnEnemyDeath;
    public System.Action<Enemy, EnemyState> OnStateChanged;

    private void Awake()
    {

        aiController = GetComponent<AIController>();
        health = GetComponent<Health>();

        if (aiController == null || health == null)
        {
            Debug.LogError($"Enemy {gameObject.name} is missing required components!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        if (!isInitialized)
        {
            Debug.LogWarning($"Enemy {gameObject.name} was not properly initialized!");
        }
    }

    private void Update()
    {
        if (!isInitialized) return;
    }

    public void Initialize(EnemySO enemyData)
    {
        if (enemyData == null)
        {
            Debug.LogError("Cannot initialize enemy with null EnemySO!");
            return;
        }

        enemySO = enemyData;

        // Initialize health with enemy data
        if (health != null)
        {
            health.SetMaxHealth(enemyData.defaultData.maxHealth, true);
            health.SetHealth(enemyData.defaultData.maxHealth);
        }

        // Set existing timer
        existingTimer = enemyData.defaultData.existingTime;

        aiController.Initialize(enemyData.behaviour);
    }

    public void UpdateExistingTime(float newTime)
    {
        existingTimer = newTime;
    }

    public void OnInteract()
    {
        //throw new System.NotImplementedException();
    }

    public InteractableType GetInteractableType() => InteractableType.Enemy;
}



