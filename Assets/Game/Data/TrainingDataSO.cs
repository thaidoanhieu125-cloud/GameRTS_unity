using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.Data
{
    [CreateAssetMenu(fileName = "VillagerTrainingData", menuName = "GameRTS/Training Data")]
    public sealed class TrainingDataSO : ScriptableObject
    {
        [SerializeField] private UnitDataSO unitData;
        [SerializeField, Min(0)] private int foodCost = 50;
        [SerializeField, Min(1)] private int durationTicks = 200;
        [SerializeField, Range(1, 5)] private int queueLimit = 5;
        public UnitDataSO UnitData => unitData;
        public TrainingDefinition CreateDefinition()
        {
            if (unitData == null) throw new System.InvalidOperationException("Training data needs a Villager UnitData.");
            return new TrainingDefinition(unitData.UnitId, Mathf.RoundToInt(unitData.MoveSpeed * 1000), Mathf.RoundToInt(unitData.CollisionRadius * 1000),
                unitData.CarryCapacity, unitData.GatherIntervalTicks, foodCost, durationTicks, queueLimit, unitData.CreateCombatStats());
        }
    }
}
