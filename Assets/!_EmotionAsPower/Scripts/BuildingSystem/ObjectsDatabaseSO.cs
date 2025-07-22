using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectsDatabase", menuName = "ScriptableObjects/ObjectsDatabase", order = 1)]
public class ObjectsDatabaseSO : ScriptableObject
{
    public List<Building> buildings;
}
