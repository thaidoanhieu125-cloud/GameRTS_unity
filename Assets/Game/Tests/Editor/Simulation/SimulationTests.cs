using System;
using System.Collections.Generic;
using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;

namespace GameRTS.Tests
{
    public sealed class SimulationTests
    {
        [Test]
        public void AStarDetoursThroughGapAndUsesOnlyAdjacentWalkableCells()
        {
            var map = new GridMap(12, 10);
            for (int y = 0; y < 9; y++) map.SetWalkable(5, y, false);
            int start = map.Index(1, 1), goal = map.Index(9, 1);
            List<int> path = GridPathfinder.FindPath(map, start, goal);
            Assert.That(path, Is.Not.Null);
            int previous = start;
            foreach (int cell in path)
            {
                Assert.That(map.IsWalkable(cell), Is.True);
                Assert.That(Math.Max(Math.Abs(previous % map.Width - cell % map.Width),
                    Math.Abs(previous / map.Width - cell / map.Width)), Is.EqualTo(1));
                Assert.That(map.CanTraverse(previous, cell), Is.True);
                previous = cell;
            }
            Assert.That(path, Does.Contain(map.Index(5, 9)));
            Assert.That(previous, Is.EqualTo(goal));
        }

        [Test]
        public void AStarRejectsBlockedOrDisconnectedDestinationAndHasStableTies()
        {
            var map = new GridMap(8, 8);
            for (int y = 0; y < 8; y++) map.SetWalkable(4, y, false);
            Assert.That(GridPathfinder.FindPath(map, 0, map.Index(4, 0)), Is.Null);
            Assert.That(GridPathfinder.FindPath(map, 0, map.Index(6, 0)), Is.Null);
            List<int> expected = GridPathfinder.FindPath(map, 0, map.Index(3, 3));
            for (int i = 0; i < 10; i++)
                CollectionAssert.AreEqual(expected, GridPathfinder.FindPath(map, 0, map.Index(3, 3)));
        }

        [Test]
        public void OpenDiagonalUsesFiveDiagonalStepsInsteadOfTenCardinalSteps()
        {
            var map = new GridMap(10, 10);
            List<int> path = GridPathfinder.FindPath(map, map.Index(1, 1), map.Index(6, 6));
            Assert.That(path.Count, Is.EqualTo(5));
            for (int i = 0; i < path.Count; i++) Assert.That(path[i], Is.EqualTo(map.Index(i + 2, i + 2)));
        }

        [TestCase(1, 0)]
        [TestCase(-1, 0)]
        [TestCase(0, 1)]
        [TestCase(0, -1)]
        [TestCase(1, 1)]
        [TestCase(-1, 1)]
        [TestCase(-1, -1)]
        [TestCase(1, -1)]
        public void AllEightDirectionsTravelSameDistancePerSecond(int dx, int dy)
        {
            var world = new SimulationWorld(new GridMap(18, 18));
            UnitState unit = world.AddUnit("soldier", 1, 3000, 450, 8500, 8500);
            world.Submit(new MoveCommand(1, 1, 1, new[] { 1 }, 8500 + dx * 6000, 8500 + dy * 6000));
            for (int tick = 0; tick < 20; tick++) { world.Step(); AssertNoOverlap(world); }
            int expected = dx != 0 && dy != 0 ? 2121 : 3000;
            Assert.That(unit.X - 8500, Is.EqualTo(dx * expected).Within(1));
            Assert.That(unit.Y - 8500, Is.EqualTo(dy * expected).Within(1));
            long x = unit.X - 8500, y = unit.Y - 8500;
            Assert.That(Math.Sqrt(x * x + y * y), Is.EqualTo(3000).Within(3));
        }

        [Test]
        public void DiagonalCannotCutOneBlockedCornerOrSqueezeBetweenTwoBlockedCorners()
        {
            var map = new GridMap(5, 5);
            int start = map.Index(0, 0), goal = map.Index(1, 1);
            map.SetWalkable(1, 0, false);
            Assert.That(map.CanTraverse(start, goal), Is.False);
            CollectionAssert.AreEqual(new[] { map.Index(0, 1), goal }, GridPathfinder.FindPath(map, start, goal));
            var world = new SimulationWorld(map);
            world.AddUnit("soldier", 1, 3000, 499, 500, 500);
            world.Submit(new MoveCommand(1, 1, 1, new[] { 1 }, 1500, 1500));
            for (int tick = 0; tick < 80; tick++) { world.Step(); AssertNoOverlap(world); }
            Assert.That(world.GetUnit(1).Cell, Is.EqualTo(goal));
            map.SetWalkable(0, 1, false);
            Assert.That(GridPathfinder.FindPath(map, start, goal), Is.Null);
            Assert.That(map.ReachableFrom(start)[goal], Is.False);
        }

        [Test]
        public void OccupiedSideCellAlsoPreventsDiagonalShortcut()
        {
            var map = new GridMap(5, 5);
            int start = map.Index(1, 1), goal = map.Index(2, 2), occupied = map.Index(2, 1);
            Assert.That(map.CanTraverse(start, goal, c => c == occupied), Is.False);
            List<int> path = GridPathfinder.FindPath(map, start, goal, c => c == occupied);
            CollectionAssert.AreEqual(new[] { map.Index(1, 2), goal }, path);
        }

        [Test]
        public void CrossingDiagonalOrdersReachBothGoalsWithoutPassingThroughEachOther()
        {
            var world = new SimulationWorld(new GridMap(9, 9));
            UnitState first = world.AddUnit("soldier", 1, 3000, 499, 2500, 2500);
            UnitState second = world.AddUnit("soldier", 1, 3000, 499, 3500, 2500);
            world.Submit(new MoveCommand(1, 1, 1, new[] { 1 }, 3500, 3500));
            world.Submit(new MoveCommand(1, 1, 2, new[] { 2 }, 2500, 3500));
            var trace = new System.Text.StringBuilder();
            for (int tick = 0; tick < 400; tick++)
            {
                world.Step(); AssertNoOverlap(world);
                if (tick % 12 == 0) trace.AppendLine($"{world.Tick}: A=({first.X},{first.Y}), goal {first.GoalCell}, moving {first.IsMoving}; B=({second.X},{second.Y}), goal {second.GoalCell}, moving {second.IsMoving}");
            }
            Assert.That(first.X, Is.EqualTo(3500), trace.ToString()); Assert.That(first.Y, Is.EqualTo(3500));
            Assert.That(second.X, Is.EqualTo(2500)); Assert.That(second.Y, Is.EqualTo(3500));
            Assert.That(first.IsMoving || second.IsMoving, Is.False);
        }

        [Test]
        public void ChangingOrderDuringDiagonalKeepsContinuousPositionAndArrives()
        {
            var world = new SimulationWorld(new GridMap(16, 16));
            UnitState unit = world.AddUnit("soldier", 1, 3000, 450, 2500, 2500);
            world.Submit(new MoveCommand(1, 1, 1, new[] { 1 }, 12500, 12500));
            world.Step(); world.Step();
            Assert.That(unit.X, Is.GreaterThan(2500)); Assert.That(unit.Y, Is.GreaterThan(2500));
            world.Submit(new MoveCommand(3, 1, 2, new[] { 1 }, 1500, 11500));
            for (int tick = 0; tick < 250; tick++)
            {
                int oldX = unit.X, oldY = unit.Y;
                world.Step(); AssertNoOverlap(world);
                long dx = unit.X - oldX, dy = unit.Y - oldY;
                Assert.That(dx * dx + dy * dy, Is.LessThanOrEqualTo(300L * 300));
            }
            Assert.That(unit.X, Is.EqualTo(1500)); Assert.That(unit.Y, Is.EqualTo(11500));
            Assert.That(unit.IsMoving, Is.False);
        }

        [Test]
        public void FutureCommandExecutesOnlyAtItsTickAndSpeedRetainsFractionalDistance()
        {
            var world = new SimulationWorld(new GridMap(12, 4));
            UnitState unit = world.AddUnit("soldier", 1, 3000, 450, 500, 500);
            Assert.That(world.Submit(new MoveCommand(5, 1, 1, new[] { unit.Id }, 9500, 500)), Is.True);
            for (int i = 0; i < 4; i++) world.Step();
            Assert.That(unit.X, Is.EqualTo(500));
            for (int i = 0; i < 20; i++) world.Step();
            Assert.That(unit.X, Is.EqualTo(3500));
            Assert.That(unit.Y, Is.EqualTo(500));
        }

        [Test]
        public void CommandsAreImmutableCanonicalizedAndDuplicatesOrStaleCommandsAreRejected()
        {
            int[] ids = { 2, 1, 2 };
            var command = new MoveCommand(1, 1, 9, ids, 1500, 1500);
            ids[0] = 99;
            CollectionAssert.AreEqual(new[] { 1, 2 }, command.UnitIds);
            var world = new SimulationWorld(new GridMap(8, 8));
            Assert.That(world.Submit(command), Is.True);
            Assert.That(world.Submit(command), Is.False);
            world.Step();
            Assert.That(world.Submit(new MoveCommand(1, 1, 10, new[] { 1 }, 500, 500)), Is.False);
        }

        [Test]
        public void ForeignOrUnknownUnitRejectsEntireCommand()
        {
            var world = new SimulationWorld(new GridMap(8, 8));
            UnitState first = world.AddUnit("soldier", 1, 3000, 450, 500, 500);
            UnitState second = world.AddUnit("soldier", 2, 3000, 450, 1500, 500);
            world.Submit(new MoveCommand(1, 1, 1, new[] { first.Id, second.Id }, 5500, 5500));
            world.Submit(new MoveCommand(2, 1, 2, new[] { first.Id, 999 }, 5500, 5500));
            for (int i = 0; i < 30; i++) world.Step();
            Assert.That(first.X, Is.EqualTo(500)); Assert.That(second.X, Is.EqualTo(1500));
        }

        [Test]
        public void CommandArrivalOrderAndIdOrderDoNotChangeAnyTickChecksum()
        {
            SimulationWorld first = SmallWorld(), second = SmallWorld();
            var a = new MoveCommand(1, 1, 1, new[] { 1, 2 }, 9500, 2500);
            var b = new MoveCommand(1, 1, 2, new[] { 2, 1 }, 9500, 5500);
            first.Submit(a); first.Submit(b); second.Submit(b); second.Submit(a);
            for (int tick = 0; tick < 160; tick++)
            {
                first.Step(); second.Step();
                Assert.That(first.ComputeChecksum(), Is.EqualTo(second.ComputeChecksum()), "Tick " + first.Tick);
            }
        }

        [Test]
        public void NewOrderWhileBetweenCellsReachesNewGoalWithoutCuttingObstacles()
        {
            SimulationWorld world = SmallWorld();
            world.Submit(new MoveCommand(1, 1, 1, new[] { 1 }, 10500, 500));
            world.Step(); world.Step();
            world.Submit(new MoveCommand(3, 1, 2, new[] { 1 }, 500, 7500));
            for (int i = 0; i < 300; i++) { world.Step(); AssertNoOverlap(world); }
            UnitState unit = world.GetUnit(1);
            Assert.That(unit.IsMoving, Is.False);
            Assert.That(unit.Cell, Is.EqualTo(unit.GoalCell));
        }

        [Test]
        public void ClickingBlockedOrUnreachableAreaChoosesAccessibleFreeDestination()
        {
            var map = new GridMap(10, 8);
            for (int y = 0; y < 8; y++) map.SetWalkable(4, y, false);
            var world = new SimulationWorld(map);
            UnitState unit = world.AddUnit("soldier", 1, 3000, 450, 500, 500);
            world.Submit(new MoveCommand(1, 1, 1, new[] { 1 }, int.MaxValue, int.MinValue));
            for (int i = 0; i < 200; i++) world.Step();
            Assert.That(map.IsWalkable(unit.GoalCell), Is.True);
            Assert.That(unit.Cell, Is.EqualTo(unit.GoalCell));
            Assert.That(unit.Cell % map.Width, Is.LessThan(4));
        }

        [Test]
        public void GroupPassesWallAndStationaryFriendWithoutOverlapping()
        {
            var map = new GridMap(24, 18);
            for (int y = 2; y < 15; y++) map.SetWalkable(11, y, false);
            var world = new SimulationWorld(map);
            for (int i = 0; i < 5; i++) world.AddUnit("soldier", 1, 3000, 450, 2500, (i + 5) * 1000 + 500);
            world.AddUnit("soldier", 1, 3000, 450, 18500, 8500);
            world.Submit(new MoveCommand(1, 1, 1, new[] { 1, 2, 3, 4, 5 }, 18500, 8500));
            for (int i = 0; i < 800; i++) { world.Step(); AssertNoOverlap(world); }
            var goals = new HashSet<int>();
            foreach (UnitState unit in world.Units)
            {
                Assert.That(unit.IsMoving, Is.False, "Unit " + unit.Id);
                Assert.That(unit.Cell, Is.EqualTo(unit.GoalCell));
                Assert.That(goals.Add(unit.GoalCell), Is.True);
            }
        }

        [Test]
        public void HundredUnitsReachFormationThroughGapWithoutOverlap()
        {
            SimulationWorld world = DeterminismScenario.CreateWorld();
            var ids = new int[100]; for (int i = 0; i < 100; i++) ids[i] = i + 1;
            world.Submit(new MoveCommand(1, 1, 1, ids, 30500, 26500));
            for (int tick = 0; tick < 1800; tick++) { world.Step(); AssertNoOverlap(world); }
            int arrived = 0;
            foreach (UnitState unit in world.Units) if (!unit.IsMoving && unit.Cell == unit.GoalCell) arrived++;
            if (arrived != 100)
            {
                var description = new System.Text.StringBuilder();
                for (int y = world.Map.Height - 1; y >= 0; y--)
                {
                    for (int x = 0; x < world.Map.Width; x++)
                    {
                        int cell = world.Map.Index(x, y);
                        char symbol = world.Map.IsWalkable(cell) ? '.' : '#';
                        foreach (UnitState unit in world.Units)
                            if (unit.Cell == cell) symbol = unit.IsMoving ? 'M' : 'A';
                        description.Append(symbol);
                    }
                    description.AppendLine();
                }
                foreach (UnitState unit in world.Units)
                    if (unit.IsMoving) description.AppendLine($"{unit.Id}: ({unit.X},{unit.Y}) -> ({world.Map.CenterX(unit.GoalCell)},{world.Map.CenterY(unit.GoalCell)})");
                TestContext.WriteLine(description.ToString());
            }
            Assert.That(arrived, Is.EqualTo(100));
        }

        [Test]
        public void ReplayWithHundredUnitsMatchesAllSixHundredTickChecksums()
        {
            string first = DeterminismScenario.Run();
            Assert.That(DeterminismScenario.Run(), Is.EqualTo(first));
        }

        [Test]
        public void FootprintLimitsAndSpawnOccupancyAreValidated()
        {
            var world = new SimulationWorld(new GridMap(2, 1));
            Assert.Throws<ArgumentException>(() => world.AddUnit("large", 1, 3000, 500, 500, 500));
            UnitState a = world.AddUnit("soldier", 1, 3000, 450, 500, 500);
            UnitState b = world.AddUnit("soldier", 1, 3000, 450, 500, 500);
            Assert.That(a.Cell, Is.Not.EqualTo(b.Cell));
            Assert.Throws<InvalidOperationException>(() => world.AddUnit("soldier", 1, 3000, 450, 500, 500));
        }

        private static SimulationWorld SmallWorld()
        {
            var world = new SimulationWorld(new GridMap(14, 10));
            world.AddUnit("soldier", 1, 3000, 450, 500, 500);
            world.AddUnit("soldier", 1, 3000, 450, 1500, 2500);
            return world;
        }

        private static void AssertNoOverlap(SimulationWorld world)
        {
            for (int i = 0; i < world.Units.Count; i++)
            {
                UnitState a = world.Units[i];
                for (int part = 0; part <= 4; part++) AssertFootprintClear(world.Map, a, part);
                for (int j = i + 1; j < world.Units.Count; j++)
                {
                    UnitState b = world.Units[j];
                    long radius = a.Radius + b.Radius;
                    // Check quarter-frame interpolation as well as the tick endpoints.
                    for (int part = 0; part <= 4; part++)
                    {
                        long dx = (long)a.PreviousX * (4 - part) + a.X * part
                            - (long)b.PreviousX * (4 - part) - b.X * part;
                        long dy = (long)a.PreviousY * (4 - part) + a.Y * part
                            - (long)b.PreviousY * (4 - part) - b.Y * part;
                        if (dx * dx + dy * dy < radius * radius * 16)
                            Assert.Fail($"Units {a.Id}/{b.Id} overlapped at tick {world.Tick}, sample {part}.");
                    }
                }
            }
        }

        private static void AssertFootprintClear(GridMap map, UnitState unit, int part)
        {
            long px = (long)unit.PreviousX * (4 - part) + unit.X * part;
            long py = (long)unit.PreviousY * (4 - part) + unit.Y * part;
            int near = map.NearestCell((int)(px / 4), (int)(py / 4));
            for (int y = near / map.Width - 1; y <= near / map.Width + 1; y++)
            for (int x = near % map.Width - 1; x <= near % map.Width + 1; x++)
            {
                if (map.IsWalkable(map.Index(x, y))) continue;
                long left = ((long)map.OriginX + x * GridMap.UnitsPerCell) * 4;
                long bottom = ((long)map.OriginY + y * GridMap.UnitsPerCell) * 4;
                long dx = Math.Max(0L, Math.Max(left - px, px - left - GridMap.UnitsPerCell * 4));
                long dy = Math.Max(0L, Math.Max(bottom - py, py - bottom - GridMap.UnitsPerCell * 4));
                if (dx * dx + dy * dy < (long)unit.Radius * unit.Radius * 16)
                    Assert.Fail($"Unit {unit.Id} clipped blocked tile ({x},{y}), sample {part}.");
            }
        }
    }
}
