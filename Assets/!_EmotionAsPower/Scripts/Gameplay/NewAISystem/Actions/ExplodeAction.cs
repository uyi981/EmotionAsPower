using UnityEngine;

[CreateAssetMenu(fileName = "Explode Action", menuName = "Scriptable Objects/AI/Actions/Explode")]
public class ExplodeAction : NewAIAction
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRange = 3f;
    [SerializeField] private float explosionDamage = 50f;
    [SerializeField] private float triggerRange = 1f;
    [SerializeField] private DetectableType[] targetTypes;
    [SerializeField] private LayerMask damageLayerMask = -1;

    [Header("Self Destruction")]
    [SerializeField] private bool destroySelfOnExplode = true;
    [SerializeField] private float explosionDelay = 0.5f;

    [Header("Explosive Object")]
    [SerializeField] private GameObject explosiveObjectPrefab;

    [Header("Visual Effects (Optional)")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;

    public float ExplosionRange => explosionRange;
    public float ExplosionDamage => explosionDamage;
    public float TriggerRange => triggerRange;
    public LayerMask DamageLayerMask => damageLayerMask;
    public bool DestroySelfOnExplode => destroySelfOnExplode;
    public float ExplosionDelay => explosionDelay;
    public GameObject ExplosiveObjectPrefab => explosiveObjectPrefab;
    public GameObject ExplosionPrefab => explosionPrefab;
    public AudioClip ExplosionSound => explosionSound;

    public override DetectableType[] TargetTypes()
    {
        return targetTypes;
    }

    public override bool Interruptible()
    {
        return true;
    }

    public override AIActionExecutor CreateExecutor(AIActionData actionData)
    {
        return new ExplodeActionExecutor(actionData, this);
    }
}