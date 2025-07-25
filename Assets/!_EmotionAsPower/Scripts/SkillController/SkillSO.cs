using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "ScriptableObjects/Skill")]
public class  SkillSO  : ScriptableObject
{
    public int skillID;
    public string skillName;
    public string description;
    public int cooldown;
    public Sprite icon;
}
