using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.Data
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "GameRTS/Building Data")]
    public sealed class BuildingDataSO : ScriptableObject
    {
        [SerializeField] private string typeId = "house";
        [SerializeField] private string displayName = "Nhà";
        [SerializeField, Range(1, 8)] private int width = 2, height = 2;
        [SerializeField, Min(0)] private int woodCost = 50, foodCost, goldCost;
        [SerializeField, Min(1)] private int requiredWork = 200;
        [SerializeField, Min(1)] private int maxHitPoints = 300;
        [SerializeField, Min(0)] private int populationBonus = 5;
        [SerializeField] private ResourceAcceptance accepts;
        [SerializeField] private Sprite foundationSprite, completedSprite;
        [SerializeField] private GameObject viewPrefab;
        public string TypeId => typeId;
        public string DisplayName => displayName;
        public int Width => width;
        public int Height => height;
        public int WoodCost => woodCost;
        public int FoodCost => foodCost;
        public int GoldCost => goldCost;
        public int RequiredWork => requiredWork;
        public Sprite FoundationSprite => foundationSprite;
        public Sprite CompletedSprite => completedSprite;
        public GameObject ViewPrefab => viewPrefab;
        public BuildingDefinition CreateDefinition() => new BuildingDefinition(typeId, width, height, woodCost, foodCost, goldCost,
            requiredWork, maxHitPoints, populationBonus, accepts);
        private void OnValidate()
        {
            typeId = typeId?.Trim(); width = Mathf.Clamp(width, 1, 8); height = Mathf.Clamp(height, 1, 8);
            woodCost = Mathf.Max(0, woodCost); foodCost = Mathf.Max(0, foodCost); goldCost = Mathf.Max(0, goldCost);
            requiredWork = Mathf.Clamp(requiredWork, 1, 1000000); maxHitPoints = Mathf.Clamp(maxHitPoints, 1, 10000000);
            populationBonus = Mathf.Clamp(populationBonus, 0, 1000);
        }
    }
}
