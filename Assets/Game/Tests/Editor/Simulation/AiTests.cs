using System;
using System.Linq;
using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;

namespace GameRTS.Tests
{
    public sealed class AiTests
    {
        [TestCase(3, 1, 1, 1)]
        [TestCase(5, 2, 2, 1)]
        [TestCase(10, 4, 4, 2)]
        [TestCase(11, 4, 5, 2)]
        public void WorkerSplit(int total, int wood, int food, int gold)
        { Assert.That(AiController.WorkerTargets(total), Is.EqualTo(new[] { wood, food, gold })); }

        [Test]
        public void WorkersKeepRatio()
        {
            var w = new SimulationWorld(new GridMap(28, 28)); w.AddPlayer(2, maxPopulation: 20);
            int town = w.AddDropOff(2, w.Map.Index(12, 12), 2, 2, ResourceAcceptance.All).Id;
            w.AddTrainingCenter(town, new TrainingDefinition("villager", 3000, 450, 10, 10));
            w.AddResource("tree", ResourceType.Wood, 10000, w.Map.Index(7, 12));
            w.AddResource("berry", ResourceType.Food, 10000, w.Map.Index(18, 12));
            w.AddResource("gold", ResourceType.Gold, 10000, w.Map.Index(12, 18));
            for (int i = 0; i < 10; i++) w.AddUnit("villager", 2, 3000, 450, 9500 + i % 5 * 1000, 9500 + i / 5 * 1000, 10);
            w.ConfigureVision(); w.AddAiController(2, town);
            for (int i = 0; i < 600; i++) w.Step();
            var jobs = w.Units.Select(u => w.GetResource(u.Villager.ResourceId).ResourceType).ToList();
            Assert.That(jobs.Count(t => t == ResourceType.Wood), Is.EqualTo(4));
            Assert.That(jobs.Count(t => t == ResourceType.Food), Is.EqualTo(4));
            Assert.That(jobs.Count(t => t == ResourceType.Gold), Is.EqualTo(2));
        }
        [Test]
        public void AiPaysCosts()
        {
            var w = AiScenario.CreateWorld(); var wallet = w.GetPlayerEconomy(2); w.Step();
            Assert.That(new[] { wallet.Wood, wallet.Food, wallet.Gold, wallet.Population }, Is.EqualTo(new[] { 200, 200, 100, 3 }));
            var commands = w.GetAi(2).LastCommands;
            Assert.That(commands.Any(c => c is TrainingCommand), Is.True);
            Assert.That(commands.All(c => c.PlayerId == 2 && c.Tick == 3 && !(c is EconomyDebugCommand) && !(c is ResourceDebugCommand)), Is.True);
            w.Step(); Assert.That(wallet.Food, Is.EqualTo(200)); w.Step();
            Assert.That(wallet.Food, Is.EqualTo(150)); Assert.That(w.QueuedPopulation(2), Is.EqualTo(1));
            Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(200));
        }
        [TestCase(20)]
        [TestCase(30)]
        [TestCase(40)]
        public void FixedDecisionRate(int interval)
        {
            var w = AiScenario.CreateWorld(new AiSettings(interval));
            for (int tick = 1; tick <= 100; tick++)
            { w.Step(); Assert.That(w.GetAi(2).Decisions, Is.EqualTo(1 + (tick - 1) / interval)); }
        }
        [Test]
        public void InvalidAiSetup()
        {
            Assert.Throws<ArgumentException>(() => new AiSettings(19));
            Assert.Throws<ArgumentException>(() => new AiSettings(41));
            Assert.Throws<ArgumentException>(() => new AiSettings(villagerTarget: 4, militaryVillagers: 8));
            var w = AiScenario.CreateWorld();
            Assert.Throws<InvalidOperationException>(() => w.AddAiController(1, w.GetAi(2).TownCenterId));
            Assert.Throws<InvalidOperationException>(() => w.AddAiController(2, w.GetAi(2).TownCenterId));
        }
        [Test]
        public void HiddenInfoIgnored()
        {
            var a = AiScenario.CreateWorld(); var b = AiScenario.CreateWorld();
            b.GetPlayerEconomy(1).TryAddResources(50000, 80000, 90000);
            var secret = b.AddResource("secret", ResourceType.Gold, 77777, b.Map.Index(2, 2));
            for (int i = 0; i < 500; i++)
            {
                a.Step(); b.Step();
                Assert.That(a.GetAi(2).LastCommands.Select(Key), Is.EqualTo(b.GetAi(2).LastCommands.Select(Key)));
                Assert.That(b.GetAi(2).KnownResources.Any(r => r.Id == secret.Id), Is.False);
            }
        }
        [Test]
        public void UnseenCommandsRejected()
        {
            var w = AiScenario.CreateWorld(); int enemy = w.Units.First(u => u.OwnerId == 2).Id;
            int secretCell = w.DropOffs.First(d => d.OwnerId == 2).Cell;
            Assert.That(w.IsCellVisible(1, secretCell), Is.False);
            Assert.That(w.IsCellVisible(2, w.DropOffs.First(d => d.OwnerId == 1).Cell), Is.False);
            Assert.That(w.CheckBuildingPlacement(1, "house", w.Map.Index(50, 50), new[] { 1 }), Is.EqualTo(PlacementIssue.NotVisible));
            var source = w.ResourceNodes.First(n => !w.IsCellVisible(1, n.Cell));
            w.Submit(new AttackCommand(1, 1, 1, new[] { 1 }, AttackTargetKind.Unit, enemy));
            w.Submit(new GatherCommand(1, 1, 2, new[] { 2 }, source.Id)); w.Step();
            Assert.That(w.GetUnit(1).TargetId, Is.Zero); Assert.That(w.GetUnit(2).Villager.ResourceId, Is.Zero);
            Assert.That(w.GetPlayerEconomy(1).Wood, Is.EqualTo(200));
        }
        [Test]
        public void AggroUsesSight()
        {
            var w = new SimulationWorld(new GridMap(20, 20)); w.AddPlayer(1); w.AddPlayer(2);
            var own = w.AddUnit("spear", 1, 3000, 450, 2500, 5500, stats: new CombatStats(120, 2, 12, aggroRange: 6000));
            var enemy = w.AddUnit("enemy", 2, 3000, 450, 7500, 5500, stats: new CombatStats(120));
            w.ConfigureVision(3, 3); w.Step(); Assert.That(own.TargetId, Is.Zero);
            w.Submit(new MoveCommand(2, 1, 1, new[] { own.Id }, 5500, 5500));
            for (int i = 0; i < 45; i++) w.Step();
            Assert.That(w.IsCellVisible(1, enemy.Cell), Is.True); Assert.That(enemy.CurrentHP, Is.LessThan(120));
        }
        [Test]
        public void SightAndMemory()
        {
            var w = new SimulationWorld(new GridMap(30, 20)); w.AddPlayer(1);
            var u = w.AddUnit("scout", 1, 3000, 450, 2500, 2500); w.ConfigureVision(3, 3);
            Assert.That(w.IsCellVisible(1, w.Map.Index(5, 2)), Is.True); Assert.That(w.IsCellVisible(1, w.Map.Index(6, 2)), Is.False);
            w.Submit(new MoveCommand(1, 1, 1, new[] { u.Id }, 12500, 2500));
            for (int i = 0; i < 100; i++) w.Step();
            Assert.That(w.IsCellVisible(1, w.Map.Index(2, 2)), Is.False); Assert.That(w.IsCellExplored(1, w.Map.Index(2, 2)), Is.True);
        }
        [Test]
        public void AiRespectsPopulation()
        {
            var w = new SimulationWorld(new GridMap(24, 24)); w.AddPlayer(2, wood: 0, maxPopulation: 3);
            int town = w.AddDropOff(2, w.Map.Index(10, 10), 2, 2, ResourceAcceptance.All).Id;
            w.AddTrainingCenter(town, new TrainingDefinition("villager", 3000, 450, 10, 10));
            for (int i = 0; i < 3; i++) w.AddUnit("villager", 2, 3000, 450, 8500, 9500 + i * 1000, 10);
            w.ConfigureVision(); w.AddAiController(2, town);
            for (int i = 0; i < 500; i++) w.Step();
            Assert.That(w.GetPlayerEconomy(2).Population, Is.EqualTo(3)); Assert.That(w.QueuedPopulation(2), Is.Zero);
            Assert.That(w.GetPlayerEconomy(2).Food, Is.EqualTo(200));
        }
        [Test]
        public void SoldiersRegroup()
        {
            var w = AiScenario.CreateWorld(); int far = w.Map.Index(53, 45);
            var unit = w.AddUnit("melee", 2, 3000, 450, w.Map.CenterX(far), w.Map.CenterY(far), stats: new CombatStats(120, 2, 12));
            w.Step(); Assert.That(w.GetAi(2).LastCommands.OfType<MoveCommand>().Any(c => c.UnitIds.Contains(unit.Id)), Is.True);
            for (int i = 0; i < 220; i++) w.Step();
            int stage = w.GetAi(2).StagingCell, dx = unit.Cell % w.Map.Width - stage % w.Map.Width, dy = unit.Cell / w.Map.Width - stage / w.Map.Width;
            Assert.That(dx * dx + dy * dy, Is.LessThanOrEqualTo(25));
        }
        [Test]
        public void BuilderRecovery()
        {
            var w = AiScenario.CreateWorld(); int spot = w.Map.Index(44, 35);
            var attacker = w.AddUnit("test_attacker", 1, 20000, 450, w.Map.CenterX(spot), w.Map.CenterY(spot), stats: new CombatStats(1000, 0, 1000));
            BuildingState foundation = null;
            for (int i = 0; i < 800 && foundation == null; i++) { w.Step(); foundation = w.Buildings.FirstOrDefault(b => !b.IsComplete); }
            Assert.That(foundation, Is.Not.Null);
            var builder = w.Units.First(u => u.IsAlive && u.OwnerId == 2 && u.Villager?.BuildingId == foundation.Id);
            w.Submit(new AttackCommand(w.Tick + 1, 1, 10000, new[] { attacker.Id }, AttackTargetKind.Unit, builder.Id));
            for (int i = 0; i < 1000 && !foundation.IsComplete; i++) w.Step();
            Assert.That(builder.IsAlive, Is.False); Assert.That(foundation.IsComplete, Is.True);
            Assert.That(w.Buildings.Count(b => b.Definition.TypeId == foundation.Definition.TypeId), Is.EqualTo(1));
        }
        [Test]
        public void EconomyToAttack()
        {
            var w = AiScenario.CreateWorld(); var twin = AiScenario.CreateWorld();
            var totals = Totals(w); int lastDecision = 0;
            for (int i = 0; i < 7000 && !w.IsMatchOver; i++)
            {
                w.Step(); twin.Step();
                var state = w.GetAi(2);
                if (lastDecision == state.Decisions) continue;
                lastDecision = state.Decisions;
                Assert.That(w.ComputeChecksum(), Is.EqualTo(twin.ComputeChecksum()));
                Assert.That(Totals(w), Is.EqualTo(totals), "resources at tick " + w.Tick);
                var wallet = w.GetPlayerEconomy(2);
                Assert.That(wallet.Population + w.QueuedPopulation(2), Is.LessThanOrEqualTo(wallet.MaxPopulation));
                foreach (var attack in state.LastCommands.OfType<AttackCommand>())
                {
                    if (attack.TargetKind == AttackTargetKind.Unit) Assert.That(w.IsCellVisible(2, w.GetUnit(attack.TargetId).Cell), Is.True);
                    else
                    { var target = w.GetDropOff(attack.TargetId); Assert.That(w.IsFootprintVisible(2, target.Cell, target.Width, target.Height), Is.True); }
                }
            }
            Assert.That(w.GetAi(2).Phase, Is.EqualTo(AiPhase.Attack)); Assert.That(w.WinnerId, Is.EqualTo(2));
            foreach (string type in new[] { "house", "lumber_camp", "mining_camp", "barracks", "archery_range", "stable" })
                Assert.That(w.Buildings.Any(b => b.OwnerId == 2 && b.IsComplete && b.Definition.TypeId == type), Is.True, type);
            foreach (var type in new[] { UnitClass.Spearman, UnitClass.Archer, UnitClass.Cavalry })
                Assert.That(w.Units.Any(u => u.OwnerId == 2 && u.Stats.Class == type), Is.True);
            var counts = new[] { UnitClass.Spearman, UnitClass.Archer, UnitClass.Cavalry }.Select(c => w.Units.Count(u => u.OwnerId == 2 && u.Stats.Class == c)).ToArray();
            Assert.That(counts.Max() - counts.Min(), Is.LessThanOrEqualTo(1));
            Assert.That(w.Units.Count(u => u.OwnerId == 2 && u.Villager != null), Is.EqualTo(10));
            ulong hash = w.ComputeChecksum(); int decisions = w.GetAi(2).Decisions;
            for (int i = 0; i < 100; i++) w.Step();
            Assert.That(w.ComputeChecksum(), Is.EqualTo(hash)); Assert.That(w.GetAi(2).Decisions, Is.EqualTo(decisions));
        }
        private static long[] Totals(SimulationWorld w)
        {
            var total = new long[3];
            foreach (var r in w.ResourceNodes) total[(int)r.ResourceType] += r.Remaining;
            for (int player = 1; player <= 2; player++)
            { var e = w.GetPlayerEconomy(player); total[0] += e.Wood; total[1] += e.Food; total[2] += e.Gold; }
            foreach (var u in w.Units)
            {
                if (u.Villager != null) total[(int)u.Villager.CarriedType] += u.Villager.CarriedAmount;
                if (u.Id <= 6) continue;
                if (u.Villager != null) total[1] += 50;
                else if (u.Stats.Class == UnitClass.Spearman) { total[0] += 25; total[1] += 50; }
                else if (u.Stats.Class == UnitClass.Archer) { total[0] += 25; total[2] += 45; }
                else if (u.Stats.Class == UnitClass.Cavalry) { total[1] += 70; total[2] += 60; }
            }
            foreach (var b in w.Buildings) { total[0] += b.Definition.WoodCost; total[1] += b.Definition.FoodCost; total[2] += b.Definition.GoldCost; }
            foreach (var t in w.TrainingCenters) foreach (var q in t.Queue) { total[0] += q.PaidWood; total[1] += q.PaidFood; total[2] += q.PaidGold; }
            return total;
        }
        private static string Key(ISimulationCommand command)
        {
            string head = command.GetType().Name + ":" + command.Tick + ":" + command.Sequence;
            if (command is GatherCommand g) return head + ":" + g.NodeId + ":" + string.Join(",", g.UnitIds);
            if (command is BuildCommand b) return head + ":" + b.TypeId + ":" + b.Cell + ":" + string.Join(",", b.BuilderIds);
            if (command is TrainingCommand t) return head + ":" + t.BuildingId + ":" + t.Operation + ":" + t.Target;
            if (command is MoveCommand m) return head + ":" + m.TargetX + ":" + m.TargetY;
            if (command is AttackCommand a) return head + ":" + a.TargetKind + ":" + a.TargetId;
            return head;
        }
    }
}
