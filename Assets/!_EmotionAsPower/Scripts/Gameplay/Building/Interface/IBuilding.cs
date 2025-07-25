using UnityEngine;

public interface IBuilding
{
    string Name { get; }
    int Health { get; }
    int MaxHealth { get; }
    bool IsDestroyed { get; }
    
    void TakeDamage(int damage); 
    void Heal(int amount);
    void UpdateBuilding();
    void OnBuildingDestroyed();
}
