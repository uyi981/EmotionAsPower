using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "Buildings/Building")]
public class SO_Building : ScriptableObject
{
    public string buildingName;
    public BuildingType type; 
    public List<ResourceRequirement> requiredResources;
    public float buildTime;
    public GameObject blueprintPrefab;
    public GameObject redprintPrefab;
    public GameObject buildingPrefab;


    public int maxHP;           // Máu tối đa của công trình
    public int damage;          // Sát thương (nếu là tháp bắn, v.v.)
    public float attackRange;   // Tầm bắn (nếu là tháp)
    public float attackSpeed;   // Tốc độ tấn công
    public string description;  // Mô tả công trình
}

[System.Serializable]
public class ResourceRequirement
{
    //public SO_Resource resource;
    public int amount;
}

public enum BuildingType
{
    Defense,
    Tower,
    Production,
    Decoration,
    Bed,
    //SpawnPoint,
    MainBase
}