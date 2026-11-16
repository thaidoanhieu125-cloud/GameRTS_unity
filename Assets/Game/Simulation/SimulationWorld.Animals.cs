using System;
using System.Collections.Generic;
using System.Linq;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public sealed partial class SimulationWorld
    {
        private int nextResourceId;
        private static readonly CombatStats HuntingStats = new CombatStats(40, 0, 8, 20, 4000, 10000);
        public IEnumerable<UnitState> Animals => units.Where(u => u.Animal != null);

        public UnitState AddAnimal(AnimalDefinition definition, int cell)
        {
            if (Tick != 0 || definition == null || !Map.IsWalkable(cell) || reservations[cell] != 0 || GetBuildingAt(cell) != null)
                throw new ArgumentException("Load animals on empty cells before tick 1.");
            // Neutral owner 0: no economy, population or vision source. Still shares movement reservations.
            var animal = new UnitState(units.Count + 1, 0, definition.TypeId, definition.Speed, 450, cell, Map, 0, 10, definition.Stats)
            { Animal = new AnimalState(definition) };
            units.Add(animal); reservations[cell] = goalReservations[cell] = animal.Id;
            return animal;
        }
        public UnitState GetAnimalAt(int x, int y)
        {
            foreach (var u in Animals)
            {
                long dx = (long)x - u.X, dy = (long)y - u.Y;
                if (dx * dx + dy * dy <= 450L * 450 && (u.IsAlive || GetResource(u.Animal.CorpseResourceId)?.IsDepleted == false)) return u;
            }
            return null;
        }
        private CombatStats AttackStats(UnitState unit) => unit.Villager != null && unit.TargetKind == AttackTargetKind.Unit
            && GetUnit(unit.TargetId)?.Animal != null ? HuntingStats : unit.Stats;

        private void Apply(HuntCommand c)
        {
            var animal = GetUnit(c.AnimalId);
            if (animal?.Animal == null || !IsCellVisible(c.PlayerId, animal.Cell)) return;
            var ids = LivingOwnedUnits(c.UnitIds, c.PlayerId);
            if (ids == null || ids.Count == 0 || ids.Any(id => GetUnit(id).Villager == null || GetUnit(id).Speed == 0)) return;
            foreach (int id in ids)
            {
                var u = GetUnit(id); CancelWorkerJob(u); Park(u);
                u.Villager.HuntAnimalId = animal.Id;
            }
        }
        private void PrepareHunters()
        {
            foreach (var u in units)
            {
                var v = u.Villager;
                if (!u.IsAlive || v == null || v.HuntAnimalId == 0 || v.BuildingId > 0) continue;
                var animal = GetUnit(v.HuntAnimalId);
                if (animal?.Animal == null || !IsCellVisible(u.OwnerId, animal.Cell))
                { v.HuntAnimalId = 0; CancelAttack(u); Park(u); continue; }
                if (animal.IsAlive)
                { u.TargetKind = AttackTargetKind.Unit; u.TargetId = animal.Id; continue; }
                v.HuntAnimalId = 0; CancelAttack(u); Park(u);
                var meat = GetResource(animal.Animal.CorpseResourceId);
                if (meat == null || meat.IsDepleted) continue;
                v.ResourceId = meat.Id; v.WorkTicks = 0;
                if (v.CarriedAmount > 0 && (v.CarriedType != ResourceType.Food || v.CarriedAmount >= v.Capacity)) StartDropOff(u);
                else StartResource(u);
            }
        }
        private void AnimalWasHit(UnitState animal, UnitState source)
        {
            if (source == null || !source.IsAlive || source.OwnerId == 0) return;
            var a = animal.Animal; a.ThreatId = source.Id; a.NextThinkTick = Tick;
            if (a.Definition.Aggressive)
            { animal.TargetKind = AttackTargetKind.Unit; animal.TargetId = source.Id; animal.RepathAt = 0; }
            else { a.FleeUntil = Tick + 60; CancelAttack(animal); }
        }
        private void PrepareAnimals()
        {
            foreach (var u in units)
            {
                var a = u.Animal;
                if (!u.IsAlive || a == null || a.ThreatId == 0) continue;
                var threat = GetUnit(a.ThreatId);
                long dx = threat == null ? 0 : (long)u.X - threat.X, dy = threat == null ? 0 : (long)u.Y - threat.Y;
                if (threat == null || !threat.IsAlive || dx * dx + dy * dy > 160000000L)
                { a.ThreatId = 0; CancelAttack(u); Park(u); continue; }
                if (a.Definition.Aggressive) continue;
                if (Tick >= a.FleeUntil) { a.ThreatId = 0; Park(u); continue; }
                if (Tick < a.NextThinkTick || u.IsMoving) continue;
                a.NextThinkTick = Tick + 20;
                int anchor = u.Cell, tx = anchor % Map.Width + (dx >= 0 ? 4 : -4), ty = anchor / Map.Width + (dy >= 0 ? 3 : -3);
                var candidates = new List<int>();
                for (int yy = Math.Max(0, anchor / Map.Width - 5); yy <= Math.Min(Map.Height - 1, anchor / Map.Width + 5); yy++)
                    for (int xx = Math.Max(0, anchor % Map.Width - 5); xx <= Math.Min(Map.Width - 1, anchor % Map.Width + 5); xx++) candidates.Add(Map.Index(xx, yy));
                foreach (int cell in candidates.OrderBy(c => (c % Map.Width - tx) * (c % Map.Width - tx) + (c / Map.Width - ty) * (c / Map.Width - ty)).ThenBy(c => c))
                {
                    if (!Map.IsWalkable(cell) || Unavailable(cell, u.Id)) continue;
                    var path = GridPathfinder.FindPath(Map, anchor, cell, c => Unavailable(c, u.Id));
                    if (path == null) continue;
                    SetDestination(u, cell, path); break;
                }
            }
        }
        private void CreateCarcass(UnitState animal)
        {
            int cell = Map.CellAt(animal.X, animal.Y);
            animal.Cell = animal.GoalCell = cell;
            animal.X = animal.PreviousX = Map.CenterX(cell); animal.Y = animal.PreviousY = Map.CenterY(cell);
            int id = cell + 1;
            if (resources.ContainsKey(id))
            {
                if (nextResourceId == 0) nextResourceId = Map.Count + 1;
                if (nextResourceId == int.MaxValue) throw new InvalidOperationException("Resource IDs exhausted.");
                id = nextResourceId++;
            }
            var meat = new ResourceNodeState(cell, animal.TypeId + "_meat", ResourceType.Food, animal.Animal.Definition.Food, id, 150);
            resources.Add(id, meat); animal.Animal.CorpseResourceId = id;
            Map.SetResourceBlocked(cell, true);
        }
        private void HashAnimals(Action<int> add)
        {
            add(nextResourceId);
            foreach (var u in units)
            {
                add(u.Animal == null ? 0 : 1);
                if (u.Villager != null) { add(u.Villager.HuntAnimalId); add(u.Villager.BaseCapacity); }
                if (u.Animal == null) continue;
                var a = u.Animal; add((int)a.Definition.Kind); add(a.Definition.Food); add(a.ThreatId); add(a.FleeUntil); add(a.NextThinkTick); add(a.CorpseResourceId);
            }
            foreach (var r in resources.Values) add(r.GatherRatePercent);
        }
    }
}
