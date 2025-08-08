using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(NewAIController), typeof(Health),  typeof(ItemDropper))]
public class Enemy : MonoBehaviour, IInteractable
{
    [Header("Enemy Configuration")]
    [SerializeField] private EnemySO enemySO;

    private NewAIController aiController;
    private Health health;
    private ItemDropper itemDropper;
    [SerializeField]
    private float existingTimer;
    [SerializeField]
    private bool isInitialized = false;
    private EnemyState currentState = EnemyState.Spawning;

    public EnemySO EnemySO => enemySO;
    public Health Health => health;
    public ItemDropper ItemDropper => itemDropper;
    public NewAIController AIController => aiController;
    public float ExistingTimer => existingTimer;
    public EnemyState CurrentState => currentState;
    public bool IsInitialized => isInitialized;

    public System.Action<Enemy> OnEnemyDeath;
    public System.Action<Enemy, EnemyState> OnStateChanged;

    private void Awake()
    {

        aiController = GetComponent<NewAIController>();
        health = GetComponent<Health>();
        itemDropper = GetComponent<ItemDropper>();

        if (aiController == null || health == null ||  itemDropper == null)
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
        UpdateLife();
    }

    public void Initialize(EnemySO enemyData)
    {
        if (enemyData == null)
        {
            Debug.LogError("Cannot initialize enemy with null EnemySO!");
            return;
        }

        enemySO = enemyData;
        itemDropper.Initialize(enemySO.dropableItems);

        // Initialize visual
        //GetComponentInChildren<SpriteRenderer>().sprite = enemySO.Icon;

        // Initialize health with enemy data
        if (health != null)
        {
            health.SetMaxHealth(enemyData.defaultData.maxHealth, true);
            health.SetHealth(enemyData.defaultData.maxHealth);
            health.OnDeath.AddListener(Die);
        }

        // Set existing timer
        existingTimer = enemyData.defaultData.existingTime;

        aiController.Initialize(enemyData);

        isInitialized = true;
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

    public void UpdateLife()
    {
        existingTimer -= Time.deltaTime;
        if (existingTimer <= 0)
        {
            Health.Die();
        }
    }

    public void Die()
    {
        ItemDropper.DropItems();
        Destroy(gameObject);
    }
}



