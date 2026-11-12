using UnityEngine;

namespace GameRTS.Data
{
    [CreateAssetMenu(fileName = "UnitData", menuName = "GameRTS/Unit Data")]
    public sealed class UnitDataSO : ScriptableObject
    {
        [SerializeField] private string unitId = "sample_soldier";
        [SerializeField] private string displayName = "Lính mẫu";
        [SerializeField, Min(0f)] private float moveSpeed = 3f;
        [SerializeField, Min(0.01f)] private float collisionRadius = 0.45f;
        [SerializeField] private GameObject viewPrefab;

        public string UnitId => unitId;
        public string DisplayName => displayName;
        public float MoveSpeed => moveSpeed;
        public float CollisionRadius => collisionRadius;
        public GameObject ViewPrefab => viewPrefab;

        private void OnValidate()
        {
            unitId = unitId?.Trim();
            displayName = displayName?.Trim();
            moveSpeed = Mathf.Max(0f, moveSpeed);
            collisionRadius = Mathf.Max(0.01f, collisionRadius);
        }
    }
}
