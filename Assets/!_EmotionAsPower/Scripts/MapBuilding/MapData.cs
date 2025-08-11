using UnityEngine;

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMapData", menuName = "Map/Map Data")]
public class MapData : ScriptableObject
{
    [Serializable]
    public class PlacedObjectData
    {
        public int id; // tên prefab để load lại
        public Vector3Int position;
        public Quaternion rotation;
    }

    public List<PlacedObjectData> placedObjects = new List<PlacedObjectData>();
}
