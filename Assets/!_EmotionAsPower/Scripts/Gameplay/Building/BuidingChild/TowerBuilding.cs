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
        public string vfxName;

        [Header("Pool Settings")]
        [SerializeField] int poolDefaultCapacity = 5;
        [SerializeField] int poolMaxSize = 20;

        private Vector2Int gridPosition;
        public LayerMask enemyLayer;
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
            Vector3 worldPos = transform.position;
            gridPosition = new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.z));

            InvokeRepeating(nameof(CheckForEnemies), 0f, CHECK_INTERVAL);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(CheckForEnemies));
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

            GameObject obj = Singleton<VFXPoolManager>.Instance.PopSKillObject(vfxName);
            VFXInstance vFXInstance = obj.GetComponent<VFXInstance>();
            if (vFXInstance.skillType == SkillType.Static)
            {
                // Skill đứng yên tại endPos
                obj.transform.position = startPos;
                obj.transform.rotation = Quaternion.LookRotation(endPos - startPos); // Không cần xoay nếu là static
            }
            else if (vFXInstance.skillType == SkillType.Projectile)
            {
                // Skill bắn từ start -> end
                obj.transform.position = startPos;
                obj.transform.rotation = Quaternion.LookRotation(endPos - startPos);
                StartCoroutine(MoveVFXWithDelay(obj, startPos, endPos, 0.2f,0.2f));
            }
        }

        private IEnumerator MoveVFXWithDelay(GameObject vfxObj, Vector3 start, Vector3 end, float delayBeforeMove, float travelTime)
        {
            // Đứng yên delayBeforeMove giây
            yield return new WaitForSeconds(delayBeforeMove);

            float elapsed = 0f;
            while (elapsed < travelTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / travelTime;
                vfxObj.transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            // Đảm bảo tới đúng vị trí cuối
            vfxObj.transform.position = end;

            // Play effect nổ/hit
            var vfx = vfxObj.GetComponent<UnityEngine.VFX.VisualEffect>();
            if (vfx != null) vfx.Play();
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
