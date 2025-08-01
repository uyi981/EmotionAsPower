using System.Collections;
using UnityEngine;
using Assets.__EmotionAsPower.Scripts.Gameplay.Building.Interface;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.Building.BuidingChild
{
    public class EntertainmentBuilding : BuildingBase
    {
        [Header("Entertainment Settings")]
        [Tooltip("Tầm ảnh hưởng của công trình giải trí (số ô)")]
        public int effectRange = 5;
        [Tooltip("Thời gian giữa các lần kiểm tra (giây)")]
        public float effectCooldown = 1f;
        [Tooltip("Loại cảm xúc sẽ cộng")]
        public EmotionType emotionType = EmotionType.Fun;
        [Tooltip("Số lượng năng lượng cảm xúc cộng mỗi lần")]
        public int emotionAmount = 1;

        private Vector2Int gridPosition;
        private LayerMask targetLayer;
        private const float CHECK_INTERVAL = 0.1f;
        private float lastEffectTime = 0f;

        public override void Start()
        {
            base.Start();
            // Giả sử Villager nằm ở layer "Villager"
            targetLayer = LayerMask.GetMask("Villager");

            Vector3 worldPos = transform.position;
            gridPosition = new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.z));

            InvokeRepeating(nameof(CheckForTargets), 0f, CHECK_INTERVAL);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(CheckForTargets));
        }

        private void CheckForTargets()
        {
            if (!isBuild) return;

            if (Time.time - lastEffectTime >= effectCooldown)
            {
                lastEffectTime = Time.time;
                TryAffectTargets();
            }
        }

        private void TryAffectTargets()
        {
            Vector3 boxSize = new Vector3(effectRange * 2 + 1, 10f, effectRange * 2 + 1);

            Collider[] hitColliders = Physics.OverlapBox(
                transform.position,
                boxSize * 0.5f,
                Quaternion.identity,
                targetLayer
            );

            foreach (var hitCollider in hitColliders)
            {
                // Ví dụ: chỉ cộng năng lượng cho Villager
                Villager villager = hitCollider.GetComponent<Villager>();
                if (villager != null)
                {
                    // Cộng năng lượng cảm xúc cho Villager
                    EmotionEnergyManager.Instance.AddEnergy(emotionType, emotionAmount);
                    Debug.Log($"Đã cộng {emotionAmount} {emotionType} cho {villager.name} từ {Name}!");
                }
            }
        }

        // Vẽ Gizmos để xem vùng ảnh hưởng trong Editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 0, 1, 0.3f);
            Vector3 boxSize = new Vector3(effectRange * 2 + 1, 10f, effectRange * 2 + 1);
            Gizmos.DrawWireCube(transform.position, boxSize);
        }
    }
}
