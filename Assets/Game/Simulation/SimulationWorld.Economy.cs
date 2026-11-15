using System;
using System.Collections.Generic;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public sealed partial class SimulationWorld
    {
        public const int ResourceSearchRadius = 8;
        private readonly SortedDictionary<int, DropOffState> dropOffs = new SortedDictionary<int, DropOffState>();
        public IEnumerable<DropOffState> DropOffs => dropOffs.Values;
        public int DropOffCount => dropOffs.Count;
        public DropOffState GetDropOff(int id) => dropOffs.TryGetValue(id, out DropOffState depot) ? depot : null;

        public DropOffState AddDropOff(int ownerId, int cell, int width, int height, ResourceAcceptance accepts, int maxHP = 1000, int armor = 2)
        {
            if (Tick != 0) throw new InvalidOperationException("Drop-off placements are loaded before the first tick.");
            if (maxHP < 1 || maxHP > 10000000 || armor < 0 || armor > 10000000 || GetPlayerEconomy(ownerId) == null || cell < 0 || cell >= Map.Count || width <= 0 || height <= 0
                || width > 8 || height > 8 || (int)accepts <= 0 || (accepts & ~ResourceAcceptance.All) != 0
                || cell % Map.Width + width > Map.Width || cell / Map.Width + height > Map.Height)
                throw new ArgumentException("Invalid drop-off owner, footprint or resource filter.");
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            {
                int at = cell + y * Map.Width + x;
                if (!Map.IsWalkable(at) || reservations[at] != 0) throw new ArgumentException("Drop-off footprint must be empty and walkable.");
            }
            var depot = new DropOffState(ownerId, cell, width, height, accepts, maxHP, armor, AllocateStructureId(cell));
            dropOffs.Add(depot.Id, depot); allDropOffs.Add(depot.Id, depot);
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) Map.SetBuildingBlocked(cell + y * Map.Width + x, true);
            return depot;
        }

        private void Apply(GatherCommand command)
        {
            ResourceNodeState node = GetResource(command.NodeId);
            if (node == null || node.IsDepleted || !IsCellExplored(command.PlayerId, node.Cell)) return;
            var ids = LivingOwnedUnits(command.UnitIds, command.PlayerId); if (ids == null || ids.Count == 0) return;
            foreach (int id in ids)
            {
                UnitState unit = GetUnit(id);
                if (unit == null || !unit.IsAlive || unit.OwnerId != command.PlayerId || unit.Villager == null || unit.Speed == 0) return;
            }
            foreach (int id in ids)
            {
                UnitState unit = GetUnit(id); VillagerState worker = unit.Villager;
                CancelAttack(unit); worker.BuildingId = 0;
                worker.ResourceId = node.Id; worker.WorkTicks = worker.RetryTicks = 0;
                Park(unit);
                if (worker.CarriedAmount > 0 && (worker.CarriedType != node.ResourceType || worker.CarriedAmount >= worker.Capacity))
                    StartDropOff(unit);
                else StartResource(unit);
            }
        }

        private void AdvanceVillager(UnitState unit)
        {
            VillagerState worker = unit.Villager;
            if (!unit.IsAlive || worker == null || worker.Activity == VillagerActivity.Idle) return;
            if (worker.BuildingId > 0) { AdvanceBuilder(unit); return; }
            ResourceNodeState node = GetResource(worker.ResourceId);
            if (worker.Activity == VillagerActivity.WaitingForDropOff || worker.Activity == VillagerActivity.WaitingForResource
                || worker.Activity == VillagerActivity.SearchingForResource)
            {
                if (--worker.RetryTicks > 0) return;
                if (worker.Activity == VillagerActivity.WaitingForDropOff) StartDropOff(unit);
                else StartResource(unit);
                return;
            }
            if (worker.Activity == VillagerActivity.GoingToDropOff)
            {
                DropOffState depot = GetDropOff(worker.DropOffId);
                if (unit.IsMoving) return;
                if (depot == null || depot.OwnerId != unit.OwnerId || !depot.AcceptsResource(worker.CarriedType) || !Adjacent(unit.Cell, depot))
                { StartDropOff(unit); return; }
                PlayerEconomyState economy = GetPlayerEconomy(unit.OwnerId);
                bool deposited = economy.TryAddResources(worker.CarriedType == ResourceType.Wood ? worker.CarriedAmount : 0,
                    worker.CarriedType == ResourceType.Food ? worker.CarriedAmount : 0,
                    worker.CarriedType == ResourceType.Gold ? worker.CarriedAmount : 0);
                if (!deposited) { Wait(unit, VillagerActivity.WaitingForDropOff); return; }
                worker.CarriedAmount = 0; worker.DropOffId = 0; StartResource(unit); return;
            }
            if (node == null || node.IsDepleted)
            {
                StartResource(unit);
                return;
            }
            if (unit.IsMoving) return;
            if (!Adjacent(unit.Cell, node.Cell)) { StartResource(unit); return; }
            worker.Activity = VillagerActivity.Gathering;
            if (++worker.WorkTicks < worker.GatherIntervalTicks) return;
            worker.WorkTicks = 0; worker.CarriedType = node.ResourceType;
            worker.CarriedAmount += ExtractResource(node.Id, Math.Min(1, worker.Capacity - worker.CarriedAmount));
            if (worker.CarriedAmount >= worker.Capacity || node.IsDepleted) StartDropOff(unit);
        }

        private void StartResource(UnitState unit)
        {
            VillagerState worker = unit.Villager; ResourceNodeState node = GetResource(worker.ResourceId);
            worker.WorkTicks = 0;
            if (node == null || node.IsDepleted)
            {
                if (worker.CarriedAmount > 0) StartDropOff(unit);
                else if (node != null) FindReplacementResource(unit, node);
                else { worker.Activity = VillagerActivity.Idle; Park(unit); }
                return;
            }
            if (worker.CarriedAmount > 0 && worker.CarriedType != node.ResourceType) { StartDropOff(unit); return; }
            var cells = new List<int>();
            for (int d = 0; d < 4; d++) { int cell = Map.Neighbour(node.Cell, d); if (cell >= 0) cells.Add(cell); }
            if (FindApproach(unit, cells, out int goal, out List<int> path, out int cost))
            {
                SetDestination(unit, goal, path); worker.Activity = VillagerActivity.GoingToResource; worker.RetryTicks = 0;
            }
            else Wait(unit, VillagerActivity.WaitingForResource);
        }

        private void FindReplacementResource(UnitState unit, ResourceNodeState exhausted)
        {
            ResourceNodeState best = null; int bestGoal = -1, bestCost = int.MaxValue; List<int> bestPath = null;
            bool hasNearbySource = false;
            foreach (ResourceNodeState node in resources.Values)
            {
                if (node.IsDepleted || node.ResourceType != exhausted.ResourceType || !IsCellVisible(unit.OwnerId, node.Cell)) continue;
                int dx = node.Cell % Map.Width - exhausted.Cell % Map.Width;
                int dy = node.Cell / Map.Width - exhausted.Cell / Map.Width;
                if ((long)dx * dx + (long)dy * dy > ResourceSearchRadius * ResourceSearchRadius) continue;
                hasNearbySource = true;
                var cells = new List<int>();
                for (int d = 0; d < 4; d++) { int cell = Map.Neighbour(node.Cell, d); if (cell >= 0) cells.Add(cell); }
                if (!FindApproach(unit, cells, out int goal, out List<int> path, out int cost)) continue;
                // Resource IDs are sorted; equal path costs choose the lower ID.
                if (cost < bestCost) { best = node; bestGoal = goal; bestPath = path; bestCost = cost; }
            }
            if (best != null)
            {
                unit.Villager.ResourceId = best.Id; unit.Villager.WorkTicks = unit.Villager.RetryTicks = 0;
                SetDestination(unit, bestGoal, bestPath); unit.Villager.Activity = VillagerActivity.GoingToResource;
            }
            else if (hasNearbySource) Wait(unit, VillagerActivity.SearchingForResource);
            else { unit.Villager.Activity = VillagerActivity.Idle; Park(unit); }
        }

        private void StartDropOff(UnitState unit)
        {
            VillagerState worker = unit.Villager;
            if (worker.CarriedAmount == 0) { StartResource(unit); return; }
            DropOffState bestDepot = null; int bestGoal = -1, bestCost = int.MaxValue; List<int> bestPath = null;
            foreach (DropOffState depot in dropOffs.Values)
            {
                if (depot.OwnerId != unit.OwnerId || !depot.AcceptsResource(worker.CarriedType)) continue;
                if (!FindApproach(unit, Perimeter(depot), out int goal, out List<int> path, out int cost)) continue;
                // Sorted depot IDs and sorted cell ties provide a stable nearest-reachable choice.
                if (cost < bestCost) { bestCost = cost; bestDepot = depot; bestGoal = goal; bestPath = path; }
            }
            if (bestDepot == null) { worker.DropOffId = 0; Wait(unit, VillagerActivity.WaitingForDropOff); return; }
            worker.DropOffId = bestDepot.Id; worker.WorkTicks = worker.RetryTicks = 0;
            SetDestination(unit, bestGoal, bestPath); worker.Activity = VillagerActivity.GoingToDropOff;
        }

        private void Wait(UnitState unit, VillagerActivity activity)
        {
            Park(unit); unit.Villager.Activity = activity; unit.Villager.RetryTicks = TickRate;
        }

        private void Park(UnitState unit)
        {
            int anchor = unit.NextCell >= 0 ? unit.NextCell : unit.Cell;
            SetDestination(unit, anchor, new List<int>()); unit.Carry = 0;
        }

        private void SetDestination(UnitState unit, int goal, List<int> path)
        {
            if (goalReservations[unit.GoalCell] == unit.Id) goalReservations[unit.GoalCell] = 0;
            unit.GoalCell = goal; goalReservations[goal] = unit.Id;
            unit.Path = path; unit.PathIndex = unit.WaitTicks = 0;
        }

        private bool FindApproach(UnitState unit, List<int> cells, out int goal, out List<int> path, out int cost)
        {
            goal = -1; path = null; cost = int.MaxValue; cells.Sort();
            int anchor = unit.NextCell >= 0 ? unit.NextCell : unit.Cell;
            foreach (int candidate in cells)
            {
                if (!Map.IsWalkable(candidate) || Unavailable(candidate, unit.Id)) continue;
                List<int> route = GridPathfinder.FindPath(Map, anchor, candidate, c => Unavailable(c, unit.Id));
                if (route == null) continue;
                int length = 0, previous = anchor;
                foreach (int cell in route) { length += Map.StepDistance(previous, cell); previous = cell; }
                if (length < cost) { cost = length; goal = candidate; path = route; }
            }
            return goal >= 0;
        }

        private List<int> Perimeter(DropOffState depot)
        {
            var cells = new List<int>(); int x = depot.Cell % Map.Width, y = depot.Cell / Map.Width;
            for (int i = 0; i < depot.Width; i++) { cells.Add(Map.Index(x + i, y - 1)); cells.Add(Map.Index(x + i, y + depot.Height)); }
            for (int i = 0; i < depot.Height; i++) { cells.Add(Map.Index(x - 1, y + i)); cells.Add(Map.Index(x + depot.Width, y + i)); }
            return cells;
        }
        private bool Adjacent(int a, int b) => Math.Abs(a % Map.Width - b % Map.Width) + Math.Abs(a / Map.Width - b / Map.Width) == 1;
        private bool Adjacent(int cell, DropOffState depot)
        {
            int dx = cell % Map.Width - depot.Cell % Map.Width, dy = cell / Map.Width - depot.Cell / Map.Width;
            return ((dx == -1 || dx == depot.Width) && dy >= 0 && dy < depot.Height)
                || ((dy == -1 || dy == depot.Height) && dx >= 0 && dx < depot.Width);
        }
    }
}
