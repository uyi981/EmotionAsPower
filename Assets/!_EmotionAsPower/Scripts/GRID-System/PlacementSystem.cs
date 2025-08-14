using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static MapData;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] GameObject mouseIndicator, cellIndicator;
    [SerializeField] private InputManagerForGrid inputManager;
    [SerializeField] private Grid grid;
    public float[,] gridMap = new float[100,100];
    [SerializeField] public ObjectsDatabaseSO database;
    [SerializeField] public int selectedObjectIndex = -1;
    [SerializeField] private GameObject gridVisualization;
    public MapData mapData; // Dữ liệu bản đồ để lưu trữ các đối tượng đã đặt
    [SerializeField] private GameObject blueprintInstance;
    [SerializeField] private GameObject prefabInstance;
    [SerializeField]
    private Vector2Int currentSize = Vector2Int.one;
    [Tooltip("Offset để chuyển toạ độ lưới (có thể âm) sang chỉ số mảng >= 0")]
    [SerializeField] private Vector2Int gridOffset = new Vector2Int(50, 50);
    [SerializeField] private SelectedFrame selectedFrame;

    private Vector3Int baseCell;
    private Quaternion rotationBuilding;
    public SpriteRenderer cellIndicatorRenderer;
    private GameObject movingBuilding; // Building đang được di chuyển
    public Vector2Int CurrentSize => currentSize;
    private void Update()
    {
        if (selectedObjectIndex == -1 || !gridVisualization.activeSelf)
        {
            return; // No object selected or grid visualization is not active
        }
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int cellUnderMouse = grid.WorldToCell(mousePosition);

        // Base cell (bottom-left of footprint) so that the footprint is centred on the cursor
        Vector3Int baseCell = new Vector3Int(
            cellUnderMouse.x - currentSize.x / 2,
            cellUnderMouse.y,
            cellUnderMouse.z - currentSize.y / 2);

        // Offset from baseCell to the footprint centre
        Vector3 offset = new Vector3((currentSize.x - 1) * 0.5f, 0f, (currentSize.y - 1) * 0.5f);

        Vector3 worldPos = grid.CellToWorld(baseCell) + offset;

        // Preview cell indicator
        cellIndicator.transform.position = worldPos;
        prefabInstance = database.buildings[selectedObjectIndex].buildingPrefab;
        // Blueprint preview
        if (blueprintInstance != null)
        {
            blueprintInstance.transform.position = worldPos;
            if (Input.GetMouseButtonDown(1))
            {
                if (prefabInstance.GetComponent<BuildingBase>().BuildingType == BuildingType.Defense)
                {
                    rotationBuilding = Quaternion.Euler(0, rotationBuilding.eulerAngles.y + 90, 0);
                    blueprintInstance.transform.rotation = rotationBuilding;
                    Debug.Log("Rotating building by 90 degrees for defense type building.");
                }
            }
        }
        if (Singleton<InputManagerForGrid>.Instance.CurrentState.Equals(State.Building))
        {
            if (!CanPlace(baseCell))
            {
                cellIndicatorRenderer.color = Color.red; // Set to red if cannot place
            }
            else
            {
                cellIndicatorRenderer.color = Color.green; // Set to green if can place
            }
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
        rotationBuilding = Quaternion.Euler(0, 0, 0);

        currentSize = database.buildings[selectedObjectIndex].size;
        if (blueprintInstance != null)
            Destroy(blueprintInstance.gameObject);
        //blueprintInstance = Instantiate(database.buildings[selectedObjectIndex].blueprintPrefab);

        selectedFrame.SetSize(currentSize);
        gridVisualization.SetActive(true);
        mouseIndicator.SetActive(true);
        cellIndicator.SetActive(true);
        inputManager.CurrentState = State.Building;
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;

        cellIndicatorRenderer = cellIndicator.GetComponentInChildren<SpriteRenderer>();
    }

    // Bắt đầu di chuyển một building đã đặt
    public void StartMovingBuilding(GameObject building)
    {
        if (building == null)
        {
            Debug.LogError("No building provided for moving.");
            return;
        }

        var buildingComponent = building.GetComponent<BuildingBase>();
        if (buildingComponent == null)
        {
            Debug.LogError("Provided building does not have a BuildingBase component.");
            return;
        }

        selectedObjectIndex = database.buildings.FindIndex(b => b.buildingID == buildingComponent.ID);
        if (selectedObjectIndex == -1)
        {
            Debug.LogError("Building with ID " + buildingComponent.ID + " not found in database.");
            return;
        }

        movingBuilding = building;
        movingBuilding.SetActive(false); 
        currentSize = database.buildings[selectedObjectIndex].size;
        rotationBuilding = building.transform.rotation;

        // Giải phóng các ô hiện tại của building trên gridMap
        OccupyCells(buildingComponent.BaseCell, 0); // Đặt các ô về 0 (trống)

        // Hiển thị blueprint preview
        if (blueprintInstance != null)
            Destroy(blueprintInstance.gameObject);
        blueprintInstance = Instantiate(database.buildings[selectedObjectIndex].blueprintPrefab);   
        blueprintInstance.transform.rotation = rotationBuilding;

        selectedFrame.SetSize(currentSize);
        gridVisualization.SetActive(true);
        mouseIndicator.SetActive(true);
        cellIndicator.SetActive(true);
        inputManager.CurrentState = State.Building;
        inputManager.OnClicked += MoveBuilding;
        inputManager.OnExit += StopPlacement;

        cellIndicatorRenderer = cellIndicator.GetComponentInChildren<SpriteRenderer>();
        Debug.Log("Started moving building: " + database.buildings[selectedObjectIndex].buildingName);
    }
    public GameObject SpawnBuildingInstant(Vector3Int gridPos, int buildingID, Quaternion? rotation = null)
    {
        int index = database.buildings.FindIndex(b => b.buildingID == buildingID);
        if (index == -1)
        {
            Debug.LogError("Building with ID " + buildingID + " not found in database.");
            return null;
        }

        Vector2Int size = database.buildings[index].size;
        Vector3Int baseCell = new Vector3Int(
            gridPos.x - size.x / 2,
            gridPos.y,
            gridPos.z - size.y / 2);

        currentSize = size;
        if (!CanPlace(baseCell))
        {
            Debug.LogWarning("Cannot spawn building at " + baseCell);
            return null;
        }

        GameObject go = Instantiate(database.buildings[index].buildingPrefab);
        Vector3 offset = new Vector3((size.x - 1) * 0.5f, 0f, (size.y - 1) * 0.5f);
        go.transform.position = grid.CellToWorld(baseCell) + offset;
        go.transform.rotation = rotation ?? Quaternion.identity;

        var building = go.GetComponent<BuildingBase>();
        if (building != null)
        {
            building.ID = database.buildings[index].buildingID;
            building.selectedBuilding = database.buildings[index];
            building.workerPositions = MarkWorkerSpots(baseCell, index);
            building.BaseCell = baseCell;
            building.isBuild = true; // đánh dấu đã xây xong
            building.buildProgress = 1f;

            // Ẩn thanh tiến độ và hiện thanh máu
            if (building.buildingBar != null)
                building.buildingBar.SetActive(false);
            if (building.healthBar != null)
                building.healthBar.SetActive(true);
        }
        OccupyCells(baseCell, 1);
        Debug.Log($"Spawned building instantly: {database.buildings[index].buildingName} at {baseCell}");
        return go;
    }



    // Di chuyển building đến vị trí mới
    private void MoveBuilding()
    {
        //if (inputManager.IsPointerOverUI())
        //{
        //    Debug.Log("Pointer is over UI, cannot move structure.");
        //    return;
        //}

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int cellUnderMouse = grid.WorldToCell(mousePosition);
        baseCell = new Vector3Int(
            cellUnderMouse.x - currentSize.x / 2,
            cellUnderMouse.y,
            cellUnderMouse.z - currentSize.y / 2);

        if (!CanPlace(baseCell))
        {
            Debug.LogWarning("Area is occupied or out of bounds, cannot move structure.");
            return;
        }

        List<Vector2Int> workerPositions = MarkWorkerSpots(baseCell,selectedObjectIndex);

        // Cập nhật vị trí và thông tin của building
        Vector3 offset = new Vector3((currentSize.x - 1) * 0.5f, 0f, (currentSize.y - 1) * 0.5f);
        movingBuilding.transform.position = grid.CellToWorld(baseCell) + offset;
        movingBuilding.transform.rotation = rotationBuilding;
        movingBuilding.SetActive(true);

        var building = movingBuilding.GetComponent<BuildingBase>();
        if (building != null)
        {
            building.BaseCell = baseCell;
            building.workerPositions = workerPositions;
        }

        OccupyCells(baseCell, 1);
        Debug.Log("Moved structure: " + database.buildings[selectedObjectIndex].buildingName + " to base cell: " + baseCell);

        // Kết thúc di chuyển
        StopPlacement();
    }

    public void PlaceStructure()
    {
        Debug.Log("Attempting to place structure: " + database.buildings[selectedObjectIndex].buildingName);

        //if (inputManager.IsPointerOverUI())
        //{
        //    Debug.Log("Pointer is over UI, cannot place structure.");
        //    return;
        //}

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int cellUnderMouse = grid.WorldToCell(mousePosition);
        baseCell = new Vector3Int(
            cellUnderMouse.x - currentSize.x / 2,
            cellUnderMouse.y,
            cellUnderMouse.z - currentSize.y / 2);

        Debug.Log("Base cell for placement: " + baseCell);

        if (!CanPlace(baseCell))
        {
            Debug.LogWarning("Area is occupied or out of bounds, cannot place structure.");
            return;
        }
        List<Vector2Int> workerPositions = MarkWorkerSpots(baseCell,selectedObjectIndex);

        GameObject gameObject = Instantiate(database.buildings[selectedObjectIndex].buildingPrefab);
        Vector3 baseWorld = grid.CellToWorld(baseCell);
        Vector3 offset = new Vector3((currentSize.x - 1) * 0.5f, 0f, (currentSize.y - 1) * 0.5f);
        gameObject.transform.position = baseWorld + offset;
        gameObject.transform.rotation = rotationBuilding;

        var building = gameObject.GetComponent<BuildingBase>();
        if (building != null)
        {
            building.ID = database.buildings[selectedObjectIndex].buildingID;
            building.selectedBuilding = database.buildings[selectedObjectIndex];
            building.workerPositions = workerPositions;
            building.BaseCell = baseCell;
            if (!building.IsBuild)
                building.TryConsumeRequiredItems();
        }

        Debug.Log("Placed structure: " + database.buildings[selectedObjectIndex].buildingName + " at base cell: " + baseCell);
        OccupyCells(baseCell, 1);
        mapData.placedObjects.Add(new PlacedObjectData
        {
            position = baseCell,
            id = database.buildings[selectedObjectIndex].buildingID,
            rotation = rotationBuilding
        });
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
    public void OccupyCells(Vector3Int basePos, int check)
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
                Debug.Log($"Occupying cell at: {gx}, {gz} with value {check}");
                gridMap[gx + gridOffset.x, gz + gridOffset.y] = check;
            }
        }
    }

    public void StopPlacement()
    {
        selectedObjectIndex = -1;
        gridVisualization.SetActive(false);
        mouseIndicator.SetActive(false);
        cellIndicator.SetActive(false);
        cellIndicator.transform.localScale = Vector3.one;
        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnClicked -= MoveBuilding; // Hủy sự kiện MoveBuilding
        inputManager.OnExit -= StopPlacement;
        if (blueprintInstance != null)
            blueprintInstance.gameObject.SetActive(false);
        movingBuilding = null; // Reset movingBuilding
        inputManager.CurrentState = State.Moving;
    }

    public List<Vector2Int> MarkWorkerSpots(Vector3Int baseCell,int index)
    {
        Vector2Int baseVector2 = new Vector2Int(baseCell.x, baseCell.z);
        return GetAdjacentCells(baseVector2, database.buildings[index].size);
    }

    public static List<Vector2Int> GetAdjacentCells(Vector2Int origin, Vector2Int size)
    {
        var adjacent = new HashSet<Vector2Int>();

        // Phía trước (dưới building)
        //for (int dx = 0; dx < size.x; dx++)
        //{
        //    adjacent.Add(new Vector2Int(origin.x + dx, origin.y - 1));
        //}

        // Bên trái
        for (int dy = 0; dy < size.y; dy++)
        {
            adjacent.Add(new Vector2Int(origin.x - 1, origin.y + dy));
        }

        // Bên phải
        for (int dy = 0; dy < size.y; dy++)
        {
            adjacent.Add(new Vector2Int(origin.x + size.x, origin.y + dy));
        }

        return new List<Vector2Int>(adjacent);
    }

    public Vector3Int GetRandomCellAndOccupy()
    {
        List<Vector3Int> unoccupiedCells = new List<Vector3Int>();

        // Collect all unoccupied cells within grid bounds
        for (int x = 0; x < gridMap.GetLength(0); x++)
        {
            for (int z = 0; z < gridMap.GetLength(1); z++)
            {
                if (gridMap[x, z] == 0)
                {
                    int worldX = x - gridOffset.x;
                    int worldZ = z - gridOffset.y;
                    unoccupiedCells.Add(new Vector3Int(worldX, 0, worldZ));
                }
            }
        }

        // If no unoccupied cells, return invalid position
        if (unoccupiedCells.Count == 0)
        {
            Debug.LogWarning("No unoccupied cells available.");
            return new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
        }

        // Select random cell
        int randomIndex = Random.Range(0, unoccupiedCells.Count);
        Vector3Int selectedCell = unoccupiedCells[randomIndex];

        // Mark as occupied
        gridMap[selectedCell.x + gridOffset.x, selectedCell.z + gridOffset.y] = 1;
        Debug.Log($"Occupied cell at: {selectedCell.x}, {selectedCell.z}");

        return selectedCell;
    }

    public Vector3 CellToWorldPosition(Vector3Int cell)
    {
        // Convert grid cell to world position using the grid's CellToWorld method
        Vector3 worldPos = grid.CellToWorld(cell);

        // Apply offset to center the position (consistent with your existing logic)
        Vector3 offset = new Vector3((currentSize.x - 1) * 0.5f, 0f, (currentSize.y - 1) * 0.5f);
        return worldPos + offset;
    }
}