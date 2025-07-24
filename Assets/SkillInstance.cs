using UnityEngine;

public class SkillInstance : MonoBehaviour
{
    public SkillSO skillSO;
    public int level;
    public float cooldown;
    public float duration;
    public float dame;
    public SkillInstance(SkillSO skillSO, int level, float cooldown, float duration)
    {
        this.skillSO = skillSO;
        this.level = level;
        this.cooldown = cooldown;
        this.duration = duration;
    }
    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enermy"))
        {
            Health enemyHealth = other.GetComponent<Health>();
            if (enemyHealth != null)
            {
                // Calculate damage based on skillSO and level
                enemyHealth.TakeDamage(dame);
            }
            else
            {
                Debug.LogWarning("No Health component found on the enemy.");
            }
        }
    }
}
