using System.Collections.Generic;
using UnityEngine;
public class PlayerController : Singleton<PlayerController>
{
    List<Villager> selectedVillagers = new List<Villager>();
    InputManagerForGrid inputManager;
    Grid grid;
    float[,] gridMap;
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
        Singleton<VillagerDetailUI>.Instance.ReceiveVillagerData(villager);
        Singleton<VillagerDetailUI>.Instance.transform.position = villager.transform.position + new Vector3(1, 1, 0); // Adjust the position of the UI to be above the villager
    }
    public void RemoveVillagerOutOfList(Villager villager)
    {
        if (selectedVillagers.Contains(villager))
        {
            selectedVillagers.Remove(villager);
            Singleton<VillagerDetailUI>.Instance.transform.position+= Vector3.up * 1000; // Move the UI far away when the villager is removed
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

        foreach (Villager villager in selectedVillagers)
        {
            if (villager != null)
            {
                Vector2Int targetPosition = new Vector2Int(gridPosition.x, gridPosition.z);
                villager.Move(targetPosition, 1f); // Assuming Move takes a Vector2Int position and speed

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
