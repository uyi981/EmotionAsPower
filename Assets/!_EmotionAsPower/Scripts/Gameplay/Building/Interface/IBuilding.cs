using System.Collections;
using UnityEngine;

public interface IBuilding
{
    int ID { get; set; }
    string Name { get; }
    int Health { get; }
    int MaxHealth { get; }
    bool IsDestroyed { get; }
    bool IsBuild { get; }

    IEnumerator Building();


    void TakeDamage(int damage); 
    void RepairBuilding(int amount);
    void MoveBuilding();
    void OnBuildingDestroyed();
    void OnBuildingComplete();
}
