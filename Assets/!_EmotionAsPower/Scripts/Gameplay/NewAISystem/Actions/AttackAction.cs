using UnityEngine;

[CreateAssetMenu(fileName = "Attack Action", menuName = "Scriptable Objects/AI/Actions/Attack")]
public class AttackAction : NewAIAction
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private DetectableType[] targetTypes;

    [Header("Positioning")]
    [SerializeField] private bool useSmartPositioning = true;
    [SerializeField] private float positioningOffset = 1.5f; // Distance to offset from target center

    [Header("Visual Effects")]
    [SerializeField] private GameObject attackEffect;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private float effectDuration = 0.5f;

    [Header("Animation")]
    [SerializeField] private string attackAnimationTrigger = "Attack";
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private float damageDelayFromAnimStart = 0.3f; // When in animation to apply damage

    public float AttackRange => attackRange;
    public float AttackDamage => attackDamage;
    public float AttackSpeed => attackSpeed;
    public bool UseSmartPositioning => useSmartPositioning;
    public float PositioningOffset => positioningOffset;
    public GameObject AttackEffect => attackEffect;
    public AudioClip AttackSound => attackSound;
    public float EffectDuration => effectDuration;
    public string AttackAnimationTrigger => attackAnimationTrigger;
    public float AnimationDuration => animationDuration;
    public float DamageDelayFromAnimStart => damageDelayFromAnimStart;

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