using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] GameObject mouseIndicator, cellIndicator;
    [SerializeField] private InputManagerForGrid inputManager;
    [SerializeField] private Grid grid;
    public float[,] gridMap;
    [SerializeField] public ObjectsDatabaseSO database;
    [SerializeField] public int selectedObjectIndex = -1;
    [SerializeField] private GameObject gridVisualization;

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

        if (!CanPlace(baseCell))
        {
            cellIndicatorRenderer.color = Color.red; // Set to red if cannot place
            Debug.LogWarning("Cannot place structure at base cell: " + baseCell);
        }
        else
        {
            cellIndicatorRenderer.color = Color.white; // Set to green if can place
            Debug.Log("Can place structure at base cell: " + baseCell);
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
        blueprintInstance = Instantiate(database.buildings[selectedObjectIndex].blueprintPrefab);

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

    // Di chuyển building đến vị trí mới
    private void MoveBuilding()
    {
        if (inputManager.IsPointerOverUI())
        {
            Debug.Log("Pointer is over UI, cannot move structure.");
            return;
        }

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

        List<Vector2Int> workerPositions = MarkWorkerSpots(baseCell);

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

        if (inputManager.IsPointerOverUI())
        {
            Debug.Log("Pointer is over UI, cannot place structure.");
            return;
        }

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
        List<Vector2Int> workerPositions = MarkWorkerSpots(baseCell);

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

    public List<Vector2Int> MarkWorkerSpots(Vector3Int baseCell)
    {
        Vector3 worldPos;
        Vector3Int[] workerSpots = new Vector3Int[]
        {
        new Vector3Int(baseCell.x + 1, baseCell.y, baseCell.z),
        new Vector3Int(baseCell.x - 1, baseCell.y, baseCell.z),
        new Vector3Int(baseCell.x, baseCell.y, baseCell.z - 1)
        };

        List<Vector2Int> workerSpotsResult = new List<Vector2Int>();

        foreach (var spot in workerSpots)
        {
            Vector3 offset = new Vector3((currentSize.x - 1) * 0.5f, 0f, (currentSize.y - 1) * 0.5f);
            worldPos = grid.CellToWorld(spot) + offset;
            workerSpotsResult.Add(new Vector2Int((int)worldPos.x, (int)worldPos.z));
        }
        Vector2Int baseVector2 = new Vector2Int(baseCell.x, baseCell.z);
        return GetAdjacentCells(baseVector2, database.buildings[selectedObjectIndex].size);
    }

    public static List<Vector2Int> GetAdjacentCells(Vector2Int origin, Vector2Int size)
    {
        var adjacent = new HashSet<Vector2Int>();

        // Phía trước (dưới building)
        for (int dx = 0; dx < size.x; dx++)
        {
            adjacent.Add(new Vector2Int(origin.x + dx, origin.y - 1));
        }

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
}