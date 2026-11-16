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
        public int RegroupTicks { get; }
        public int WaveInterval { get; }
        public bool MultiFront { get; }
        public bool FastEconomy { get; }
        public AiSettings(int decisionInterval = 30, int villagerTarget = 10, int militaryVillagers = 8,
            int waveSize = 9, int armyLimit = 18, int baseRadius = 10, int regroupTicks = 200, int waveInterval = 0, bool multiFront = false, bool fastEconomy = false)
        {
            if (decisionInterval < 20 || decisionInterval > 40 || villagerTarget < 3 || villagerTarget > 40
                || militaryVillagers < 3 || militaryVillagers > villagerTarget || waveSize < 3 || waveSize > 30
                || armyLimit < waveSize || armyLimit > 100 || baseRadius < 4 || baseRadius > 16
                || waveInterval < 0 || waveInterval > 10000
                || regroupTicks < 20 || regroupTicks > 1200)
                throw new ArgumentException("Invalid AI settings.");
            DecisionInterval = decisionInterval; VillagerTarget = villagerTarget; MilitaryVillagers = militaryVillagers;
            WaveSize = waveSize; ArmyLimit = armyLimit; BaseRadius = baseRadius;
            RegroupTicks = regroupTicks; WaveInterval = waveInterval; MultiFront = multiFront; FastEconomy = fastEconomy;
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
        public int ApproachUntilTick { get; internal set; }
        public int NextWaveTick { get; internal set; }
        public int WavesLaunched { get; internal set; }
        public int WaveInitialSize { get; internal set; }
        public int Regroups { get; internal set; }
        public bool IsRegrouping { get; internal set; }
        public int RegroupUntilTick { get; internal set; }
        internal readonly List<int> Returning = new List<int>();
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
