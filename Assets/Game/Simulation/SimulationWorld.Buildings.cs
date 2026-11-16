using System;
using System.Collections.Generic;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public sealed partial class SimulationWorld
    {
        private readonly SortedDictionary<string, BuildingDefinition> buildingDefinitions = new SortedDictionary<string, BuildingDefinition>(StringComparer.Ordinal);
        private readonly SortedDictionary<int, BuildingState> buildings = new SortedDictionary<int, BuildingState>();
        public IEnumerable<BuildingState> Buildings => buildings.Values;
        public int BuildingCount => buildings.Count;
        public int LastBuildPlayer { get; private set; }
        public int LastBuildSequence { get; private set; } = -1;
        public PlacementIssue LastBuildIssue { get; private set; }
        public int LastPlacedBuildingId { get; private set; }
        public BuildingState GetBuilding(int id) => buildings.TryGetValue(id, out BuildingState value) ? value : null;
        public BuildingDefinition GetBuildingDefinition(string id) => id != null && buildingDefinitions.TryGetValue(id, out BuildingDefinition value) ? value : null;
        public void RegisterBuildingDefinition(BuildingDefinition definition)
        {
            if (Tick != 0) throw new InvalidOperationException("Building definitions are loaded before tick 1.");
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            buildingDefinitions.Add(definition.TypeId, definition);
        }
        public BuildingState GetBuildingAt(int cell)
        {
            if (cell < 0 || cell >= Map.Count) return null;
            foreach (BuildingState building in buildings.Values)
                if (!building.IsDestroyed && InFootprint(cell, building.Cell, building.Width, building.Height)) return building;
            return null;
        }
        private bool InFootprint(int cell, int origin, int width, int height)
        {
            int x = cell % Map.Width - origin % Map.Width, y = cell / Map.Width - origin / Map.Width;
            return x >= 0 && y >= 0 && x < width && y < height;
        }
        private bool ValidBuilders(int ownerId, IReadOnlyList<int> ids)
        {
            if (ids == null || ids.Count == 0) return false;
            foreach (int id in ids)
            {
                UnitState unit = GetUnit(id);
                if (unit == null || !unit.IsAlive || unit.OwnerId != ownerId || unit.Villager == null || unit.Speed == 0) return false;
            }
            return true;
        }
        private bool CanIncreasePopulationLimit(int ownerId, int amount)
        {
            PlayerEconomyState economy = GetPlayerEconomy(ownerId);
            if (economy == null || amount < 0) return false;
            long maximum = (long)economy.MaxPopulation + amount;
            foreach (BuildingState building in buildings.Values)
                if (building.OwnerId == ownerId && !building.IsDestroyed && !building.IsComplete) maximum += building.Definition.PopulationBonus;
            return maximum <= int.MaxValue;
        }

        public PlacementIssue CheckBuildingPlacement(int ownerId, string typeId, int cell, IReadOnlyList<int> builderIds)
        {
            BuildingDefinition definition = GetBuildingDefinition(typeId);
            if (definition == null) return PlacementIssue.UnknownBuilding;
            PlayerEconomyState economy = GetPlayerEconomy(ownerId);
            if (economy == null) return PlacementIssue.InvalidPlayer;
            if (builderIds == null || builderIds.Count == 0) return PlacementIssue.NoBuilders;
            if (!ValidBuilders(ownerId, builderIds)) return PlacementIssue.InvalidBuilder;
            if (cell < 0 || cell >= Map.Count || cell % Map.Width + definition.Width > Map.Width || cell / Map.Width + definition.Height > Map.Height)
                return PlacementIssue.OutsideMap;
            if (VisionEnabled && !IsFootprintVisible(ownerId, cell, definition.Width, definition.Height, true)) return PlacementIssue.NotVisible;
            for (int y = 0; y < definition.Height; y++) for (int x = 0; x < definition.Width; x++)
            {
                int at = cell + y * Map.Width + x;
                if (GetBuildingAt(at) != null) return PlacementIssue.Occupied;
                if (!Map.IsWalkable(at)) return PlacementIssue.BlockedTerrain;
                // Includes both ends and side cells of a diagonal segment, as well as future destinations.
                if (reservations[at] != 0 || goalReservations[at] != 0 || releasedThisTick[at]) return PlacementIssue.Occupied;
            }
            if (economy.Wood < definition.WoodCost || economy.Food < definition.FoodCost || economy.Gold < definition.GoldCost)
                return PlacementIssue.InsufficientResources;
            if (!CanIncreasePopulationLimit(ownerId, definition.PopulationBonus)) return PlacementIssue.PopulationOverflow;
            var perimeter = Perimeter(cell, definition.Width, definition.Height);
            foreach (int id in builderIds)
            {
                UnitState unit = GetUnit(id); int anchor = unit.NextCell >= 0 ? unit.NextCell : unit.Cell;
                foreach (int target in perimeter)
                {
                    if (!Map.IsWalkable(target) || Unavailable(target, unit.Id)) continue;
                    if (GridPathfinder.FindPath(Map, anchor, target,
                        c => Unavailable(c, unit.Id) || InFootprint(c, cell, definition.Width, definition.Height)) != null)
                        return PlacementIssue.None;
                }
            }
            return PlacementIssue.NoAccess;
        }

        private void Apply(BuildCommand command)
        {
            var ids = LivingOwnedUnits(command.BuilderIds, command.PlayerId);
            PlacementIssue issue = ids == null ? PlacementIssue.InvalidBuilder : CheckBuildingPlacement(command.PlayerId, command.TypeId, command.Cell, ids);
            LastBuildPlayer = command.PlayerId; LastBuildSequence = command.Sequence; LastBuildIssue = issue; LastPlacedBuildingId = 0;
            if (issue != PlacementIssue.None) return;
            BuildingDefinition definition = GetBuildingDefinition(command.TypeId);
            if (!GetPlayerEconomy(command.PlayerId).TrySpendResources(definition.WoodCost, definition.FoodCost, definition.GoldCost))
            { LastBuildIssue = PlacementIssue.InsufficientResources; return; }
            var building = new BuildingState(command.PlayerId, command.Cell, definition, AllocateStructureId(command.Cell));
            buildings.Add(building.Id, building);
            for (int y = 0; y < building.Height; y++) for (int x = 0; x < building.Width; x++) Map.SetBuildingBlocked(building.Cell + y * Map.Width + x, definition.BlocksMovement);
            LastPlacedBuildingId = building.Id;
            AssignBuilders(building, ids);
        }
        private void Apply(ResumeBuildCommand command)
        {
            var ids = LivingOwnedUnits(command.BuilderIds, command.PlayerId);
            BuildingState building = GetBuilding(command.BuildingId);
            if (building == null || building.IsDestroyed || building.OwnerId != command.PlayerId || building.IsComplete || !ValidBuilders(command.PlayerId, ids)) return;
            AssignBuilders(building, ids);
        }
        private void AssignBuilders(BuildingState building, IReadOnlyList<int> ids)
        {
            foreach (int id in ids)
            {
                UnitState unit = GetUnit(id); CancelAttack(unit); VillagerState worker = unit.Villager;
                // Keep the gather target so the worker can resume after construction.
                worker.DropOffId = worker.WorkTicks = worker.RetryTicks = 0;
                worker.BuildingId = building.Id; Park(unit); StartBuilding(unit);
            }
        }
        private void StartBuilding(UnitState unit)
        {
            BuildingState building = GetBuilding(unit.Villager.BuildingId);
            if (building == null || building.IsDestroyed || building.IsComplete) { EndBuildingJob(unit); return; }
            if (FindApproach(unit, Perimeter(building.Cell, building.Width, building.Height), out int goal, out List<int> path, out int cost))
            {
                SetDestination(unit, goal, path); unit.Villager.Activity = VillagerActivity.GoingToBuild; unit.Villager.RetryTicks = 0;
            }
            else Wait(unit, VillagerActivity.WaitingToBuild);
        }
        private void EndBuildingJob(UnitState unit)
        {
            unit.Villager.BuildingId = 0; unit.Villager.Activity = VillagerActivity.Idle;
            unit.Villager.WorkTicks = unit.Villager.RetryTicks = 0; Park(unit);
            if (unit.Villager.CarriedAmount > 0) StartDropOff(unit);
            else StartResource(unit);
        }
        private void AdvanceBuilder(UnitState unit)
        {
            VillagerState worker = unit.Villager; BuildingState building = GetBuilding(worker.BuildingId);
            if (building == null || building.IsDestroyed || building.IsComplete) { EndBuildingJob(unit); return; }
            if (worker.Activity == VillagerActivity.WaitingToBuild)
            { if (--worker.RetryTicks <= 0) StartBuilding(unit); return; }
            if (unit.IsMoving) return;
            if (!Adjacent(unit.Cell, building.Cell, building.Width, building.Height)) { StartBuilding(unit); return; }
            worker.Activity = VillagerActivity.Building;
            int before = (int)((long)(building.MaxHitPoints - 1) * building.CompletedWork / building.Definition.RequiredWork);
            building.CompletedWork++;
            int after = (int)((long)(building.MaxHitPoints - 1) * building.CompletedWork / building.Definition.RequiredWork);
            building.HitPoints = Math.Min(building.MaxHitPoints, building.HitPoints + after - before);
            if (!building.IsComplete) return;
            if (building.Definition.PopulationBonus > 0 && !GetPlayerEconomy(building.OwnerId).TryIncreaseMaxPopulation(building.Definition.PopulationBonus))
                throw new InvalidOperationException("Population capacity overflow at construction completion.");
            if (building.Definition.Accepts != 0)
            {
                var depot = new DropOffState(building.OwnerId, building.Cell, building.Width, building.Height, building.Definition.Accepts, building.MaxHitPoints, building.Armor, building.Id) { Building = building };
                dropOffs.Add(building.Id, depot); allDropOffs.Add(depot.Id, depot);
            }
            if (building.Definition.Training != null)
                trainingCenters.Add(building.Id, new TrainingState(building.Id, building.OwnerId, building.Cell, building.Width, building.Height, building.Definition.Training));
            EndBuildingJob(unit);
        }
        private List<int> Perimeter(int cell, int width, int height)
        {
            var cells = new List<int>(); int x = cell % Map.Width, y = cell / Map.Width;
            for (int i = 0; i < width; i++) { cells.Add(Map.Index(x + i, y - 1)); cells.Add(Map.Index(x + i, y + height)); }
            for (int i = 0; i < height; i++) { cells.Add(Map.Index(x - 1, y + i)); cells.Add(Map.Index(x + width, y + i)); }
            return cells;
        }
        private bool Adjacent(int cell, int origin, int width, int height)
        {
            int dx = cell % Map.Width - origin % Map.Width, dy = cell / Map.Width - origin / Map.Width;
            return ((dx == -1 || dx == width) && dy >= 0 && dy < height) || ((dy == -1 || dy == height) && dx >= 0 && dx < width);
        }
    }
}
