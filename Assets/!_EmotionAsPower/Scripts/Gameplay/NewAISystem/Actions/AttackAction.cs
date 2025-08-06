using UnityEngine;

[CreateAssetMenu(fileName = "Attack Action", menuName = "Scriptable Objects/AI/Actions/Attack")]
public class AttackAction : NewAIAction
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private DetectableType[] targetTypes;
    public float AttackRange => attackRange;
    public float AttackDamage => attackDamage;
    public float AttackSpeed => attackSpeed;

    public override DetectableType[] TargetTypes()
    {
        return targetTypes;
    }

    public override bool Interruptible()
    {
        return false;
    }

    public override AIActionExecutor CreateExecutor(AIActionData actionData)
    {
        return new AttackActionExecutor(actionData, this);
    }
}