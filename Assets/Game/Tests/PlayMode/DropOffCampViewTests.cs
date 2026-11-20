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
    public sealed class DropOffCampViewTests
    {
        private RtsGameView game;
        private UnitSelectionInput selection;
        private BuildingPlacementInput placementInput;
        private BuildingPlacementView preview;
        private ConstructionHudView hud;
        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single); yield return null;
            game = Object.FindFirstObjectByType<RtsGameView>(); game.enabled = false;
            selection = Object.FindFirstObjectByType<UnitSelectionInput>(); placementInput = Object.FindFirstObjectByType<BuildingPlacementInput>();
            preview = Object.FindFirstObjectByType<BuildingPlacementView>(); hud = Object.FindFirstObjectByType<ConstructionHudView>();
        }
        private static Vector2 Screen(float x, float y) => Camera.main.WorldToScreenPoint(new Vector3(x, y, 0));
        private void Select(int index)
        {
            Vector2 point = Camera.main.WorldToScreenPoint(game.Units[index].transform.position);
            selection.ProcessPointer(point, true, false, false, false, false);
            selection.ProcessPointer(point, false, false, true, false, false); hud.RefreshBuildOptions();
        }
        private Button Button(string name) => hud.transform.Find("Top Bar/" + name).GetComponent<Button>();

        [UnityTest]
        public IEnumerator BuildChoicesFollowWorkerSelectionAndSwitchGhostType()
        {
            string[] buttons = { "Build House", "Build Lumber Camp", "Build Mill", "Build Mining Camp" };
            string[] ids = { "house", "lumber_camp", "mill", "mining_camp" };
            hud.RefreshBuildOptions(); foreach (string name in buttons) Assert.That(Button(name).gameObject.activeSelf, Is.False);
            Select(0); ulong before = game.World.ComputeChecksum();
            for (int i = 0; i < buttons.Length; i++)
            {
                Assert.That(Button(buttons[i]).gameObject.activeSelf, Is.True); Button(buttons[i]).onClick.Invoke();
                Assert.That(preview.IsActive, Is.True); Assert.That(preview.Data.TypeId, Is.EqualTo(ids[i]));
            }
            placementInput.CancelPlacement(); Assert.That(game.World.ComputeChecksum(), Is.EqualTo(before)); yield return null;
        }

        [UnityTest]
        public IEnumerator AllThreeCampButtonsBuildAndReceiveTheirResource()
        {
            string[] buttons = { "Build Lumber Camp", "Build Mill", "Build Mining Camp" };
            Vector2[] sites = { new Vector2(-5.5f, -4.5f), new Vector2(-9.5f, -3.5f), new Vector2(2.5f, 6.5f) };
            for (int kind = 0; kind < 3; kind++)
            {
                if (kind > 0) yield return Load(); Select(0); Button(buttons[kind]).onClick.Invoke();
                Assert.That(placementInput.ConfirmAtScreen(Screen(sites[kind].x, sites[kind].y)), Is.True,"Camp "+kind+" at "+sites[kind]);
                game.World.Step(); game.World.Step(); var camp = game.World.Buildings.Single();
                Assert.That(game.World.GetPlayerEconomy(1).Wood, Is.EqualTo(100)); Assert.That(game.World.GetDropOff(camp.Id), Is.Null);
                for (int i = 0; i < 900 && !camp.IsComplete; i++) game.World.Step();
                Assert.That(camp.IsComplete, Is.True); Assert.That(game.World.GetPlayerEconomy(1).MaxPopulation, Is.EqualTo(10));
                game.RenderBuildings(); Assert.That(game.BuildingViews[camp.Id].DisplayedSprite, Is.SameAs(game.GetBuildingData(camp.Definition.TypeId).CompletedSprite));
                yield return null; yield return null;
                var source = game.ResourceViews.First(v => (int)v.State.ResourceType == kind);
                selection.ProcessPointer(Camera.main.WorldToScreenPoint(source.transform.position), false, false, false, true, false);
                var worker = game.World.GetUnit(1).Villager;
                for (int i = 0; i < 900 && worker.Activity != VillagerActivity.GoingToDropOff; i++) game.World.Step();
                Assert.That(worker.CarriedAmount, Is.EqualTo(10)); Assert.That(worker.DropOffId, Is.EqualTo(camp.Id));
                var economy = game.World.GetPlayerEconomy(1);
                for (int i = 0; i < 600 && worker.CarriedAmount > 0; i++) game.World.Step();
                Assert.That(kind == 0 ? economy.Wood : kind == 1 ? economy.Food : economy.Gold, Is.EqualTo(kind == 0 ? 110 : kind == 1 ? 210 : 110));
            }
        }

        [UnityTest]
        public IEnumerator CampButtonsConsumeMapPointerGestures()
        {
            Select(0); var rect = Button("Build Lumber Camp").GetComponent<RectTransform>();
            Vector2 point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            Assert.That(EconomyHudView.BlocksPointer(point), Is.True);
            selection.ProcessPointer(point, true, false, false, false, false);
            selection.ProcessPointer(point, false, false, true, false, false);
            selection.ProcessPointer(point, false, false, false, true, false);
            game.World.Step(); game.World.Step(); Assert.That(game.SelectedCount, Is.EqualTo(1));
            Assert.That(game.World.GetUnit(1).IsMoving, Is.False); yield return null;
        }

        [UnityTest]
        public IEnumerator UnaffordableCampCannotSpendOrPlace()
        {
            Select(0); game.World.GetPlayerEconomy(1).TrySpendResources(150, 0, 0);
            Button("Build Mining Camp").onClick.Invoke(); placementInput.PreviewAtScreen(Screen(5.5f, -6.5f));
            Assert.That(preview.Issue, Is.EqualTo(PlacementIssue.InsufficientResources));
            Assert.That(placementInput.ConfirmAtScreen(Screen(5.5f, -6.5f)), Is.False);
            game.World.Step(); game.World.Step(); Assert.That(game.World.BuildingCount, Is.Zero);
            Assert.That(game.World.GetPlayerEconomy(1).Wood, Is.EqualTo(50)); yield return null;
        }
    }
}
