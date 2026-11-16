using System.Linq;
using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;
namespace GameRTS.Tests
{
    public sealed class DifficultyTests
    {
        [TestCase(AiDifficulty.Easy,40,6)]
        [TestCase(AiDifficulty.Medium,30,11)]
        [TestCase(AiDifficulty.Hard,20,15)]
        public void Presets(AiDifficulty level,int interval,int size)
        {
            var config=AiDifficulties.Create(level); Assert.That(config.DecisionInterval,Is.EqualTo(interval)); Assert.That(config.WaveSize,Is.EqualTo(size));
            var w=AiScenario.CreateWorld(config); w.Step(); Assert.That(w.GetPlayerEconomy(2).Food,Is.EqualTo(200));
            Assert.That(w.GetPlayerEconomy(2).Population,Is.EqualTo(3));
        }
        [TestCase(AiDifficulty.Easy)]
        [TestCase(AiDifficulty.Medium)]
        [TestCase(AiDifficulty.Hard)]
        public void ProducesAndAttacks(AiDifficulty level)
        {
            var w=AiScenario.CreateWorld(AiDifficulties.Create(level)); bool split=false;
            for(int i=0;i<14000 && !w.IsMatchOver;i++)
            {
                w.Step(); var ai=w.GetAi(2);
                if(ai.LastCommands.OfType<MoveCommand>().Count(c=>c.UnitIds.All(id=>w.GetUnit(id).Villager==null))>=2) split=true;
            }
            Assert.That(w.GetAi(2).WavesLaunched,Is.GreaterThanOrEqualTo(1),level.ToString());
            Assert.That(w.WinnerId,Is.EqualTo(2),level.ToString());
            if(level==AiDifficulty.Hard) Assert.That(split,Is.True);
        }
        [Test]
        public void PlayerCanWinFairEconomy()
        {
            var w=AiScenario.CreateWorld(AiDifficulties.Create(AiDifficulty.Easy));
            w.AddAiController(1,w.DropOffs.Single(d=>d.OwnerId==1).Id,new AiSettings(20,8,5,6,18,10,100));
            for(int i=0;i<20000 && !w.IsMatchOver;i++) w.Step();
            Assert.That(w.WinnerId,Is.EqualTo(1));
            Assert.That(w.Units.Count(u=>u.OwnerId==1),Is.GreaterThan(3));
            Assert.That(w.Buildings.Any(b=>b.OwnerId==1 && b.Definition.TypeId=="barracks"),Is.True);
        }
    }
}
