using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(ItemDropper))]
public class Enemy : MonoBehaviour
{
    [SerializeField]
    private EnemySO enemySO;
    public EnemySO EnemySO => enemySO;

    private Health health;
    private ItemDropper itemDropper;
    private SpriteRenderer spriteRenderer;
    private IEnemyBehaviour behaviourInstance;
    [SerializeField]
    private float existingTimer;
    public float ExistingTimer => existingTimer;    
    public Health Health => health;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (health == null)
        {
            health = gameObject.AddComponent<Health>();
        }

        itemDropper = GetComponent<ItemDropper>();
        if (itemDropper == null)
        {
            itemDropper = gameObject.AddComponent<ItemDropper>();
        }

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Initialize(EnemySO enemySO)
    {
        if (enemySO == null)
        {
            Debug.LogWarning("Cannot initialize Enemy with null EnemySO!");
            return;
        }

        this.enemySO = enemySO;

        // Initialize Health
        health.SetMaxHealth(enemySO.defaultData.maxHealth, true);
        health.SetHealth(enemySO.defaultData.maxHealth);

        // Initialize ItemDropper
        if (itemDropper != null && enemySO.dropableItems != null && enemySO.dropableItems.Length > 0)
        {
            itemDropper.Initialize(enemySO.dropableItems);
        }

        // Set existing time
        existingTimer = enemySO.defaultData.existingTime;

        // Set sprite
        if (spriteRenderer != null && enemySO.Icon != null)
        {
            spriteRenderer.sprite = enemySO.Icon;
        }

        // Initialize behavior
        if (enemySO.behaviour != null)
        {
            behaviourInstance = enemySO.behaviour.CreateBehaviour(this);
        }

        this.gameObject.name = "Enemy_" + enemySO.name + "_" + GetInstanceID();
    }

    public void UpdateExistingTime(float time)
    {
        this.existingTimer = time;
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDeath.AddListener(OnDeath);
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDeath.RemoveListener(OnDeath);
        }
    }

    private void Update()
    {
        if (existingTimer > 0f)
        {
            existingTimer -= Time.deltaTime;

            // If timer expires, die automatically
            if (existingTimer <= 0f)
            {
                health.Die();
                return;
            }
        }

        if (behaviourInstance != null)
        {
            behaviourInstance.Update();
        }
    }

    private void OnDeath()
    {
        if (itemDropper != null)
        {
            itemDropper.DropFunction(transform.position);
        }

        Destroy(gameObject);
    }

}