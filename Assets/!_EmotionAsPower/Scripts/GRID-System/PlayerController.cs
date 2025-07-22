using System.Collections.Generic;
using UnityEngine;
public class PlayerController : Singleton<PlayerController>
{
    public GameObject prefab;
    List<Villager> selectedVillagers = new List<Villager>();
    InputManagerForGrid inputManager;
    Grid grid;
    float[,] gridMap;
    APathFinding pathFinding = new APathFinding();
    public void Start()
    {
        grid = Singleton<GridSystem>.Instance.grid;
        gridMap = Singleton<GridSystem>.Instance.gridMap;
        inputManager = Singleton<InputManagerForGrid>.Instance;
        inputManager.OnRightClicked += Moving;
    }
    public void AddVillagerToList(Villager villager)
    {
        selectedVillagers.Add(villager);
    }
    public void RemoveVillagerOutOfList(Villager villager)
    {
        if (selectedVillagers.Contains(villager))
        {
            selectedVillagers.Remove(villager);
        }
        else
        {
            Debug.LogWarning("Villager not found in the selected list.");
        }
    }
    public void Moving()
    {

        if(inputManager.IsPointerOverUI())
        {
            Debug.Log("Pointer is over UI, skipping movement.");
            return;
        }
        if(inputManager.CurrentState != State.Moving)
        {
            Debug.Log("Current state is not Moving, skipping movement.");
            return;
        }
        if(selectedVillagers.Count == 0)
        {
            Debug.LogWarning("No villagers selected to move.");
            return;
        }
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        GameObject obj = Instantiate(prefab);
        obj.transform.position = gridPosition+Vector3.up;

        foreach (Villager villager in selectedVillagers)
        {
            if (villager != null)
            {
                Vector2Int targetPosition = new Vector2Int(gridPosition.x, gridPosition.z);
                Vector3Int villagerPosition = grid.WorldToCell(villager.transform.position);
                Vector2Int startPosition = new Vector2Int(villagerPosition.x, villagerPosition.z);
               
                Debug.Log(gridMap.Length);
                
                List<Vector2Int> path = pathFinding.GetPathResult(NormalizeGridPosition(startPosition,100,100), NormalizeGridPosition(targetPosition, 100, 100), gridMap, 1);
                if (path != null && path.Count > 0)
                {
                    villager.Move(path, 1f); // Assuming speed is 1f, adjust as needed
                    Debug.Log("Moving villager to: " + targetPosition);
                }
                else
                {
                    Debug.LogWarning("No valid path found for villager to move to: " + targetPosition);
                }
            }
            else
            {
                Debug.LogWarning("Villager is null, cannot move.");
            }
        }
    }
    Vector2Int NormalizeGridPosition(Vector2Int pos, int gridWidth, int gridHeight)
    {
        return new Vector2Int(pos.x + gridWidth / 2, pos.y + gridHeight / 2);
    }
}
