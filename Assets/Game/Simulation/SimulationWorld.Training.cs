using System;
using System.Collections.Generic;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public sealed partial class SimulationWorld
    {
        private readonly SortedDictionary<int, TrainingState> trainingCenters = new SortedDictionary<int, TrainingState>();
        private int nextTrainingTicket = 1;
        public IEnumerable<TrainingState> TrainingCenters => trainingCenters.Values;
        public int LastTrainingPlayer { get; private set; }
        public int LastTrainingSequence { get; private set; } = -1;
        public TrainingIssue LastTrainingIssue { get; private set; }
        public TrainingState GetTraining(int buildingId) => trainingCenters.TryGetValue(buildingId, out TrainingState value) ? value : null;
        public TrainingState GetTrainingAt(int cell)
        {
            if (cell < 0 || cell >= Map.Count) return null;
            foreach (var state in trainingCenters.Values)
            {
                var depot = GetDropOff(state.BuildingId);
                if (InFootprint(cell, depot.Cell, depot.Width, depot.Height)) return state;
            }
            return null;
        }
        public void AddTrainingCenter(int buildingId, TrainingDefinition definition)
        {
            if (Tick != 0) throw new InvalidOperationException("Training centers are loaded before tick 1.");
            DropOffState depot = GetDropOff(buildingId);
            if (depot == null || definition == null) throw new ArgumentException("Training needs an existing owned drop-off and a definition.");
            trainingCenters.Add(buildingId, new TrainingState(buildingId, depot.OwnerId, definition));
        }
        public int QueuedPopulation(int playerId)
        {
            int total = 0;
            foreach (var state in trainingCenters.Values) if (state.OwnerId == playerId) total += state.Jobs.Count;
            return total;
        }
        private void Apply(TrainingCommand command)
        {
            LastTrainingPlayer = command.PlayerId; LastTrainingSequence = command.Sequence; LastTrainingIssue = TrainingIssue.None;
            TrainingState state = GetTraining(command.BuildingId);
            if (state == null) { LastTrainingIssue = TrainingIssue.InvalidBuilding; return; }
            if (state.OwnerId != command.PlayerId) { LastTrainingIssue = TrainingIssue.NotOwner; return; }
            var wallet = GetPlayerEconomy(command.PlayerId);
            if (command.Operation == TrainingOperation.Enqueue)
            {
                if (state.Jobs.Count >= state.Definition.QueueLimit) { LastTrainingIssue = TrainingIssue.QueueFull; return; }
                if ((long)wallet.Population + QueuedPopulation(command.PlayerId) >= wallet.MaxPopulation)
                { LastTrainingIssue = TrainingIssue.PopulationLimit; return; }
                if (nextTrainingTicket == int.MaxValue) { LastTrainingIssue = TrainingIssue.QueueFull; return; }
                if (!wallet.TrySpendResources(0, state.Definition.FoodCost, 0)) { LastTrainingIssue = TrainingIssue.NotEnoughFood; return; }
                state.Jobs.Add(new TrainingTicket(nextTrainingTicket++, state.Definition.FoodCost));
            }
            else if (command.Operation == TrainingOperation.Cancel)
            {
                int index = state.Jobs.FindIndex(j => j.Id == command.Target);
                if (index < 0) { LastTrainingIssue = TrainingIssue.InvalidTicket; return; }
                if (!wallet.TryAddResources(0, state.Jobs[index].PaidFood, 0)) { LastTrainingIssue = TrainingIssue.RefundOverflow; return; }
                state.Jobs.RemoveAt(index); if (index == 0) state.WaitingForSpawn = false;
            }
            else
            {
                if (command.Target < 0 || command.Target >= Map.Count) { LastTrainingIssue = TrainingIssue.InvalidRally; return; }
                state.RallyCell = command.Target; var source = GetResourceAt(command.Target);
                state.RallyResourceId = source != null && !source.IsDepleted ? source.Id : 0;
            }
        }
        private void AdvanceTraining()
        {
            foreach (TrainingState state in trainingCenters.Values)
            {
                if (state.Jobs.Count == 0) { state.WaitingForSpawn = false; continue; }
                TrainingTicket head = state.Jobs[0];
                if (head.ProgressTicks < state.Definition.DurationTicks) head.ProgressTicks++;
                if (head.ProgressTicks < state.Definition.DurationTicks) continue;
                DropOffState depot = GetDropOff(state.BuildingId); var cells = Perimeter(depot); cells.Sort(); int spawn = -1;
                foreach (int cell in cells)
                    if (Map.IsWalkable(cell) && reservations[cell] == 0 && goalReservations[cell] == 0 && !releasedThisTick[cell]) { spawn = cell; break; }
                var wallet = GetPlayerEconomy(state.OwnerId);
                if (spawn < 0 || !wallet.TryReservePopulation(1)) { state.WaitingForSpawn = true; continue; }
                var definition = state.Definition;
                var unit = new UnitState(units.Count + 1, state.OwnerId, definition.UnitTypeId, definition.Speed, definition.Radius,
                    spawn, Map, definition.CarryCapacity, definition.GatherIntervalTicks, definition.Stats);
                units.Add(unit); reservations[spawn] = goalReservations[spawn] = unit.Id;
                state.Jobs.RemoveAt(0); state.WaitingForSpawn = false;
                if (state.RallyResourceId > 0)
                { unit.Villager.ResourceId = state.RallyResourceId; StartResource(unit); }
                else if (state.RallyCell >= 0)
                    Apply(new MoveCommand(Tick, state.OwnerId, 0, new[] { unit.Id }, Map.CenterX(state.RallyCell), Map.CenterY(state.RallyCell)));
            }
        }
    }
}
