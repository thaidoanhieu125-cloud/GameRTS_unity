using System;
using System.Linq;
using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;

namespace GameRTS.Tests
{
    public sealed class TrainingTests
    {
        private static SimulationWorld World(int food = 500, int max = 10, bool second = false, int ticks = 200)
        {
            var w = new SimulationWorld(new GridMap(20, 18)); w.AddPlayer(1, 200, food, 100, max);
            w.AddPlayer(2); w.AddDropOff(1, w.Map.Index(8, 8), 2, 2, ResourceAcceptance.All);
            w.AddTrainingCenter(Center(w), new TrainingDefinition("villager", 3000, 450, 10, 10, durationTicks: ticks));
            if (second)
            {
                int cell = w.Map.Index(12, 8); w.AddDropOff(1, cell, 2, 2, ResourceAcceptance.All);
                w.AddTrainingCenter(cell + 1, new TrainingDefinition("villager", 3000, 450, 10, 10, durationTicks: ticks));
            }
            w.AddUnit("villager", 1, 3000, 450, 2500, 2500, 10, 10); return w;
        }
        private static int Center(SimulationWorld w) => w.Map.Index(8, 8) + 1;
        private static void Send(SimulationWorld w, TrainingOperation op = TrainingOperation.Enqueue, int target = 0, int center = 0, int player = 1)
        { Assert.That(w.Submit(new TrainingCommand(w.Tick + 1, player, w.Tick + 1, center == 0 ? Center(w) : center, op, target)), Is.True); w.Step(); }
        private static void StepTo(SimulationWorld w, int tick) { while (w.Tick < tick) w.Step(); }

        [Test]
        public void ChargesImmediatelyButSpawnsOnlyAtTick200()
        {
            var w = World(); Send(w); var q = w.GetTraining(Center(w));
            Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(450)); Assert.That(w.GetPlayerEconomy(1).Population, Is.EqualTo(1));
            StepTo(w, 199); Assert.That(w.Units.Count, Is.EqualTo(1)); Assert.That(q.Queue[0].ProgressTicks, Is.EqualTo(199));
            w.Step(); Assert.That(w.Units.Count, Is.EqualTo(2)); Assert.That(q.Queue.Count, Is.Zero);
            Assert.That(w.GetPlayerEconomy(1).Population, Is.EqualTo(2)); Assert.That(w.GetUnit(2).Villager.Capacity, Is.EqualTo(10));
            Assert.That(w.Map.IsWalkable(w.GetUnit(2).Cell), Is.True);
        }
        [Test]
        public void FiveJobsTrainSequentiallyAndSixthCostsNothing()
        {
            var w = World(); for (int i = 0; i < 6; i++) Send(w);
            Assert.That(w.LastTrainingIssue, Is.EqualTo(TrainingIssue.QueueFull)); Assert.That(w.QueuedPopulation(1), Is.EqualTo(5));
            Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(250)); Assert.That(w.GetTraining(Center(w)).Queue[1].ProgressTicks, Is.Zero);
            StepTo(w, 1000); Assert.That(w.Units.Count, Is.EqualTo(6)); Assert.That(w.QueuedPopulation(1), Is.Zero);
            Assert.That(w.Units.Select(u => u.Cell).Distinct().Count(), Is.EqualTo(6));
        }
        [Test]
        public void PopulationReservationsAreSharedAcrossCenters()
        {
            var w = World(max: 3, second: true); Send(w); Send(w, center: w.Map.Index(12, 8) + 1); Send(w);
            Assert.That(w.LastTrainingIssue, Is.EqualTo(TrainingIssue.PopulationLimit)); Assert.That(w.QueuedPopulation(1), Is.EqualTo(2));
            Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(400)); StepTo(w, 201);
            Assert.That(w.GetPlayerEconomy(1).Population, Is.EqualTo(3)); Assert.That(w.Units.Count, Is.EqualTo(3));
        }
        [TestCase(49, 10, 1, TrainingIssue.NotEnoughFood)]
        [TestCase(500, 1, 1, TrainingIssue.PopulationLimit)]
        [TestCase(500, 10, 2, TrainingIssue.NotOwner)]
        public void RejectedOrdersNeverCharge(int food, int max, int player, TrainingIssue issue)
        {
            var w = World(food, max); Send(w, player: player);
            Assert.That(w.LastTrainingIssue, Is.EqualTo(issue)); Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(food));
            Assert.That(w.QueuedPopulation(1), Is.Zero);
        }
        [Test]
        public void CancelTailRefundsAndPreservesHeadProgress()
        {
            var w = World(); Send(w); Send(w); var q = w.GetTraining(Center(w)); int tail = q.Queue[1].Id;
            StepTo(w, 80); Send(w, TrainingOperation.Cancel, tail);
            Assert.That(q.Queue.Count, Is.EqualTo(1)); Assert.That(q.Queue[0].ProgressTicks, Is.EqualTo(81));
            Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(450)); Send(w, TrainingOperation.Cancel, tail);
            Assert.That(w.LastTrainingIssue, Is.EqualTo(TrainingIssue.InvalidTicket)); Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(450));
        }
        [Test]
        public void CancelHeadStartsNextFromZeroAndReleasesPopulationSlot()
        {
            var w = World(max: 3); Send(w); Send(w); var q = w.GetTraining(Center(w)); int head = q.Queue[0].Id;
            StepTo(w, 80); Send(w, TrainingOperation.Cancel, head);
            Assert.That(q.Queue[0].ProgressTicks, Is.EqualTo(1)); Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(450));
            Send(w); Assert.That(q.Queue.Count, Is.EqualTo(2)); StepTo(w, 279); Assert.That(w.Units.Count, Is.EqualTo(1));
            w.Step(); Assert.That(w.Units.Count, Is.EqualTo(2));
        }
        [Test]
        public void StaleCancelCannotRefundCompletedJobOrRemoveNext()
        {
            var w = World(); Send(w); Send(w); var q = w.GetTraining(Center(w)); int old = q.Queue[0].Id, next = q.Queue[1].Id;
            StepTo(w, 200); Send(w, TrainingOperation.Cancel, old);
            Assert.That(q.Queue.Single().Id, Is.EqualTo(next)); Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(400));
        }
        [Test]
        public void RefundOverflowKeepsJobUntilWalletHasRoom()
        {
            var w = World(int.MaxValue); Send(w); w.GetPlayerEconomy(1).TryAddResources(0, 50, 0);
            int id = w.GetTraining(Center(w)).Queue[0].Id; Send(w, TrainingOperation.Cancel, id);
            Assert.That(w.LastTrainingIssue, Is.EqualTo(TrainingIssue.RefundOverflow)); Assert.That(w.QueuedPopulation(1), Is.EqualTo(1));
            w.GetPlayerEconomy(1).TrySpendResources(0, 50, 0); Send(w, TrainingOperation.Cancel, id);
            Assert.That(w.QueuedPopulation(1), Is.Zero); Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(int.MaxValue));
        }
        [Test]
        public void BlockedExitWaitsAt100ThenUsesFreedAdjacentCell()
        {
            var w = World(); int[] cells = { w.Map.Index(8, 7), w.Map.Index(9, 7), w.Map.Index(7, 8), w.Map.Index(10, 8),
                w.Map.Index(7, 9), w.Map.Index(10, 9), w.Map.Index(8, 10), w.Map.Index(9, 10) };
            foreach (int cell in cells) w.Map.SetWalkable(cell % w.Map.Width, cell / w.Map.Width, false);
            Send(w); StepTo(w, 250); var q = w.GetTraining(Center(w));
            Assert.That(q.WaitingForSpawn, Is.True); Assert.That(q.ProgressPercent, Is.EqualTo(100)); Assert.That(w.Units.Count, Is.EqualTo(1));
            int freed = cells[3]; w.Map.SetWalkable(freed % w.Map.Width, freed / w.Map.Width, true); w.Step();
            Assert.That(w.GetUnit(2).Cell, Is.EqualTo(freed)); Assert.That(w.GetPlayerEconomy(1).Population, Is.EqualTo(2));
        }
        [Test]
        public void OccupiedExitNeverSpawnsOnAnotherUnit()
        {
            var w = World(ticks: 1); w.AddUnit("blocker", 1, 3000, 450, 8500, 7500);
            Send(w); Assert.That(w.Units.Count, Is.EqualTo(3)); Assert.That(w.GetUnit(3).Cell, Is.EqualTo(w.Map.Index(9, 7)));
        }
        [Test]
        public void GroundRallyMovesNewVillagerToTarget()
        {
            var w = World(); int target = w.Map.Index(15, 14); Send(w, TrainingOperation.SetRally, target); Send(w);
            StepTo(w, 450); Assert.That(w.GetUnit(2).Cell, Is.EqualTo(target)); Assert.That(w.GetUnit(2).IsMoving, Is.False);
        }
        [Test]
        public void ResourceRallyAutomaticallyHarvestsAndDeposits()
        {
            var w = World(); var node = w.AddResource("tree", ResourceType.Wood, 100, w.Map.Index(5, 7));
            Send(w, TrainingOperation.SetRally, node.Cell); Send(w); StepTo(w, 700);
            Assert.That(w.GetPlayerEconomy(1).Wood, Is.GreaterThan(200)); Assert.That(node.Remaining, Is.LessThan(100));
            Assert.That(w.GetUnit(2).Villager.ResourceId, Is.EqualTo(node.Id));
        }
        [Test]
        public void CommandArrivalOrderProducesIdenticalReplay()
        {
            var a = World(); var b = World(); int center = Center(a);
            var commands = new[] { new TrainingCommand(1, 1, 1, center, TrainingOperation.Enqueue),
                new TrainingCommand(1, 1, 2, center, TrainingOperation.Enqueue),
                new TrainingCommand(100, 1, 3, center, TrainingOperation.Cancel, 2),
                new TrainingCommand(250, 1, 4, center, TrainingOperation.SetRally, a.Map.Index(15, 14)) };
            foreach (var command in commands) a.Submit(command); foreach (var command in commands.Reverse()) b.Submit(command);
            for (int i = 0; i < 600; i++) { a.Step(); b.Step(); Assert.That(a.ComputeChecksum(), Is.EqualTo(b.ComputeChecksum())); }
        }
        [Test]
        public void SharedEconomyCycleBuildsCampBanksGoldAndTrainsThree()
        {
            var w = TrainingScenario.CreateWorld();
            bool sawGoldTripToCamp = false;
            for (int i = 0; i < 1200; i++)
            {
                w.Step(); var wallet = w.GetPlayerEconomy(1);
                if (w.BuildingCount > 0 && w.GetUnit(2).Villager.DropOffId == w.Buildings.Single().Id) sawGoldTripToCamp = true;
                int carried = w.Units.Sum(u => u.Villager.CarriedAmount), stock = w.ResourceNodes.Sum(n => n.Remaining);
                int paid = (w.Units.Count - 3 + w.QueuedPopulation(1)) * 50;
                Assert.That(wallet.Wood + wallet.Food + wallet.Gold + carried + stock + paid + w.BuildingCount * 100, Is.EqualTo(1800));
                Assert.That(wallet.Population + w.QueuedPopulation(1), Is.LessThanOrEqualTo(wallet.MaxPopulation));
            }
            Assert.That(w.Buildings.Single().IsComplete, Is.True); Assert.That(w.GetPlayerEconomy(1).Gold, Is.GreaterThan(100));
            Assert.That(w.Units.Count, Is.EqualTo(6)); Assert.That(w.QueuedPopulation(1), Is.Zero);
            Assert.That(sawGoldTripToCamp, Is.True);
            Assert.That(w.Units.Skip(3).All(u => u.Villager.ResourceId > 0), Is.True);
        }
    }
}
