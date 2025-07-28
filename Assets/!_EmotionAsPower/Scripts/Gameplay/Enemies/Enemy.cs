using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AIController), typeof(Health), typeof(UnitMover))]
public class Enemy : MonoBehaviour, IInteractable
{
    [Header("Enemy Configuration")]
    [SerializeField] private EnemySO enemySO;

    [Header("AI Behaviours")]
    [SerializeField] private AIBehaviour defaultBehaviour;
    [SerializeField] private AIBehaviour combatBehaviour;
    [SerializeField] private AIBehaviour fleeingBehaviour;
    [SerializeField] private AIBehaviour lowHealthBehaviour;

    [Header("Behavior Switching Settings")]
    [SerializeField] private float combatHealthThreshold = 0.8f;
    [SerializeField] private float fleeHealthThreshold = 0.3f;
    [SerializeField] private float threatDetectionRange = 8f;
    [SerializeField] private LayerMask threatLayers = -1;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    private AIController aiController;
    private Health health;
    private UnitMover unitMover;

    private float existingTimer;
    private bool isInitialized = false;
    private EnemyState currentState = EnemyState.Spawning;

    public EnemySO EnemySO => enemySO;
    public Health Health => health;
    public UnitMover UnitMover => unitMover;
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
        unitMover = GetComponent<UnitMover>();

        if (aiController == null || health == null || unitMover == null)
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

        SetupEventListeners();
        StartBehaviorLoop();
    }

    private void Update()
    {
        if (!isInitialized) return;

        UpdateExistingTimer();
        UpdateBehaviorBasedOnState();
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

        // Set initial behavior
        if (defaultBehaviour != null)
        {
            aiController.SetBehavior(defaultBehaviour);
        }

        isInitialized = true;
        ChangeState(EnemyState.Active);

        if (debugMode)
            Debug.Log($"Enemy {gameObject.name} initialized with {enemyData.name}");
    }

    public void UpdateExistingTime(float newTime)
    {
        existingTimer = newTime;
    }

    private void SetupEventListeners()
    {
        if (health != null)
        {
            health.OnDamageTaken.AddListener(OnDamageTaken);
            health.OnDeath.AddListener(OnDeath);
            health.OnHealthChanged.AddListener(OnHealthChanged);
        }

        if (aiController != null)
        {
            aiController.OnActionStarted += OnActionStarted;
            aiController.OnActionEnded += OnActionEnded;
        }
    }

    private void UpdateExistingTimer()
    {
        if (existingTimer > 0)
        {
            existingTimer -= Time.deltaTime;

            if (existingTimer <= 0)
            {
                ChangeState(EnemyState.Dying);
                StartCoroutine(DestroyAfterDelay(0.1f));
            }
        }
    }

    private void UpdateBehaviorBasedOnState()
    {
        switch (currentState)
        {
            case EnemyState.Active:
                HandleActiveState();
                break;

            case EnemyState.Combat:
                HandleCombatState();
                break;

            case EnemyState.Fleeing:
                HandleFleeingState();
                break;

            case EnemyState.LowHealth:
                HandleLowHealthState();
                break;
        }
    }

    private void HandleActiveState()
    {
        // Check for enter combat
        if (IsThreatsNearby() && combatBehaviour != null)
        {
            ChangeState(EnemyState.Combat);
            return;
        }

        // Check health-based state changes
        float healthPercentage = health.HealthPercentage;

        if (healthPercentage <= fleeHealthThreshold && fleeingBehaviour != null)
        {
            ChangeState(EnemyState.Fleeing);
        }
        else if (healthPercentage <= combatHealthThreshold && lowHealthBehaviour != null)
        {
            ChangeState(EnemyState.LowHealth);
        }
    }

    private void HandleCombatState()
    {
        // Check if should flee
        if (health.HealthPercentage <= fleeHealthThreshold && fleeingBehaviour != null)
        {
            ChangeState(EnemyState.Fleeing);
            return;
        }

        // Return to active state if no threats
        if (!IsThreatsNearby())
        {
            ChangeState(EnemyState.Active);
        }
    }

    private void HandleFleeingState()
    {
        // Only return to other states if health has recovered
        if (health.HealthPercentage > combatHealthThreshold)
        {
            if (IsThreatsNearby() && combatBehaviour != null)
            {
                ChangeState(EnemyState.Combat);
            }
            else
            {
                ChangeState(EnemyState.Active);
            }
        }
    }

    private void HandleLowHealthState()
    {
        if (health.HealthPercentage <= fleeHealthThreshold && fleeingBehaviour != null)
        {
            ChangeState(EnemyState.Fleeing);
        }
        else if (health.HealthPercentage > combatHealthThreshold)
        {
            ChangeState(EnemyState.Active);
        }
    }

    private bool IsThreatsNearby()
    {
        Collider[] threats = Physics.OverlapSphere(transform.position, threatDetectionRange, threatLayers);

        foreach (var threat in threats)
        {
            // Check for villagers, buildings, or others
            if (threat.GetComponent<Villager>() != null ||
                threat.GetComponent<BuildingBase>() != null ||
                threat.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        EnemyState oldState = currentState;
        currentState = newState;

        AIBehaviour targetBehavior = GetBehaviorForState(newState);
        if (targetBehavior != null && aiController.CurrentBehaviour != targetBehavior)
        {
            aiController.SetBehavior(targetBehavior);
        }

        OnStateChanged?.Invoke(this, newState);

        if (debugMode)
            Debug.Log($"Enemy {gameObject.name} changed state from {oldState} to {newState}");
    }

    private AIBehaviour GetBehaviorForState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Active:
                return defaultBehaviour;
            case EnemyState.Combat:
                return combatBehaviour ?? defaultBehaviour;
            case EnemyState.Fleeing:
                return fleeingBehaviour ?? defaultBehaviour;
            case EnemyState.LowHealth:
                return lowHealthBehaviour ?? defaultBehaviour;
            default:
                return defaultBehaviour;
        }
    }

    private void StartBehaviorLoop()
    {
        if (isInitialized && defaultBehaviour != null)
        {
            aiController.SetBehavior(defaultBehaviour);
        }
    }

    private void OnDamageTaken(float damage)
    {
        if (debugMode)
            Debug.Log($"Enemy {gameObject.name} took {damage} damage");
    }

    private void OnHealthChanged(float newHealth)
    {
        // Handled in UpdateBehaviorBasedOnState
    }

    private void OnDeath()
    {
        ChangeState(EnemyState.Dying);

        // Stop AI
        if (aiController != null)
        {
            aiController.ForceStopCurrentAction();
            aiController.enabled = false;
        }

        // Stop movement
        if (unitMover != null)
        {
            unitMover.StopMovement();
        }

        OnEnemyDeath?.Invoke(this);

        // Apply death effects here
        StartCoroutine(DestroyAfterDelay(2f));
    }

    private void OnActionStarted(AIAction action)
    {
        if (debugMode)
            Debug.Log($"Enemy {gameObject.name} started action: {action.actionName}");
    }

    private void OnActionEnded(AIAction action, ActionResult result)
    {
        if (debugMode)
            Debug.Log($"Enemy {gameObject.name} ended action: {action.actionName} with result: {result}");
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(gameObject);
    }

    public void ForceState(EnemyState newState)
    {
        ChangeState(newState);
    }

    public void AddExistingTime(float additionalTime)
    {
        existingTimer += additionalTime;
    }

    public void SetExistingTime(float newTime)
    {
        existingTimer = newTime;
    }

    private void OnDrawGizmosSelected()
    {
        if (!debugMode) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, threatDetectionRange);

        Gizmos.color = GetStateColor();
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
    }

    private Color GetStateColor()
    {
        switch (currentState)
        {
            case EnemyState.Spawning: return Color.white;
            case EnemyState.Active: return Color.green;
            case EnemyState.Combat: return Color.red;
            case EnemyState.Fleeing: return Color.yellow;
            case EnemyState.LowHealth: return Color.orange;
            case EnemyState.Dying: return Color.black;
            default: return Color.gray;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamageTaken.RemoveListener(OnDamageTaken);
            health.OnDeath.RemoveListener(OnDeath);
            health.OnHealthChanged.RemoveListener(OnHealthChanged);
        }

        if (aiController != null)
        {
            aiController.OnActionStarted -= OnActionStarted;
            aiController.OnActionEnded -= OnActionEnded;
        }
    }

    public void OnInteract()
    {
        //throw new System.NotImplementedException();
    }

    public InteractableType GetInteractableType()=> InteractableType.Enemy;
}



