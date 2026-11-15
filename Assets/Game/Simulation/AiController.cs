using System;
using System.Collections.Generic;
using System.Linq;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    // A player of the existing game rules. Gameplay changes below only use Submit.
    public sealed class AiController
    {
        public AiState State { get; }
        private static readonly ResourceType[] ResourceOrder = { ResourceType.Food, ResourceType.Wood, ResourceType.Gold };
        private static readonly string[] MilitaryTypes = { "barracks", "archery_range", "stable" };
        private static readonly UnitClass[] MilitaryClasses = { UnitClass.Spearman, UnitClass.Archer, UnitClass.Cavalry };
        internal AiController(AiState state) { State = state; }

        internal void Think(SimulationWorld world)
        {
            State.LastIssued.Clear(); State.Decisions++; State.NextDecisionTick = world.Tick + State.Settings.DecisionInterval;
            var town = world.GetDropOff(State.TownCenterId); if (town == null || world.IsMatchOver) return;
            var own = world.Units.Where(u => u.IsAlive && u.OwnerId == State.PlayerId).ToList();
            var workers = own.Where(u => u.Villager != null).ToList();
            foreach (var node in world.ResourceNodes)
            {
                if (!world.IsCellVisible(State.PlayerId, node.Cell)) continue;
                if (!State.Resources.TryGetValue(node.Id, out var memory)) State.Resources.Add(node.Id, memory = new AiResourceMemory(node));
                memory.LastKnownAmount = node.Remaining;
            }
            if (State.StagingCell < 0) State.StagingCell = ChooseStaging(world, town);
            AssignWorkers(world, workers);
            var army = own.Where(u => u.Villager == null && u.Stats.AttackDamage > 0).ToList();
            var stragglers = army.Where(u => !State.Wave.Contains(u.Id) && !u.IsMoving && u.TargetId == 0
                && Distance(world.Map, u.Cell, State.StagingCell) > 25).Select(u => u.Id).ToArray();
            if (stragglers.Length > 0) Send(world, seq => new MoveCommand(world.Tick + 2, State.PlayerId, seq, stragglers,
                world.Map.CenterX(State.StagingCell), world.Map.CenterY(State.StagingCell)));
            if (State.Phase == AiPhase.Economy && workers.Count >= State.Settings.MilitaryVillagers) State.Phase = AiPhase.Military;
            if (State.Phase == AiPhase.Military && ReadyAtBase(world, army).Count >= State.Settings.WaveSize) State.Phase = AiPhase.Attack;
            if (State.Phase == AiPhase.Attack) LeadArmy(world, army);

            var buildings = world.Buildings.Where(b => b.OwnerId == State.PlayerId && !b.IsDestroyed).ToList();
            var foundation = buildings.FirstOrDefault(b => !b.IsComplete);
            var wallet = world.GetPlayerEconomy(State.PlayerId);
            int free = wallet.MaxPopulation - wallet.Population - world.QueuedPopulation(State.PlayerId);
            if (free <= 2 && !buildings.Any(b => b.Definition.PopulationBonus > 0 && !b.IsComplete)
                && foundation == null && TryBuild(world, workers, "house", town.Cell)) return;
            if (foundation != null && !workers.Any(u => u.Villager.BuildingId == foundation.Id))
            {
                var builder = ChooseBuilder(world, workers, foundation.Cell);
                if (builder != null) Send(world, seq => new ResumeBuildCommand(world.Tick + 2, State.PlayerId, seq, foundation.Id, new[] { builder.Id }));
            }
            int queuedWorkers = world.TrainingCenters.Where(t => t.OwnerId == State.PlayerId && t.Definition.CarryCapacity > 0).Sum(t => t.Queue.Count);
            var tc = world.GetTraining(State.TownCenterId);
            if (workers.Count + queuedWorkers < State.Settings.VillagerTarget && tc != null && tc.Queue.Count == 0
                && free > 0 && CanPay(wallet, tc.Definition))
            { Enqueue(world, tc); return; }
            if (foundation != null) { TrainArmy(world, army, free); return; }

            foreach (var type in new[] { ResourceType.Wood, ResourceType.Gold })
            {
                string id = type == ResourceType.Wood ? "lumber_camp" : "mining_camp";
                if (buildings.Any(b => b.Definition.TypeId == id)) continue;
                var node = State.Resources.Values.Where(n => n.Type == type && n.LastKnownAmount > 0)
                    .OrderBy(n => Distance(world.Map, n.Cell, town.Cell)).ThenBy(n => n.Id).FirstOrDefault();
                if (node != null && TryBuild(world, workers, id, node.Cell)) return;
            }
            if (State.Phase == AiPhase.Economy) return;
            foreach (string type in MilitaryTypes)
                if (!buildings.Any(b => b.Definition.TypeId == type))
                { TryBuild(world, workers, type, town.Cell); return; }
            TrainArmy(world, army, free);
        }

        // Largest-remainder allocation: 10 workers -> Wood4/Food4/Gold2, never a missing worker.
        public static int[] WorkerTargets(int count)
        {
            if (count < 0 || count > 1000000) throw new ArgumentOutOfRangeException(nameof(count));
            int[] weights = { 40, 40, 20 }, values = new int[3], rest = new int[3]; int used = 0;
            for (int i = 0; i < 3; i++) { values[i] = count * weights[i] / 100; rest[i] = count * weights[i] % 100; used += values[i]; }
            while (used++ < count)
            {
                int best = (int)ResourceType.Food;
                foreach (var type in ResourceOrder) if (rest[(int)type] > rest[best]) best = (int)type;
                values[best]++; rest[best] = -1;
            }
            return values;
        }
        private void AssignWorkers(SimulationWorld world, List<UnitState> workers)
        {
            var available = workers.Where(u => u.Villager.BuildingId == 0 && u.TargetId == 0).ToList();
            int[] wanted = WorkerTargets(available.Count), assigned = new int[3];
            var loads = new Dictionary<int, int>(); var pending = new List<UnitState>();
            foreach (var unit in available)
            {
                var job = unit.Villager;
                if (job.Activity != VillagerActivity.Idle && State.Resources.TryGetValue(job.ResourceId, out var source) && source.LastKnownAmount > 0
                    && (assigned[(int)source.Type] < wanted[(int)source.Type] || job.CarriedAmount > 0))
                { assigned[(int)source.Type]++; loads[source.Id] = loads.TryGetValue(source.Id, out int load) ? load + 1 : 1; }
                else pending.Add(unit);
            }
            foreach (var unit in pending)
            {
                ResourceType choice = ResourceType.Food; AiResourceMemory target = null; int deficit = int.MinValue;
                foreach (var type in ResourceOrder)
                {
                    int need = wanted[(int)type] - assigned[(int)type];
                    if (need <= deficit) continue;
                    var candidate = State.Resources.Values.Where(n => n.Type == type && n.LastKnownAmount > 0)
                        .OrderBy(n => (loads.TryGetValue(n.Id, out int load) ? load : 0) / 3)
                        .ThenBy(n => Distance(world.Map, n.Cell, unit.Cell)).ThenBy(n => n.Id).FirstOrDefault();
                    if (candidate != null) { target = candidate; choice = type; deficit = need; }
                }
                if (target == null) continue;
                assigned[(int)choice]++; loads[target.Id] = loads.TryGetValue(target.Id, out int current) ? current + 1 : 1;
                // Do not reset an active identical job every decision; it would never harvest.
                if (unit.Villager.ResourceId == target.Id && unit.Villager.Activity != VillagerActivity.Idle) continue;
                Send(world, seq => new GatherCommand(world.Tick + 2, State.PlayerId, seq, new[] { unit.Id }, target.Id));
            }
        }

        private bool TryBuild(SimulationWorld world, List<UnitState> workers, string type, int anchor)
        {
            var definition = world.GetBuildingDefinition(type); var wallet = world.GetPlayerEconomy(State.PlayerId);
            if (definition == null || wallet.Wood < definition.WoodCost || wallet.Food < definition.FoodCost || wallet.Gold < definition.GoldCost) return false;
            var builder = ChooseBuilder(world, workers, anchor); if (builder == null) return false;
            var town = world.GetDropOff(State.TownCenterId); int r = State.Settings.BaseRadius;
            var candidates = new List<int>(); int cx = town.Cell % world.Map.Width, cy = town.Cell / world.Map.Width;
            for (int y = Math.Max(1, cy - r); y <= Math.Min(world.Map.Height - definition.Height - 1, cy + r); y++)
                for (int x = Math.Max(1, cx - r); x <= Math.Min(world.Map.Width - definition.Width - 1, cx + r); x++)
                { int cell = world.Map.Index(x, y); if (Distance(world.Map, cell, town.Cell) <= r * r) candidates.Add(cell); }
            candidates.Sort((a, b) => { int d = Distance(world.Map, a, anchor).CompareTo(Distance(world.Map, b, anchor)); return d != 0 ? d : a.CompareTo(b); });
            foreach (int cell in candidates)
            {
                if (!world.IsFootprintVisible(State.PlayerId, cell, definition.Width, definition.Height, true)
                    || !HasWalkingLanes(world, cell, definition.Width, definition.Height)) continue;
                if (world.CheckBuildingPlacement(State.PlayerId, type, cell, new[] { builder.Id }) != PlacementIssue.None) continue;
                return Send(world, seq => new BuildCommand(world.Tick + 2, State.PlayerId, seq, type, cell, new[] { builder.Id }));
            }
            return false;
        }
        private bool HasWalkingLanes(SimulationWorld world, int cell, int width, int height)
        {
            int sx = cell % world.Map.Width, sy = cell / world.Map.Width;
            for (int y = sy - 1; y <= sy + height; y++) for (int x = sx - 1; x <= sx + width; x++)
            {
                int at = world.Map.Index(x, y);
                if (world.IsCellVisible(State.PlayerId, at) && world.Map.HasBuildingBlocker(at)) return false;
            }
            for (int y = sy; y < sy + height; y++) for (int x = sx; x < sx + width; x++)
            {
                int at = world.Map.Index(x, y);
                if (Distance(world.Map, at, State.StagingCell) <= 4) return false;
                foreach (var node in State.Resources.Values)
                    if (node.LastKnownAmount > 0 && Math.Abs(x - node.Cell % world.Map.Width) <= 1 && Math.Abs(y - node.Cell / world.Map.Width) <= 1) return false;
            }
            return true;
        }
        private static UnitState ChooseBuilder(SimulationWorld world, List<UnitState> workers, int cell) => workers
            .Where(u => u.Villager.BuildingId == 0 && u.TargetId == 0)
            .OrderBy(u => u.Villager.CarriedAmount).ThenBy(u => Distance(world.Map, u.Cell, cell)).ThenBy(u => u.Id).FirstOrDefault();
        private void TrainArmy(SimulationWorld world, List<UnitState> army, int free)
        {
            var centers = world.TrainingCenters.Where(t => t.OwnerId == State.PlayerId && t.Definition.CarryCapacity == 0).ToList();
            int queued = centers.Sum(t => t.Queue.Count);
            if (free <= 0 || army.Count + queued >= State.Settings.ArmyLimit) return;
            var counts = new int[3];
            for (int i = 0; i < 3; i++) counts[i] = army.Count(u => u.Stats.Class == MilitaryClasses[i])
                + centers.Where(t => t.Definition.Stats.Class == MilitaryClasses[i]).Sum(t => t.Queue.Count);
            int min = Math.Min(counts[0], Math.Min(counts[1], counts[2]));
            foreach (var center in centers)
                if (center.RallyCell != State.StagingCell)
                { Send(world, seq => new TrainingCommand(world.Tick + 2, State.PlayerId, seq, center.BuildingId, TrainingOperation.SetRally, State.StagingCell)); return; }
            foreach (int i in Enumerable.Range(0, 3).OrderBy(i => counts[i]).ThenBy(i => i))
            {
                if (counts[i] > min) continue;
                var center = centers.FirstOrDefault(t => t.Definition.Stats.Class == MilitaryClasses[i] && t.Queue.Count == 0);
                if (center != null && CanPay(world.GetPlayerEconomy(State.PlayerId), center.Definition)) { Enqueue(world, center); return; }
            }
        }
        private static bool CanPay(PlayerEconomyState wallet, TrainingDefinition price) => wallet.Wood >= price.WoodCost && wallet.Food >= price.FoodCost && wallet.Gold >= price.GoldCost;
        private void Enqueue(SimulationWorld world, TrainingState center) => Send(world, seq => new TrainingCommand(world.Tick + 2, State.PlayerId, seq, center.BuildingId, TrainingOperation.Enqueue));
        private bool Send(SimulationWorld world, Func<int, ISimulationCommand> create)
        {
            if (State.Sequence == int.MaxValue) return false;
            var command = create(++State.Sequence);
            if (!world.Submit(command)) return false;
            State.CommandsIssued++; State.LastIssued.Add(command); return true;
        }
        private int ChooseStaging(SimulationWorld world, DropOffState town)
        {
            int x = town.Cell % world.Map.Width, y = town.Cell / world.Map.Width;
            int anchor = world.Map.Index(Math.Max(1, Math.Min(world.Map.Width - 2, x + (x < world.Map.Width / 2 ? 4 : -4))),
                Math.Max(1, Math.Min(world.Map.Height - 2, y + (y < world.Map.Height / 2 ? 2 : -2))));
            return Enumerable.Range(0, world.Map.Count).Where(c => world.IsCellVisible(State.PlayerId, c) && world.Map.IsWalkable(c))
                .OrderBy(c => Distance(world.Map, c, anchor)).ThenBy(c => c).First();
        }
        private List<UnitState> ReadyAtBase(SimulationWorld world, List<UnitState> army) => army.Where(u => !u.IsMoving && u.TargetId == 0
            && !State.Wave.Contains(u.Id) && Distance(world.Map, u.Cell, State.StagingCell) <= 25).ToList();
        private void LeadArmy(SimulationWorld world, List<UnitState> army)
        {
            State.Wave.RemoveAll(id => !army.Any(u => u.Id == id));
            var ready = ReadyAtBase(world, army);
            if (ready.Count >= State.Settings.WaveSize) { foreach (var unit in ready) State.Wave.Add(unit.Id); State.Wave.Sort(); State.ScoutCell = -1; }
            if (State.Wave.Count == 0) return;
            var group = army.Where(u => State.Wave.Contains(u.Id)).ToList(); int origin = group[0].Cell;
            var target = world.DropOffs.Where(d => d.OwnerId != State.PlayerId && world.IsFootprintVisible(State.PlayerId, d.Cell, d.Width, d.Height))
                .OrderBy(d => Distance(world.Map, d.Cell, origin)).ThenBy(d => d.Id).FirstOrDefault();
            var enemy = world.Units.Where(u => u.IsAlive && u.OwnerId != State.PlayerId && world.IsCellVisible(State.PlayerId, u.Cell))
                .OrderBy(u => Distance(world.Map, u.Cell, origin)).ThenBy(u => u.Id).FirstOrDefault();
            if (enemy != null || target != null)
            {
                var kind = enemy != null ? AttackTargetKind.Unit : AttackTargetKind.Building; int id = enemy != null ? enemy.Id : target.Id;
                if (group.Any(u => u.TargetId != id || u.TargetKind != kind))
                    Send(world, seq => new AttackCommand(world.Tick + 2, State.PlayerId, seq, group.Select(u => u.Id), kind, id));
                return;
            }
            if (group.Any(u => u.IsMoving || u.TargetId != 0)) return;
            if (State.ScoutCell < 0 || world.IsCellExplored(State.PlayerId, State.ScoutCell))
            {
                var town = world.GetDropOff(State.TownCenterId);
                int opposite = world.Map.Index(world.Map.Width - 1 - town.Cell % world.Map.Width, world.Map.Height - 1 - town.Cell / world.Map.Width);
                // A map-derived search waypoint, never an unseen enemy ID/position.
                State.ScoutCell = !world.IsCellExplored(State.PlayerId, opposite) ? opposite
                    : Enumerable.Range(0, world.Map.Count).Where(c => !world.IsCellExplored(State.PlayerId, c))
                        .OrderBy(c => Distance(world.Map, c, origin)).ThenBy(c => c).DefaultIfEmpty(-1).First();
            }
            if (State.ScoutCell >= 0) Send(world, seq => new MoveCommand(world.Tick + 2, State.PlayerId, seq, group.Select(u => u.Id),
                world.Map.CenterX(State.ScoutCell), world.Map.CenterY(State.ScoutCell)));
        }
        private static int Distance(GridMap map, int a, int b)
        { int dx = a % map.Width - b % map.Width, dy = a / map.Width - b / map.Width; return dx * dx + dy * dy; }
    }
}
