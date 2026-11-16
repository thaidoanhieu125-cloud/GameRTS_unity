using System;
using System.Collections.Generic;
using System.Linq;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public sealed partial class AiController
    {
        private void ManageArmy(SimulationWorld world, List<UnitState> army, int workers)
        {
            if (State.Phase == AiPhase.Economy && !State.IsRegrouping && workers >= State.Settings.MilitaryVillagers)
                State.Phase = AiPhase.Military;
            State.Wave.RemoveAll(id => !army.Any(u => u.Id == id));
            if (State.WaveInitialSize > 0 && State.Wave.Count * 3 <= State.WaveInitialSize)
                BeginRegroup(world, army);

            var town = world.GetDropOff(State.TownCenterId);
            int defenseRadius = State.Settings.BaseRadius + 2;
            var intruder = VisibleEnemies(world).Where(u => Distance(world.Map, u.Cell, town.Cell) <= defenseRadius * defenseRadius)
                .OrderBy(UnitPriority).ThenBy(u => Distance(world.Map, u.Cell, town.Cell)).ThenBy(u => u.Id).FirstOrDefault();
            if (intruder != null)
            {
                if (State.WaveInitialSize > 0) BeginRegroup(world, army);
                OrderAttack(world, army, AttackTargetKind.Unit, intruder.Id);
                return; // Economy and production still run in Think; only offensive decisions pause.
            }

            ReturnToStage(world, army.Where(u => !State.Wave.Contains(u.Id)).ToList());
            if (State.IsRegrouping)
            {
                State.Returning.RemoveAll(id => !army.Any(u => u.Id == id));
                bool home = State.Returning.All(id => IsReady(world, world.GetUnit(id)));
                // Give survivors time to walk home; a blocked/lost soldier must not freeze the AI forever.
                if (world.Tick < State.RegroupUntilTick || !home && world.Tick < State.RegroupUntilTick + 400) return;
                State.IsRegrouping = false; State.Returning.Clear();
            }
            if (State.Phase == AiPhase.Economy && workers >= State.Settings.MilitaryVillagers) State.Phase = AiPhase.Military;
            var ready = army.Where(u => !State.Wave.Contains(u.Id) && IsReady(world, u)).ToList();
            if (State.Phase == AiPhase.Military && ready.Count >= State.Settings.WaveSize && world.Tick >= State.NextWaveTick)
            {
                State.Wave.AddRange(ready.Select(u => u.Id)); State.Wave.Sort();
                State.WaveInitialSize = State.Wave.Count; State.WavesLaunched++;
                State.NextWaveTick = world.Tick + State.Settings.WaveInterval;
                State.ScoutCell = -1; State.Phase = AiPhase.Attack;
                if (State.Settings.MultiFront)
                {
                    State.ApproachUntilTick = world.Tick + 300;
                    int ox = world.Map.Width - 1 - town.Cell % world.Map.Width;
                    int oy = world.Map.Height - 1 - town.Cell / world.Map.Width;
                    for (int flank = 0; flank < 2; flank++)
                    {
                        int side = flank == 0 ? -1 : 1;
                        int cell = world.Map.Index(Math.Max(1, Math.Min(world.Map.Width - 2, ox + side * 8)),
                            Math.Max(1, Math.Min(world.Map.Height - 2, oy + side * 6)));
                        var ids = ready.Where((u, index) => index % 2 == flank).Select(u => u.Id).ToArray();
                        Send(world, seq => new MoveCommand(world.Tick + 2, State.PlayerId, seq, ids, world.Map.CenterX(cell), world.Map.CenterY(cell)));
                    }
                }
            }
            if (State.Phase == AiPhase.Attack && world.Tick >= State.ApproachUntilTick)
                LeadWave(world, army.Where(u => State.Wave.Contains(u.Id)).ToList());
        }

        private bool IsReady(SimulationWorld world, UnitState unit) => !unit.IsMoving && unit.TargetId == 0
            && Distance(world.Map, unit.Cell, State.StagingCell) <= (State.Settings.WaveSize >= 15 ? 49 : 25);

        private void BeginRegroup(SimulationWorld world, List<UnitState> army)
        {
            var survivors = army.Where(u => State.Wave.Contains(u.Id)).ToList();
            State.Returning.Clear(); State.Returning.AddRange(survivors.Select(u => u.Id));
            State.Wave.Clear(); State.WaveInitialSize = 0; State.ScoutCell = -1;
            State.Phase = AiPhase.Economy; State.IsRegrouping = true;
            State.ApproachUntilTick = 0;
            State.NextWaveTick = Math.Max(State.NextWaveTick, world.Tick + State.Settings.WaveInterval);
            State.RegroupUntilTick = world.Tick + State.Settings.RegroupTicks; State.Regroups++;
            MoveToStage(world, survivors);
        }

        private void ReturnToStage(SimulationWorld world, List<UnitState> army)
        {
            var returning = army.Where(u => u.TargetId != 0 || !u.IsMoving && Distance(world.Map, u.Cell, State.StagingCell) > (State.Settings.WaveSize >= 15 ? 49 : 25)).ToList();
            if (returning.Count > 0) MoveToStage(world, returning);
        }
        private void MoveToStage(SimulationWorld world, List<UnitState> group)
        {
            if (group.Count > 0) Send(world, seq => new MoveCommand(world.Tick + 2, State.PlayerId, seq, group.Select(u => u.Id),
                world.Map.CenterX(State.StagingCell), world.Map.CenterY(State.StagingCell)));
        }
        private IEnumerable<UnitState> VisibleEnemies(SimulationWorld world) => world.Units
            .Where(u => u.IsAlive && u.OwnerId != State.PlayerId && world.IsCellVisible(State.PlayerId, u.Cell));
        private static int UnitPriority(UnitState unit) => unit.Villager != null || unit.Stats.Class == UnitClass.Villager ? 1 : 0;
        private void OrderAttack(SimulationWorld world, List<UnitState> group, AttackTargetKind kind, int id)
        {
            var changed = group.Where(u => u.TargetId != id || u.TargetKind != kind).Select(u => u.Id).ToArray();
            if (changed.Length > 0) Send(world, seq => new AttackCommand(world.Tick + 2, State.PlayerId, seq, changed, kind, id));
        }

        private void LeadWave(SimulationWorld world, List<UnitState> group)
        {
            if (group.Count == 0) return;
            int origin = group[0].Cell;
            var enemy = VisibleEnemies(world).OrderBy(UnitPriority)
                .ThenBy(u => Distance(world.Map, u.Cell, origin)).ThenBy(u => u.Id).FirstOrDefault();
            if (enemy != null) { OrderAttack(world, group, AttackTargetKind.Unit, enemy.Id); return; }

            var targets = world.Buildings.Where(b => b.OwnerId != State.PlayerId && !b.IsDestroyed && world.CanObserveBuilding(State.PlayerId, b.Id)
                    && world.IsFootprintVisible(State.PlayerId, b.Cell, b.Width, b.Height))
                .Select(b => new { b.Id, b.Cell, Priority = 1 })
                .Concat(world.DropOffs.Where(d => d.Building == null && d.OwnerId != State.PlayerId
                        && world.IsFootprintVisible(State.PlayerId, d.Cell, d.Width, d.Height))
                    .Select(d => new { d.Id, d.Cell, Priority = d.Accepts == ResourceAcceptance.All
                        && world.GetTraining(d.Id)?.Definition.CarryCapacity > 0 ? 0 : 1 }));
            var target = targets.OrderBy(t => t.Priority).ThenBy(t => Distance(world.Map, t.Cell, origin)).ThenBy(t => t.Id).FirstOrDefault();
            if (target != null) { OrderAttack(world, group, AttackTargetKind.Building, target.Id); return; }

            // The enemy has disappeared from sight. Cancel stale pursuit before searching, using a normal Move.
            if (group.Any(u => u.TargetId != 0)) { Scout(world, group, origin); return; }
            if (!group.Any(u => u.IsMoving)) Scout(world, group, origin);
        }
        private void Scout(SimulationWorld world, List<UnitState> group, int origin)
        {
            if (State.ScoutCell < 0 || world.IsCellExplored(State.PlayerId, State.ScoutCell))
            {
                var town = world.GetDropOff(State.TownCenterId);
                int ox = world.Map.Width - 1 - town.Cell % world.Map.Width, oy = world.Map.Height - 1 - town.Cell / world.Map.Width;
                int opposite = world.Map.Index(ox, oy);
                if (State.Settings.MultiFront && State.WavesLaunched % 3 != 0)
                    opposite = world.Map.Index(Math.Max(1, Math.Min(world.Map.Width - 2, ox + (State.WavesLaunched % 3 == 1 ? 10 : -10))), oy);
                // A public symmetric-map waypoint, never the actual position of an unseen opponent.
                State.ScoutCell = !world.IsCellExplored(State.PlayerId, opposite) ? opposite
                    : Enumerable.Range(0, world.Map.Count).Where(c => !world.IsCellExplored(State.PlayerId, c))
                        .OrderBy(c => Distance(world.Map, c, origin)).ThenBy(c => c).DefaultIfEmpty(-1).First();
            }
            if (State.ScoutCell >= 0) Send(world, seq => new MoveCommand(world.Tick + 2, State.PlayerId, seq, group.Select(u => u.Id),
                world.Map.CenterX(State.ScoutCell), world.Map.CenterY(State.ScoutCell)));
            else MoveToStage(world, group);
        }
    }
}
