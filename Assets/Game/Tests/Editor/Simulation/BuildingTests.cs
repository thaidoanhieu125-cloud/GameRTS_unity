using System;
using System.Linq;
using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;

namespace GameRTS.Tests
{
    public sealed class BuildingTests
    {
        private static SimulationWorld World(int work = 40)
        {
            var world = new SimulationWorld(new GridMap(20, 16), true); world.AddPlayer(1);
            world.RegisterBuildingDefinition(new BuildingDefinition("house", 2, 2, 50, 0, 0, work, 300, 5));
            world.AddUnit("villager", 1, 3000, 450, 2500, 4500, 10, 10); return world;
        }
        private static int Site(SimulationWorld world) => world.Map.Index(5, 4);
        private static void Place(SimulationWorld world, int sequence = 1, int cell = -2, params int[] ids)
        {
            if (ids.Length == 0) ids = new[] { 1 };
            Assert.That(world.Submit(new BuildCommand(world.Tick + 1, 1, sequence, "house", cell == -2 ? Site(world) : cell, ids)), Is.True);
            world.Step();
        }
        private static void Until(SimulationWorld world, Func<bool> condition, int limit = 1000)
        {
            for (int i = 0; i < limit && !condition(); i++) world.Step();
            Assert.That(condition(), Is.True, "Construction condition not reached at " + world.Tick);
        }

        [Test]
        public void CommandCopiesBuilderIdsAndValidatesTrustedDefinition()
        {
            int[] ids = { 3, 1, 3 }; var command = new BuildCommand(1, 1, 1, "house", 10, ids); ids[1] = 99;
            Assert.That(command.BuilderIds, Is.EqualTo(new[] { 1, 3 }));
            Assert.Throws<ArgumentException>(() => new BuildCommand(1, 1, 1, "house", 10, Array.Empty<int>()));
            Assert.Throws<ArgumentException>(() => new BuildingDefinition("house", 0, 2, 50, 0, 0, 10, 300));
            Assert.Throws<ArgumentException>(() => new BuildingDefinition("house", 2, 2, -1, 0, 0, 10, 300));
        }

        [Test]
        public void PreviewIsReadOnlyAndFoundationChargesOnceBeforeWorkBegins()
        {
            var world = World(); ulong checksum = world.ComputeChecksum();
            Assert.That(world.CheckBuildingPlacement(1, "house", Site(world), new[] { 1 }), Is.EqualTo(PlacementIssue.None));
            Assert.That(world.ComputeChecksum(), Is.EqualTo(checksum)); Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(200));
            Place(world); BuildingState house = world.Buildings.Single();
            Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(150));
            Assert.That(house.CompletedWork, Is.Zero); Assert.That(house.HitPoints, Is.EqualTo(1));
            Assert.That(world.GetPlayerEconomy(1).MaxPopulation, Is.EqualTo(10));
            for (int y = 0; y < 2; y++) for (int x = 0; x < 2; x++) Assert.That(world.Map.IsWalkable(Site(world) + y * 20 + x), Is.False);
        }

        [TestCase("terrain")]
        [TestCase("resource")]
        [TestCase("unit")]
        [TestCase("building")]
        public void AnyObstructedCellRejectsWholeFootprintWithoutCharging(string kind)
        {
            var world = World(); int blocked = world.Map.Index(6, 5);
            if (kind == "terrain") world.Map.SetWalkable(6, 5, false);
            if (kind == "resource") world.AddResource("tree", ResourceType.Wood, 100, blocked);
            if (kind == "unit") world.AddUnit("soldier", 1, 3000, 450, 6500, 5500);
            if (kind == "building") world.AddDropOff(1, blocked, 1, 1, ResourceAcceptance.All);
            Place(world);
            Assert.That(world.BuildingCount, Is.Zero); Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(200));
            Assert.That(world.Map.HasBuildingBlocker(Site(world)), Is.False);
        }

        [Test]
        public void BoundsValidationDoesNotWrapOrClampTheFootprint()
        {
            var world = World();
            Assert.That(world.CheckBuildingPlacement(1, "house", -1, new[] { 1 }), Is.EqualTo(PlacementIssue.OutsideMap));
            Assert.That(world.CheckBuildingPlacement(1, "house", world.Map.Index(19, 5), new[] { 1 }), Is.EqualTo(PlacementIssue.OutsideMap));
            Assert.That(world.CheckBuildingPlacement(1, "house", world.Map.Index(5, 15), new[] { 1 }), Is.EqualTo(PlacementIssue.OutsideMap));
        }

        [Test]
        public void MovingUnitsReserveDiagonalSidesNextCellAndDestinationAgainstPlacement()
        {
            var world = World(); world.RegisterBuildingDefinition(new BuildingDefinition("tiny", 1, 1, 0, 0, 0, 1, 10));
            world.Submit(new MoveCommand(1, 1, 1, new[] { 1 }, 8500, 10500)); world.Step();
            Assert.That(world.CheckBuildingPlacement(1, "tiny", world.Map.Index(3, 4), new[] { 1 }), Is.EqualTo(PlacementIssue.Occupied));
            Assert.That(world.CheckBuildingPlacement(1, "tiny", world.Map.Index(3, 5), new[] { 1 }), Is.EqualTo(PlacementIssue.Occupied));
            Assert.That(world.CheckBuildingPlacement(1, "tiny", world.Map.Index(8, 10), new[] { 1 }), Is.EqualTo(PlacementIssue.Occupied));
        }

        [Test]
        public void ExecutionRechecksOccupancyAfterAnEarlierGreenPreview()
        {
            var world = World(); world.AddUnit("soldier", 1, 20000, 450, 5500, 3500);
            Assert.That(world.CheckBuildingPlacement(1, "house", Site(world), new[] { 1 }), Is.EqualTo(PlacementIssue.None));
            world.Submit(new BuildCommand(3, 1, 2, "house", Site(world), new[] { 1 }));
            world.Submit(new MoveCommand(1, 1, 1, new[] { 2 }, 5500, 4500));
            world.Step(); world.Step(); world.Step();
            Assert.That(world.LastBuildIssue, Is.EqualTo(PlacementIssue.Occupied)); Assert.That(world.BuildingCount, Is.Zero);
            Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(200));
        }

        [Test]
        public void InsufficientOneResourceDoesNotSpendOtherResourcesOrInterruptWork()
        {
            var world = World(); world.RegisterBuildingDefinition(new BuildingDefinition("expensive", 2, 2, 20, 20, 200, 10, 300));
            world.Submit(new BuildCommand(1, 1, 1, "expensive", Site(world), new[] { 1 })); world.Step();
            var e = world.GetPlayerEconomy(1);
            Assert.That(new[] { e.Wood, e.Food, e.Gold }, Is.EqualTo(new[] { 200, 200, 100 }));
            Assert.That(world.LastBuildIssue, Is.EqualTo(PlacementIssue.InsufficientResources));
            Assert.That(world.GetUnit(1).Villager.Activity, Is.EqualTo(VillagerActivity.Idle)); Assert.That(world.BuildingCount, Is.Zero);
        }

        [Test]
        public void InvalidBuilderOrUnknownTypeCannotPlaceOrCharge()
        {
            var world = World(); world.AddPlayer(2); world.AddUnit("villager", 2, 3000, 450, 2500, 6500, 10, 10);
            Assert.That(world.CheckBuildingPlacement(1, "house", Site(world), new[] { 1, 2 }), Is.EqualTo(PlacementIssue.InvalidBuilder));
            Assert.That(world.CheckBuildingPlacement(1, "unknown", Site(world), new[] { 1 }), Is.EqualTo(PlacementIssue.UnknownBuilding));
            Assert.That(world.CheckBuildingPlacement(1, "house", Site(world), Array.Empty<int>()), Is.EqualTo(PlacementIssue.NoBuilders));
            Assert.That(world.BuildingCount, Is.Zero);
        }

        [Test]
        public void NoReachableWorkSiteRejectsFoundationWithoutSpending()
        {
            var world = World();
            for (int x = 5; x <= 6; x++) { world.Map.SetWalkable(x, 3, false); world.Map.SetWalkable(x, 6, false); }
            for (int y = 4; y <= 5; y++) { world.Map.SetWalkable(4, y, false); world.Map.SetWalkable(7, y, false); }
            Place(world);
            Assert.That(world.LastBuildIssue, Is.EqualTo(PlacementIssue.NoAccess)); Assert.That(world.BuildingCount, Is.Zero);
            Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(200));
        }

        [Test]
        public void CompletionSetsHealthAndPopulationBonusOnlyOnce()
        {
            var world = World(); Place(world); var house = world.Buildings.Single();
            Until(world, () => house.IsComplete);
            Assert.That(house.ProgressPercent, Is.EqualTo(100)); Assert.That(house.CompletedWork, Is.EqualTo(40));
            Assert.That(house.HitPoints, Is.EqualTo(300)); Assert.That(world.GetPlayerEconomy(1).MaxPopulation, Is.EqualTo(15));
            for (int i = 0; i < 100; i++) world.Step();
            Assert.That(world.GetPlayerEconomy(1).MaxPopulation, Is.EqualTo(15)); Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(150));
        }

        [Test]
        public void DuplicateAndCompetingOrdersCannotChargeForTheSameSiteTwice()
        {
            var world = World(); var command = new BuildCommand(1, 1, 1, "house", Site(world), new[] { 1 });
            Assert.That(world.Submit(command), Is.True); Assert.That(world.Submit(command), Is.False);
            world.Submit(new BuildCommand(1, 1, 2, "house", Site(world), new[] { 1 })); world.Step();
            Assert.That(world.BuildingCount, Is.EqualTo(1)); Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(150));
        }

        [Test]
        public void MovePausesAndResumeBuildContinuesWithoutAnotherPayment()
        {
            var world = World(200); Place(world); var house = world.Buildings.Single();
            Until(world, () => house.CompletedWork >= 8); int progress = house.CompletedWork;
            world.Submit(new MoveCommand(world.Tick + 1, 1, 2, new[] { 1 }, 1500, 1500));
            for (int i = 0; i < 100; i++) world.Step(); Assert.That(house.CompletedWork, Is.EqualTo(progress));
            world.Submit(new ResumeBuildCommand(world.Tick + 1, 1, 3, house.Id, new[] { 1 }));
            Until(world, () => house.IsComplete); Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(150));
        }

        [Test]
        public void GatherOrderCanInterruptConstructionAndCargoIsKept()
        {
            var world = World(200); var node = world.AddResource("tree", ResourceType.Wood, 20, world.Map.Index(1, 8));
            Place(world); var house = world.Buildings.Single(); Until(world, () => house.CompletedWork >= 5);
            int progress = house.CompletedWork;
            world.Submit(new GatherCommand(world.Tick + 1, 1, 2, new[] { 1 }, node.Id));
            Until(world, () => world.GetUnit(1).Villager.CarriedAmount == 3);
            Assert.That(house.CompletedWork, Is.EqualTo(progress));
            world.Submit(new ResumeBuildCommand(world.Tick + 1, 1, 3, house.Id, new[] { 1 })); world.Step();
            Assert.That(world.GetUnit(1).Villager.CarriedAmount, Is.EqualTo(3));
        }

        [Test]
        public void ExtraBuildersIncreaseWorkRateWithoutOverCompleting()
        {
            var solo = World(120); var group = World(120);
            group.AddUnit("villager", 1, 3000, 450, 2500, 6500, 10, 10);
            Place(solo); Place(group, 1, -2, 1, 2);
            int soloFinish = 0, groupFinish = 0;
            for (int i = 0; i < 300; i++)
            {
                solo.Step(); group.Step();
                if (solo.Buildings.Single().IsComplete && soloFinish == 0) soloFinish = solo.Tick;
                if (group.Buildings.Single().IsComplete && groupFinish == 0) groupFinish = group.Tick;
            }
            Assert.That(groupFinish, Is.GreaterThan(0)); Assert.That(groupFinish, Is.LessThan(soloFinish));
            Assert.That(group.Buildings.Single().CompletedWork, Is.EqualTo(120)); Assert.That(group.GetPlayerEconomy(1).MaxPopulation, Is.EqualTo(15));
        }

        [Test]
        public void ThreeByThreeDropOffOnlyAcceptsDepositsAfterCompletion()
        {
            var world = World(); world.RegisterBuildingDefinition(new BuildingDefinition("store", 3, 3, 30, 0, 0, 10, 600, 0, ResourceAcceptance.Wood));
            world.Submit(new BuildCommand(1, 1, 1, "store", world.Map.Index(8, 7), new[] { 1 })); world.Step();
            var store = world.Buildings.Single(); Assert.That(world.DropOffCount, Is.Zero); Assert.That(store.Width, Is.EqualTo(3));
            Until(world, () => store.IsComplete);
            Assert.That(world.DropOffCount, Is.EqualTo(1)); Assert.That(world.GetDropOff(store.Id).AcceptsResource(ResourceType.Wood), Is.True);
            Assert.That(world.GetDropOff(store.Id).AcceptsResource(ResourceType.Food), Is.False);
            for (int y = 0; y < 3; y++) for (int x = 0; x < 3; x++) Assert.That(world.Map.IsWalkable(store.Cell + y * 20 + x), Is.False);
        }

        [Test]
        public void FootprintInsertedAcrossAnExistingRouteIsAvoidedByMovingUnit()
        {
            var world = World(); var soldier = world.AddUnit("soldier", 1, 3000, 450, 1500, 8500);
            world.Submit(new MoveCommand(1, 1, 1, new[] { soldier.Id }, 14500, 8500)); world.Step();
            Place(world, 2, world.Map.Index(6, 8)); Assert.That(world.BuildingCount, Is.EqualTo(1));
            for (int i = 0; i < 300; i++)
            {
                world.Step(); Assert.That(world.Map.IsWalkable(world.Map.CellAt(soldier.X, soldier.Y)), Is.True);
            }
            Assert.That(soldier.IsMoving, Is.False); Assert.That(soldier.Cell, Is.EqualTo(world.Map.Index(14, 8)));
        }

        [Test]
        public void PendingPopulationBonusesCannotOverflowTheLimit()
        {
            var world = World(200); world.GetPlayerEconomy(1).TryIncreaseMaxPopulation(int.MaxValue - 15);
            Place(world);
            Assert.That(world.CheckBuildingPlacement(1, "house", world.Map.Index(10, 4), new[] { 1 }), Is.EqualTo(PlacementIssue.PopulationOverflow));
            world.Submit(new EconomyDebugCommand(world.Tick + 1, 1, 2, EconomyDebugOperation.IncreasePopulationLimit, 1)); world.Step();
            Assert.That(world.LastEconomyDebugSucceeded, Is.False);
            Until(world, () => world.Buildings.Single().IsComplete);
            Assert.That(world.GetPlayerEconomy(1).MaxPopulation, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void ConflictingPlayersUseStableOrderAndOnlyWinningPlayerPays()
        {
            var a = World(); var b = World();
            foreach (var world in new[] { a, b })
            { world.AddPlayer(2); world.AddUnit("villager", 2, 3000, 450, 2500, 6500, 10, 10); }
            var first = new BuildCommand(1, 1, 1, "house", Site(a), new[] { 1 });
            var second = new BuildCommand(1, 2, 1, "house", Site(a), new[] { 2 });
            a.Submit(second); a.Submit(first); b.Submit(first); b.Submit(second);
            for (int i = 0; i < 200; i++) { a.Step(); b.Step(); Assert.That(a.ComputeChecksum(), Is.EqualTo(b.ComputeChecksum())); }
            Assert.That(a.Buildings.Single().OwnerId, Is.EqualTo(1)); Assert.That(a.GetPlayerEconomy(2).Wood, Is.EqualTo(200));
        }

        [Test]
        public void SharedScenarioCompletesBothFootprintsAndActivatesItsDropOff()
        {
            var world = ConstructionScenario.CreateWorld(); for (int i = 0; i < 600; i++) world.Step();
            Assert.That(world.BuildingCount, Is.EqualTo(2)); Assert.That(world.Buildings.All(b => b.IsComplete), Is.True);
            Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(375)); Assert.That(world.DropOffCount, Is.EqualTo(1));
        }
    }
}
