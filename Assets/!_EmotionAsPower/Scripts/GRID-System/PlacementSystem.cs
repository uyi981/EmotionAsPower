using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] GameObject mouseIndicator, cellIndicator;
    [SerializeField] private InputManagerForGrid inputManager;
    [SerializeField] private Grid grid;
    public float[,] gridMap;
    [SerializeField] private ObjectsDatabaseSO database;
    [SerializeField] private int selectedObjectIndex = -1;
    [SerializeField] private GameObject gridVisualization;
    private void Update()
    {
        if (selectedObjectIndex == -1 || !gridVisualization.activeSelf)
        {
            return; // No object selected or grid visualization is not active
        }
        Vector3 mousePostion = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePostion);
        // mouseIndicator.transform.position = mousePostion;
        cellIndicator.transform.position = gridPosition;
    }
    private void Start()
    {
        StopPlacement();
        grid = Singleton<GridSystem>.Instance.grid;
        gridMap = Singleton<GridSystem>.Instance.gridMap;
    }
    public void StartPlacement(int objectIndex)
    {
        if (objectIndex < 0 || objectIndex >= database.buildings.Count)
        {
            Debug.LogError("Invalid object index: " + objectIndex);
            return;
        }
        selectedObjectIndex = database.buildings.FindIndex(b => b.buildingID == objectIndex);
        if (selectedObjectIndex == -1)
        {
            Debug.LogError("Building with ID " + objectIndex + " not found in database.");
            return;
        }
        gridVisualization.SetActive(true);
        //  gridVisualization.transform.position = grid.CellToWorld(grid.WorldToCell(inputManager.GetSelectedMapPosition()))+new Vector3(0.5f,0,-0.5f);
        mouseIndicator.SetActive(true);
        cellIndicator.SetActive(true);
        inputManager.CurrentState = State.Building;
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }
    public void PlaceStructure()
    {
        Debug.Log("Attempting to place structure: " + database.buildings[selectedObjectIndex].buildingName);
        if (inputManager.IsPointerOverUI())
        {
            Debug.Log("Pointer is over UI, cannot place structure.");
            return;
        }
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        GameObject gameObject = Instantiate(database.buildings[selectedObjectIndex].buildingPrefab, gridPosition, Quaternion.identity);
        gameObject.transform.position = grid.CellToWorld(gridPosition);
        Debug.Log("Placed structure: " + database.buildings[selectedObjectIndex].buildingName + " at position: " + gridPosition);
        gridPosition = NormalizeGridPosition(gridPosition, 100, 100); // Assuming grid size is 100x100, adjust as needed
        gridMap[gridPosition.x, gridPosition.z] = 1; // Mark the cell as occupied
    }
    public void StopPlacement()
    {
        selectedObjectIndex = -1;
        gridVisualization.SetActive(false);
        mouseIndicator.SetActive(false);
        cellIndicator.SetActive(false);
        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;
        inputManager.CurrentState = State.Moving; // Reset to idle state
<<<<<<< HEAD
=======
    }
    Vector3Int NormalizeGridPosition(Vector3Int pos, int gridWidth, int gridHeight)
    {
        return new Vector3Int(pos.x + gridWidth / 2, pos.z + gridHeight / 2);
>>>>>>> 5e794eaf6625fab593e429efe110e37de06b0650
    }
    Vector3Int NormalizeGridPosition(Vector3Int pos, int gridWidth, int gridHeight)
    {
        return new Vector3Int(pos.x + gridWidth / 2, pos.z + gridHeight / 2);
    }
}