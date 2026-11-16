using System;
using System.Linq;
using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;

namespace GameRTS.Tests
{
    public sealed class AiWaveTests
    {
        private static SimulationWorld Arena(int soldiers = 9, bool targets = false, bool killer = false, bool stragglers = false)
        {
            var w = new SimulationWorld(new GridMap(50, 50));
            w.AddPlayer(1, 0, 0, 0, 100); w.AddPlayer(2, 0, 0, 0, 100);
            int town = w.AddDropOff(2, w.Map.Index(30, 28), 2, 2, ResourceAcceptance.All).Id;
            w.AddTrainingCenter(town, new TrainingDefinition("villager", 3000, 450, 10, 10));
            for (int i = 0; i < 3; i++) Add(w, "worker", 2, 33 + i, 32, true);
            for (int i = 0; i < soldiers; i++) Add(w, "soldier", 2, stragglers && i >= 9 ? 40 + i % 3 * 2 : 24 + i % 3 * 2,
                stragglers && i >= 9 ? 40 : 22 + i / 3 * 2);
            if (targets)
            {
                Add(w, "enemy_soldier", 1, 17, 20, hp: 1);
                Add(w, "enemy_worker", 1, 20, 20, true, hp: 1);
                int enemyTown = w.AddDropOff(1, w.Map.Index(18, 17), 2, 2, ResourceAcceptance.All, 1, 0).Id;
                w.AddTrainingCenter(enemyTown, new TrainingDefinition("villager", 3000, 450, 10, 10));
                w.AddDropOff(1, w.Map.Index(21, 19), 1, 1, ResourceAcceptance.Wood, 1, 0);
            }
            if (killer) w.AddUnit("killer", 1, 0, 450, 7500, 7500,
                stats: new CombatStats(100000, 0, 1000, 1, 16000, 40000));
            w.ConfigureVision(); w.AddAiController(2, town, new AiSettings(villagerTarget: 3, militaryVillagers: 3));
            return w;
        }
        private static UnitState Add(SimulationWorld w, string type, int owner, int x, int y, bool worker = false, int hp = 100)
            => w.AddUnit(type, owner, 3000, 450, x * 1000 + 500, y * 1000 + 500, worker ? 10 : 0,
                stats: new CombatStats(hp, 0, owner == 2 && !worker ? 10 : 0, 30,
                    unitClass: worker ? UnitClass.Villager : UnitClass.Spearman));

        [Test]
        public void WaitForNine()
        {
            var waiting = Arena(8); for (int i = 0; i < 100; i++) waiting.Step();
            Assert.That(waiting.GetAi(2).Phase, Is.EqualTo(AiPhase.Military));
            Assert.That(waiting.GetAi(2).WavesLaunched, Is.Zero);
            var ready = Arena(); ready.Step();
            Assert.That(ready.GetAi(2).Phase, Is.EqualTo(AiPhase.Attack));
            Assert.That(ready.GetAi(2).WaveIds.Count, Is.EqualTo(9));
            Assert.That(ready.GetAi(2).LastCommands.OfType<MoveCommand>().Any(c => c.UnitIds.Count == 9), Is.True);
        }
        [Test]
        public void ReinforcementsWait()
        {
            var w = Arena(12, stragglers: true); w.Step(); var original = w.GetAi(2).WaveIds.ToArray();
            Assert.That(original.Length, Is.EqualTo(9));
            for (int i = 0; i < 400; i++) w.Step();
            Assert.That(w.GetAi(2).WaveIds, Is.EqualTo(original)); Assert.That(w.GetAi(2).WavesLaunched, Is.EqualTo(1));
            Assert.That(w.Units.Where(u => u.OwnerId == 2 && u.Villager == null && !original.Contains(u.Id))
                .All(u => Squared(w, u.Cell, w.GetAi(2).StagingCell) <= 25), Is.True);
        }
        [Test]
        public void TargetOrder()
        {
            var w = Arena(targets: true); var seen = new System.Collections.Generic.List<string>(); int decision = 0;
            for (int i = 0; i < 900 && w.DropOffs.Any(d => d.OwnerId == 1); i++)
            {
                w.Step(); var ai = w.GetAi(2); if (decision == ai.Decisions) continue; decision = ai.Decisions;
                foreach (var c in ai.LastCommands.OfType<AttackCommand>())
                {
                    string key = c.TargetKind == AttackTargetKind.Unit ? w.GetUnit(c.TargetId).TypeId
                        : w.GetTraining(c.TargetId) != null ? "town" : "camp";
                    if (!seen.Contains(key)) seen.Add(key);
                }
            }
            Assert.That(seen, Is.EqualTo(new[] { "enemy_soldier", "enemy_worker", "town", "camp" }));
            Assert.That(w.DropOffs.Count(d => d.OwnerId == 1), Is.Zero);
        }
        [Test]
        public void DefendWithoutFullWave()
        {
            var w = Arena(3); var invader = Add(w, "invader", 1, 35, 28); w.Step();
            Assert.That(w.GetAi(2).WavesLaunched, Is.Zero);
            var attack = w.GetAi(2).LastCommands.OfType<AttackCommand>().Single();
            Assert.That(attack.TargetId, Is.EqualTo(invader.Id)); Assert.That(attack.UnitIds.Count, Is.EqualTo(3));
            Assert.That(attack.UnitIds.All(id => w.GetUnit(id).Villager == null), Is.True);
        }
        [Test]
        public void HiddenArmyIgnored()
        {
            var a = Arena(targets: true); var b = Arena(targets: true);
            var hidden = Add(b, "hidden", 1, 1, 1); a.Step(); b.Step();
            Assert.That(b.IsCellVisible(2, hidden.Cell), Is.False);
            Assert.That(b.GetAi(2).LastCommands.OfType<AttackCommand>().Single().TargetId,
                Is.EqualTo(a.GetAi(2).LastCommands.OfType<AttackCommand>().Single().TargetId));
        }
        [Test]
        public void RecallToDefend()
        {
            var w = Arena(); var invader = Add(w, "invader", 1, 44, 28, hp: 10000);
            w.Step(); Assert.That(w.GetAi(2).WavesLaunched, Is.EqualTo(1));
            w.Submit(new MoveCommand(2, 1, 1, new[] { invader.Id }, 35500, 28500));
            for (int i = 0; i < 40; i++) w.Step();
            var ai = w.GetAi(2); Assert.That(ai.IsRegrouping, Is.True); Assert.That(ai.Phase, Is.EqualTo(AiPhase.Economy));
            Assert.That(ai.WaveIds.Count, Is.Zero);
            Assert.That(ai.LastCommands.OfType<AttackCommand>().Any(c => c.TargetId == invader.Id && c.UnitIds.Count == 9), Is.True);
        }
        [Test]
        public void SurvivorsReturn()
        {
            var w = Arena(killer: true); w.Step(); var original = w.GetAi(2).WaveIds.ToArray();
            var killer = w.Units.Single(u => u.TypeId == "killer");
            // Let the initial tick+2 wave command execute before this fixture moves the army.
            for (int i = 0; i < 3; i++) w.Step();
            w.Submit(new MoveCommand(w.Tick + 1, 2, 100000, original, 11500, 11500));
            for (int i = 0; i < 240; i++) w.Step();
            int seq = 1;
            foreach (int id in original.Take(6))
            {
                w.Submit(new AttackCommand(w.Tick + 1, 1, seq++, new[] { killer.Id }, AttackTargetKind.Unit, id));
                for (int i = 0; i < 300 && w.GetUnit(id).IsAlive; i++) w.Step();
                Assert.That(w.GetUnit(id).IsAlive, Is.False);
            }
            for (int i = 0; i < 40 && !w.GetAi(2).IsRegrouping; i++) w.Step();
            var ai = w.GetAi(2); Assert.That(ai.IsRegrouping, Is.True); Assert.That(ai.Phase, Is.EqualTo(AiPhase.Economy));
            Assert.That(ai.WaveIds.Count, Is.Zero); Assert.That(ai.Regroups, Is.EqualTo(1));
            Assert.That(ai.RegroupUntilTick, Is.GreaterThan(w.Tick));
            Assert.That(ai.LastCommands.OfType<MoveCommand>().Any(c => c.UnitIds.All(id => original.Contains(id))), Is.True);
            for (int i = 0; i < 650; i++) w.Step();
            Assert.That(original.Skip(6).All(id => !w.GetUnit(id).IsAlive || Squared(w, w.GetUnit(id).Cell, ai.StagingCell) <= 25), Is.True);
            Assert.That(ai.IsRegrouping, Is.False); Assert.That(ai.WavesLaunched, Is.EqualTo(1));
        }
        [Test]
        public void RebuildNextWave()
        {
            var a = AiScenario.CreateRepelledWorld(); var b = AiScenario.CreateRepelledWorld(); int decision = 0; bool recovery = false;
            for (int i = 0; i < 12000 && a.GetAi(2).WavesLaunched < 2; i++)
            {
                a.Step(); b.Step(); var ai = a.GetAi(2); if (decision == ai.Decisions) continue; decision = ai.Decisions;
                Assert.That(a.ComputeChecksum(), Is.EqualTo(b.ComputeChecksum()));
                if (ai.Regroups > 0 && ai.Phase == AiPhase.Economy) recovery = true;
                Assert.That(a.GetPlayerEconomy(2).Population + a.QueuedPopulation(2), Is.LessThanOrEqualTo(a.GetPlayerEconomy(2).MaxPopulation));
            }
            Assert.That(recovery, Is.True); Assert.That(a.GetAi(2).WavesLaunched, Is.GreaterThanOrEqualTo(2));
            Assert.That(a.GetAi(2).WaveInitialSize, Is.GreaterThanOrEqualTo(9));
            Assert.That(a.Units.Count(u => u.OwnerId == 2 && u.IsAlive && u.Villager != null), Is.EqualTo(10));
        }
        [Test]
        public void RegroupConfigInHash()
        {
            var a = AiScenario.CreateWorld(new AiSettings(regroupTicks: 200));
            var b = AiScenario.CreateWorld(new AiSettings(regroupTicks: 201));
            Assert.That(a.ComputeChecksum(), Is.Not.EqualTo(b.ComputeChecksum()));
            Assert.Throws<ArgumentException>(() => new AiSettings(regroupTicks: 0));
        }
        private static int Squared(SimulationWorld w, int a, int b)
        { int dx = a % w.Map.Width - b % w.Map.Width, dy = a / w.Map.Width - b / w.Map.Width; return dx * dx + dy * dy; }
    }
}
