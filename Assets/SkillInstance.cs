using UnityEngine;

public class SkillInstance : MonoBehaviour
{
    public SkillSO skillSO;
    public int level;
    public float cooldown;
    public float duration;
    public SkillInstance(SkillSO skillSO, int level, float cooldown, float duration)
    {
        this.skillSO = skillSO;
        this.level = level;
        this.cooldown = cooldown;
        this.duration = duration;
    }
}
