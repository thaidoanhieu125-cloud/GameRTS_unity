using System.Collections;
using GameRTS.Commands;
using GameRTS.Input;
using GameRTS.Simulation;
using GameRTS.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GameRTS.Tests
{
    public sealed class ResourceNodeViewTests
    {
        private RtsGameView game;
        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single); yield return null;
            game = Object.FindFirstObjectByType<RtsGameView>(); game.enabled = false;
        }

        [UnityTest]
        public IEnumerator SceneRegistersOriginalNodesAndExpandedForestBeforeSpawn()
        {
            Assert.That(game.ResourceViews.Count, Is.GreaterThan(9)); Assert.That(game.World.ResourceCount, Is.EqualTo(game.ResourceViews.Count));
            foreach (var view in game.ResourceViews)
            {
                Assert.That(view.IsVisible, Is.True); Assert.That(game.World.Map.IsWalkable(view.State.Cell), Is.False);
                foreach (var unit in game.World.Units) Assert.That(unit.Cell, Is.Not.EqualTo(view.State.Cell));
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestExtractionUpdatesLabelHidesDepletedSpriteAndOpensCell()
        {
            var view = game.ResourceViews[0]; var node = view.State;
            var input = Object.FindFirstObjectByType<ResourceTestInput>();
            Vector2 screen = Camera.main.WorldToScreenPoint(view.transform.position);
            Assert.That(input.RequestAtScreen(screen), Is.True);
            Assert.That(node.Remaining, Is.EqualTo(100)); game.World.Step(); game.World.Step();
            yield return null; view.Render();
            Assert.That(node.Remaining, Is.EqualTo(50)); Assert.That(view.IsVisible, Is.True);
            Assert.That(view.GetComponentInChildren<TextMesh>().text, Is.EqualTo("50"));
            Assert.That(input.RequestAtScreen(screen, true), Is.True); game.World.Step(); game.World.Step(); yield return null; view.Render();
            Assert.That(view.IsVisible, Is.False); Assert.That(view.transform.Find("Stock").gameObject.activeSelf, Is.False);
            Assert.That(game.World.Map.IsWalkable(node.Cell), Is.True);
            Assert.That(game.ResourceViews[1].State.Remaining, Is.EqualTo(100));
            Assert.That(game.World.GetPlayerEconomy(1).Wood, Is.EqualTo(200));
            Assert.That(input.RequestAtScreen(screen), Is.False);
        }

        [UnityTest]
        public IEnumerator UnitDetoursAroundTreesThenWalksThroughDepletedCell()
        {
            ResourceNodeState tree = game.World.GetResourceAt(game.World.Map.CellAt(-3500, -3500));
            UnitState unit = game.World.GetUnit(1);
            game.Submit(new MoveCommand(game.NextCommandTick, 1, game.NextLocalSequence(), new[] { 1 }, -3500, -6500));
            for (int i = 0; i < 140; i++)
            {
                game.World.Step(); Assert.That(unit.Cell, Is.Not.EqualTo(tree.Cell));
                Assert.That(game.World.Map.IsWalkable(game.World.Map.CellAt(unit.X, unit.Y)), Is.True);
            }
            Assert.That(unit.IsMoving, Is.False); Assert.That(unit.Y, Is.EqualTo(-6500));
            int tick = game.NextCommandTick;
            game.Submit(new ResourceDebugCommand(tick, 1, game.NextLocalSequence(), tree.Id, 100));
            game.Submit(new MoveCommand(tick, 1, game.NextLocalSequence(), new[] { 1 }, -3500, -3500));
            for (int i = 0; i < 100; i++) game.World.Step(); yield return null;
            Assert.That(unit.Cell, Is.EqualTo(tree.Cell)); Assert.That(unit.IsMoving, Is.False);
        }
    }
}
