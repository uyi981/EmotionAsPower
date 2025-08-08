using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectsDatabase", menuName = "ScriptableObjects/SkillDatabase", order = 1)]
public class ObjectSkillDatabaseSO : ScriptableObject
{
    public List<SkillInstance> skills;
}
