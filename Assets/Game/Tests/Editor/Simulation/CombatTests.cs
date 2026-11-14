using System;
using System.Linq;
using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;

namespace GameRTS.Tests
{
    public sealed class CombatTests
    {
        private static SimulationWorld World()
        { var w = new SimulationWorld(new GridMap(24, 20)); w.AddPlayer(1, maxPopulation: 20); w.AddPlayer(2, maxPopulation: 20); return w; }
        private static UnitState Unit(SimulationWorld w, int owner, int x, int y, CombatStats stats, bool worker = false)
            => w.AddUnit(worker ? "villager" : "soldier", owner, 3000, 450, x * 1000 + 500, y * 1000 + 500, worker ? 10 : 0, 10, stats);
        private static void Attack(SimulationWorld w, int attacker, int target, AttackTargetKind kind = AttackTargetKind.Unit, int sequence = 1)
            => w.Submit(new AttackCommand(w.Tick + 1, w.GetUnit(attacker).OwnerId, sequence, new[] { attacker }, kind, target));
        private static void Step(SimulationWorld w, int count) { for (int i = 0; i < count; i++) w.Step(); }

        [TestCase(12, 3, 9)]
        [TestCase(3, 99, 1)]
        public void ArmorAndCooldownApplyExactly(int damage, int armor, int received)
        {
            var w = World(); Unit(w, 1, 2, 5, new CombatStats(100, 0, damage, 20)); var target = Unit(w, 2, 3, 5, new CombatStats(100, armor));
            Attack(w, 1, 2); w.Step(); Assert.That(target.CurrentHP, Is.EqualTo(100 - received));
            Step(w, 19); Assert.That(target.CurrentHP, Is.EqualTo(100 - received)); w.Step(); Assert.That(target.CurrentHP, Is.EqualTo(100 - received * 2));
        }
        [Test]
        public void IdleVictimRetaliatesButBusyWorkerKeepsWorking()
        {
            var w = World(); var attacker = Unit(w, 1, 2, 5, new CombatStats(100, 0, 4)); var defender = Unit(w, 2, 3, 5, new CombatStats(100, 0, 3));
            Attack(w, 1, 2); w.Step(); Assert.That(defender.TargetId, Is.EqualTo(1)); Assert.That(attacker.CurrentHP, Is.EqualTo(97));
            var busy = World(); var tree = busy.AddResource("tree", ResourceType.Wood, 100, busy.Map.Index(4, 5));
            Unit(busy, 1, 2, 5, new CombatStats(100, 0, 4)); var worker = Unit(busy, 2, 3, 5, new CombatStats(100, 0, 3), true);
            busy.Submit(new GatherCommand(1, 2, 1, new[] { 2 }, tree.Id)); Attack(busy, 1, 2); Step(busy, 5);
            Assert.That(worker.TargetId, Is.Zero); Assert.That(worker.Villager.ResourceId, Is.EqualTo(tree.Id));
        }
        [Test]
        public void AggroUsesNearestEnemyAndStableTie()
        {
            var w = World(); var attacker = Unit(w, 1, 5, 5, new CombatStats(100, 0, 1, aggroRange: 6000));
            Unit(w, 2, 6, 5, new CombatStats(100)); Unit(w, 2, 5, 6, new CombatStats(100)); w.Step();
            Assert.That(attacker.TargetId, Is.EqualTo(2));
        }
        [Test]
        public void MeleeChasesMovingTargetAroundWall()
        {
            var w = World(); for (int y = 0; y < 15; y++) if (y != 10) w.Map.SetWalkable(8, y, false);
            Unit(w, 1, 3, 6, new CombatStats(100, 0, 12)); var target = Unit(w, 2, 12, 6, new CombatStats(100));
            Attack(w, 1, 2); w.Submit(new MoveCommand(40, 2, 1, new[] { 2 }, 16500, 12500)); Step(w, 600);
            Assert.That(target.CurrentHP, Is.LessThan(100)); Assert.That(w.GetUnit(1).X, Is.GreaterThan(8000));
        }
        [Test]
        public void DiagonalMeleeCannotStrikeThroughBlockedCorner()
        {
            var w = World(); w.Map.SetWalkable(3, 2, false); w.Map.SetWalkable(2, 3, false);
            Unit(w, 1, 2, 2, new CombatStats(100, 0, 12)); var target = Unit(w, 2, 3, 3, new CombatStats(100));
            Attack(w, 1, 2); w.Step(); Assert.That(target.CurrentHP, Is.EqualTo(100));
        }
        [Test]
        public void ArrowHitsOnlyAfterTravelAndArcherStaysAtRange()
        {
            var w = World(); var archer = Unit(w, 1, 2, 5, new CombatStats(100, 0, 12, 40, 5000, 2000));
            var target = Unit(w, 2, 7, 5, new CombatStats(100, 3)); Attack(w, 1, 2); w.Step();
            Assert.That(w.ProjectileCount, Is.EqualTo(1)); Assert.That(target.CurrentHP, Is.EqualTo(100));
            Step(w, 49); Assert.That(target.CurrentHP, Is.EqualTo(100)); w.Step(); Assert.That(target.CurrentHP, Is.EqualTo(91));
            Assert.That(archer.Cell, Is.EqualTo(w.Map.Index(2, 5))); Assert.That(archer.IsMoving, Is.False);
        }
        [Test]
        public void ArrowTracksMovingTargetAndOutlivesShooter()
        {
            var w = World(); var source = Unit(w, 1, 2, 5, new CombatStats(20, 0, 12, 40, 5000, 2000));
            Unit(w, 2, 3, 5, new CombatStats(100, 0, 50)); var target = Unit(w, 2, 7, 5, new CombatStats(100));
            Attack(w, 1, 3); Attack(w, 2, 1); w.Submit(new MoveCommand(2, 2, 2, new[] { 3 }, 10500, 8500));
            w.Step(); Assert.That(source.IsAlive, Is.False); Assert.That(w.ProjectileCount, Is.EqualTo(1));
            Step(w, 150); Assert.That(target.CurrentHP, Is.EqualTo(88)); Assert.That(w.ProjectileCount, Is.Zero);
        }
        [Test]
        public void DeadTargetConsumesPendingArrowsWithoutExtraDeaths()
        {
            var w = World(); Unit(w, 1, 2, 5, new CombatStats(100, 0, 100, 1, 5000, 2000)); var target = Unit(w, 2, 5, 5, new CombatStats(5));
            Attack(w, 1, 2); Step(w, 100); Assert.That(target.CurrentHP, Is.Zero); Assert.That(w.GetPlayerEconomy(2).Population, Is.Zero);
            Assert.That(w.ProjectileCount, Is.Zero); Assert.That(w.GetUnit(1).TargetId, Is.Zero);
        }
        [Test]
        public void DeathReleasesDiagonalCellsDestinationAndPopulation()
        {
            var w = World(); Unit(w, 1, 1, 1, new CombatStats(100, 0, 100, 20, 6000, 40000)); var target = Unit(w, 2, 4, 4, new CombatStats(5));
            Unit(w, 1, 1, 12, new CombatStats(40), true); w.RegisterBuildingDefinition(new BuildingDefinition("marker", 1, 1, 0, 0, 0, 200, 100));
            Attack(w, 1, 2); w.Submit(new MoveCommand(2, 2, 1, new[] { 2 }, 10500, 10500)); Step(w, 4);
            Assert.That(target.IsAlive, Is.False); Assert.That(target.IsMoving, Is.False); Assert.That(w.GetPlayerEconomy(2).Population, Is.Zero); w.Step();
            foreach (int cell in new[] { w.Map.Index(4, 4), w.Map.Index(5, 5), w.Map.Index(5, 4), w.Map.Index(4, 5), w.Map.Index(10, 10) })
                Assert.That(w.CheckBuildingPlacement(1, "marker", cell, new[] { 3 }), Is.EqualTo(PlacementIssue.None), "Reserved dead cell " + cell);
        }
        [Test]
        public void DeathLosesUnbankedCargoAndStopsHarvesting()
        {
            var w = World(); var tree = w.AddResource("tree", ResourceType.Wood, 100, w.Map.Index(4, 5));
            var worker = Unit(w, 1, 3, 5, new CombatStats(40), true); Unit(w, 2, 2, 5, new CombatStats(100, 0, 100));
            w.Submit(new GatherCommand(1, 1, 1, new[] { 1 }, tree.Id)); Step(w, 20); Assert.That(worker.Villager.CarriedAmount, Is.EqualTo(2));
            Attack(w, 2, 1); Step(w, 50); Assert.That(worker.IsAlive, Is.False); Assert.That(worker.Villager.CarriedAmount, Is.Zero);
            Assert.That(tree.Remaining, Is.EqualTo(98)); Assert.That(w.GetPlayerEconomy(1).Wood, Is.EqualTo(200)); Assert.That(w.GetPlayerEconomy(1).Population, Is.Zero);
        }
        [Test]
        public void TrainingAfterDeathUsesNewIdAndRejectsOldOrders()
        {
            var w = World(); Unit(w, 1, 2, 5, new CombatStats(100, 0, 100)); Unit(w, 2, 3, 5, new CombatStats(5));
            int cell = w.Map.Index(15, 15); var town = w.AddDropOff(2, cell, 2, 2, ResourceAcceptance.All);
            w.AddTrainingCenter(town.Id, new TrainingDefinition("villager", 3000, 450, 10, 10, durationTicks: 2));
            Attack(w, 1, 2); w.Submit(new TrainingCommand(2, 2, 1, town.Id, TrainingOperation.Enqueue));
            w.Submit(new MoveCommand(4, 2, 2, new[] { 2 }, 12500, 12500)); Step(w, 6);
            Assert.That(w.GetUnit(2).IsAlive, Is.False); Assert.That(w.GetUnit(3).IsAlive, Is.True); Assert.That(w.GetUnit(3).IsMoving, Is.False);
            Assert.That(w.GetPlayerEconomy(2).Population, Is.EqualTo(1));
        }
        [Test]
        public void GroupOrderStillMovesSurvivorsWhenOneMemberDied()
        {
            var w = World(); Unit(w, 1, 2, 5, new CombatStats(100, 0, 100)); Unit(w, 2, 3, 5, new CombatStats(5));
            var survivor = Unit(w, 2, 10, 10, new CombatStats(100)); Attack(w, 1, 2);
            w.Submit(new MoveCommand(3, 2, 1, new[] { 2, 3 }, 16500, 16500)); Step(w, 200);
            Assert.That(w.GetUnit(2).IsAlive, Is.False); Assert.That(survivor.X, Is.GreaterThan(15000)); Assert.That(survivor.IsMoving, Is.False);
        }
        [Test]
        public void DestroyedTownStopsQueueAndOpensFootprint()
        {
            var w = World(); Unit(w, 1, 4, 5, new CombatStats(100, 0, 100)); var town = w.AddDropOff(2, w.Map.Index(5, 5), 2, 2, ResourceAcceptance.All, 10);
            w.AddTrainingCenter(town.Id, new TrainingDefinition("villager", 3000, 450, 10, 10));
            w.Submit(new TrainingCommand(1, 2, 1, town.Id, TrainingOperation.Enqueue)); Attack(w, 1, town.Id, AttackTargetKind.Building); Step(w, 220);
            Assert.That(town.IsDestroyed, Is.True); Assert.That(w.GetDropOff(town.Id), Is.Null); Assert.That(w.GetTraining(town.Id), Is.Null);
            Assert.That(w.QueuedPopulation(2), Is.Zero); Assert.That(w.GetPlayerEconomy(2).Food, Is.EqualTo(150));
            Assert.That(w.Units.Count, Is.EqualTo(1)); Assert.That(w.Map.IsWalkable(town.Cell), Is.True);
        }
        [Test]
        public void DestroyedHouseRemovesCapacityAndRebuildGetsNewId()
        {
            var w = World(); Unit(w, 1, 4, 5, new CombatStats(40), true); Unit(w, 2, 5, 4, new CombatStats(100, 0, 100));
            w.RegisterBuildingDefinition(new BuildingDefinition("house", 2, 2, 50, 0, 0, 1, 50, 5)); int cell = w.Map.Index(5, 5);
            w.Submit(new BuildCommand(1, 1, 1, "house", cell, new[] { 1 })); w.Step(); Assert.That(w.GetPlayerEconomy(1).MaxPopulation, Is.EqualTo(25));
            int old = w.GetBuildingAt(cell).Id; Attack(w, 2, old, AttackTargetKind.Building); w.Step();
            Assert.That(w.GetBuilding(old).IsDestroyed, Is.True); Assert.That(w.GetPlayerEconomy(1).MaxPopulation, Is.EqualTo(20));
            w.Submit(new MoveCommand(3, 2, 2, new[] { 2 }, 15500, 15500)); Step(w, 50);
            w.Submit(new BuildCommand(w.Tick + 1, 1, 2, "house", cell, new[] { 1 })); w.Step();
            var next = w.GetBuildingAt(cell); Assert.That(next, Is.Not.Null); Assert.That(next.Id, Is.Not.EqualTo(old));
            Assert.That(next.HitPoints, Is.EqualTo(50)); Assert.That(w.GetPlayerEconomy(1).MaxPopulation, Is.EqualTo(25));
        }
        [Test]
        public void ConstructionPreservesDamageInsteadOfHealingToProgress()
        {
            var w = World(); Unit(w, 1, 4, 5, new CombatStats(40), true); Unit(w, 2, 5, 4, new CombatStats(100, 0, 1, 1000));
            w.RegisterBuildingDefinition(new BuildingDefinition("house", 2, 2, 0, 0, 0, 10, 100)); int cell = w.Map.Index(5, 5);
            w.Submit(new BuildCommand(1, 1, 1, "house", cell, new[] { 1 })); w.Step(); var building = w.GetBuildingAt(cell);
            Attack(w, 2, building.Id, AttackTargetKind.Building); Step(w, 20);
            Assert.That(building.IsComplete, Is.True); Assert.That(building.HitPoints, Is.EqualTo(99));
        }
        [Test]
        public void FriendlyFireAndForgedOwnershipCannotCancelExistingMove()
        {
            var w = World(); Unit(w, 1, 2, 5, new CombatStats(100, 0, 12)); Unit(w, 1, 3, 5, new CombatStats(100)); Unit(w, 2, 10, 5, new CombatStats(100));
            w.Submit(new MoveCommand(1, 1, 1, new[] { 1 }, 2500, 10500));
            w.Submit(new AttackCommand(2, 1, 2, new[] { 1 }, AttackTargetKind.Unit, 2));
            w.Submit(new AttackCommand(2, 2, 1, new[] { 1 }, AttackTargetKind.Unit, 3)); Step(w, 5);
            Assert.That(w.GetUnit(1).TargetId, Is.Zero); Assert.That(w.GetUnit(1).IsMoving, Is.True); Assert.That(w.GetUnit(2).CurrentHP, Is.EqualTo(100));
        }
        [Test]
        public void MoveCancelsAttackWithoutResettingCooldown()
        {
            var w = World(); Unit(w, 1, 2, 5, new CombatStats(100, 0, 12)); var target = Unit(w, 2, 3, 5, new CombatStats(100));
            Attack(w, 1, 2); w.Step(); w.Submit(new MoveCommand(2, 1, 2, new[] { 1 }, 2500, 5500));
            w.Submit(new AttackCommand(3, 1, 3, new[] { 1 }, AttackTargetKind.Unit, 2)); Step(w, 5);
            Assert.That(target.CurrentHP, Is.EqualTo(88));
        }
        [Test]
        public void CombatReplayMatchesWithReversedCommandArrival()
        {
            var a = CombatScenario.CreateWorld(); var b = CombatScenario.CreateWorld(true);
            bool arrow = false, death = false;
            for (int i = 0; i < 600; i++)
            { a.Step(); b.Step(); arrow |= a.ProjectileCount > 0; death |= a.AliveUnitCount < a.Units.Count; Assert.That(a.ComputeChecksum(), Is.EqualTo(b.ComputeChecksum())); }
            Assert.That(arrow && death, Is.True); Assert.That(a.GetPlayerEconomy(2).Population, Is.Zero); Assert.That(a.DropOffCount, Is.EqualTo(1));
        }
    }
}
