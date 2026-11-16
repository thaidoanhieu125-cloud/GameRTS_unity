using GameRTS.Commands;
using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.Data
{
    [CreateAssetMenu(menuName = "GameRTS/Technology Data")]
    public sealed class TechnologyDataSO : ScriptableObject
    {
        [SerializeField] private TechnologyId technology;
        [SerializeField] private string displayName;
        [SerializeField] private string buildingType = "town_center";
        [SerializeField, Range(1, 2)] private int requiredAge = 1;
        [SerializeField, Min(0)] private int wood, food, gold;
        [SerializeField, Min(1)] private int durationTicks = 400;
        public TechnologyId Technology => technology;
        public string DisplayName => displayName;
        public ResearchDefinition CreateDefinition() => new ResearchDefinition(technology, buildingType, requiredAge, wood, food, gold, durationTicks);
    }
}
