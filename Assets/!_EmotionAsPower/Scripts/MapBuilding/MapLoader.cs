using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class MapLoader : MonoBehaviour
{
    public MapData mapData;
    public Transform parent;
    public PlacementSystem placementSystem;

    [ContextMenu("Load Map From ScriptableObject")]
    public void LoadMap()
    {
        foreach(var item in mapData.placedObjects)
        {
            placementSystem.SpawnBuildingInstant(item.position,item.id,item.rotation);

        }
    }
}
