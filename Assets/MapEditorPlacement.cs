using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class MapEditorPlacement : MonoBehaviour
{
    public PlacementSystem placementSystem;
    public Grid grid; // Grid reference for placement
    public int buildingID = 0; // ID công trình muốn spawn
    public Vector3Int gridPos = Vector3Int.zero;
    public Quaternion rotation = Quaternion.identity;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (placementSystem == null) return;

        // Vẽ preview vị trí spawn
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
        grid.CellToWorld(gridPos) + Vector3.up * 0.5f,
        new Vector3(1, 1, 1)
        );
    }
    [ContextMenu("Spawn Building Here")]
    public void SpawnBuildingHere()
    {
        if (placementSystem == null)
        {
            Debug.LogError("Chưa gán PlacementSystem!");
            return;
        }

        var go = placementSystem.SpawnBuildingInstant(gridPos, buildingID, rotation);
        if (go != null)
        {
            Undo.RegisterCreatedObjectUndo(go, "Spawn Building");
            Debug.Log($"Spawned building ID {buildingID} tại {gridPos}");
        }
    }
#endif
}
