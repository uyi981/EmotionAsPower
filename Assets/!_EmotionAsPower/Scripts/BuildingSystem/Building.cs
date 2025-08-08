using LgTyUtils;
using UnityEngine;

[System.Serializable]
public class Building
{
    [Header("Building Properties")]
    public int buildingID;
    public string buildingName;
    public BuildingType type;
    public GameObject blueprintPrefab;
    public GameObject redprintPrefab;
    public GameObject buildingPrefab;
    public SerializableDictionary<ItemSO, int> keyValuePairs;
    public int buildTime;
    public Vector2Int size;

    [Header("Building Stats")]
    public int maxHP;           // Máu tối đa của công trình
    public int damage;          // Sát thương (nếu là tháp bắn, v.v.)
    public float attackRange;   // Tầm bắn (nếu là tháp)
    public float attackSpeed;   // Tốc độ tấn công
    public string description;  // Mô tả công trình
}

public enum BuildingType
{
    Defense,
    Tower,
    Production,
    Entertainment,
    Housing,
    FoodFactory,
    Breeding,
    Special,
}