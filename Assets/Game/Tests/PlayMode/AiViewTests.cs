using System.Collections;
using System.Linq;
using GameRTS.Simulation;
using GameRTS.UI;
using GameRTS.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GameRTS.Tests
{
    public sealed class AiViewTests
    {
        private RtsGameView game;
        private IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("AiScene", LoadSceneMode.Single); yield return null;
            game = Object.FindFirstObjectByType<RtsGameView>(); game.enabled = false;
        }
        [UnityTest]
        public IEnumerator FairStartAndHiddenEnemy()
        {
            yield return Load(); var w = game.World;
            Assert.That(w.VisionEnabled, Is.True); Assert.That(w.AllowEconomyDebugCommands || w.AllowResourceDebugCommands, Is.False);
            Assert.That(w.GetAi(2), Is.Not.Null); Assert.That(w.Units.Count(u => u.OwnerId == 1), Is.EqualTo(3));
            Assert.That(w.Units.Count(u => u.OwnerId == 2), Is.EqualTo(3));
            Assert.That(w.GetPlayerEconomy(1).Population, Is.EqualTo(3)); Assert.That(w.GetPlayerEconomy(2).MaxPopulation, Is.EqualTo(10));
            Assert.That(w.GetPlayerEconomy(2).Food + w.TrainingCenters.Where(t => t.OwnerId == 2).Sum(t => t.Queue.Sum(q => q.PaidFood)), Is.EqualTo(200));
            game.RenderUnits(); yield return null;
            Assert.That(game.DropOffViews.Single(d => d.State.OwnerId == 2).gameObject.activeSelf, Is.False);
            Assert.That(game.Units.Where(v => w.GetUnit(v.InstanceId).OwnerId == 2).All(v => !v.gameObject.activeSelf), Is.True);
        }
        [UnityTest]
        public IEnumerator AutonomousArmyAndRestart()
        {
            yield return Load(); var w = game.World;
            for (int i = 0; i < 7000 && !w.IsMatchOver; i++)
            {
                w.Step();
                if (i % 100 == 0) { game.RenderUnits(); game.RenderBuildings(); yield return null; }
            }
            game.RenderUnits(); game.RenderBuildings();
            Assert.That(w.GetAi(2).Phase, Is.EqualTo(AiPhase.Attack)); Assert.That(w.WinnerId, Is.EqualTo(2));
            foreach (string type in new[] { "house", "lumber_camp", "mining_camp", "barracks", "archery_range", "stable" })
                Assert.That(w.Buildings.Any(b => b.OwnerId == 2 && b.IsComplete && b.Definition.TypeId == type), Is.True, type);
            Assert.That(game.Units.Count, Is.EqualTo(w.Units.Count));
            var result = Object.FindFirstObjectByType<MatchHudView>(); result.Refresh(); Assert.That(result.Title, Does.Contain("DEFEAT"));
            result.transform.Find("Match Result/Panel/Restart").GetComponent<Button>().onClick.Invoke(); yield return null; yield return null;
            var fresh = Object.FindFirstObjectByType<RtsGameView>();
            Assert.That(fresh.World.IsMatchOver, Is.False); Assert.That(fresh.World.Units.Count, Is.EqualTo(6));
            Assert.That(fresh.World.GetAi(2).Phase, Is.EqualTo(AiPhase.Economy)); Assert.That(fresh.World.BuildingCount, Is.Zero);
        }
        [UnityTest]
        public IEnumerator OpenAiFromHud()
        {
            yield return SceneManager.LoadSceneAsync("GameScene"); yield return null;
            var button = Object.FindFirstObjectByType<EconomyHudView>().transform.Find("AI Match").GetComponent<Button>();
            Assert.That(EconomyHudView.BlocksPointer(button.transform.position), Is.True);
            button.onClick.Invoke(); yield return null; yield return null;
            Assert.That(Object.FindFirstObjectByType<AiOpponentView>(), Is.Not.Null);
            Object.FindFirstObjectByType<EconomyHudView>().transform.Find("Switch Map").GetComponent<Button>().onClick.Invoke(); yield return null; yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("GameScene"));
        }
    }
}
