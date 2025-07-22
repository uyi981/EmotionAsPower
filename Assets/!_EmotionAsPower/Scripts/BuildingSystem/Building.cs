using UnityEngine;

[System.Serializable]
public class Building
{
    public int buildingID;
    public string buildingName;
    public GameObject buildingPrefab;
    public uint cost;
    public int buildTime;
    public Vector2Int size;
}
