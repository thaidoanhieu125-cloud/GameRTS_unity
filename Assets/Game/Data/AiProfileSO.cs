using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.Data
{
    [CreateAssetMenu(menuName = "GameRTS/AI Profile")]
    public sealed class AiProfileSO : ScriptableObject
    {
        [SerializeField, Range(20, 40)] private int decisionIntervalTicks = 30;
        [SerializeField, Range(3, 40)] private int villagerTarget = 10;
        [SerializeField, Range(3, 40)] private int militaryVillagers = 8;
        [SerializeField, Range(3, 30)] private int waveSize = 9;
        [SerializeField, Range(3, 100)] private int armyLimit = 18;
        [SerializeField, Range(4, 16)] private int baseRadius = 10;
        [SerializeField, Range(20, 1200)] private int regroupTicks = 200;
        public AiSettings CreateSettings() => new AiSettings(decisionIntervalTicks, villagerTarget, militaryVillagers, waveSize, armyLimit, baseRadius, regroupTicks);
        private void OnValidate()
        {
            militaryVillagers = Mathf.Clamp(militaryVillagers, 3, villagerTarget);
            armyLimit = Mathf.Max(armyLimit, waveSize);
        }
    }
}
