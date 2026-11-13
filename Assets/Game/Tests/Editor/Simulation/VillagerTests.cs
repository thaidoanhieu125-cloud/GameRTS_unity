using System;
using System.Linq;
using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;

namespace GameRTS.Tests
{
    public sealed class VillagerTests
    {
        private static SimulationWorld Create(ResourceType type = ResourceType.Wood, int stock = 100, bool depot = true, int interval = 2)
        {
            var world = new SimulationWorld(new GridMap(20, 14)); world.AddPlayer(1, 0, 0, 0);
            world.AddResource("source", type, stock, world.Map.Index(4, 4));
            if (depot) world.AddDropOff(1, world.Map.Index(8, 4), 2, 2, ResourceAcceptance.All);
            world.AddUnit("villager", 1, 3000, 450, 3500, 4500, 10, interval);
            return world;
        }
        private static ResourceNodeState Node(SimulationWorld world) => world.GetResourceAt(world.Map.Index(4, 4));
        private static void Gather(SimulationWorld world, int sequence = 1, int nodeId = 0, params int[] ids)
        {
            if (ids.Length == 0) ids = new[] { 1 };
            Assert.That(world.Submit(new GatherCommand(world.Tick + 1, 1, sequence, ids, nodeId == 0 ? Node(world).Id : nodeId)), Is.True);
        }
        private static void Until(SimulationWorld world, Func<bool> condition, int limit = 2000)
        {
            for (int tick = 0; tick < limit && !condition(); tick++) world.Step();
            Assert.That(condition(), Is.True, "Condition not reached at tick " + world.Tick);
        }

        [Test]
        public void GatherCommandSnapshotsAndSortsIdsAndRejectsInvalidData()
        {
            int[] ids = { 3, 1, 3 }; var command = new GatherCommand(1, 1, 1, ids, 20); ids[1] = 99;
            Assert.That(command.UnitIds, Is.EqualTo(new[] { 1, 3 }));
            Assert.Throws<ArgumentException>(() => new GatherCommand(1, 1, 1, Array.Empty<int>(), 20));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GatherCommand(0, 1, 1, ids, 20));
        }

        [Test]
        public void HarvestUsesFixedTicksAndDoesNotCreditWalletAtTheTree()
        {
            var world = Create(interval: 10); Gather(world);
            for (int i = 0; i < 9; i++) world.Step();
            Assert.That(Node(world).Remaining, Is.EqualTo(100));
            world.Step();
            Assert.That(Node(world).Remaining, Is.EqualTo(99));
            Assert.That(world.GetUnit(1).Villager.CarriedAmount, Is.EqualTo(1));
            Assert.That(world.GetPlayerEconomy(1).Wood, Is.Zero);
        }

        [Test]
        public void OneOrderCompletesMultipleTripsAndAutomaticallyReturnsToSource()
        {
            var world = Create(); Gather(world);
            Until(world, () => world.GetPlayerEconomy(1).Wood == 20);
            var worker = world.GetUnit(1).Villager;
            Assert.That(worker.CarriedAmount, Is.Zero); Assert.That(Node(world).Remaining, Is.EqualTo(80));
            Assert.That(worker.Activity, Is.EqualTo(VillagerActivity.GoingToResource));
            Until(world, () => worker.CarriedAmount > 0);
            Assert.That(worker.ResourceId, Is.EqualTo(Node(world).Id));
        }

        [TestCase(ResourceType.Wood)]
        [TestCase(ResourceType.Food)]
        [TestCase(ResourceType.Gold)]
        public void ExhaustionDepositsLastPartialLoadAndConservesAllResources(ResourceType type)
        {
            var world = Create(type, 23); Gather(world);
            var worker = world.GetUnit(1).Villager; var economy = world.GetPlayerEconomy(1);
            Until(world, () => worker.Activity == VillagerActivity.Idle && Node(world).IsDepleted && worker.CarriedAmount == 0);
            Assert.That(economy.Wood, Is.EqualTo(type == ResourceType.Wood ? 23 : 0));
            Assert.That(economy.Food, Is.EqualTo(type == ResourceType.Food ? 23 : 0));
            Assert.That(economy.Gold, Is.EqualTo(type == ResourceType.Gold ? 23 : 0));
            Assert.That(world.Map.IsWalkable(Node(world).Cell), Is.True);
        }

        [Test]
        public void MissingDepotKeepsFullCargoAndDoesNotExtractExtraStock()
        {
            var world = Create(depot: false); Gather(world);
            for (int i = 0; i < 500; i++) world.Step();
            var worker = world.GetUnit(1).Villager;
            Assert.That(worker.CarriedAmount, Is.EqualTo(10)); Assert.That(Node(world).Remaining, Is.EqualTo(90));
            Assert.That(worker.Activity, Is.EqualTo(VillagerActivity.WaitingForDropOff));
            Assert.That(world.GetPlayerEconomy(1).Wood, Is.Zero);
        }

        [Test]
        public void NearestDepotMustBeReachableOwnedAndAcceptCarriedType()
        {
            var world = Create(depot: false); world.AddPlayer(2);
            world.AddDropOff(2, world.Map.Index(2, 6), 1, 1, ResourceAcceptance.All);
            world.AddDropOff(1, world.Map.Index(2, 2), 1, 1, ResourceAcceptance.Gold);
            world.AddDropOff(1, world.Map.Index(1, 1), 1, 1, ResourceAcceptance.Wood);
            world.Map.SetWalkable(0, 1, false); world.Map.SetWalkable(2, 1, false);
            world.Map.SetWalkable(1, 0, false); world.Map.SetWalkable(1, 2, false);
            DropOffState nearest = world.AddDropOff(1, world.Map.Index(7, 4), 1, 1, ResourceAcceptance.Wood);
            world.AddDropOff(1, world.Map.Index(15, 9), 2, 2, ResourceAcceptance.All);
            Gather(world);
            Until(world, () => world.GetUnit(1).Villager.Activity == VillagerActivity.GoingToDropOff);
            Assert.That(world.GetUnit(1).Villager.DropOffId, Is.EqualTo(nearest.Id));
        }

        [Test]
        public void BlockedWorkSitesWaitAndRetryWhenAnApproachOpens()
        {
            var world = Create(depot: false);
            world.Map.SetWalkable(3, 4, false); world.Map.SetWalkable(5, 4, false);
            world.Map.SetWalkable(4, 3, false); world.Map.SetWalkable(4, 5, false);
            // The initial unit is west of the source; restore its cell and block it with a stationary soldier instead.
            world.Map.SetWalkable(3, 4, true);
            var blocker = world.AddUnit("soldier", 1, 0, 450, 3500, 4500);
            Assert.That(blocker.Cell, Is.Not.EqualTo(world.GetUnit(1).Cell));
            // Move the worker away first, then close its former work site.
            world.Submit(new MoveCommand(1, 1, 1, new[] { 1 }, 1500, 6500));
            for (int i = 0; i < 50; i++) world.Step();
            world.Map.SetWalkable(3, 4, false);
            Gather(world, 2);
            for (int i = 0; i < 80; i++) world.Step();
            Assert.That(world.GetUnit(1).Villager.Activity, Is.EqualTo(VillagerActivity.WaitingForResource));
            Assert.That(Node(world).Remaining, Is.EqualTo(100));
            world.Map.SetWalkable(4, 5, true);
            Until(world, () => world.GetUnit(1).Villager.CarriedAmount > 0);
        }

        [Test]
        public void FiveWorkersShareFourSitesWithoutDuplicatingOrLosingResources()
        {
            var world = Create(stock: 100);
            world.AddUnit("villager", 1, 3000, 450, 4500, 3500, 10, 2);
            world.AddUnit("villager", 1, 3000, 450, 5500, 4500, 10, 2);
            world.AddUnit("villager", 1, 3000, 450, 4500, 5500, 10, 2);
            world.AddUnit("villager", 1, 3000, 450, 2500, 4500, 10, 2);
            Gather(world, 1, 0, 1, 2, 3, 4, 5);
            for (int i = 0; i < 2400; i++)
            {
                world.Step(); int carried = world.Units.Sum(u => u.Villager.CarriedAmount);
                Assert.That(world.GetPlayerEconomy(1).Wood + carried + Node(world).Remaining, Is.EqualTo(100));
                foreach (var a in world.Units) foreach (var b in world.Units)
                {
                    if (a.Id >= b.Id) continue;
                    long dx = a.X - b.X, dy = a.Y - b.Y;
                    Assert.That(dx * dx + dy * dy, Is.GreaterThanOrEqualTo(900L * 900));
                }
            }
            Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(100));
        }

        [Test]
        public void OwnershipAndNonVillagerValidationRejectTheWholeOrder()
        {
            var world = Create(); world.AddPlayer(2);
            world.AddUnit("soldier", 1, 3000, 450, 1500, 1500);
            world.AddUnit("villager", 2, 3000, 450, 2500, 1500, 10, 2);
            Gather(world, 1, 0, 1, 3); world.Step();
            Assert.That(world.GetUnit(1).Villager.Activity, Is.EqualTo(VillagerActivity.Idle));
            Gather(world, 2, 0, 1, 2); world.Step();
            Assert.That(world.GetUnit(1).Villager.Activity, Is.EqualTo(VillagerActivity.Idle));
            Assert.That(Node(world).Remaining, Is.EqualTo(100));
        }

        [Test]
        public void SwitchingResourceTypeDepositsOldCargoBeforeCollectingNewType()
        {
            var world = Create(); var food = world.AddResource("bush", ResourceType.Food, 30, world.Map.Index(4, 8));
            Gather(world); Until(world, () => world.GetUnit(1).Villager.CarriedAmount == 5);
            Gather(world, 2, food.Id); world.Step();
            Assert.That(world.GetUnit(1).Villager.CarriedType, Is.EqualTo(ResourceType.Wood));
            Until(world, () => world.GetPlayerEconomy(1).Food >= 10);
            Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(5)); Assert.That(Node(world).Remaining, Is.EqualTo(95));
        }

        [Test]
        public void MoveCancelsAutomaticJobAndKeepsTheCarriedLoad()
        {
            var world = Create(); Gather(world); Until(world, () => world.GetUnit(1).Villager.CarriedAmount == 3);
            world.Submit(new MoveCommand(world.Tick + 1, 1, 2, new[] { 1 }, 1500, 8500));
            for (int i = 0; i < 100; i++) world.Step();
            Assert.That(world.GetUnit(1).Villager.Activity, Is.EqualTo(VillagerActivity.Idle));
            Assert.That(world.GetUnit(1).Villager.CarriedAmount, Is.EqualTo(3)); Assert.That(Node(world).Remaining, Is.EqualTo(97));
            Assert.That(world.GetPlayerEconomy(1).Wood, Is.Zero);
        }

        [Test]
        public void OverflowingWalletDoesNotDestroyCargo()
        {
            var world = Create(); world.GetPlayerEconomy(1).TryAddResources(int.MaxValue, 0, 0); Gather(world);
            for (int i = 0; i < 600; i++) world.Step();
            Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(int.MaxValue));
            Assert.That(world.GetUnit(1).Villager.CarriedAmount, Is.EqualTo(10)); Assert.That(Node(world).Remaining, Is.EqualTo(90));
        }

        [Test]
        public void InvalidBuildingFootprintDoesNotPartiallyBlockTheMap()
        {
            var world = Create(depot: false); int at = world.Map.Index(3, 3);
            ulong before = world.ComputeChecksum();
            Assert.Throws<ArgumentException>(() => world.AddDropOff(1, at, 2, 2, ResourceAcceptance.All));
            Assert.That(world.DropOffCount, Is.Zero); Assert.That(world.Map.HasBuildingBlocker(at), Is.False);
            Assert.That(world.ComputeChecksum(), Is.EqualTo(before));
        }

        [Test]
        public void MidSegmentGatherOrderDoesNotTeleportUnit()
        {
            var world = Create(); var unit = world.GetUnit(1);
            world.Submit(new MoveCommand(1, 1, 1, new[] { 1 }, 9500, 1500));
            for (int i = 0; i < 5; i++) world.Step(); int x = unit.X, y = unit.Y;
            Gather(world, 2); world.Step();
            long dx = unit.X - x, dy = unit.Y - y;
            Assert.That(dx * dx + dy * dy, Is.LessThanOrEqualTo(151L * 151));
            Until(world, () => unit.Villager.CarriedAmount > 0);
        }

        [Test]
        public void CommandArrivalOrderAndBuildingRegistrationOrderHaveStableChecksums()
        {
            var a = Create(depot: false); var b = Create(depot: false);
            a.AddDropOff(1, a.Map.Index(8, 4), 1, 1, ResourceAcceptance.All);
            a.AddDropOff(1, a.Map.Index(12, 6), 1, 1, ResourceAcceptance.All);
            b.AddDropOff(1, b.Map.Index(12, 6), 1, 1, ResourceAcceptance.All);
            b.AddDropOff(1, b.Map.Index(8, 4), 1, 1, ResourceAcceptance.All);
            var gather = new GatherCommand(2, 1, 2, new[] { 1 }, Node(a).Id);
            var move = new MoveCommand(2, 1, 1, new[] { 1 }, 2500, 4500);
            a.Submit(gather); a.Submit(move); b.Submit(move); b.Submit(gather);
            for (int i = 0; i < 600; i++) { a.Step(); b.Step(); Assert.That(a.ComputeChecksum(), Is.EqualTo(b.ComputeChecksum())); }
        }
    }
}
