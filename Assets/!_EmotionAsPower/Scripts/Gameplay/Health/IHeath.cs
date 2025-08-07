using UnityEngine;

public interface IHealth
{
    void TakeDamage(float damage);

    bool IsDead();
}
