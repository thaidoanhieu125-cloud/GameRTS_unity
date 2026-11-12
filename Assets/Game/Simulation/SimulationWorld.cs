using System;
using System.Collections.Generic;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public sealed class SimulationWorld
    {
        public const int TickRate = 20;
        private readonly List<UnitState> units = new List<UnitState>();
        private readonly IReadOnlyList<UnitState> readOnlyUnits;
        private readonly SortedDictionary<int, List<MoveCommand>> pending = new SortedDictionary<int, List<MoveCommand>>();
        private readonly SortedSet<long> received = new SortedSet<long>();
        private readonly int[] reservations;
        private readonly int[] goalReservations;
        private readonly bool[] releasedThisTick;
        public GridMap Map { get; }
        public int Tick { get; private set; }
        public IReadOnlyList<UnitState> Units => readOnlyUnits;

        public SimulationWorld(GridMap map)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
            readOnlyUnits = units.AsReadOnly();
            reservations = new int[map.Count];
            goalReservations = new int[map.Count];
            releasedThisTick = new bool[map.Count];
        }

        public UnitState AddUnit(string typeId, int ownerId, int speed, int radius, int x, int y)
        {
            if (Tick != 0) throw new InvalidOperationException("Spawn definitions are loaded before the first tick.");
            if (string.IsNullOrWhiteSpace(typeId) || ownerId <= 0 || speed < 0 || speed > 20000
                || radius <= 0 || radius >= GridMap.UnitsPerCell / 2)
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
            var unit = new UnitState(units.Count + 1, ownerId, typeId, speed, radius, cell, Map);
            units.Add(unit); reservations[cell] = goalReservations[cell] = unit.Id; return unit;
        }

        public UnitState GetUnit(int id) => id > 0 && id <= units.Count ? units[id - 1] : null;

        public bool Submit(MoveCommand command)
        {
            if (command == null || command.Tick <= Tick || (long)command.Tick > (long)Tick + 10000) return false;
            long key = ((long)command.PlayerId << 32) | (uint)command.Sequence;
            if (!received.Add(key)) return false;
            if (!pending.TryGetValue(command.Tick, out List<MoveCommand> list))
                pending.Add(command.Tick, list = new List<MoveCommand>());
            list.Add(command); return true;
        }

        public void Step()
        {
            Tick++;
            Array.Clear(releasedThisTick, 0, releasedThisTick.Length);
            foreach (UnitState unit in units) { unit.PreviousX = unit.X; unit.PreviousY = unit.Y; }
            if (pending.TryGetValue(Tick, out List<MoveCommand> commands))
            {
                commands.Sort(CompareCommands);
                foreach (MoveCommand command in commands) Apply(command);
                pending.Remove(Tick);
            }
            // Rotate priority deterministically so one low ID does not always win a junction.
            for (int i = 0; i < units.Count; i++) Advance(units[(i + Tick % units.Count) % units.Count]);
        }

        private static int CompareCommands(MoveCommand a, MoveCommand b)
        {
            int player = a.PlayerId.CompareTo(b.PlayerId);
            return player != 0 ? player : a.Sequence.CompareTo(b.Sequence);
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
            for (int i = 0; i < Map.Count; i++) { add(Map.IsWalkable(i) ? 1 : 0); add(reservations[i]); add(goalReservations[i]); }
            add(units.Count);
            foreach (UnitState u in units)
            {
                add(u.Id); add(u.OwnerId); add(u.TypeId.Length); foreach (char c in u.TypeId) add(c);
                add(u.Speed); add(u.Radius); add(u.X); add(u.Y); add(u.PreviousX); add(u.PreviousY);
                add(u.Cell); add(u.GoalCell); add(u.NextCell); add(u.PathIndex); add(u.WaitTicks); add(u.Remainder); add(u.Carry);
                add(u.SegmentProgress);
                add(u.Path.Count); foreach (int cell in u.Path) add(cell);
            }
            add(received.Count);
            foreach (long key in received) { add((int)(key >> 32)); add(unchecked((int)key)); }
            add(pending.Count);
            foreach (var pair in pending)
            {
                add(pair.Key); var sorted = new List<MoveCommand>(pair.Value); sorted.Sort(CompareCommands); add(sorted.Count);
                foreach (MoveCommand c in sorted)
                {
                    add(c.PlayerId); add(c.Sequence); add(c.TargetX); add(c.TargetY); add(c.UnitIds.Count);
                    foreach (int id in c.UnitIds) add(id);
                }
            }
            return hash;
        }
    }
}
