using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.Building.BuidingChild
{
    public class TowerBuilding : BuildingBase
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

        private float attackTimer = 0f;
        private Vector2Int gridPosition;
        private LayerMask enemyLayer;
        private GameObject attackEffect;

        public override void Start()
        {
            base.Start();
            enemyLayer = LayerMask.GetMask("Enemy");
            
            // Lấy vị trí grid của tòa tháp (tính từ góc dưới trái)
            Vector3 worldPos = transform.position;
            gridPosition = new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.z));
        }

        private void Update()
        {
            if (!isBuild) return;
            
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                TryAttack();
            }
        }

        private void TryAttack()
        {
            // Tạo kích thước của box dựa trên tầm tấn công
            Vector3 boxSize = new Vector3(attackRange * 2 + 1, 10f, attackRange * 2 + 1);
            
            // Lấy tất cả kẻ địch trong phạm vi box
            Collider[] hitColliders = Physics.OverlapBox(
                transform.position, 
                boxSize * 0.5f, // Half extents
                Quaternion.identity, 
                enemyLayer
            );
            
            // Tìm kẻ địch gần nhất
            Transform nearestEnemy = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var hitCollider in hitColliders)
            {
                // Kiểm tra xem kẻ địch có nằm trong tầm tấn công theo grid không
                Vector3 enemyPos = hitCollider.transform.position;
                Vector2Int enemyGridPos = new Vector2Int(Mathf.FloorToInt(enemyPos.x), Mathf.FloorToInt(enemyPos.z));
                
                // Tính khoảng cách grid (Chebyshev distance)
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
            
            // Tấn công kẻ địch gần nhất
            if (nearestEnemy != null)
            {
                Attack(nearestEnemy);
            }
        }

        private void Attack(Transform target)
        {
            // Tính toán vị trí bắt đầu và kết thúc chính xác hơn
            Vector3 startPos = GetAttackOrigin();
            Vector3 endPos = GetTargetCenter(target);

            attackEffect = Instantiate(attackEffectPrefab, endPos, Quaternion.identity);
            VisualEffect vfx = attackEffect.GetComponent<VisualEffect>();
            if (vfx != null)
            {
                vfx.SetVector3("TargetPosition", endPos);
                vfx.SetFloat("Damage", attackDamage);
                vfx.Play();
            }

            StartCoroutine(DrawAttackLine(startPos, endPos));
        }
        
        // Lấy vị trí bắt đầu của tia bắn (giữa đỉnh tháp)
        private Vector3 GetAttackOrigin()
        {
            // Nếu có collider, lấy điểm trên cùng của collider
            if (TryGetComponent<Collider>(out var collider))
            {
                return new Vector3(
                    transform.position.x,
                    collider.bounds.max.y,
                    transform.position.z
                );
            }
            
            // Mặc định trả về vị trí hiện tại + 1.5 đơn vị lên trên
            return transform.position + Vector3.up * 1.5f;
        }
        
        // Lấy vị trí giữa của mục tiêu
        private Vector3 GetTargetCenter(Transform target)
        {
            // Nếu mục tiêu có collider, lấy điểm giữa của collider
            if (target.TryGetComponent<Collider>(out var collider))
            {
                return collider.bounds.center;
            }
            
            // Mặc định trả về vị trí hiện tại của mục tiêu
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
                
                // Thêm thuộc tính để tia bắn mượt hơn
                line.useWorldSpace = true;
                line.positionCount = 2;
            }
            
            // Đặt vị trí bắt đầu và kết thúc cho tia bắn
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.enabled = true;
            
            // Hiển thị tia bắn trong 0.1 giây
            yield return new WaitForSeconds(0.1f);
            line.enabled = false;
        }
        
        // Vẽ Gizmos để xem tầm tấn công trong Editor
        private void OnDrawGizmosSelected()
        {
            // Vẽ box thể hiện tầm tấn công
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Vector3 boxSize = new Vector3(attackRange * 2 + 1, 10f, attackRange * 2 + 1);
            Gizmos.DrawWireCube(transform.position, boxSize);
            
            // Vẽ hình vuông 2D thể hiện tầm tấn công trên mặt phẳng
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            float gridSize = attackRange * 2 + 1; // +1 để tính cả ô trung tâm
            Gizmos.DrawCube(transform.position, new Vector3(gridSize, 0.1f, gridSize));
        }
    }
}