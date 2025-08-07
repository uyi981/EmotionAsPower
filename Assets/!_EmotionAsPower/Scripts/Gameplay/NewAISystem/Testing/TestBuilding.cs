using UnityEngine;

public class TestBuilding : BuildingBase, IHealth
{
    public float health = 100f;
    public override void Start()
    {
        base.Start();
        Debug.LogWarning(GetComponent<BuildingBase>()!=null);
    }
    public bool IsDead()
    {
        return health <= 0;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (IsDead()) { 
            Destroy(gameObject);
        }
    }


}
