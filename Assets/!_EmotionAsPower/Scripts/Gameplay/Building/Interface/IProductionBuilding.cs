using UnityEngine;

public interface IProductionBuilding
{
    bool IsProducing { get; }
    void StartProduction();
    void StopProduction();
    void Produce();
}
