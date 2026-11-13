using System;
using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;

namespace GameRTS.Tests
{
    public sealed class ResourceNodeTests
    {
        private static SimulationWorld World(bool debug = true)
        {
            var world = new SimulationWorld(new GridMap(8, 6), true, debug);
            world.AddPlayer(1); return world;
        }
        private static void Extract(SimulationWorld world, ResourceNodeState node, int amount, int sequence)
        {
            Assert.That(world.Submit(new ResourceDebugCommand(world.Tick + 1, 1, sequence, node.Id, amount)), Is.True);
            world.Step();
        }

        [Test]
        public void ResourceBlocksAStarAndAdjacentDiagonalCorners()
        {
            var world = World(); int blocked = world.Map.Index(3, 2);
            world.AddResource("tree", ResourceType.Wood, 100, blocked);
            Assert.That(world.Map.IsWalkable(blocked), Is.False);
            Assert.That(world.Map.CanTraverse(world.Map.Index(2, 2), world.Map.Index(3, 3)), Is.False);
            var path = GridPathfinder.FindPath(world.Map, world.Map.Index(1, 2), world.Map.Index(5, 2));
            CollectionAssert.DoesNotContain(path, blocked);
            int previous = world.Map.Index(1, 2), length = 0;
            foreach (int cell in path) { length += world.Map.StepDistance(previous, cell); previous = cell; }
            Assert.That(length, Is.GreaterThan(4 * GridMap.StraightDistance));
        }

        [Test]
        public void PartialExtractionKeepsCellBlockedAndExhaustionOpensItWithoutCreditingWallet()
        {
            var world = World(); var node = world.AddResource("tree", ResourceType.Wood, 100, world.Map.Index(3, 2));
            Extract(world, node, 50, 1);
            Assert.That(node.Remaining, Is.EqualTo(50)); Assert.That(world.Map.IsWalkable(node.Cell), Is.False);
            Extract(world, node, int.MaxValue, 2);
            Assert.That(node.Remaining, Is.Zero); Assert.That(node.IsDepleted, Is.True);
            Assert.That(world.Map.IsWalkable(node.Cell), Is.True);
            Assert.That(world.GetPlayerEconomy(1).Wood, Is.EqualTo(200));
            Extract(world, node, 50, 3); Assert.That(node.Remaining, Is.Zero);
            Assert.That(world.GetResource(node.Id), Is.SameAs(node));
        }

        [Test]
        public void ResourceStockAndKindAreIndependentPerNode()
        {
            var world = World();
            var tree = world.AddResource("tree", ResourceType.Wood, 100, 10);
            var bush = world.AddResource("bush", ResourceType.Food, 150, 11);
            var gold = world.AddResource("gold", ResourceType.Gold, 800, 12);
            Extract(world, bush, 50, 1);
            Assert.That(new[] { tree.Remaining, bush.Remaining, gold.Remaining }, Is.EqualTo(new[] { 100, 100, 800 }));
            Assert.That(bush.ResourceType, Is.EqualTo(ResourceType.Food));
        }

        [Test]
        public void DepletionDoesNotOverwriteUnderlyingTerrainBlocker()
        {
            var world = World(); var node = world.AddResource("tree", ResourceType.Wood, 100, 10);
            world.Map.SetWalkable(2, 1, false);
            Extract(world, node, 100, 1);
            Assert.That(world.Map.HasResourceBlocker(10), Is.False);
            Assert.That(world.Map.IsWalkable(10), Is.False);
        }

        [Test]
        public void InvalidDuplicateOccupiedAndOutsidePlacementsLeaveWorldUnchanged()
        {
            var world = World(); world.AddResource("tree", ResourceType.Wood, 100, 10);
            world.Map.SetWalkable(3, 1, false);
            world.AddUnit("soldier", 1, 3000, 450, 4500, 1500);
            ulong before = world.ComputeChecksum();
            Assert.Throws<ArgumentException>(() => world.AddResource("tree", ResourceType.Wood, 100, 10));
            Assert.Throws<ArgumentException>(() => world.AddResource("tree", ResourceType.Wood, 100, 11));
            Assert.Throws<ArgumentException>(() => world.AddResource("tree", ResourceType.Wood, 100, 12));
            Assert.Throws<ArgumentException>(() => world.AddResource("tree", ResourceType.Wood, 100, -1));
            Assert.Throws<ArgumentException>(() => world.AddResource("tree", (ResourceType)99, 100, 13));
            Assert.Throws<ArgumentException>(() => world.AddResource("tree", ResourceType.Wood, 0, 13));
            Assert.That(world.Map.CellAt(-1, 500), Is.EqualTo(-1));
            Assert.That(world.Map.CellAt(8000, 500), Is.EqualTo(-1));
            Assert.That(world.ComputeChecksum(), Is.EqualTo(before));
        }

        [Test]
        public void SpawnAvoidsResourceCellsAndRuntimePlacementIsRejected()
        {
            var world = World(); var node = world.AddResource("tree", ResourceType.Wood, 100, 10);
            UnitState unit = world.AddUnit("soldier", 1, 3000, 450, 2500, 1500);
            Assert.That(unit.Cell, Is.Not.EqualTo(node.Cell)); Assert.That(world.Map.IsWalkable(unit.Cell), Is.True);
            world.Step();
            Assert.Throws<InvalidOperationException>(() => world.AddResource("tree", ResourceType.Wood, 100, 20));
        }

        [Test]
        public void StableCellIdsDoNotDependOnRegistrationOrder()
        {
            var a = World(); var b = World();
            a.AddResource("tree", ResourceType.Wood, 100, 10); a.AddResource("bush", ResourceType.Food, 150, 12);
            b.AddResource("bush", ResourceType.Food, 150, 12); b.AddResource("tree", ResourceType.Wood, 100, 10);
            Assert.That(a.GetResourceAt(10).Id, Is.EqualTo(11));
            Assert.That(a.ComputeChecksum(), Is.EqualTo(b.ComputeChecksum()));
        }

        [Test]
        public void DebugRequiresFlagAndUsesSharedSequenceAndScheduledTick()
        {
            var closed = World(false); var node = closed.AddResource("tree", ResourceType.Wood, 100, 10);
            Assert.That(closed.Submit(new ResourceDebugCommand(1, 1, 1, node.Id, 50)), Is.False);
            var world = World(); node = world.AddResource("tree", ResourceType.Wood, 100, 10);
            Assert.That(world.Submit(new ResourceDebugCommand(2, 1, 1, node.Id, 50)), Is.True);
            Assert.That(world.Submit(new EconomyDebugCommand(2, 1, 1, EconomyDebugOperation.GrantResources, 50)), Is.False);
            world.Step(); Assert.That(node.Remaining, Is.EqualTo(100));
            world.Step(); Assert.That(node.Remaining, Is.EqualTo(50));
            Assert.That(world.Submit(new ResourceDebugCommand(2, 1, 2, node.Id, 50)), Is.False);
        }

        [Test]
        public void ChecksumIncludesHiddenTerrainStockAndQueuedExtractionAmount()
        {
            var a = World(); var b = World();
            var nodeA = a.AddResource("tree", ResourceType.Wood, 100, 10);
            var nodeB = b.AddResource("tree", ResourceType.Wood, 100, 10);
            b.Map.SetWalkable(2, 1, false);
            Assert.That(a.Map.IsWalkable(10), Is.EqualTo(b.Map.IsWalkable(10)));
            Assert.That(a.ComputeChecksum(), Is.Not.EqualTo(b.ComputeChecksum()));
            b.Map.SetWalkable(2, 1, true);
            a.Submit(new ResourceDebugCommand(1, 1, 1, nodeA.Id, 50));
            b.Submit(new ResourceDebugCommand(1, 1, 1, nodeB.Id, 51));
            Assert.That(a.ComputeChecksum(), Is.Not.EqualTo(b.ComputeChecksum()));
            a.Step(); b.Step(); Assert.That(a.ComputeChecksum(), Is.Not.EqualTo(b.ComputeChecksum()));
        }

        [Test]
        public void UnitCanEnterFormerResourceCellAfterExhaustion()
        {
            var world = World(); var node = world.AddResource("tree", ResourceType.Wood, 100, 10);
            var unit = world.AddUnit("soldier", 1, 3000, 450, 500, 1500);
            world.Submit(new ResourceDebugCommand(1, 1, 1, node.Id, 100));
            world.Submit(new MoveCommand(1, 1, 2, new[] { unit.Id }, 2500, 1500));
            for (int i = 0; i < 60; i++) world.Step();
            Assert.That(unit.Cell, Is.EqualTo(node.Cell)); Assert.That(unit.IsMoving, Is.False);
        }
    }
}
