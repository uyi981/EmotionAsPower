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

    [SerializeField]
    private GameObject blueprintInstance;
    [SerializeField]
    private Vector2Int currentSize = Vector2Int.one;
    // Offset để chuyển toạ độ lưới (có thể âm) sang chỉ số mảng >= 0
    [SerializeField] private Vector2Int gridOffset = new Vector2Int(50, 50);
    private void Update()
    {
        if (selectedObjectIndex == -1 || !gridVisualization.activeSelf)
        {
            return; // No object selected or grid visualization is not active
        }
        Vector3 mousePostion = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePostion);

        // Tính offset để tâm vào giữa footprint
        Vector3 offset = new Vector3((currentSize.x - 1) * 0.5f, 0, (currentSize.y - 1) * 0.5f);

        // Hiển thị ô preview
        cellIndicator.transform.position = grid.CellToWorld(gridPosition) + offset;
        cellIndicator.transform.localScale = new Vector3(currentSize.x, 1, currentSize.y);

        // Hiển thị blueprint
        if (blueprintInstance != null)
        {
            blueprintInstance.transform.position = grid.CellToWorld(gridPosition) + offset;
        }
    }
    private void Start()
    {
        StopPlacement();
        grid = Singleton<GridSystem>.Instance.grid;
        gridMap = Singleton<GridSystem>.Instance.gridMap;
    }

    // Sự kiện sau khi click vào vật thể trong shop (UI)
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

        currentSize = database.buildings[selectedObjectIndex].size;
        blueprintInstance = Instantiate(database.buildings[selectedObjectIndex].blueprintPrefab);


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

        if (!CanPlace(gridPosition))
        {
            Debug.LogWarning("Area is occupied or out of bounds, cannot place structure.");
            return;
        }

        GameObject gameObject = Instantiate(database.buildings[selectedObjectIndex].buildingPrefab);
        Vector3 baseWorld = grid.CellToWorld(gridPosition);
        Vector3 offset = new Vector3((currentSize.x - 1) * 0.5f, 0, (currentSize.y - 1) * 0.5f);
        gameObject.transform.position = baseWorld + offset;

        var towerScript = gameObject.GetComponent<BuildingTower>();
        if (towerScript != null)
        {
            towerScript.selectedBuilding = database.buildings[selectedObjectIndex];
        }

        Debug.Log("Placed structure: " + database.buildings[selectedObjectIndex].buildingName + " at base cell: " + gridPosition);
        OccupyCells(gridPosition);
    }

    // Kiểm tra một footprint kích thước currentSize có thể đặt tại basePos hay không
    private bool CanPlace(Vector3Int basePos)
    {
        for (int dx = 0; dx < currentSize.x; dx++)
            for (int dz = 0; dz < currentSize.y; dz++)
            {
                int gx = basePos.x + dx + gridOffset.x;
                int gz = basePos.z + dz + gridOffset.y;

                // Ngoài biên ma trận
                if (gx < 0 || gz < 0 || gx >= gridMap.GetLength(0) || gz >= gridMap.GetLength(1))
                    return false;

                // Ô đã bị chiếm
                if (gridMap[gx, gz] != 0)
                    return false;
            }
        return true;
    }

    // Đánh dấu các ô đã chiếm
    private void OccupyCells(Vector3Int basePos)
    {

        Debug.Log($"Occupying cells for object at base position: {basePos} with size {currentSize.x}");
        Debug.Log($"Occupying cells for object at base position: {basePos} with size {currentSize.y}");
        for (int dx = 0; dx < currentSize.x; dx++)
        {
            Debug.Log($"currentSize = {currentSize}");
            for (int dz = 0; dz < currentSize.y; dz++)
            {
                int gx = basePos.x + dx;
                int gz = basePos.z + dz;
                Debug.Log($"Occupying cell at: {gx}, {gz} with value 1");
                gridMap[gx + gridOffset.x, gz + gridOffset.y] = 1;


            }
        }

    }
    public void StopPlacement()
    {
        selectedObjectIndex = -1;
        gridVisualization.SetActive(false);
        mouseIndicator.SetActive(false);
        cellIndicator.SetActive(false);
        // Reset scale về 1×1 để không làm ảnh hưởng lần đặt sau
        cellIndicator.transform.localScale = Vector3.one;
        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;
    }
}
