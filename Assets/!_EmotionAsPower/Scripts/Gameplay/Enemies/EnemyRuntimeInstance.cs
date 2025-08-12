using System;
using UnityEngine;

[Serializable]
public class EnemyRuntimeInstance
{
    public string id;
    public Vector3 position;
    public float currentHealth;
    public float remainExistingTime;
}

[Serializable]
public class ExplosiveRuntimeInstance
{
    public string id; // For identifying the explosive prefab
    public Vector3 position;
    public Quaternion rotation;
    public float explosionRange;
    public float explosionDamage;
    public LayerMask damageLayerMask;
    public bool hasExploded;
    public float currentTimer; // For delayed explosions
}

[Serializable]
public class BulletRuntimeInstance
{
    public string id; // For identifying the bullet prefab
    public Vector3 position;
    public Vector3 direction;
    public float damage;
    public float speed;
    public float remainingLifetime;
    public LayerMask damageLayerMask;
}