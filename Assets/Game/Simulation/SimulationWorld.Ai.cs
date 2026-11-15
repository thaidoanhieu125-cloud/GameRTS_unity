using System;
using System.Collections.Generic;

namespace GameRTS.Simulation
{
    public sealed partial class SimulationWorld
    {
        private readonly SortedDictionary<int, AiController> aiControllers = new SortedDictionary<int, AiController>();
        public AiState GetAi(int playerId) => aiControllers.TryGetValue(playerId, out var ai) ? ai.State : null;
        public AiState AddAiController(int playerId, int townCenterId, AiSettings settings = null)
        {
            var town = GetDropOff(townCenterId);
            if (Tick != 0 || !VisionEnabled || town == null || town.OwnerId != playerId || GetTraining(townCenterId) == null
                || aiControllers.ContainsKey(playerId)) throw new InvalidOperationException("AI needs sight and an owned training TC before tick 1.");
            var controller = new AiController(new AiState(playerId, townCenterId, settings ?? new AiSettings()));
            aiControllers.Add(playerId, controller); return controller.State;
        }
        private void ThinkAi()
        {
            foreach (var ai in aiControllers.Values)
                if (Tick >= ai.State.NextDecisionTick && GetDropOff(ai.State.TownCenterId) != null) ai.Think(this);
        }
        private void HashAi(Action<int> add)
        {
            add(aiControllers.Count);
            foreach (var controller in aiControllers.Values)
            {
                var state = controller.State; var cfg = state.Settings;
                add(state.PlayerId); add(state.TownCenterId); add((int)state.Phase); add(state.NextDecisionTick); add(state.Decisions);
                add(state.Sequence); add(state.CommandsIssued); add(state.StagingCell); add(state.ScoutCell);
                add(cfg.DecisionInterval); add(cfg.VillagerTarget); add(cfg.MilitaryVillagers); add(cfg.WaveSize); add(cfg.ArmyLimit); add(cfg.BaseRadius);
                add(state.Resources.Count);
                foreach (var memory in state.Resources.Values) { add(memory.Id); add(memory.Cell); add((int)memory.Type); add(memory.LastKnownAmount); }
                add(state.Wave.Count); foreach (int id in state.Wave) add(id);
            }
        }
    }
}
