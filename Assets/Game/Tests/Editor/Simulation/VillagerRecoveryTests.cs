using System;
using System.Linq;
using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;

namespace GameRTS.Tests
{
    public sealed class VillagerRecoveryTests
    {
        private static SimulationWorld World(int stock = 3, ResourceType type = ResourceType.Wood, bool depot = true)
        {
            var world = new SimulationWorld(new GridMap(24, 18)); world.AddPlayer(1, 200, 0, 0);
            world.AddResource("first", type, stock, world.Map.Index(4, 4));
            if (depot) world.AddDropOff(1, world.Map.Index(10, 4), 2, 2, ResourceAcceptance.All);
            world.RegisterBuildingDefinition(new BuildingDefinition("house", 2, 2, 50, 0, 0, 30, 300, 5));
            world.AddUnit("villager", 1, 3000, 450, 3500, 4500, 10, 2);
            world.Submit(new GatherCommand(1, 1, 1, new[] { 1 }, First(world).Id));
            return world;
        }
        private static ResourceNodeState First(SimulationWorld world) => world.GetResourceAt(world.Map.Index(4, 4));
        private static void Until(SimulationWorld world, Func<bool> done, int limit = 1800)
        {
            for (int i = 0; i < limit && !done(); i++) world.Step();
            Assert.That(done(), Is.True, "Condition not reached at " + world.Tick);
        }
        private static int Balance(SimulationWorld world, ResourceType type)
        {
            var wallet = world.GetPlayerEconomy(1);
            return type == ResourceType.Wood ? wallet.Wood - 200 : type == ResourceType.Food ? wallet.Food : wallet.Gold;
        }

        [TestCase(ResourceType.Wood)]
        [TestCase(ResourceType.Food)]
        [TestCase(ResourceType.Gold)]
        public void LastPartialLoadIsBankedBeforeAutomaticallyHarvestingNearbySameType(ResourceType type)
        {
            var world = World(type: type); var next = world.AddResource("next", type, 4, world.Map.Index(4, 7));
            var worker = world.GetUnit(1).Villager;
            Until(world, () => First(world).IsDepleted);
            Assert.That(worker.CarriedAmount, Is.EqualTo(3)); Assert.That(Balance(world, type), Is.Zero);
            Assert.That(worker.Activity, Is.EqualTo(VillagerActivity.GoingToDropOff));
            Until(world, () => Balance(world, type) == 3);
            Assert.That(next.Remaining, Is.EqualTo(4)); Assert.That(worker.ResourceId, Is.EqualTo(next.Id));
            Until(world, () => Balance(world, type) == 7 && worker.CarriedAmount == 0);
            Assert.That(worker.Activity, Is.EqualTo(VillagerActivity.Idle));
        }

        [TestCase(3)]
        [TestCase(10)]
        public void BuildInterruptsPartialOrFullDeliveryThenBanksCargoAndResumesGathering(int amount)
        {
            var world = World(stock: amount == 3 ? 3 : 30); var next = world.AddResource("next", ResourceType.Wood, 20, world.Map.Index(4, 7));
            var worker = world.GetUnit(1).Villager;
            Until(world, () => worker.Activity == VillagerActivity.GoingToDropOff);
            Assert.That(worker.CarriedAmount, Is.EqualTo(amount));
            world.Submit(new BuildCommand(world.Tick + 1, 1, 2, "house", world.Map.Index(7, 8), new[] { 1 })); world.Step();
            var house = world.Buildings.Single();
            Until(world, () => house.IsComplete);
            Assert.That(worker.CarriedAmount, Is.EqualTo(amount)); Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(150));
            Assert.That(worker.Activity, Is.EqualTo(VillagerActivity.GoingToDropOff));
            Until(world, () => world.GetPlayerEconomy(1).Wood == 150 + amount);
            Assert.That(worker.CarriedAmount, Is.Zero);
            Until(world, () => worker.CarriedAmount > 0);
            Assert.That(worker.ResourceId, Is.EqualTo(amount == 3 ? next.Id : First(world).Id));
        }

        [Test]
        public void BuilderWithNoPreviousGatherOrderReturnsStoredCargoThenWaits()
        {
            var world = World(stock: 30); var worker = world.GetUnit(1).Villager;
            Until(world, () => worker.CarriedAmount == 3);
            world.Submit(new MoveCommand(world.Tick + 1, 1, 2, new[] { 1 }, 1500, 1500)); world.Step();
            Assert.That(worker.ResourceId, Is.Zero);
            world.Submit(new BuildCommand(world.Tick + 1, 1, 3, "house", world.Map.Index(7, 8), new[] { 1 }));
            Until(world, () => world.GetPlayerEconomy(1).Wood == 153 && worker.CarriedAmount == 0);
            Assert.That(worker.Activity, Is.EqualTo(VillagerActivity.Idle)); Assert.That(First(world).Remaining, Is.EqualTo(27));
        }

        [Test]
        public void MoveDuringConstructionCancelsTheRememberedGatherOrderWithoutLosingCargo()
        {
            var world = World(stock: 30); var worker = world.GetUnit(1).Villager;
            Until(world, () => worker.CarriedAmount == 3);
            world.Submit(new BuildCommand(world.Tick + 1, 1, 2, "house", world.Map.Index(7, 8), new[] { 1 })); world.Step();
            world.Submit(new MoveCommand(world.Tick + 1, 1, 3, new[] { 1 }, 1500, 1500));
            for (int i = 0; i < 150; i++) world.Step();
            Assert.That(worker.ResourceId, Is.Zero); Assert.That(worker.Activity, Is.EqualTo(VillagerActivity.Idle));
            Assert.That(worker.CarriedAmount, Is.EqualTo(3)); Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(150));
        }

        [Test]
        public void NearbySearchSkipsOtherTypesDistantAndUnreachableSources()
        {
            var world = World(stock: 1, depot: false);
            world.AddResource("food", ResourceType.Food, 5, world.Map.Index(3, 5));
            var blocked = world.AddResource("blocked", ResourceType.Wood, 5, world.Map.Index(6, 4));
            world.Map.SetWalkable(5, 4, false); world.Map.SetWalkable(7, 4, false);
            world.Map.SetWalkable(6, 3, false); world.Map.SetWalkable(6, 5, false);
            var reachable = world.AddResource("next", ResourceType.Wood, 5, world.Map.Index(4, 9));
            var distant = world.AddResource("far", ResourceType.Wood, 5, world.Map.Index(18, 4));
            world.AddDropOff(1, world.Map.Index(2, 4), 1, 1, ResourceAcceptance.All);
            Until(world, () => world.GetUnit(1).Villager.ResourceId == reachable.Id);
            Assert.That(blocked.Remaining, Is.EqualTo(5)); Assert.That(distant.Remaining, Is.EqualTo(5));
        }

        [Test]
        public void TemporarilyBlockedReplacementIsRetriedWhenAWorkSiteOpens()
        {
            var world = World(stock: 1); var next = world.AddResource("next", ResourceType.Wood, 4, world.Map.Index(4, 7));
            world.Map.SetWalkable(3, 7, false); world.Map.SetWalkable(5, 7, false);
            world.Map.SetWalkable(4, 6, false); world.Map.SetWalkable(4, 8, false);
            Until(world, () => world.GetUnit(1).Villager.Activity == VillagerActivity.SearchingForResource);
            Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(201));
            world.Map.SetWalkable(3, 7, true);
            Until(world, () => next.Remaining < 4);
        }

        [Test]
        public void NoNearbySameTypeLeavesWorkerIdleAfterPartialDeposit()
        {
            var world = World(); world.AddResource("food", ResourceType.Food, 10, world.Map.Index(4, 7));
            var distant = world.AddResource("far", ResourceType.Wood, 10, world.Map.Index(15, 4));
            Until(world, () => world.GetPlayerEconomy(1).Wood == 203);
            for (int i = 0; i < 80; i++) world.Step();
            Assert.That(world.GetUnit(1).Villager.Activity, Is.EqualTo(VillagerActivity.Idle));
            Assert.That(world.GetUnit(1).Villager.CarriedAmount, Is.Zero); Assert.That(distant.Remaining, Is.EqualTo(10));
        }

        [Test]
        public void MissingDepotAfterConstructionKeepsCargoAndWaits()
        {
            var world = World(stock: 30, depot: false); var worker = world.GetUnit(1).Villager;
            Until(world, () => worker.CarriedAmount == 10);
            world.Submit(new BuildCommand(world.Tick + 1, 1, 2, "house", world.Map.Index(7, 8), new[] { 1 }));
            Until(world, () => world.BuildingCount == 1 && world.Buildings.Single().IsComplete);
            for (int i = 0; i < 100; i++) world.Step();
            Assert.That(worker.CarriedAmount, Is.EqualTo(10)); Assert.That(worker.Activity, Is.EqualTo(VillagerActivity.WaitingForDropOff));
            Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(150)); Assert.That(First(world).Remaining, Is.EqualTo(20));
        }

        [Test]
        public void EqualDistanceReplacementUsesStableNodeIdRegardlessOfRegistrationOrder()
        {
            var a = World(stock: 1, depot: false); var b = World(stock: 1, depot: false);
            int left = a.Map.Index(2, 5), right = a.Map.Index(4, 5);
            a.AddResource("left", ResourceType.Wood, 5, left); a.AddResource("right", ResourceType.Wood, 5, right);
            b.AddResource("right", ResourceType.Wood, 5, right); b.AddResource("left", ResourceType.Wood, 5, left);
            a.AddDropOff(1, a.Map.Index(3, 2), 1, 1, ResourceAcceptance.All);
            b.AddDropOff(1, b.Map.Index(3, 2), 1, 1, ResourceAcceptance.All);
            Until(a, () => a.GetPlayerEconomy(1).Wood == 201);
            for (int i = 0; i < a.Tick; i++) b.Step();
            Assert.That(a.GetUnit(1).Villager.ResourceId, Is.EqualTo(left + 1));
            Assert.That(a.ComputeChecksum(), Is.EqualTo(b.ComputeChecksum()));
        }

        [Test]
        public void MultipleWorkersRetargetEvenWhenOnlyOneGetsTheLastResource()
        {
            var world = World(stock: 1); var next = world.AddResource("next", ResourceType.Wood, 9, world.Map.Index(4, 7));
            world.AddUnit("villager", 1, 3000, 450, 5500, 4500, 10, 2);
            world.Submit(new GatherCommand(1, 1, 2, new[] { 1, 2 }, First(world).Id));
            Until(world, () => world.GetPlayerEconomy(1).Wood == 210);
            Assert.That(next.IsDepleted, Is.True); Assert.That(world.Units.Sum(u => u.Villager.CarriedAmount), Is.Zero);
        }

        [Test]
        public void SharedRecoveryReplayConservesResourcesAndCompletesConstruction()
        {
            var a = VillagerScenario.CreateRecoveryWorld(); var b = VillagerScenario.CreateRecoveryWorld();
            for (int i = 0; i < 600; i++)
            {
                a.Step(); b.Step(); Assert.That(a.ComputeChecksum(), Is.EqualTo(b.ComputeChecksum()));
                int spent = a.BuildingCount * 50;
                Assert.That(a.GetPlayerEconomy(1).Wood + spent + a.GetUnit(1).Villager.CarriedAmount + a.ResourceNodes.Sum(n => n.Remaining), Is.EqualTo(220));
            }
            Assert.That(a.GetPlayerEconomy(1).Wood, Is.EqualTo(170)); Assert.That(a.GetPlayerEconomy(1).MaxPopulation, Is.EqualTo(15));
            Assert.That(a.ResourceNodes.Sum(n => n.Remaining), Is.Zero); Assert.That(a.GetUnit(1).Villager.CarriedAmount, Is.Zero);
        }
    }
}
