using System;
using System.Collections.Generic;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public enum AiPhase { Economy, Military, Attack }

    // Trusted match settings, copied from a ScriptableObject before tick 1.
    public sealed class AiSettings
    {
        public int DecisionInterval { get; }
        public int VillagerTarget { get; }
        public int MilitaryVillagers { get; }
        public int WaveSize { get; }
        public int ArmyLimit { get; }
        public int BaseRadius { get; }
        public AiSettings(int decisionInterval = 30, int villagerTarget = 10, int militaryVillagers = 8,
            int waveSize = 6, int armyLimit = 18, int baseRadius = 10)
        {
            if (decisionInterval < 20 || decisionInterval > 40 || villagerTarget < 3 || villagerTarget > 40
                || militaryVillagers < 3 || militaryVillagers > villagerTarget || waveSize < 3 || waveSize > 30
                || armyLimit < waveSize || armyLimit > 100 || baseRadius < 4 || baseRadius > 16)
                throw new ArgumentException("Invalid AI settings.");
            DecisionInterval = decisionInterval; VillagerTarget = villagerTarget; MilitaryVillagers = militaryVillagers;
            WaveSize = waveSize; ArmyLimit = armyLimit; BaseRadius = baseRadius;
        }
    }

    public sealed class AiResourceMemory
    {
        public int Id { get; }
        public int Cell { get; }
        public ResourceType Type { get; }
        public int LastKnownAmount { get; internal set; }
        internal AiResourceMemory(ResourceNodeState node)
        { Id = node.Id; Cell = node.Cell; Type = node.ResourceType; LastKnownAmount = node.Remaining; }
    }

    public sealed class AiState
    {
        public int PlayerId { get; }
        public int TownCenterId { get; }
        public AiSettings Settings { get; }
        public AiPhase Phase { get; internal set; }
        public int NextDecisionTick { get; internal set; } = 1;
        public int Decisions { get; internal set; }
        public int CommandsIssued { get; internal set; }
        public int StagingCell { get; internal set; } = -1;
        public int ScoutCell { get; internal set; } = -1;
        internal int Sequence;
        internal readonly SortedDictionary<int, AiResourceMemory> Resources = new SortedDictionary<int, AiResourceMemory>();
        internal readonly List<int> Wave = new List<int>();
        internal readonly List<ISimulationCommand> LastIssued = new List<ISimulationCommand>();
        public IEnumerable<AiResourceMemory> KnownResources => Resources.Values;
        public IReadOnlyList<int> WaveIds => Wave.AsReadOnly();
        public IReadOnlyList<ISimulationCommand> LastCommands => LastIssued.AsReadOnly();
        internal AiState(int playerId, int townCenterId, AiSettings settings)
        { PlayerId = playerId; TownCenterId = townCenterId; Settings = settings; }
    }
}
