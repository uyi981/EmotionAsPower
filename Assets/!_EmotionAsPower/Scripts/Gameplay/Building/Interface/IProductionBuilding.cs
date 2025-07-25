using UnityEngine;

public interface IProductionBuilding : IBuilding
{
    bool IsProducing { get; }
    float ProductionRate { get; }
    void StartProduction();
    void StopProduction();
    void Produce();
}
