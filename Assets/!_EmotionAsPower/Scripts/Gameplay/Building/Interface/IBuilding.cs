using UnityEngine;

public interface IBuilding
{
    string Name { get; }
    int Health { get; }
    int MaxHealth { get; }
    bool IsDestroyed { get; }
    bool IsBuild { get; }

    void TakeDamage(int damage); 
    void Heal(int amount);
    void UpdateBuilding();
    void OnBuildingDestroyed();
    void OnBuildingComplete();
}
