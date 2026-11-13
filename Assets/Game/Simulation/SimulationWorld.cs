using System;
using System.Collections.Generic;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public sealed partial class SimulationWorld
    {
        public const int TickRate = 20;
        private readonly List<UnitState> units = new List<UnitState>();
        private readonly IReadOnlyList<UnitState> readOnlyUnits;
        private readonly SortedDictionary<int, List<ISimulationCommand>> pending = new SortedDictionary<int, List<ISimulationCommand>>();
        private readonly SortedDictionary<int, PlayerEconomyState> economies = new SortedDictionary<int, PlayerEconomyState>();
        private readonly SortedDictionary<int, ResourceNodeState> resources = new SortedDictionary<int, ResourceNodeState>();
        public IEnumerable<ResourceNodeState> ResourceNodes => resources.Values;
        public int ResourceCount => resources.Count;
        public bool AllowResourceDebugCommands { get; }
        public bool AllowEconomyDebugCommands { get; }
        public int LastEconomyDebugPlayer { get; private set; }
        public int LastEconomyDebugSequence { get; private set; } = -1;
        public EconomyDebugOperation LastEconomyDebugOperation { get; private set; }
        public bool LastEconomyDebugSucceeded { get; private set; }
        private readonly SortedSet<long> received = new SortedSet<long>();
        private readonly int[] reservations;
        private readonly int[] goalReservations;
        private readonly bool[] releasedThisTick;
        public GridMap Map { get; }
        public int Tick { get; private set; }
        public IReadOnlyList<UnitState> Units => readOnlyUnits;

        public SimulationWorld(GridMap map, bool allowEconomyDebugCommands = false, bool allowResourceDebugCommands = false)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
            AllowEconomyDebugCommands = allowEconomyDebugCommands;
            AllowResourceDebugCommands = allowResourceDebugCommands;
            readOnlyUnits = units.AsReadOnly();
            reservations = new int[map.Count];
            goalReservations = new int[map.Count];
            releasedThisTick = new bool[map.Count];
        }

        public PlayerEconomyState AddPlayer(int playerId, int wood = 200, int food = 200, int gold = 100, int maxPopulation = 10)
        {
            if (Tick != 0) throw new InvalidOperationException("Players are loaded before the first tick.");
            if (playerId <= 0 || economies.ContainsKey(playerId)) throw new ArgumentException("Invalid or duplicate player.");
            var economy = new PlayerEconomyState(wood, food, gold, 0, maxPopulation);
            economies.Add(playerId, economy); return economy;
        }

        public PlayerEconomyState GetPlayerEconomy(int playerId) =>
            economies.TryGetValue(playerId, out PlayerEconomyState economy) ? economy : null;

        public ResourceNodeState AddResource(string typeId, ResourceType resourceType, int amount, int cell)
        {
            if (Tick != 0) throw new InvalidOperationException("Resource placements are loaded before the first tick.");
            if (string.IsNullOrWhiteSpace(typeId) || !Enum.IsDefined(typeof(ResourceType), resourceType) || amount <= 0)
                throw new ArgumentException("A resource needs a valid type and positive stock.");
            if (!Map.IsWalkable(cell) || reservations[cell] != 0 || resources.ContainsKey(cell + 1))
                throw new ArgumentException("A resource needs an empty, walkable cell inside the map.");
            var node = new ResourceNodeState(cell, typeId, resourceType, amount);
            resources.Add(node.Id, node); Map.SetResourceBlocked(cell, true); return node;
        }

        public ResourceNodeState GetResource(int id) => resources.TryGetValue(id, out ResourceNodeState node) ? node : null;
        public ResourceNodeState GetResourceAt(int cell) => cell >= 0 && cell < Map.Count ? GetResource(cell + 1) : null;

        private int ExtractResource(int id, int requested)
        {
            ResourceNodeState node = GetResource(id);
            if (node == null || requested <= 0 || node.IsDepleted) return 0;
            int extracted = Math.Min(node.Remaining, requested);
            node.Remaining -= extracted;
            if (node.IsDepleted) Map.SetResourceBlocked(node.Cell, false);
            return extracted;
        }

        public UnitState AddUnit(string typeId, int ownerId, int speed, int radius, int x, int y, int carryCapacity = 0, int gatherIntervalTicks = 10)
        {
            if (Tick != 0) throw new InvalidOperationException("Spawn definitions are loaded before the first tick.");
            if (string.IsNullOrWhiteSpace(typeId) || ownerId <= 0 || speed < 0 || speed > 20000
                || radius <= 0 || radius >= GridMap.UnitsPerCell / 2 || carryCapacity < 0 || carryCapacity > 1000
                || gatherIntervalTicks <= 0 || gatherIntervalTicks > 1200)
                throw new ArgumentException("Unit needs valid type/owner, speed 0..20000 and radius 1..499.");
            int near = Map.NearestCell(x, y);
            x = Map.CenterX(near); y = Map.CenterY(near);
            int cell = -1; long best = long.MaxValue;
            for (int i = 0; i < Map.Count; i++)
            {
                if (!Map.IsWalkable(i) || reservations[i] != 0) continue;
                long distance = DistanceSquared(i, x, y);
                if (distance < best) { cell = i; best = distance; }
            }
            if (cell < 0) throw new InvalidOperationException("No free spawn cell.");
            PlayerEconomyState economy = GetPlayerEconomy(ownerId) ?? AddPlayer(ownerId);
            if (!economy.TryReservePopulation(1)) throw new InvalidOperationException("Population limit reached.");
            var unit = new UnitState(units.Count + 1, ownerId, typeId, speed, radius, cell, Map, carryCapacity, gatherIntervalTicks);
            units.Add(unit); reservations[cell] = goalReservations[cell] = unit.Id; return unit;
        }

        public UnitState GetUnit(int id) => id > 0 && id <= units.Count ? units[id - 1] : null;

        public bool Submit(ISimulationCommand command)
        {
            if (command == null || command.Tick <= Tick || (long)command.Tick > (long)Tick + 10000) return false;
            if (!(command is MoveCommand) && !(command is GatherCommand) && !(command is BuildCommand) && !(command is ResumeBuildCommand)
                && !(command is EconomyDebugCommand) && !(command is ResourceDebugCommand)) return false;
            if (command is EconomyDebugCommand && (!AllowEconomyDebugCommands || GetPlayerEconomy(command.PlayerId) == null)) return false;
            if (command is ResourceDebugCommand && (!AllowResourceDebugCommands || GetPlayerEconomy(command.PlayerId) == null)) return false;
            long key = ((long)command.PlayerId << 32) | (uint)command.Sequence;
            if (!received.Add(key)) return false;
            if (!pending.TryGetValue(command.Tick, out List<ISimulationCommand> list))
                pending.Add(command.Tick, list = new List<ISimulationCommand>());
            list.Add(command); return true;
        }

        public void Step()
        {
            Tick++;
            Array.Clear(releasedThisTick, 0, releasedThisTick.Length);
            foreach (UnitState unit in units) { unit.PreviousX = unit.X; unit.PreviousY = unit.Y; }
            if (pending.TryGetValue(Tick, out List<ISimulationCommand> commands))
            {
                commands.Sort(CompareCommands);
                foreach (ISimulationCommand command in commands)
                {
                    if (command is MoveCommand move) Apply(move);
                    else if (command is GatherCommand gather) Apply(gather);
                    else if (command is BuildCommand build) Apply(build);
                    else if (command is ResumeBuildCommand resume) Apply(resume);
                    else if (command is EconomyDebugCommand economy) Apply(economy);
                    else if (command is ResourceDebugCommand resource) ExtractResource(resource.NodeId, resource.Amount);
                }
                pending.Remove(Tick);
            }
            // Rotate priority deterministically so one low ID does not always win a junction.
            for (int i = 0; i < units.Count; i++) Advance(units[(i + Tick % units.Count) % units.Count]);
            for (int i = 0; i < units.Count; i++) AdvanceVillager(units[(i + Tick % units.Count) % units.Count]);
        }

        private static int CompareCommands(ISimulationCommand a, ISimulationCommand b)
        {
            int player = a.PlayerId.CompareTo(b.PlayerId);
            return player != 0 ? player : a.Sequence.CompareTo(b.Sequence);
        }

        private void Apply(EconomyDebugCommand command)
        {
            PlayerEconomyState economy = GetPlayerEconomy(command.PlayerId);
            bool success = false;
            if (economy != null)
            {
                switch (command.Operation)
                {
                    case EconomyDebugOperation.GrantResources:
                        success = economy.TryAddResources(command.Amount, command.Amount, command.Amount); break;
                    case EconomyDebugOperation.SpendResources:
                        success = economy.TrySpendResources(command.Amount, command.Amount, command.Amount); break;
                    case EconomyDebugOperation.IncreasePopulationLimit:
                        success = CanIncreasePopulationLimit(command.PlayerId, command.Amount) && economy.TryIncreaseMaxPopulation(command.Amount); break;
                }
            }
            LastEconomyDebugPlayer = command.PlayerId; LastEconomyDebugSequence = command.Sequence;
            LastEconomyDebugOperation = command.Operation; LastEconomyDebugSucceeded = success;
        }

        private void Apply(MoveCommand command)
        {
            var selected = new HashSet<int>();
            foreach (int id in command.UnitIds)
            {
                UnitState unit = GetUnit(id);
                if (unit == null || unit.OwnerId != command.PlayerId || unit.Speed == 0) return;
                selected.Add(id);
            }
            foreach (int id in command.UnitIds)
            {
                VillagerState worker = GetUnit(id).Villager;
                if (worker == null) continue;
                worker.Activity = VillagerActivity.Idle; worker.ResourceId = worker.DropOffId = worker.WorkTicks = worker.RetryTicks = 0;
                worker.BuildingId = 0;
            }
            var claimed = new bool[Map.Count];
            foreach (UnitState unit in units)
                if (!selected.Contains(unit.Id)) claimed[unit.GoalCell] = true;
            foreach (int id in command.UnitIds) goalReservations[GetUnit(id).GoalCell] = 0;
            var candidates = new List<int>();
            for (int cell = 0; cell < Map.Count; cell++)
                if (Map.IsWalkable(cell) && !claimed[cell]) candidates.Add(cell);
            int targetCell = Map.NearestCell(command.TargetX, command.TargetY);
            int targetX = Map.CenterX(targetCell), targetY = Map.CenterY(targetCell);
            int phaseX = targetCell % Map.Width % 2, phaseY = targetCell / Map.Width % 2;
            candidates.Sort((a, b) =>
            {
                // Leave a walking lane between formation slots. A dense ring of arrived
                // units would otherwise seal the remaining destinations inside the group.
                bool slotA = a % Map.Width % 2 == phaseX && a / Map.Width % 2 == phaseY;
                bool slotB = b % Map.Width % 2 == phaseX && b / Map.Width % 2 == phaseY;
                if (slotA != slotB) return slotA ? -1 : 1;
                int distance = DistanceSquared(a, targetX, targetY).CompareTo(DistanceSquared(b, targetX, targetY));
                return distance != 0 ? distance : a.CompareTo(b);
            });
            foreach (int id in command.UnitIds)
            {
                UnitState unit = GetUnit(id);
                int anchor = unit.NextCell >= 0 ? unit.NextCell : unit.Cell;
                bool[] reachable = Map.ReachableFrom(anchor);
                foreach (int goal in candidates)
                {
                    if (claimed[goal] || !reachable[goal]) continue;
                    List<int> path = GridPathfinder.FindPath(Map, anchor, goal);
                    if (path == null) continue;
                    claimed[goal] = true; unit.GoalCell = goal;
                    goalReservations[goal] = unit.Id;
                    unit.Path = path; unit.PathIndex = 0; unit.WaitTicks = 0;
                    break;
                }
            }
        }

        private long DistanceSquared(int cell, int x, int y)
        {
            long dx = (long)Map.CenterX(cell) - x, dy = (long)Map.CenterY(cell) - y;
            // Inputs are reduced to a cell on the bounded map before formation sorting.
            return dx * dx + dy * dy;
        }

        private void Advance(UnitState unit)
        {
            int budget = (unit.Speed + unit.Remainder) / TickRate + unit.Carry;
            unit.Carry = 0;
            unit.Remainder = (unit.Speed + unit.Remainder) % TickRate;
            while (budget > 0)
            {
                if (unit.NextCell < 0)
                {
                    if (unit.PathIndex >= unit.Path.Count)
                    {
                        if (unit.Cell != unit.GoalCell) RetryPath(unit);
                        break;
                    }
                    int next = unit.Path[unit.PathIndex];
                    if (!Map.CanTraverse(unit.Cell, next, c => Unavailable(c, unit.Id)))
                    {
                        RetryPath(unit);
                        break;
                    }
                    unit.NextCell = next; unit.PathIndex++; unit.WaitTicks = 0;
                    unit.SegmentProgress = 0;
                    reservations[next] = unit.Id;
                    if (Map.IsDiagonal(unit.Cell, next))
                    {
                        reservations[Map.SideCellX(unit.Cell, next)] = unit.Id;
                        reservations[Map.SideCellY(unit.Cell, next)] = unit.Id;
                    }
                }
                int tx = Map.CenterX(unit.NextCell), ty = Map.CenterY(unit.NextCell);
                int fromX = Map.CenterX(unit.Cell), fromY = Map.CenterY(unit.Cell);
                int length = Map.StepDistance(unit.Cell, unit.NextCell);
                int distance = length - unit.SegmentProgress;
                int step = Math.Min(distance, budget);
                unit.SegmentProgress += step;
                unit.X = fromX + (tx - fromX) * unit.SegmentProgress / length;
                unit.Y = fromY + (ty - fromY) * unit.SegmentProgress / length;
                budget -= step;
                if (step < distance) break;
                ReleaseCell(unit.Cell);
                if (Map.IsDiagonal(unit.Cell, unit.NextCell))
                {
                    ReleaseCell(Map.SideCellX(unit.Cell, unit.NextCell));
                    ReleaseCell(Map.SideCellY(unit.Cell, unit.NextCell));
                }
                unit.Cell = unit.NextCell; unit.NextCell = -1; unit.SegmentProgress = 0;
                // End this tick at the corner, retaining the unused distance. Rendering may
                // interpolate safely without cutting a wall or a newly vacated unit cell.
                unit.Carry = unit.PathIndex < unit.Path.Count ? budget : 0;
                break;
            }
        }

        private void ReleaseCell(int cell)
        {
            releasedThisTick[cell] = true;
            reservations[cell] = 0;
        }

        private bool Unavailable(int cell, int unitId) => releasedThisTick[cell]
            || (reservations[cell] != 0 && reservations[cell] != unitId)
            || (goalReservations[cell] != 0 && goalReservations[cell] != unitId);

        private void RetryPath(UnitState unit)
        {
            unit.WaitTicks++;
            if (unit.WaitTicks % 6 != 0) return;
            // Equal units can otherwise choose mirrored detours forever. Break that
            // symmetry before replanning: the higher ID yields to a waiting lower ID.
            if (TryYield(unit)) return;
            List<int> alternative = GridPathfinder.FindPath(Map, unit.Cell, unit.GoalCell, c => Unavailable(c, unit.Id));
            if (alternative != null)
            {
                unit.Path = alternative; unit.PathIndex = 0; return;
            }
        }

        private bool TryYield(UnitState unit)
        {
            bool lowerIdWaiting = false;
            for (int d = 0; d < GridMap.DirectionCount; d++)
            {
                int neighbour = Map.Neighbour(unit.Cell, d);
                if (neighbour < 0) continue;
                UnitState other = GetUnit(reservations[neighbour]);
                if (other != null && other.Id < unit.Id && other.IsMoving && other.WaitTicks >= 5) lowerIdWaiting = true;
            }
            if (!lowerIdWaiting) return false;
            int parking = -1; long best = -1;
            for (int d = 0; d < GridMap.DirectionCount; d++)
            {
                int next = Map.Neighbour(unit.Cell, d);
                if (!Map.CanTraverse(unit.Cell, next, c => Unavailable(c, unit.Id))) continue;
                long distance = DistanceSquared(next, Map.CenterX(unit.GoalCell), Map.CenterY(unit.GoalCell));
                if (distance > best) { best = distance; parking = next; }
            }
            if (parking < 0) return false;
            unit.Path = new List<int> { parking }; unit.PathIndex = 0;
            return true;
        }

        // Hash the full state that can influence later ticks, including queued commands and paths.
        public ulong ComputeChecksum()
        {
            ulong hash = 14695981039346656037UL;
            Action<int> add = value =>
            {
                unchecked
                {
                    uint bits = (uint)value;
                    for (int b = 0; b < 4; b++) { hash ^= (byte)(bits >> (b * 8)); hash *= 1099511628211UL; }
                }
            };
            add(Tick); add(Map.Width); add(Map.Height); add(Map.OriginX); add(Map.OriginY);
            add(buildingDefinitions.Count);
            foreach (BuildingDefinition definition in buildingDefinitions.Values)
            {
                add(definition.TypeId.Length); foreach (char c in definition.TypeId) add(c);
                add(definition.Width); add(definition.Height); add(definition.WoodCost); add(definition.FoodCost); add(definition.GoldCost);
                add(definition.RequiredWork); add(definition.MaxHitPoints); add(definition.PopulationBonus); add((int)definition.Accepts);
            }
            add(buildings.Count);
            foreach (BuildingState building in buildings.Values)
            {
                add(building.Id); add(building.OwnerId); add(building.Cell);
                add(building.Definition.TypeId.Length); foreach (char c in building.Definition.TypeId) add(c);
                add(building.CompletedWork); add(building.HitPoints);
            }
            add(LastBuildPlayer); add(LastBuildSequence); add((int)LastBuildIssue); add(LastPlacedBuildingId);
            add(AllowEconomyDebugCommands ? 1 : 0); add(economies.Count);
            add(AllowResourceDebugCommands ? 1 : 0); add(resources.Count);
            foreach (ResourceNodeState node in resources.Values)
            {
                add(node.Id); add(node.TypeId.Length); foreach (char c in node.TypeId) add(c);
                add((int)node.ResourceType); add(node.Cell); add(node.InitialAmount); add(node.Remaining);
            }
            add(dropOffs.Count);
            foreach (DropOffState depot in dropOffs.Values)
            { add(depot.Id); add(depot.OwnerId); add(depot.Cell); add(depot.Width); add(depot.Height); add((int)depot.Accepts); }
            foreach (var pair in economies)
            {
                add(pair.Key); PlayerEconomyState e = pair.Value;
                add(e.Wood); add(e.Food); add(e.Gold); add(e.Population); add(e.MaxPopulation);
            }
            add(LastEconomyDebugPlayer); add(LastEconomyDebugSequence);
            add((int)LastEconomyDebugOperation); add(LastEconomyDebugSucceeded ? 1 : 0);
            for (int i = 0; i < Map.Count; i++)
            {
                add(Map.IsTerrainWalkable(i) ? 1 : 0); add(Map.HasResourceBlocker(i) ? 1 : 0);
                add(Map.HasBuildingBlocker(i) ? 1 : 0);
                add(reservations[i]); add(goalReservations[i]);
            }
            add(units.Count);
            foreach (UnitState u in units)
            {
                add(u.Id); add(u.OwnerId); add(u.TypeId.Length); foreach (char c in u.TypeId) add(c);
                add(u.Speed); add(u.Radius); add(u.X); add(u.Y); add(u.PreviousX); add(u.PreviousY);
                add(u.Cell); add(u.GoalCell); add(u.NextCell); add(u.PathIndex); add(u.WaitTicks); add(u.Remainder); add(u.Carry);
                add(u.SegmentProgress);
                add(u.Path.Count); foreach (int cell in u.Path) add(cell);
                VillagerState worker = u.Villager; add(worker == null ? 0 : 1);
                if (worker != null)
                {
                    add(worker.Capacity); add(worker.GatherIntervalTicks); add((int)worker.Activity);
                    add(worker.ResourceId); add(worker.DropOffId); add(worker.BuildingId); add((int)worker.CarriedType); add(worker.CarriedAmount);
                    add(worker.WorkTicks); add(worker.RetryTicks);
                }
            }
            add(received.Count);
            foreach (long key in received) { add((int)(key >> 32)); add(unchecked((int)key)); }
            add(pending.Count);
            foreach (var pair in pending)
            {
                add(pair.Key); var sorted = new List<ISimulationCommand>(pair.Value); sorted.Sort(CompareCommands); add(sorted.Count);
                foreach (ISimulationCommand c in sorted)
                {
                    add(c.PlayerId); add(c.Sequence);
                    if (c is MoveCommand move)
                    {
                        add(1); add(move.TargetX); add(move.TargetY); add(move.UnitIds.Count);
                        foreach (int id in move.UnitIds) add(id);
                    }
                    else if (c is EconomyDebugCommand economy)
                    { add(2); add((int)economy.Operation); add(economy.Amount); }
                    else if (c is ResourceDebugCommand resource)
                    { add(3); add(resource.NodeId); add(resource.Amount); }
                    else if (c is GatherCommand gather)
                    { add(4); add(gather.NodeId); add(gather.UnitIds.Count); foreach (int id in gather.UnitIds) add(id); }
                    else if (c is BuildCommand build)
                    {
                        add(5); add(build.TypeId.Length); foreach (char ch in build.TypeId) add(ch);
                        add(build.Cell); add(build.BuilderIds.Count); foreach (int id in build.BuilderIds) add(id);
                    }
                    else if (c is ResumeBuildCommand resume)
                    { add(6); add(resume.BuildingId); add(resume.BuilderIds.Count); foreach (int id in resume.BuilderIds) add(id); }
                }
            }
            return hash;
        }
    }
}
