using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(NewAIController), typeof(Health),  typeof(ItemDropper))]
public class Enemy : MonoBehaviour, IInteractable
{
    [Header("Enemy Configuration")]
    public EnemyDefaultData enemyDefaultData;
    private NewAIController aiController;
    private Health health;
    private ItemDropper itemDropper;
    [SerializeField]
    private float existingTimer;
    [SerializeField]
    private bool isInitialized = false;
    private EnemyState currentState = EnemyState.Spawning;
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

    public void Initialize()
    {
        if (enemyDefaultData == null)
        {
            Debug.LogError("Cannot initialize enemy with null EnemySO!");
            return;
        }

        // Initialize visual
        //GetComponentInChildren<SpriteRenderer>().sprite = enemySO.Icon;

        // Initialize health with enemy data
        if (health != null)
        {
            health.SetMaxHealth(enemyDefaultData.maxHealth, true);
            health.SetHealth(enemyDefaultData.maxHealth);
            health.OnDeath.AddListener(Die);
        }

        // Set existing timer
        existingTimer = enemyDefaultData.existingTime;

        aiController.Initialize();

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



