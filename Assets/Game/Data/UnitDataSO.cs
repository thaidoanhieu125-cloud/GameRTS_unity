using UnityEngine;
using GameRTS.Simulation;

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
        [Header("Combat (20 ticks per second)")]
        [SerializeField] private UnitClass unitClass;
        [SerializeField, Min(1)] private int maxHP = 40;
        [SerializeField, Min(0)] private int armor;
        [SerializeField, Min(0)] private int attackDamage = 3;
        [SerializeField, Min(1)] private int attackIntervalTicks = 30;
        [SerializeField, Range(1, 16)] private int attackRangeCells = 1;
        [SerializeField, Min(0)] private int projectileSpeed = 0;
        [SerializeField, Range(0, 20)] private int aggroRangeCells;

        public string UnitId => unitId;
        public string DisplayName => displayName;
        public float MoveSpeed => moveSpeed;
        public float CollisionRadius => collisionRadius;
        public GameObject ViewPrefab => viewPrefab;
        public bool CanGather => canGather;
        public int CarryCapacity => canGather ? carryCapacity : 0;
        public int GatherIntervalTicks => gatherIntervalTicks;
        public CombatStats CreateCombatStats() => new CombatStats(maxHP, armor, attackDamage, attackIntervalTicks,
            attackRangeCells * 1000, projectileSpeed, aggroRangeCells * 1000, unitClass);

        private void OnValidate()
        {
            unitId = unitId?.Trim();
            displayName = displayName?.Trim();
            moveSpeed = Mathf.Max(0f, moveSpeed);
            collisionRadius = Mathf.Max(0.01f, collisionRadius);
            carryCapacity = Mathf.Clamp(carryCapacity, 1, 1000);
            gatherIntervalTicks = Mathf.Clamp(gatherIntervalTicks, 1, 1200);
            maxHP = Mathf.Clamp(maxHP, 1, 10000000); armor = Mathf.Clamp(armor, 0, 10000000); attackDamage = Mathf.Clamp(attackDamage, 0, 10000000);
            attackIntervalTicks = Mathf.Clamp(attackIntervalTicks, 1, 1000000); projectileSpeed = Mathf.Clamp(projectileSpeed, 0, 40000);
            attackRangeCells = projectileSpeed == 0 ? 1 : Mathf.Clamp(attackRangeCells, 1, 16); aggroRangeCells = Mathf.Clamp(aggroRangeCells, 0, 20);
        }
    }
}
