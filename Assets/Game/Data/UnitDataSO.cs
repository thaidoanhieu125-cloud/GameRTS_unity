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
        [Header("Villager (disabled for soldiers)")]
        [SerializeField] private bool canGather;
        [SerializeField, Min(1)] private int carryCapacity = 10;
        [SerializeField, Min(1)] private int gatherIntervalTicks = 10;

        public string UnitId => unitId;
        public string DisplayName => displayName;
        public float MoveSpeed => moveSpeed;
        public float CollisionRadius => collisionRadius;
        public GameObject ViewPrefab => viewPrefab;
        public bool CanGather => canGather;
        public int CarryCapacity => canGather ? carryCapacity : 0;
        public int GatherIntervalTicks => gatherIntervalTicks;

        private void OnValidate()
        {
            unitId = unitId?.Trim();
            displayName = displayName?.Trim();
            moveSpeed = Mathf.Max(0f, moveSpeed);
            collisionRadius = Mathf.Max(0.01f, collisionRadius);
            carryCapacity = Mathf.Clamp(carryCapacity, 1, 1000);
            gatherIntervalTicks = Mathf.Clamp(gatherIntervalTicks, 1, 1200);
        }
    }
}
