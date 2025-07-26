using UnityEngine;

public interface IProductionBuilding
{
    bool IsProducing { get; }
    float ProductionRate { get; }
    void StartProduction();
    void StopProduction();
    void Produce();
}
