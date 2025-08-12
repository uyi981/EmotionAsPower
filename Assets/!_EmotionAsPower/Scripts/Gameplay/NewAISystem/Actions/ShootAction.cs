using UnityEngine;

[CreateAssetMenu(fileName = "Shoot Action", menuName = "Scriptable Objects/AI/Actions/Shoot")]
public class ShootAction : NewAIAction
{
    [Header("Shooting Settings")]
    [SerializeField] private float shootingRange = 8f;
    [SerializeField] private float bulletDamage = 25f;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float fireRate = 1f; // shots per second
    [SerializeField] private DetectableType[] targetTypes;
    [SerializeField] private LayerMask damageLayerMask = -1;

    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;

    [Header("Shooting Position")]
    [SerializeField] private Vector3 shootOffset = new Vector3(0, 1.5f, 0); // Offset from AI center for bullet spawn

    [Header("Visual/Audio Effects (Optional)")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private AudioClip shootSound;

    [Header("Behavior")]
    [SerializeField] private bool requiresLineOfSight = true;
    [SerializeField] private bool stopMovingWhileShooting = true;
    [SerializeField] private float aimTime = 0.2f; // Time to aim before shooting

    public float ShootingRange => shootingRange;
    public float BulletDamage => bulletDamage;
    public float BulletSpeed => bulletSpeed;
    public float FireRate => fireRate;
    public LayerMask DamageLayerMask => damageLayerMask;
    public GameObject BulletPrefab => bulletPrefab;
    public Vector3 ShootOffset => shootOffset;
    public GameObject MuzzleFlashPrefab => muzzleFlashPrefab;
    public AudioClip ShootSound => shootSound;
    public bool RequiresLineOfSight => requiresLineOfSight;
    public bool StopMovingWhileShooting => stopMovingWhileShooting;
    public float AimTime => aimTime;

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
        return new ShootActionExecutor(actionData, this);
    }
}