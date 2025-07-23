using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private EnemySO enemySO;
    public EnemySO EnemySO => enemySO;

    private Health health;
    private IEnemyBehaviour behaviourInstance;

    public Health Health => health;

    private void Awake()
    {
        // Ensure Health component exists
        health = GetComponent<Health>();
        if (health == null)
        {
            health = gameObject.AddComponent<Health>();
        }
    }

    public void Initialize(EnemySO enemySO)
    {
        if (enemySO == null)
        {
            Debug.LogWarning("Cannot initialize Enemy with null EnemySO!");
            return;
        }

        this.enemySO = enemySO;

        health.SetMaxHealth(enemySO.defaultData.maxHealth, true);
        health.SetHealth(enemySO.defaultData.maxHealth);

        // Initialize behavior if present
        if (enemySO.behaviour != null)
        {
            behaviourInstance = enemySO.behaviour.CreateBehaviour(this);
        }

        this.gameObject.name = "Enemy_" + GetInstanceID();
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
        if (behaviourInstance != null)
        {
            behaviourInstance.Update();
        }
    }

    private void OnDeath()
    {
        Destroy(gameObject);
    }
}