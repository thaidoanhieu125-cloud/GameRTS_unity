using System;
using System.Collections.Generic;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public sealed partial class SimulationWorld
    {
        private readonly SortedDictionary<int, DropOffState> allDropOffs = new SortedDictionary<int, DropOffState>();
        private readonly SortedDictionary<int, ProjectileState> projectiles = new SortedDictionary<int, ProjectileState>();
        private int nextProjectileId = 1, nextStructureId;
        public IEnumerable<ProjectileState> Projectiles => projectiles.Values;
        public int ProjectileCount => projectiles.Count;
        public int AliveUnitCount { get { int count = 0; foreach (var unit in units) if (unit.IsAlive) count++; return count; } }
        private int AllocateStructureId(int cell)
        {
            if (!buildings.ContainsKey(cell + 1) && !allDropOffs.ContainsKey(cell + 1)) return cell + 1;
            if (nextStructureId == int.MaxValue) throw new InvalidOperationException("Structure ID space exhausted.");
            return nextStructureId++;
        }
        private struct CombatBody
        {
            public int Id, Owner, Cell, Width, Height, X, Y;
            public AttackTargetKind Kind;
            public void Point(int fromX, int fromY, out int x, out int y)
            {
                x = Math.Max(X, Math.Min(X + (Width - 1) * 1000, fromX));
                y = Math.Max(Y, Math.Min(Y + (Height - 1) * 1000, fromY));
            }
        }
        private bool TryBody(AttackTargetKind kind, int id, out CombatBody body)
        {
            body = new CombatBody { Id = id, Kind = kind };
            if (kind == AttackTargetKind.Unit)
            {
                var unit = GetUnit(id); if (unit == null || !unit.IsAlive) return false;
                body.Owner = unit.OwnerId; body.Cell = unit.Cell; body.Width = body.Height = 1; body.X = unit.X; body.Y = unit.Y;
                return true;
            }
            var building = GetBuilding(id);
            if (building != null)
            {
                if (building.IsDestroyed) return false;
                body.Owner = building.OwnerId; body.Cell = building.Cell; body.Width = building.Width; body.Height = building.Height;
            }
            else
            {
                if (!allDropOffs.TryGetValue(id, out var depot) || depot.IsDestroyed) return false;
                body.Owner = depot.OwnerId; body.Cell = depot.Cell; body.Width = depot.Width; body.Height = depot.Height;
            }
            body.X = Map.CenterX(body.Cell); body.Y = Map.CenterY(body.Cell); return true;
        }
        public bool TryGetEnemyTarget(int owner, int x, int y, out AttackTargetKind kind, out int id)
        {
            kind = AttackTargetKind.Unit; id = 0; long best = long.MaxValue;
            foreach (var unit in units)
            {
                if (!unit.IsAlive || unit.OwnerId == owner || !IsCellVisible(owner, unit.Cell)) continue;
                long dx = (long)unit.X - x, dy = (long)unit.Y - y, distance = dx * dx + dy * dy;
                int radius = Math.Max(300, unit.Radius);
                if (distance <= (long)radius * radius && distance < best) { best = distance; id = unit.Id; }
            }
            if (id > 0) return true;
            int cell = Map.CellAt(x, y); if (cell < 0) return false;
            foreach (var building in buildings.Values)
                if (!building.IsDestroyed && CanObserveBuilding(owner, building.Id) && building.OwnerId != owner && InFootprint(cell, building.Cell, building.Width, building.Height) && IsFootprintVisible(owner, building.Cell, building.Width, building.Height))
                { kind = AttackTargetKind.Building; id = building.Id; return true; }
            foreach (var depot in dropOffs.Values)
                if (depot.OwnerId != owner && InFootprint(cell, depot.Cell, depot.Width, depot.Height) && IsFootprintVisible(owner, depot.Cell, depot.Width, depot.Height))
                { kind = AttackTargetKind.Building; id = depot.Id; return true; }
            return false;
        }
        private void CancelAttack(UnitState unit) { unit.TargetId = unit.RepathAt = 0; }
        private IReadOnlyList<int> LivingOwnedUnits(IReadOnlyList<int> ids, int owner)
        {
            var alive = new List<int>();
            foreach (int id in ids)
            {
                var unit = GetUnit(id);
                if (unit == null || unit.OwnerId != owner) return null;
                if (unit.IsAlive) alive.Add(id);
            }
            return alive;
        }
        private void CancelWorkerJob(UnitState unit)
        {
            var worker = unit.Villager; if (worker == null) return;
            worker.HuntAnimalId = 0; worker.Activity = VillagerActivity.Idle; worker.BuildingId = worker.ResourceId = worker.DropOffId = worker.WorkTicks = worker.RetryTicks = 0;
        }
        private void Apply(AttackCommand command)
        {
            if (!TryBody(command.TargetKind, command.TargetId, out var target) || target.Owner == command.PlayerId || !CanSeeBody(command.PlayerId, target)) return;
            var ids = LivingOwnedUnits(command.UnitIds, command.PlayerId); if (ids == null || ids.Count == 0) return;
            foreach (int id in ids)
            {
                var unit = GetUnit(id);
                if (unit == null || !unit.IsAlive || unit.OwnerId != command.PlayerId || unit.Stats.AttackDamage == 0 && !(target.Kind == AttackTargetKind.Unit && GetUnit(target.Id)?.Animal != null && unit.Villager != null)) return;
            }
            foreach (int id in ids)
            {
                var unit = GetUnit(id); CancelWorkerJob(unit); Park(unit);
                unit.TargetKind = command.TargetKind; unit.TargetId = command.TargetId; unit.RepathAt = 0;
                if (unit.Villager != null && command.TargetKind == AttackTargetKind.Unit && GetUnit(command.TargetId)?.Animal != null)
                    unit.Villager.HuntAnimalId = command.TargetId;
            }
        }
        private bool IsIdle(UnitState unit) => !unit.IsMoving && (unit.Villager == null || unit.Villager.Activity == VillagerActivity.Idle);
        private void PrepareCombat()
        {
            foreach (var unit in units)
            {
                if (!unit.IsAlive || AttackStats(unit).AttackDamage == 0) continue;
                if (unit.Animal == null && unit.TargetId == 0 && IsIdle(unit) && unit.Stats.AggroRange > 0) AcquireTarget(unit);
                if (unit.TargetId == 0) continue;
                if (!TryBody(unit.TargetKind, unit.TargetId, out var target) || target.Owner == unit.OwnerId)
                { CancelAttack(unit); Park(unit); continue; }
                if (unit.TargetKind == AttackTargetKind.Building && !CanObserveBuilding(unit.OwnerId,target.Id))
                { CancelAttack(unit); Park(unit); continue; }
                if (unit.Animal == null && unit.TargetKind == AttackTargetKind.Unit && !CanSeeBody(unit.OwnerId, target))
                { CancelAttack(unit); Park(unit); continue; }
                if (InAttackRange(unit, unit.X, unit.Y, target))
                { if (unit.IsMoving) Park(unit); continue; }
                if (unit.Speed == 0 || Tick < unit.RepathAt) continue;
                unit.RepathAt = Tick + 10;
                if (FindAttackPosition(unit, target, out int goal, out var path)) SetDestination(unit, goal, path);
                else Park(unit);
            }
        }
        private void AcquireTarget(UnitState unit)
        {
            long best = (long)unit.Stats.AggroRange * unit.Stats.AggroRange + 1;
            foreach (var other in units)
                if (other.IsAlive && other.Animal == null && other.OwnerId != unit.OwnerId && TryBody(AttackTargetKind.Unit, other.Id, out var target) && CanSeeBody(unit.OwnerId, target)) PickTarget(unit, target, ref best);
            foreach (var building in buildings.Values)
                if (!building.IsDestroyed && building.OwnerId != unit.OwnerId && TryBody(AttackTargetKind.Building, building.Id, out var target) && CanSeeBody(unit.OwnerId, target)) PickTarget(unit, target, ref best);
            foreach (var depot in dropOffs.Values)
                if (depot.Building == null && depot.OwnerId != unit.OwnerId && TryBody(AttackTargetKind.Building, depot.Id, out var target) && CanSeeBody(unit.OwnerId, target)) PickTarget(unit, target, ref best);
        }
        private static void PickTarget(UnitState unit, CombatBody target, ref long best)
        {
            target.Point(unit.X, unit.Y, out int x, out int y); long dx = (long)x - unit.X, dy = (long)y - unit.Y;
            long distance = dx * dx + dy * dy;
            // Units, then structures, in stable ID order. Equal distances keep the first.
            if (distance >= best) return;
            best = distance; unit.TargetKind = target.Kind; unit.TargetId = target.Id; unit.RepathAt = 0;
        }
        private bool InAttackRange(UnitState unit, int x, int y, CombatBody target)
        {
            target.Point(x, y, out int tx, out int ty); long dx = (long)x - tx, dy = (long)y - ty;
            var stats = AttackStats(unit);
            if (stats.IsRanged) return dx * dx + dy * dy <= (long)stats.AttackRange * stats.AttackRange;
            if (Math.Abs(dx) > 1000 || Math.Abs(dy) > 1000) return false;
            int from = Map.CellAt(x, y), to = Map.CellAt(tx, ty);
            if (from < 0 || to < 0) return false;
            if (from % Map.Width != to % Map.Width && from / Map.Width != to / Map.Width)
                return Map.IsWalkable(Map.SideCellX(from, to)) && Map.IsWalkable(Map.SideCellY(from, to));
            return true;
        }
        private bool FindAttackPosition(UnitState unit, CombatBody target, out int goal, out List<int> path)
        {
            goal = -1; path = null; int radius = (AttackStats(unit).AttackRange + 999) / 1000;
            int tx = target.Cell % Map.Width, ty = target.Cell / Map.Width;
            var cells = new List<int>(); int anchor = unit.NextCell >= 0 ? unit.NextCell : unit.Cell;
            for (int y = Math.Max(0, ty - radius); y <= Math.Min(Map.Height - 1, ty + target.Height - 1 + radius); y++)
                for (int x = Math.Max(0, tx - radius); x <= Math.Min(Map.Width - 1, tx + target.Width - 1 + radius); x++)
                {
                    int cell = Map.Index(x, y);
                    if (Map.IsWalkable(cell) && !Unavailable(cell, unit.Id) && InAttackRange(unit, Map.CenterX(cell), Map.CenterY(cell), target)) cells.Add(cell);
                }
            cells.Sort((a, b) => { int order = DistanceSquared(a, Map.CenterX(anchor), Map.CenterY(anchor)).CompareTo(DistanceSquared(b, Map.CenterX(anchor), Map.CenterY(anchor))); return order != 0 ? order : a.CompareTo(b); });
            foreach (int cell in cells)
            {
                var route = GridPathfinder.FindPath(Map, anchor, cell, c => Unavailable(c, unit.Id));
                if (route != null) { goal = cell; path = route; return true; }
            }
            return false;
        }
        private void AdvanceCombat()
        {
            foreach (var unit in units)
            {
                if (!unit.IsAlive || unit.TargetId == 0 || unit.IsMoving || Tick < unit.NextAttackTick
                    || !TryBody(unit.TargetKind, unit.TargetId, out var target) || !InAttackRange(unit, unit.X, unit.Y, target)) continue;
                var stats = AttackStats(unit);
                unit.NextAttackTick = Tick + stats.AttackIntervalTicks; unit.LastAttackTick = Tick;
                if (stats.IsRanged)
                {
                    if (nextProjectileId == int.MaxValue) throw new InvalidOperationException("Projectile ID space exhausted.");
                    var arrow = new ProjectileState(nextProjectileId++, unit, Tick, stats); projectiles.Add(arrow.Id, arrow);
                }
                else DamageTarget(unit.TargetKind, unit.TargetId, stats.AttackDamage, unit.OwnerId, unit.Id, stats.Class);
            }
            AdvanceProjectiles();
        }
        private void AdvanceProjectiles()
        {
            var remove = new List<int>();
            foreach (var arrow in projectiles.Values)
            {
                if (!TryBody(arrow.TargetKind, arrow.TargetId, out var target) || target.Owner == arrow.OwnerId) { remove.Add(arrow.Id); continue; }
                arrow.PreviousX = arrow.X; arrow.PreviousY = arrow.Y;
                if (arrow.LaunchTick == Tick) continue;
                target.Point(arrow.X, arrow.Y, out int x, out int y);
                long dx = (long)x - arrow.X, dy = (long)y - arrow.Y;
                int distance = IntegerDistance(dx * dx + dy * dy);
                int budget = (arrow.Speed + arrow.Remainder) / TickRate; arrow.Remainder = (arrow.Speed + arrow.Remainder) % TickRate;
                if (distance <= budget)
                { DamageTarget(arrow.TargetKind, arrow.TargetId, arrow.Damage, arrow.OwnerId, arrow.SourceId, arrow.SourceClass); remove.Add(arrow.Id); }
                else { arrow.X += (int)(dx * budget / distance); arrow.Y += (int)(dy * budget / distance); }
            }
            foreach (int id in remove) projectiles.Remove(id);
        }
        private static int IntegerDistance(long square)
        {
            long low = 0, high = 4000000;
            while (low < high) { long mid = (low + high) / 2; if (mid * mid >= square) high = mid; else low = mid + 1; }
            return (int)low;
        }
        private void DamageTarget(AttackTargetKind kind, int id, int damage, int owner, int sourceId, UnitClass sourceClass)
        {
            if (!TryBody(kind, id, out var target) || target.Owner == owner) return;
            if (kind == AttackTargetKind.Unit)
            {
                var victim = GetUnit(id); victim.CurrentHP = Math.Max(0, victim.CurrentHP - CombatStats.DamageAgainst(damage, sourceClass, victim.Stats.Class, victim.Armor)); victim.LastDamageTick = Tick;
                if (!victim.IsAlive) { KillUnit(victim); return; }
                var source = GetUnit(sourceId);
                if (victim.Animal != null) { AnimalWasHit(victim, source); return; }
                if (victim.TargetId == 0 && victim.Stats.AttackDamage > 0 && IsIdle(victim) && source != null && source.IsAlive && source.OwnerId != victim.OwnerId && IsCellVisible(victim.OwnerId, source.Cell))
                { victim.TargetKind = AttackTargetKind.Unit; victim.TargetId = sourceId; victim.RepathAt = 0; }
                return;
            }
            var building = GetBuilding(id);
            if (building != null)
            {
                building.HitPoints = Math.Max(0, building.HitPoints - Math.Max(1, damage - building.Armor));
                if (building.HitPoints == 0) DestroyStructure(id, building.Cell, building.Width, building.Height, building);
            }
            else if (allDropOffs.TryGetValue(id, out var depot))
            {
                depot.CurrentHP = Math.Max(0, depot.CurrentHP - Math.Max(1, damage - depot.Armor));
                if (depot.IsDestroyed) DestroyStructure(id, depot.Cell, depot.Width, depot.Height, null);
            }
        }
        private void KillUnit(UnitState unit)
        {
            // Keep a dead record so IDs and in-flight/queued commands can never hit a replacement.
            for (int cell = 0; cell < Map.Count; cell++)
            {
                if (reservations[cell] == unit.Id) ReleaseCell(cell);
                if (goalReservations[cell] == unit.Id) goalReservations[cell] = 0;
            }
            if (unit.Animal == null) GetPlayerEconomy(unit.OwnerId).TryReleasePopulation(1);
            CancelAttack(unit); CancelWorkerJob(unit);
            if (unit.Villager != null) unit.Villager.CarriedAmount = 0;
            unit.Path.Clear(); unit.PathIndex = unit.WaitTicks = unit.Remainder = unit.Carry = unit.SegmentProgress = 0;
            unit.NextCell = -1; unit.GoalCell = unit.Cell;
            if (unit.Animal != null) CreateCarcass(unit);
        }
        private void DestroyStructure(int id, int cell, int width, int height, BuildingState building)
        {
            if (building != null)
            {
                if (building.IsDestroyed) return;
                if (building.IsComplete && building.Definition.PopulationBonus > 0)
                    GetPlayerEconomy(building.OwnerId).TryDecreaseMaxPopulation(building.Definition.PopulationBonus);
                building.IsDestroyed = true;
            }
            dropOffs.Remove(id); trainingCenters.Remove(id);
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) Map.SetBuildingBlocked(cell + y * Map.Width + x, false);
        }
        private void HashCombat(Action<int> add)
        {
            Action<CombatStats> stats = s => { add(s.MaxHP); add(s.Armor); add(s.AttackDamage); add(s.AttackIntervalTicks); add(s.AttackRange); add(s.ProjectileSpeed); add(s.AggroRange); add((int)s.Class); };
            add(nextStructureId); add(nextProjectileId);
            foreach (var unit in units)
            { stats(unit.Stats); add(unit.CurrentHP); add((int)unit.TargetKind); add(unit.TargetId); add(unit.NextAttackTick); add(unit.RepathAt); add(unit.LastAttackTick); add(unit.LastDamageTick); }
            foreach (var definition in buildingDefinitions.Values)
            {
                add(definition.Armor); var training = definition.Training; add(training == null ? 0 : 1);
                if (training == null) continue;
                add(training.UnitTypeId.Length); foreach (char c in training.UnitTypeId) add(c);
                add(training.Speed); add(training.Radius); add(training.CarryCapacity); add(training.GatherIntervalTicks);
                add(training.WoodCost); add(training.FoodCost); add(training.GoldCost); add(training.DurationTicks); add(training.QueueLimit); stats(training.Stats);
            }
            foreach (var building in buildings.Values) add(building.IsDestroyed ? 1 : 0);
            add(allDropOffs.Count);
            foreach (var depot in allDropOffs.Values) { add(depot.Id); add(depot.OwnerId); add(depot.Cell); add(depot.Width); add(depot.Height); add((int)depot.Accepts); add(depot.MaxHP); add(depot.CurrentHP); add(depot.Armor); }
            foreach (var center in trainingCenters.Values) stats(center.Definition.Stats);
            add(projectiles.Count);
            foreach (var arrow in projectiles.Values)
            { add(arrow.Id); add(arrow.OwnerId); add(arrow.SourceId); add((int)arrow.TargetKind); add(arrow.TargetId); add(arrow.Damage); add((int)arrow.SourceClass); add(arrow.Speed); add(arrow.LaunchTick); add(arrow.X); add(arrow.Y); add(arrow.PreviousX); add(arrow.PreviousY); add(arrow.Remainder); }
        }
    }
}
