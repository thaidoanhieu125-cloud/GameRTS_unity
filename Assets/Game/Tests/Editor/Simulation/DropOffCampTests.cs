using System;
using System.Linq;
using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;

namespace GameRTS.Tests
{
    public sealed class DropOffCampTests
    {
        private static SimulationWorld World(ResourceType source = ResourceType.Wood, ResourceAcceptance accepts = ResourceAcceptance.Wood, bool town = true, int work = 20)
        {
            var world = new SimulationWorld(new GridMap(24, 18)); world.AddPlayer(1, 500, 0, 0);
            world.AddResource("source", source, 500, world.Map.Index(4, 4));
            if (town) world.AddDropOff(1, world.Map.Index(18, 4), 2, 2, ResourceAcceptance.All);
            world.RegisterBuildingDefinition(new BuildingDefinition("camp", 2, 2, 100, 0, 0, work, 400, 0, accepts));
            world.AddUnit("villager", 1, 3000, 450, 3500, 4500, 10, 2);
            world.AddUnit("villager", 1, 3000, 450, 12500, 12500, 10, 2);
            return world;
        }
        private static void Until(SimulationWorld world, Func<bool> done, int limit = 2000)
        {
            for (int i = 0; i < limit && !done(); i++) world.Step();
            Assert.That(done(), Is.True, "Camp condition not reached at " + world.Tick);
        }
        private static BuildingState Place(SimulationWorld world)
        {
            world.Submit(new BuildCommand(world.Tick + 1, 1, 1, "camp", world.Map.Index(7, 4), new[] { 2 })); world.Step();
            return world.Buildings.Single();
        }
        private static void ClearBuilder(SimulationWorld world)
        {
            world.Submit(new MoveCommand(world.Tick + 1, 1, 2, new[] { 2 }, 12500, 12500)); world.Step();
            Until(world, () => !world.GetUnit(2).IsMoving);
        }
        private static void Gather(SimulationWorld world) => world.Submit(new GatherCommand(world.Tick + 1, 1, 3, new[] { 1 }, world.GetResourceAt(world.Map.Index(4, 4)).Id));
        private static int Balance(SimulationWorld world, ResourceType type) => type == ResourceType.Wood ? world.GetPlayerEconomy(1).Wood - 400
            : type == ResourceType.Food ? world.GetPlayerEconomy(1).Food : world.GetPlayerEconomy(1).Gold;

        [TestCase(ResourceType.Wood, ResourceAcceptance.Wood)]
        [TestCase(ResourceType.Food, ResourceAcceptance.Food)]
        [TestCase(ResourceType.Gold, ResourceAcceptance.Gold)]
        public void BuiltCampReceivesMatchingLoadInsteadOfDistantTown(ResourceType type, ResourceAcceptance accepts)
        {
            var world = World(type, accepts); var camp = Place(world); Until(world, () => camp.IsComplete); ClearBuilder(world);
            Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(400)); Assert.That(world.GetPlayerEconomy(1).MaxPopulation, Is.EqualTo(10));
            Gather(world); var worker = world.GetUnit(1).Villager;
            Until(world, () => worker.Activity == VillagerActivity.GoingToDropOff);
            Assert.That(worker.CarriedAmount, Is.EqualTo(10)); Assert.That(worker.DropOffId, Is.EqualTo(camp.Id));
            Until(world, () => Balance(world, type) == 10); Assert.That(worker.CarriedAmount, Is.Zero);
        }

        [TestCase(ResourceType.Food, ResourceAcceptance.Wood)]
        [TestCase(ResourceType.Gold, ResourceAcceptance.Food)]
        [TestCase(ResourceType.Wood, ResourceAcceptance.Gold)]
        public void WrongCampIsSkippedAndTownStillReceivesCargo(ResourceType type, ResourceAcceptance accepts)
        {
            var world = World(type, accepts); var camp = Place(world); Until(world, () => camp.IsComplete); ClearBuilder(world);
            Gather(world); Until(world, () => world.GetUnit(1).Villager.Activity == VillagerActivity.GoingToDropOff);
            Assert.That(world.GetUnit(1).Villager.DropOffId, Is.EqualTo(world.Map.Index(18, 4) + 1));
            Until(world, () => Balance(world, type) == 10);
        }

        [Test]
        public void FoundationIsSkippedThenLaterTripsUseCompletedCamp()
        {
            var world = World(work: 200); var camp = Place(world); ClearBuilder(world);
            Assert.That(world.GetDropOff(camp.Id), Is.Null); Gather(world);
            Until(world, () => world.GetUnit(1).Villager.Activity == VillagerActivity.GoingToDropOff);
            Assert.That(world.GetUnit(1).Villager.DropOffId, Is.EqualTo(world.Map.Index(18, 4) + 1));
            Until(world, () => world.GetPlayerEconomy(1).Wood == 410);
            world.Submit(new ResumeBuildCommand(world.Tick + 1, 1, 4, camp.Id, new[] { 2 }));
            Until(world, () => camp.IsComplete);
            Until(world, () => world.GetUnit(1).Villager.DropOffId == camp.Id);
            Assert.That(world.GetUnit(1).Villager.CarriedAmount, Is.EqualTo(10));
        }

        [Test]
        public void WaitingCarrierFindsNewCampWithoutAnotherGatherOrder()
        {
            var world = World(town: false, work: 80); var camp = Place(world); ClearBuilder(world); Gather(world);
            Until(world, () => world.GetUnit(1).Villager.Activity == VillagerActivity.WaitingForDropOff);
            Assert.That(world.GetUnit(1).Villager.CarriedAmount, Is.EqualTo(10));
            world.Submit(new ResumeBuildCommand(world.Tick + 1, 1, 4, camp.Id, new[] { 2 }));
            Until(world, () => camp.IsComplete); Until(world, () => world.GetPlayerEconomy(1).Wood == 410);
            Assert.That(world.GetUnit(1).Villager.CarriedAmount, Is.Zero);
        }

        [Test]
        public void NearbyCampImprovesYieldAtTheSameHarvestRate()
        {
            var shortTrips = World(); var longTrips = World(); var camp = Place(shortTrips);
            Until(shortTrips, () => camp.IsComplete); ClearBuilder(shortTrips);
            while (longTrips.Tick < shortTrips.Tick) longTrips.Step();
            Gather(shortTrips); Gather(longTrips);
            for (int i = 0; i < 600; i++) { shortTrips.Step(); longTrips.Step(); }
            Assert.That(shortTrips.GetPlayerEconomy(1).Wood - 400, Is.GreaterThan(longTrips.GetPlayerEconomy(1).Wood - 500));
        }

        [Test]
        public void ThreeCampReplayBanksAllTypesAndConservesBuildCosts()
        {
            var world = ConstructionScenario.CreateCampWorld();
            for (int i = 0; i < 600; i++)
            {
                world.Step(); int remaining = world.ResourceNodes.Sum(n => n.Remaining);
                int carried = world.Units.Sum(u => u.Villager.CarriedAmount);
                var wallet = world.GetPlayerEconomy(1);
                Assert.That(wallet.Wood + wallet.Food + wallet.Gold + remaining + carried + world.BuildingCount * 100, Is.EqualTo(875));
            }
            Assert.That(world.Buildings.All(b => b.IsComplete), Is.True); Assert.That(world.DropOffCount, Is.EqualTo(4));
            Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(225)); Assert.That(world.GetPlayerEconomy(1).Food, Is.EqualTo(225));
            Assert.That(world.GetPlayerEconomy(1).Gold, Is.EqualTo(125));
        }
    }
}
