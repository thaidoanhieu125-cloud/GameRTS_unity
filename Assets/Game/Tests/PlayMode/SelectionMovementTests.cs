using System.Collections;
using GameRTS.Input;
using GameRTS.Simulation;
using GameRTS.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GameRTS.Tests
{
    public sealed class SelectionMovementTests
    {
        private RtsGameView game;
        private UnitSelectionView selection;
        private UnitSelectionInput input;
        private Camera camera;

        [UnitySetUp]
        public IEnumerator LoadGame()
        {
            yield return SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single);
            yield return null;
            game = Object.FindFirstObjectByType<RtsGameView>();
            selection = Object.FindFirstObjectByType<UnitSelectionView>();
            input = Object.FindFirstObjectByType<UnitSelectionInput>();
            camera = Camera.main;
            Assert.That(game.Units.Count, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator ClickSelectsOneEmptyClickClearsAndShiftToggles()
        {
            Click(camera.WorldToScreenPoint(game.Units[0].transform.position));
            Assert.That(selection.SelectedIds.Count, Is.EqualTo(1));
            Assert.That(game.Units[0].IsSelected, Is.True);
            Click(camera.WorldToScreenPoint(game.Units[1].transform.position), true);
            Assert.That(selection.SelectedIds.Count, Is.EqualTo(2));
            Click(camera.WorldToScreenPoint(game.Units[0].transform.position), true);
            Assert.That(game.Units[0].IsSelected, Is.False);
            Click(camera.WorldToScreenPoint(new Vector3(-12, -8, 0)));
            Assert.That(selection.SelectedIds.Count, Is.Zero);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ReverseDragSelectsGroupAndRightClickMovesAroundSceneWall()
        {
            Vector2 first = camera.WorldToScreenPoint(new Vector3(5.5f, 3, 0));
            Vector2 last = camera.WorldToScreenPoint(new Vector3(-5, -3, 0));
            input.ProcessPointer(first, true, true, false, false, false);
            input.ProcessPointer(last, false, true, false, false, false);
            Assert.That(selection.IsBoxSelecting, Is.True);
            input.ProcessPointer(last, false, false, true, false, false);
            Assert.That(selection.SelectedIds.Count, Is.EqualTo(3));
            foreach (UnitView view in game.Units) Assert.That(view.IsSelected, Is.True);
            Vector2 destination = camera.WorldToScreenPoint(new Vector3(12, 0, 0));
            input.ProcessPointer(destination, false, false, false, true, false);
            for (int tick = 0; tick < 600; tick++)
            {
                game.World.Step();
                foreach (UnitState unit in game.World.Units)
                    Assert.That(game.World.Map.IsWalkable(game.World.Map.NearestCell(unit.X, unit.Y)), Is.True);
            }
            foreach (UnitState unit in game.World.Units)
            {
                Assert.That(unit.IsMoving, Is.False);
                Assert.That(unit.Cell, Is.EqualTo(unit.GoalCell));
                Assert.That(unit.X, Is.GreaterThan(8000));
            }
            yield return null;
            foreach (UnitView view in game.Units)
                Assert.That(view.transform.position.x, Is.GreaterThan(8f));
        }

        [UnityTest]
        public IEnumerator RightClickOnDiagonalChangesBothAxesInSimulationAndView()
        {
            UnitView view = game.Units[0];
            UnitState state = game.World.GetUnit(view.InstanceId);
            int startX = state.X, startY = state.Y;
            Click(camera.WorldToScreenPoint(view.transform.position));
            int commandTick = game.NextCommandTick;
            input.ProcessPointer(camera.WorldToScreenPoint(new Vector3(-11.5f, 2.5f, 0)), false, false, false, true, false);
            while (game.World.Tick <= commandTick) game.World.Step();
            Assert.That(state.X, Is.LessThan(startX)); Assert.That(state.Y, Is.GreaterThan(startY));
            Assert.That(startX - state.X, Is.EqualTo(state.Y - startY));
            yield return null;
            Assert.That(view.transform.position.x, Is.LessThan(startX / 1000f));
            Assert.That(view.transform.position.y, Is.GreaterThan(startY / 1000f));
        }

        [UnityTest]
        public IEnumerator TinyDragIsClickAndCancelDoesNotChangeSelection()
        {
            Vector2 point = camera.WorldToScreenPoint(game.Units[0].transform.position);
            input.ProcessPointer(point, true, true, false, false, false);
            input.ProcessPointer(point + Vector2.one, false, false, true, false, false);
            Assert.That(selection.SelectedIds.Count, Is.EqualTo(1));
            selection.Begin(point); selection.Drag(point + new Vector2(100, 100)); selection.CancelDrag();
            selection.Release(point + new Vector2(100, 100));
            Assert.That(selection.SelectedIds.Count, Is.EqualTo(1));
            yield return null;
        }

        private void Click(Vector2 point, bool additive = false)
        {
            input.ProcessPointer(point, true, true, false, false, additive);
            input.ProcessPointer(point, false, false, true, false, additive);
        }
    }
}
