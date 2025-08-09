using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : Singleton<BuildingManager>, IDataPersistence
{
    private List<BuildingBase> buildings = new List<BuildingBase>();
    private List<BuildingRuntimeData> buildingRuntimeData = new List<BuildingRuntimeData>();
    public void RegisterBuilding(BuildingBase building)
    {
        if (!buildings.Contains(building))
        {
            buildings.Add(building);
            BuildingRuntimeData runtimeData = new BuildingRuntimeData
            {
                buildingId = building.ID,
                currentHP = building.Health,
                position = new Vector2Int(building.BaseCell.x, building.BaseCell.z)
            };
            buildingRuntimeData.Add(runtimeData);

     
        }
    }
    public void LoadBuilding()
    {
        if (buildingRuntimeData == null || buildingRuntimeData.Count == 0)
        {
            return;
        }
        foreach (BuildingRuntimeData runtimeData in buildingRuntimeData)
        {
            Singleton<PlacementSystem>.Instance.SpawnBuildingInstant(new Vector3Int(runtimeData.position.x, 0, runtimeData.position.y), runtimeData.buildingId);
        }
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            Singleton<DataPersistenceManager>.Instance.SaveGame();
        }
        if(Input.GetKeyDown(KeyCode.L))
        {
            Singleton<DataPersistenceManager>.Instance.LoadGame();
        }
    }
    public void UnregisterBuilding(BuildingBase building)
    {
        if (buildings.Contains(building))
        {
            buildings.Remove(building);
        }
        BuildingRuntimeData runtimeData = buildingRuntimeData.Find(data => data.position == new Vector2Int(building.BaseCell.x, building.BaseCell.z));
        if (runtimeData != null)
        {
            buildingRuntimeData.Remove(runtimeData);
        }
    }

    public List<BuildingBase> GetAllBuildings()
    {
        return new List<BuildingBase>(buildings);
    }

    public void LoadGame(GameData gameData)
    {
       buildingRuntimeData = gameData.buildings ?? new List<BuildingRuntimeData>();
        LoadBuilding();
    }

    public void SaveGame(ref GameData gameData)
    {
        if(buildingRuntimeData == null)
        {
            return;
        }
        gameData.buildings = buildingRuntimeData;
    }
}
