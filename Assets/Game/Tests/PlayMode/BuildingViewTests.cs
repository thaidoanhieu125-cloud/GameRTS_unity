using System.Collections;
using System.Linq;
using GameRTS.Input;
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
    public sealed class BuildingViewTests
    {
        private RtsGameView game;
        private UnitSelectionInput selection;
        private BuildingPlacementInput input;
        private BuildingPlacementView preview;
        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single); yield return null;
            game = Object.FindFirstObjectByType<RtsGameView>(); game.enabled = false;
            selection = Object.FindFirstObjectByType<UnitSelectionInput>(); input = Object.FindFirstObjectByType<BuildingPlacementInput>();
            preview = Object.FindFirstObjectByType<BuildingPlacementView>();
        }
        private static Vector2 Screen(float x, float y) => Camera.main.WorldToScreenPoint(new Vector3(x, y, 0));
        private static Vector2 Site => Screen(5.5f, -6.5f);
        private void Select(int index = 0)
        {
            Vector2 point = Camera.main.WorldToScreenPoint(game.Units[index].transform.position);
            selection.ProcessPointer(point, true, false, false, false, false);
            selection.ProcessPointer(point, false, false, true, false, false);
        }

        [UnityTest]
        public IEnumerator ButtonStartsGreenOrRedReadOnlyPreviewAndCancelDoesNotSpend()
        {
            Select(); ulong before = game.World.ComputeChecksum();
            Object.FindFirstObjectByType<ConstructionHudView>().transform.Find("Top Bar/Build House").GetComponent<Button>().onClick.Invoke();
            Assert.That(preview.IsActive, Is.True); input.PreviewAtScreen(Site);
            Assert.That(preview.IsValid, Is.True); Assert.That(preview.PreviewColor.g, Is.GreaterThan(preview.PreviewColor.r));
            input.PreviewAtScreen(Screen(-3.5f, -3.5f));
            Assert.That(preview.Issue, Is.EqualTo(PlacementIssue.BlockedTerrain)); Assert.That(preview.PreviewColor.r, Is.GreaterThan(preview.PreviewColor.g));
            input.PreviewAtScreen(Screen(-3.5f, -1.5f)); Assert.That(preview.Issue, Is.EqualTo(PlacementIssue.Occupied));
            input.PreviewAtScreen(new Vector2(-100, -100)); Assert.That(preview.IsValid, Is.False);
            input.CancelPlacement(); Assert.That(preview.IsActive, Is.False);
            Assert.That(game.World.ComputeChecksum(), Is.EqualTo(before)); Assert.That(game.World.BuildingCount, Is.Zero);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ConfirmationQueuesFoundationThenWorkerBuildsAndFinishedSpriteAppears()
        {
            Select(); Assert.That(input.BeginPlacement(), Is.True); Assert.That(input.ConfirmAtScreen(Site), Is.True);
            Assert.That(game.World.BuildingCount, Is.Zero); Assert.That(game.World.GetPlayerEconomy(1).Wood, Is.EqualTo(200));
            game.World.Step(); Assert.That(game.World.BuildingCount, Is.Zero);
            game.World.Step(); game.RenderBuildings();
            BuildingState house = game.World.Buildings.Single(); BuildingView view = game.BuildingViews[house.Id];
            Assert.That(game.World.GetPlayerEconomy(1).Wood, Is.EqualTo(150));
            Assert.That(view.DisplayedSprite, Is.SameAs(game.GetBuildingData("house").FoundationSprite));
            bool partial = false, hammer = false;
            for (int i = 0; i < 700 && !house.IsComplete; i++)
            {
                game.World.Step(); game.RenderBuildings(); game.Units[0].Render(game.World.GetUnit(1), 1);
                partial |= house.ProgressPercent > 0 && house.ProgressPercent < 100;
                hammer |= game.Units[0].transform.Find("Hammer").gameObject.activeSelf;
            }
            Assert.That(partial && hammer, Is.True); Assert.That(house.ProgressPercent, Is.EqualTo(100));
            Assert.That(view.DisplayedSprite, Is.SameAs(game.GetBuildingData("house").CompletedSprite));
            Assert.That(game.World.GetPlayerEconomy(1).MaxPopulation, Is.EqualTo(15));
            Assert.That(view.transform.Find("Progress Fill").gameObject.activeSelf, Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MoveInterruptsAndRightClickFoundationResumesWithoutPayingAgain()
        {
            Select(); input.BeginPlacement(); input.ConfirmAtScreen(Site); game.World.Step(); game.World.Step();
            BuildingState house = game.World.Buildings.Single();
            for (int i = 0; i < 500 && house.CompletedWork < 10; i++) game.World.Step();
            int progress = house.CompletedWork; yield return null; yield return null;
            selection.ProcessPointer(Screen(-12, -5), false, false, false, true, false);
            // Commands execute two ticks after input; work may continue until the Move takes effect.
            game.World.Step(); game.World.Step();
            Assert.That(game.World.GetUnit(1).Villager.BuildingId, Is.Zero);
            Assert.That(house.CompletedWork, Is.GreaterThanOrEqualTo(progress));
            progress = house.CompletedWork;
            for (int i = 0; i < 38; i++) game.World.Step(); Assert.That(house.CompletedWork, Is.EqualTo(progress));
            selection.ProcessPointer(Screen(6, -6), false, false, false, true, false);
            for (int i = 0; i < 700 && !house.IsComplete; i++) game.World.Step();
            Assert.That(house.IsComplete, Is.True); Assert.That(game.World.GetPlayerEconomy(1).Wood, Is.EqualTo(150));
        }

        [UnityTest]
        public IEnumerator ConstructionClickGestureKeepsTheSelectedBuilder()
        {
            Select(); input.BeginPlacement();
            selection.ProcessPointer(Site, true, false, false, false, false);
            input.ConfirmAtScreen(Site);
            selection.ProcessPointer(Site, false, false, true, false, false);
            Assert.That(game.SelectedCount, Is.EqualTo(1)); game.World.Step(); game.World.Step();
            Assert.That(game.World.BuildingCount, Is.EqualTo(1)); yield return null;
        }

        [UnityTest]
        public IEnumerator RightCancelDoesNotAlsoIssueAMoveAndNoBuilderCannotBegin()
        {
            Assert.That(input.BeginPlacement(), Is.False); Select(); input.BeginPlacement();
            selection.ProcessPointer(Site, false, false, false, true, false); input.CancelPlacement();
            selection.ProcessPointer(Site, false, false, false, true, false);
            for (int i = 0; i < 4; i++) game.World.Step();
            Assert.That(game.World.GetUnit(1).IsMoving, Is.False); Assert.That(game.World.BuildingCount, Is.Zero);
            Assert.That(game.World.GetPlayerEconomy(1).Wood, Is.EqualTo(200)); yield return null;
        }

        [UnityTest]
        public IEnumerator UnaffordablePlacementStaysRedAndCannotQueueConstruction()
        {
            Select(); game.World.GetPlayerEconomy(1).TrySpendResources(200, 0, 0); input.BeginPlacement(); input.PreviewAtScreen(Site);
            Assert.That(preview.Issue, Is.EqualTo(PlacementIssue.InsufficientResources)); Assert.That(input.ConfirmAtScreen(Site), Is.False);
            game.World.Step(); game.World.Step(); Assert.That(game.World.BuildingCount, Is.Zero); yield return null;
        }
    }
}
