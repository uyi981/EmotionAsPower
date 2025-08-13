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
        public Emotion emotionType = Emotion.Normal;
        [Tooltip("Số lượng năng lượng cảm xúc cộng mỗi lần")]
        public int emotionAmount = 1;
        [Tooltip("Layer của đối tượng mục tiêu (ví dụ: Villager)")]
        public LayerMask targetLayer;
        public string nameVFX;

        private Vector2Int gridPosition;
        private const float CHECK_INTERVAL = 5f;
        private float lastEffectTime = 0f;

        public override void Start()
        {
            base.Start();

            Vector3 worldPos = transform.position;
            gridPosition = new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.z));

        }

        private void OnDisable()
        {
            CancelInvoke(nameof(CheckForTargets));
        }

        private void CheckForTargets()
        {
            GameObject obj = Singleton<VFXPoolManager>.Instance.PopSKillObject(nameVFX);
            obj.gameObject.transform.position = transform.position+Vector3.up*0.2f;
            if (!isBuild) return;

            if (Time.time - lastEffectTime >= effectCooldown)
            {
                lastEffectTime = Time.time;
                TryAffectTargets();
            }
        }
        public override void OnBuildingComplete()
        {
            base.OnBuildingComplete();
            InvokeRepeating(nameof(CheckForTargets), 0f, CHECK_INTERVAL);

        }
        private void TryAffectTargets()
        {
            Vector3 boxSize = new Vector3(effectRange , 10f, effectRange * 2 + 1);

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
                    EmotionVector emotionVector = CreateEmotionVector(emotionType, emotionAmount);
                    villager.ReceiveEmotion(emotionVector);
                    Debug.Log($"Đã cộng {emotionAmount} {emotionType} cho {villager.name} từ {Name}!");
                }
            }
        }

        // Tạo EmotionVector dựa trên EmotionType
        private EmotionVector CreateEmotionVector(Emotion type, float amount)
        {
            switch (type)
            {
                case Emotion.Anger:
                    return new EmotionVector(amount, 0, 0, 0, 0);
                case Emotion.Joy:
                    return new EmotionVector(0, amount, 0, 0, 0);
                case Emotion.Sad:
                    return new EmotionVector(0, 0, amount, 0, 0);
                case Emotion.Fear:
                    return new EmotionVector(0, 0, 0, amount, 0);
                case Emotion.Apethatic:
                    return new EmotionVector(0, 0, 0, 0, amount);
                case Emotion.Normal:
                    return new EmotionVector(0, 0, 0, 0, 0);
                default:
                    Debug.LogWarning($"Emotion type {type} không hợp lệ, trả về EmotionVector mặc định.");
                    return new EmotionVector(0, 0, 0, 0, 0);

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
