using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;

namespace GameRTS.Tests
{
    public sealed class MatchTests
    {
        private static SimulationWorld World(out int own, out int enemy, bool draw = false)
        {
            var w = new SimulationWorld(new GridMap(20, 14)); w.AddPlayer(1); w.AddPlayer(2);
            own = w.AddDropOff(1, w.Map.Index(3, 3), 2, 2, ResourceAcceptance.All, 10).Id;
            enemy = w.AddDropOff(2, w.Map.Index(12, 3), 2, 2, ResourceAcceptance.All, 10).Id;
            w.AddUnit("killer", 1, 3000, 450, 11500, 3500, stats: new CombatStats(100, 0, 100));
            w.AddUnit("killer", 2, 3000, 450, 2500, 3500, stats: new CombatStats(100, 0, 100));
            w.ConfigureTownCenterVictory(own, enemy); return w;
        }
        [TestCase(1, MatchOutcome.Victory)]
        [TestCase(2, MatchOutcome.Defeat)]
        public void DestroyingTownDeterminesWinnerAndFreezesWorld(int attackingPlayer, MatchOutcome result)
        {
            var w = World(out int own, out int enemy);
            w.Submit(new AttackCommand(1, attackingPlayer, 1, new[] { attackingPlayer }, AttackTargetKind.Building, attackingPlayer == 1 ? enemy : own)); w.Step();
            Assert.That(w.GetOutcome(1), Is.EqualTo(result)); Assert.That(w.WinnerId, Is.EqualTo(attackingPlayer)); Assert.That(w.MatchEndTick, Is.EqualTo(1));
            ulong hash = w.ComputeChecksum(); for (int i = 0; i < 20; i++) w.Step();
            Assert.That(w.ComputeChecksum(), Is.EqualTo(hash)); Assert.That(w.Tick, Is.EqualTo(1));
            Assert.That(w.Submit(new MoveCommand(2, 1, 2, new[] { 1 }, 500, 500)), Is.False);
        }
        [Test]
        public void BothTownsDestroyedInSameTickIsDraw()
        {
            var w = World(out int own, out int enemy);
            w.Submit(new AttackCommand(1, 1, 1, new[] { 1 }, AttackTargetKind.Building, enemy));
            w.Submit(new AttackCommand(1, 2, 1, new[] { 2 }, AttackTargetKind.Building, own)); w.Step();
            Assert.That(w.GetOutcome(1), Is.EqualTo(MatchOutcome.Draw)); Assert.That(w.GetOutcome(2), Is.EqualTo(MatchOutcome.Draw)); Assert.That(w.WinnerId, Is.Zero);
        }
        [Test]
        public void DamagedButAliveTownDoesNotEndMatch()
        {
            var w = World(out _, out _); w.Step(); Assert.That(w.IsMatchOver, Is.False); Assert.That(w.GetOutcome(1), Is.EqualTo(MatchOutcome.Ongoing));
        }
        [Test]
        public void VictoryConfigurationParticipatesInChecksum()
        {
            var a = new SimulationWorld(new GridMap(12, 12)); var b = new SimulationWorld(new GridMap(12, 12));
            foreach (var w in new[] { a, b }) { w.AddPlayer(1); w.AddPlayer(2); w.AddDropOff(1, 13, 2, 2, ResourceAcceptance.All); w.AddDropOff(2, 70, 2, 2, ResourceAcceptance.All); }
            Assert.That(a.ComputeChecksum(), Is.EqualTo(b.ComputeChecksum())); a.ConfigureTownCenterVictory(14, 71);
            Assert.That(a.ComputeChecksum(), Is.Not.EqualTo(b.ComputeChecksum()));
        }
    }
}
