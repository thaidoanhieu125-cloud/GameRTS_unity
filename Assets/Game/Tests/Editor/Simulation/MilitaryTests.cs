using System.Linq;
using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;

namespace GameRTS.Tests
{
    public sealed class MilitaryTests
    {
        private static CombatStats Spear => new CombatStats(120, 2, 12, 20, unitClass: UnitClass.Spearman);
        private static CombatStats Archer => new CombatStats(45, 0, 18, 12, 6000, 10000, unitClass: UnitClass.Archer);
        private static CombatStats Horse => new CombatStats(180, 1, 16, 20, unitClass: UnitClass.Cavalry);
        private static SimulationWorld World(int wood = 500, int food = 500, int gold = 500)
        { var w = new SimulationWorld(new GridMap(24, 20)); w.AddPlayer(1, wood, food, gold, 20); w.AddPlayer(2); return w; }
        private static void Step(SimulationWorld w, int count) { for (int i = 0; i < count; i++) w.Step(); }
        private static TrainingDefinition Training(int kind, int ticks = 200) => new TrainingDefinition(kind == 0 ? "melee" : kind == 1 ? "archer" : "cavalry",
            kind == 0 ? 2000 : kind == 1 ? 3000 : 6000, 450, 0, 10, kind == 0 ? 50 : kind == 1 ? 0 : 70, ticks, 5,
            kind == 0 ? Spear : kind == 1 ? Archer : Horse, kind == 2 ? 0 : 25, kind == 0 ? 0 : kind == 1 ? 45 : 60);
        private static SimulationWorld Built(int kind, out int id, int work = 1, int ticks = 200, int wood = 500, int food = 500, int gold = 500)
        {
            var w = World(wood, food, gold); w.AddUnit("villager", 1, 3000, 450, 3500, 4500, 10);
            w.RegisterBuildingDefinition(new BuildingDefinition("military", 2, 2, 150, 0, 0, work, 600, training: Training(kind, ticks)));
            id = w.Map.Index(4, 4) + 1; w.Submit(new BuildCommand(1, 1, 1, "military", id - 1, new[] { 1 })); w.Step(); return w;
        }
        private static void Order(SimulationWorld w, int id, TrainingOperation operation = TrainingOperation.Enqueue, int target = 0, int owner = 1)
        { w.Submit(new TrainingCommand(w.Tick + 1, owner, w.Tick + 10, id, operation, target)); w.Step(); }

        [TestCase(12, 1, 29)]
        [TestCase(3, 2, 5)]
        [TestCase(1, 100, 1)]
        public void SpearBonusUsesIntegerFiveHalvesBeforeArmor(int attack, int armor, int damage)
        { Assert.That(CombatStats.DamageAgainst(attack, UnitClass.Spearman, UnitClass.Cavalry, armor), Is.EqualTo(damage)); }
        [Test]
        public void BonusIsNotAppliedToArchersOrBuildings()
        {
            Assert.That(CombatStats.DamageAgainst(12, UnitClass.Spearman, UnitClass.Archer, 1), Is.EqualTo(11));
            var w = World(); var town = w.AddDropOff(2, w.Map.Index(5, 5), 2, 2, ResourceAcceptance.All, 100, 1);
            w.AddUnit("melee", 1, 2000, 450, 4500, 5500, stats: Spear);
            w.Submit(new AttackCommand(1, 1, 1, new[] { 1 }, AttackTargetKind.Building, town.Id)); w.Step();
            Assert.That(town.CurrentHP, Is.EqualTo(89));
        }
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void ThreeDuelsDemonstrateCounterTriangle(int match)
        {
            var w = World(); CombatStats first = match == 0 ? Spear : match == 1 ? Archer : Horse;
            CombatStats second = match == 0 ? Horse : match == 1 ? Spear : Archer;
            int distance = match == 0 ? 1 : 6;
            w.AddUnit("winner", 1, match == 0 ? 2000 : match == 1 ? 3000 : 6000, 450, 2500, 5500, stats: first);
            w.AddUnit("victim", 2, match == 0 ? 6000 : match == 1 ? 2000 : 3000, 450, (2 + distance) * 1000 + 500, 5500, stats: second);
            w.Submit(new AttackCommand(1, 1, 1, new[] { 1 }, AttackTargetKind.Unit, 2));
            w.Submit(new AttackCommand(1, 2, 1, new[] { 2 }, AttackTargetKind.Unit, 1)); Step(w, 500);
            Assert.That(w.GetUnit(1).IsAlive, Is.True); Assert.That(w.GetUnit(2).IsAlive, Is.False);
        }
        [Test]
        public void FoundationCannotTrainAndCompletedBarracksIsNotDepot()
        {
            var w = Built(0, out int id, work: 4); Assert.That(w.GetTraining(id), Is.Null);
            Assert.That(w.GetTrainingAt(id - 1), Is.Null); Order(w, id);
            Assert.That(w.LastTrainingIssue, Is.EqualTo(TrainingIssue.InvalidBuilding)); Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(500));
            Step(w, 3); Assert.That(w.GetTraining(id), Is.Not.Null); Assert.That(w.GetTrainingAt(id - 1).BuildingId, Is.EqualTo(id));
            Assert.That(w.GetDropOff(id), Is.Null); Assert.That(w.GetPlayerEconomy(1).Wood, Is.EqualTo(350));
        }
        [TestCase(0, 325, 450, 500)]
        [TestCase(1, 325, 500, 455)]
        [TestCase(2, 350, 430, 440)]
        public void QueueChargesAndRefundsAllThreeResources(int kind, int wood, int food, int gold)
        {
            var w = Built(kind, out int id); Order(w, id); var wallet = w.GetPlayerEconomy(1);
            Assert.That(new[] { wallet.Wood, wallet.Food, wallet.Gold }, Is.EqualTo(new[] { wood, food, gold }));
            Order(w, id, TrainingOperation.Cancel, w.GetTraining(id).Queue[0].Id);
            Assert.That(new[] { wallet.Wood, wallet.Food, wallet.Gold }, Is.EqualTo(new[] { 350, 500, 500 }));
        }
        [TestCase(0, 160, 500, 500, TrainingIssue.NotEnoughWood)]
        [TestCase(0, 500, 49, 500, TrainingIssue.NotEnoughFood)]
        [TestCase(1, 500, 500, 44, TrainingIssue.NotEnoughGold)]
        [TestCase(2, 500, 69, 500, TrainingIssue.NotEnoughFood)]
        public void FailedMultiResourcePaymentIsAtomic(int kind, int wood, int food, int gold, TrainingIssue issue)
        {
            var w = Built(kind, out int id, wood: wood, food: food, gold: gold); Order(w, id);
            var wallet = w.GetPlayerEconomy(1); Assert.That(w.LastTrainingIssue, Is.EqualTo(issue));
            Assert.That(new[] { wallet.Wood, wallet.Food, wallet.Gold }, Is.EqualTo(new[] { wood - 150, food, gold }));
            Assert.That(w.QueuedPopulation(1), Is.Zero);
        }
        [Test]
        public void MultiResourceRefundOverflowPreservesWholeTicket()
        {
            var w = Built(1, out int id, gold: int.MaxValue); Order(w, id); var wallet = w.GetPlayerEconomy(1);
            wallet.TryAddResources(0, 0, 45); int ticket = w.GetTraining(id).Queue[0].Id; Order(w, id, TrainingOperation.Cancel, ticket);
            Assert.That(w.LastTrainingIssue, Is.EqualTo(TrainingIssue.RefundOverflow)); Assert.That(wallet.Wood, Is.EqualTo(325));
            Assert.That(w.QueuedPopulation(1), Is.EqualTo(1)); wallet.TrySpendResources(0, 0, 45); Order(w, id, TrainingOperation.Cancel, ticket);
            Assert.That(wallet.Wood, Is.EqualTo(350)); Assert.That(wallet.Gold, Is.EqualTo(int.MaxValue));
        }
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void EachBuildingSpawnsItsOwnSoldierAndRallyMovesIt(int kind)
        {
            var w = Built(kind, out int id, ticks: 2); int rally = w.Map.Index(15, 14);
            Order(w, id, TrainingOperation.SetRally, rally); Order(w, id); Step(w, 220);
            var unit = w.GetUnit(2); Assert.That(unit.Villager, Is.Null); Assert.That(unit.Stats.Class, Is.EqualTo(Training(kind).Stats.Class));
            Assert.That(unit.Cell, Is.EqualTo(rally)); Assert.That(w.GetPlayerEconomy(1).Population, Is.EqualTo(2));
        }
        [Test]
        public void MilitaryResourceRallyDoesNotStartGatherJob()
        {
            var w = World(); var tree = w.AddResource("tree", ResourceType.Wood, 100, w.Map.Index(10, 4));
            w.AddUnit("villager", 1, 3000, 450, 3500, 4500, 10);
            w.RegisterBuildingDefinition(new BuildingDefinition("military", 2, 2, 150, 0, 0, 1, 600, training: Training(0, 2)));
            int id = w.Map.Index(4, 4) + 1; w.Submit(new BuildCommand(1, 1, 1, "military", id - 1, new[] { 1 })); w.Step();
            Order(w, id, TrainingOperation.SetRally, tree.Cell); Order(w, id); Step(w, 150);
            Assert.That(w.GetUnit(2).Villager, Is.Null); Assert.That(w.GetUnit(2).IsMoving, Is.False); Assert.That(tree.Remaining, Is.EqualTo(100));
        }
        [Test]
        public void DestroyedMilitaryBuildingLosesQueueAndRebuildHasNoOldOrders()
        {
            var w = World(); w.AddUnit("villager", 1, 3000, 450, 3500, 4500, 10);
            w.AddUnit("enemy", 2, 3000, 450, 4500, 3500, stats: new CombatStats(100, 0, 1000));
            w.RegisterBuildingDefinition(new BuildingDefinition("military", 2, 2, 150, 0, 0, 1, 600, training: Training(0)));
            int id = w.Map.Index(4, 4) + 1; w.Submit(new BuildCommand(1, 1, 1, "military", id - 1, new[] { 1 })); w.Step(); Order(w, id);
            w.Submit(new AttackCommand(3, 2, 1, new[] { 2 }, AttackTargetKind.Building, id)); w.Step();
            Assert.That(w.GetTraining(id), Is.Null); Assert.That(w.QueuedPopulation(1), Is.Zero);
            Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(450)); Assert.That(w.Map.IsWalkable(id - 1), Is.True);
        }
        [TestCase(false, MatchOutcome.Victory)]
        [TestCase(true, MatchOutcome.Defeat)]
        public void FullMilitaryCycleBuildsTrainsRalliesAndEndsMatch(bool defeat, MatchOutcome outcome)
        {
            var w = MilitaryScenario.CreateWorld(defeat); var replay = MilitaryScenario.CreateWorld(defeat, true);
            for (int i = 0; i < 600; i++) { w.Step(); replay.Step(); Assert.That(w.ComputeChecksum(), Is.EqualTo(replay.ComputeChecksum())); }
            Assert.That(w.GetOutcome(1), Is.EqualTo(outcome)); Assert.That(w.Buildings.Count(), Is.EqualTo(3));
            Assert.That(w.Buildings.All(b => b.IsComplete), Is.True); Assert.That(w.QueuedPopulation(1), Is.Zero);
            foreach (var type in new[] { UnitClass.Spearman, UnitClass.Archer, UnitClass.Cavalry })
                Assert.That(w.Units.Any(u => u.OwnerId == 1 && u.Stats.Class == type), Is.True);
            var wallet = w.GetPlayerEconomy(1); Assert.That(new[] { wallet.Wood, wallet.Food, wallet.Gold }, Is.EqualTo(new[] { 0, 380, 395 }));
        }
    }
}
