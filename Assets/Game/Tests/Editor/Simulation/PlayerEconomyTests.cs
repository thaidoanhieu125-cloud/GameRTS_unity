using System;
using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;

namespace GameRTS.Tests
{
    public sealed class PlayerEconomyTests
    {
        [Test]
        public void WalletCreditsAndPaysAllResourcesAtomically()
        {
            var e = new PlayerEconomyState(200, 200, 100, 3, 10);
            Assert.That(e.TryAddResources(20, 30, 40), Is.True);
            Assert.That(e.TrySpendResources(10, 20, 30), Is.True);
            Assert.That(new[] { e.Wood, e.Food, e.Gold }, Is.EqualTo(new[] { 210, 210, 110 }));
            Assert.That(e.TrySpendResources(10, 20, 111), Is.False);
            Assert.That(new[] { e.Wood, e.Food, e.Gold }, Is.EqualTo(new[] { 210, 210, 110 }));
            Assert.That(e.TrySpendResources(210, 210, 110), Is.True);
            Assert.That(new[] { e.Wood, e.Food, e.Gold }, Is.EqualTo(new[] { 0, 0, 0 }));
        }

        [Test]
        public void NegativeAmountsAndOverflowCannotPartiallyChangeWallet()
        {
            var e = new PlayerEconomyState(200, 200, int.MaxValue, 3, 10);
            Assert.That(e.TryAddResources(50, 50, 1), Is.False);
            Assert.That(e.TryAddResources(-1, 0, 0), Is.False);
            Assert.That(e.TrySpendResources(0, -50, 0), Is.False);
            Assert.That(new[] { e.Wood, e.Food, e.Gold }, Is.EqualTo(new[] { 200, 200, int.MaxValue }));
        }

        [Test]
        public void PopulationCannotExceedCapacityOrBecomeNegative()
        {
            var e = new PlayerEconomyState(0, 0, 0, 3, 3);
            Assert.That(e.TryReservePopulation(1), Is.False);
            Assert.That(e.TryIncreaseMaxPopulation(5), Is.True);
            Assert.That(e.TryReservePopulation(5), Is.True);
            Assert.That(e.TryReleasePopulation(9), Is.False);
            Assert.That(e.TryReleasePopulation(-1), Is.False);
            Assert.That(e.TryIncreaseMaxPopulation(int.MaxValue), Is.False);
            Assert.That(e.Population, Is.EqualTo(8)); Assert.That(e.MaxPopulation, Is.EqualTo(8));
            Assert.That(e.TryReleasePopulation(8), Is.True);
            Assert.That(e.Population, Is.Zero);
        }

        [Test]
        public void InvalidStartingStateIsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerEconomyState(-1, 0, 0, 0, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerEconomyState(0, 0, 0, 11, 10));
        }

        [Test]
        public void SpawnUsesPopulationAndFailedSpawnLeavesStateUnchanged()
        {
            var w = new SimulationWorld(new GridMap(4, 4));
            PlayerEconomyState e = w.AddPlayer(1, maxPopulation: 1);
            w.AddUnit("soldier", 1, 3000, 450, 500, 500);
            ulong checksum = w.ComputeChecksum();
            Assert.Throws<InvalidOperationException>(() => w.AddUnit("soldier", 1, 3000, 450, 1500, 500));
            Assert.That(e.Population, Is.EqualTo(1)); Assert.That(w.Units.Count, Is.EqualTo(1));
            Assert.That(w.ComputeChecksum(), Is.EqualTo(checksum));
        }

        [Test]
        public void TransactionsExecuteOnScheduledTickAndOnlyForTheirPlayer()
        {
            var w = new SimulationWorld(new GridMap(4, 4), true);
            PlayerEconomyState first = w.AddPlayer(1), second = w.AddPlayer(2);
            Assert.That(w.Submit(new EconomyDebugCommand(2, 2, 1, EconomyDebugOperation.SpendResources, 50)), Is.True);
            Assert.That(second.Gold, Is.EqualTo(100)); w.Step(); Assert.That(second.Gold, Is.EqualTo(100));
            w.Step(); Assert.That(second.Gold, Is.EqualTo(50)); Assert.That(first.Gold, Is.EqualTo(100));
            Assert.That(w.Submit(new EconomyDebugCommand(3, 99, 1, EconomyDebugOperation.GrantResources, 50)), Is.False);
        }

        [Test]
        public void DebugCreditIsDisabledUnlessExplicitlyEnabled()
        {
            var w = new SimulationWorld(new GridMap(4, 4)); w.AddPlayer(1);
            Assert.That(w.Submit(new EconomyDebugCommand(1, 1, 1, EconomyDebugOperation.GrantResources, 50)), Is.False);
        }

        [Test]
        public void MoveAndEconomyShareDuplicateSequenceProtection()
        {
            var w = new SimulationWorld(new GridMap(4, 4), true);
            w.AddUnit("soldier", 1, 3000, 450, 500, 500);
            Assert.That(w.Submit(new MoveCommand(1, 1, 1, new[] { 1 }, 2500, 2500)), Is.True);
            Assert.That(w.Submit(new EconomyDebugCommand(1, 1, 1, EconomyDebugOperation.GrantResources, 50)), Is.False);
            w.Step();
            Assert.That(w.Submit(new EconomyDebugCommand(1, 1, 2, EconomyDebugOperation.GrantResources, 50)), Is.False);
        }

        [Test]
        public void ArrivalOrderDoesNotAffectEconomyOrChecksum()
        {
            var a = new SimulationWorld(new GridMap(4, 4), true);
            var b = new SimulationWorld(new GridMap(4, 4), true);
            a.AddPlayer(1); b.AddPlayer(1);
            var debit = new EconomyDebugCommand(1, 1, 1, EconomyDebugOperation.SpendResources, 150);
            var credit = new EconomyDebugCommand(1, 1, 2, EconomyDebugOperation.GrantResources, 50);
            a.Submit(debit); a.Submit(credit); b.Submit(credit); b.Submit(debit);
            Assert.That(a.ComputeChecksum(), Is.EqualTo(b.ComputeChecksum()));
            a.Step(); b.Step();
            Assert.That(a.GetPlayerEconomy(1).Gold, Is.EqualTo(150));
            Assert.That(a.GetPlayerEconomy(1).Wood, Is.EqualTo(250));
            Assert.That(a.ComputeChecksum(), Is.EqualTo(b.ComputeChecksum()));
        }

        [Test]
        public void ChecksumIncludesWalletAndQueuedTransactionPayload()
        {
            var a = new SimulationWorld(new GridMap(4, 4), true);
            var b = new SimulationWorld(new GridMap(4, 4), true);
            a.AddPlayer(1); b.AddPlayer(1);
            a.Submit(new EconomyDebugCommand(2, 1, 1, EconomyDebugOperation.GrantResources, 50));
            b.Submit(new EconomyDebugCommand(2, 1, 1, EconomyDebugOperation.GrantResources, 51));
            Assert.That(a.ComputeChecksum(), Is.Not.EqualTo(b.ComputeChecksum()));
            ulong before = a.ComputeChecksum(); a.GetPlayerEconomy(1).TryIncreaseMaxPopulation(1);
            Assert.That(a.ComputeChecksum(), Is.Not.EqualTo(before));
        }
    }
}
