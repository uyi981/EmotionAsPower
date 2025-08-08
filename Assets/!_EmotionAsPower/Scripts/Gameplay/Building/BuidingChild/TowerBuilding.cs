using System.Collections;
using System.Collections.Generic;
using Assets.__EmotionAsPower.Scripts.Gameplay.Building.Interface;
using UnityEngine;
using UnityEngine.VFX;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.Building.BuidingChild
{
    public class TowerBuilding : BuildingBase, IAttack
    {
        [Header("Tower Settings")]
        [Tooltip("Sát thương của tháp")]
        public int attackDamage = 10;
        [Tooltip("Tầm bắn của tháp (số ô)")]
        public int attackRange = 5;
        [Tooltip("Thời gian giữa các đợt tấn công (giây)")]
        public float attackCooldown = 1f;

        [Header("Visual Effects")]
        [Tooltip("Hiệu ứng tấn công (nếu có)")]
        public GameObject attackEffectPrefab;

        [Header("Pool Settings")]
        [SerializeField] int poolDefaultCapacity = 5;
        [SerializeField] int poolMaxSize = 20;

        private Vector2Int gridPosition;
        private LayerMask enemyLayer;
        private const float CHECK_INTERVAL = 0.1f;
        private float lastAttackTime = 0f;

        // Custom object pool
        private Queue<GameObject> attackEffectPool;
        private int currentPoolSize = 0;

        public int AttackDamage { get; set; }
        public int AttackRange { get; set; }
        public float AttackCooldown { get; set; }

        public override void Start()
        {
            base.Start();
            enemyLayer = LayerMask.GetMask("Enemy");

            InitializeObjectPool();
            Vector3 worldPos = transform.position;
            gridPosition = new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.z));

            InvokeRepeating(nameof(CheckForEnemies), 0f, CHECK_INTERVAL);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(CheckForEnemies));
        }

        private void InitializeObjectPool()
        {
            attackEffectPool = new Queue<GameObject>(poolDefaultCapacity);
            for (int i = 0; i < poolDefaultCapacity; i++)
            {
                var obj = Instantiate(attackEffectPrefab);
                obj.SetActive(false);
                attackEffectPool.Enqueue(obj);
                currentPoolSize++;
            }
        }

        private GameObject GetAttackEffectFromPool()
        {
            if (attackEffectPool.Count > 0)
            {
                var obj = attackEffectPool.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else if (currentPoolSize < poolMaxSize)
            {
                var obj = Instantiate(attackEffectPrefab);
                obj.SetActive(true);
                currentPoolSize++;
                return obj;
            }
            else
            {
                // Pool exhausted, optionally reuse oldest or return null
                return null;
            }
        }

        private void ReturnAttackEffectToPool(GameObject obj)
        {
            obj.SetActive(false);
            attackEffectPool.Enqueue(obj);
        }

        private void CheckForEnemies()
        {
            if (!isBuild) return;

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                TryAttack();
            }
        }

        private void TryAttack()
        {
            Vector3 boxSize = new Vector3(attackRange * 2 + 1, 10f, attackRange * 2 + 1);

            Collider[] hitColliders = Physics.OverlapBox(
                transform.position,
                boxSize * 0.5f,
                Quaternion.identity,
                enemyLayer
            );

            Transform nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (var hitCollider in hitColliders)
            {
                Vector3 enemyPos = hitCollider.transform.position;
                Vector2Int enemyGridPos = new Vector2Int(Mathf.FloorToInt(enemyPos.x), Mathf.FloorToInt(enemyPos.z));

                int distanceX = Mathf.Abs(gridPosition.x - enemyGridPos.x);
                int distanceY = Mathf.Abs(gridPosition.y - enemyGridPos.y);
                int distance = Mathf.Max(distanceX, distanceY);

                if (distance <= attackRange)
                {
                    float currentDistance = Vector3.Distance(transform.position, enemyPos);
                    if (currentDistance < nearestDistance)
                    {
                        nearestDistance = currentDistance;
                        nearestEnemy = hitCollider.transform;
                    }
                }
            }

            if (nearestEnemy != null)
            {
                Attack(nearestEnemy);
            }
        }

        public void Attack(Transform target)
        {
            Vector3 startPos = GetAttackOrigin();
            Vector3 endPos = GetTargetCenter(target);

            var attackEffect = GetAttackEffectFromPool();
            if (attackEffect != null)
            {
                attackEffect.transform.position = endPos;
                attackEffect.transform.rotation = Quaternion.identity;
                VisualEffect vfx = attackEffect.GetComponent<VisualEffect>();
                if (vfx != null)
                {
                    vfx.Play();
                }
                StartCoroutine(ReleaseEffectAfterTime(attackEffect, 0.5f));
            }

            StartCoroutine(DrawAttackLine(startPos, endPos));
        }

        private IEnumerator ReleaseEffectAfterTime(GameObject effect, float time)
        {
            yield return new WaitForSeconds(time);
            ReturnAttackEffectToPool(effect);
        }

        private Vector3 GetAttackOrigin()
        {
            if (TryGetComponent<Collider>(out var collider))
            {
                return new Vector3(
                    transform.position.x,
                    collider.bounds.max.y,
                    transform.position.z
                );
            }
            return transform.position + Vector3.up * 1.5f;
        }

        private Vector3 GetTargetCenter(Transform target)
        {
            if (target.TryGetComponent<Collider>(out var collider))
            {
                return collider.bounds.center;
            }
            return target.position;
        }

        private IEnumerator DrawAttackLine(Vector3 start, Vector3 end)
        {
            LineRenderer line = GetComponent<LineRenderer>();
            if (line == null)
            {
                line = gameObject.AddComponent<LineRenderer>();
                line.startWidth = 0.1f;
                line.endWidth = 0.1f;
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = Color.red;
                line.endColor = Color.yellow;
                line.useWorldSpace = true;
                line.positionCount = 2;
            }

            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.enabled = true;

            yield return new WaitForSeconds(0.1f);
            line.enabled = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Vector3 boxSize = new Vector3(attackRange * 2 + 1, 10f, attackRange * 2 + 1);
            Gizmos.DrawWireCube(transform.position, boxSize);

            Gizmos.color = new Color(1, 0, 0, 0.2f);
            float gridSize = attackRange * 2 + 1;
            Gizmos.DrawCube(transform.position, new Vector3(gridSize, 0.1f, gridSize));
        }
    }
}
