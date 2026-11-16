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
    public sealed class VillagerViewTests
    {
        private RtsGameView game;
        private UnitSelectionInput input;
        private EconomyHudView hud;
        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single); yield return null;
            game = Object.FindFirstObjectByType<RtsGameView>(); game.enabled = false;
            input = Object.FindFirstObjectByType<UnitSelectionInput>(); hud = Object.FindFirstObjectByType<EconomyHudView>();
        }
        private void SelectFirst()
        {
            Vector2 screen = Camera.main.WorldToScreenPoint(game.Units[0].transform.position);
            input.ProcessPointer(screen, true, false, false, false, false);
            input.ProcessPointer(screen, false, false, true, false, false);
        }
        private ResourceNodeState Order(ResourceType type)
        {
            foreach (var view in game.ResourceViews)
            {
                if (view.State.ResourceType != type) continue;
                input.ProcessPointer(Camera.main.WorldToScreenPoint(view.transform.position), false, false, false, true, false);
                return view.State;
            }
            Assert.Fail("Missing resource"); return null;
        }

        [UnityTest]
        public IEnumerator SceneHasThreeVillagersAndAnOwnedBlockedTownHall()
        {
            int workers = 0; foreach (var unit in game.World.Units) if (unit.Villager != null) workers++;
            Assert.That(workers, Is.EqualTo(3)); Assert.That(game.World.Units.Count(u => u.Animal == null), Is.EqualTo(3));
            Assert.That(game.World.DropOffCount, Is.EqualTo(1));
            foreach (var depot in game.World.DropOffs)
            {
                Assert.That(depot.OwnerId, Is.EqualTo(1)); Assert.That(depot.Accepts, Is.EqualTo(ResourceAcceptance.All));
                for (int y = 0; y < 2; y++) for (int x = 0; x < 2; x++)
                    Assert.That(game.World.Map.IsWalkable(depot.Cell + y * game.World.Map.Width + x), Is.False);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator RightClickTreeStartsRealGatherLoopAndHudTracksDeposits()
        {
            SelectFirst(); ResourceNodeState node = Order(ResourceType.Wood); VillagerState worker = game.World.GetUnit(1).Villager;
            Assert.That(worker.Activity, Is.EqualTo(VillagerActivity.Idle));
            bool sawCargo = false, sawFullLoad = false, sawSecondLoad = false;
            for (int i = 0; i < 700; i++)
            {
                game.World.Step();
                if (worker.CarriedAmount > 0)
                {
                    sawCargo = true;
                    if (game.World.GetPlayerEconomy(1).Wood >= 210) sawSecondLoad = true;
                }
                if (worker.CarriedAmount == 10) sawFullLoad = true;
                if (game.World.GetPlayerEconomy(1).Wood >= 220 && sawSecondLoad) break;
            }
            Assert.That(sawCargo && sawFullLoad && sawSecondLoad, Is.True);
            Assert.That(game.World.GetPlayerEconomy(1).Wood, Is.GreaterThanOrEqualTo(220));
            Assert.That(node.Remaining + worker.CarriedAmount + game.World.GetPlayerEconomy(1).Wood - 200, Is.EqualTo(100));
            game.Units[0].Render(game.World.GetUnit(1), 1); hud.Refresh();
            Assert.That(game.Units[0].transform.Find("Cargo").GetComponent<TextMesh>().text, Is.EqualTo(worker.CarriedAmount + "/10"));
            Assert.That(hud.WoodText, Is.EqualTo(game.World.GetPlayerEconomy(1).Wood.ToString()));
            Assert.That(hud.transform.Find("Worker Status/Details").GetComponent<Text>().text, Does.Contain("Dân 1"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator FoodAndGoldRightClicksDepositIntoTheirOwnHudCounters()
        {
            SelectFirst(); Order(ResourceType.Food);
            for (int i = 0; i < 700 && game.World.GetPlayerEconomy(1).Food < 210; i++) game.World.Step();
            Assert.That(game.World.GetPlayerEconomy(1).Food, Is.GreaterThanOrEqualTo(210));
            Order(ResourceType.Gold);
            for (int i = 0; i < 900 && game.World.GetPlayerEconomy(1).Gold < 110; i++) game.World.Step();
            Assert.That(game.World.GetPlayerEconomy(1).Gold, Is.GreaterThanOrEqualTo(110));
            hud.Refresh(); Assert.That(hud.FoodText, Is.EqualTo("210")); Assert.That(hud.GoldText, Is.EqualTo("110"));
            Assert.That(hud.WoodText, Is.EqualTo("200")); yield return null;
        }

        [UnityTest]
        public IEnumerator GroupSelectionSendsGatherToAllThreeVillagers()
        {
            Vector2 a = Camera.main.WorldToScreenPoint(new Vector3(-5, -3, 0));
            Vector2 b = Camera.main.WorldToScreenPoint(new Vector3(5.5f, 3, 0));
            input.ProcessPointer(a, true, false, false, false, false);
            input.ProcessPointer(b, false, true, false, false, false);
            input.ProcessPointer(b, false, false, true, false, false);
            Assert.That(game.SelectedCount, Is.EqualTo(3)); Order(ResourceType.Wood);
            for (int i = 0; i < 3; i++) game.World.Step();
            for (int i = 1; i <= 3; i++) Assert.That(game.World.GetUnit(i).Villager.ResourceId, Is.GreaterThan(0));
            yield return null;
        }

        [UnityTest]
        public IEnumerator GroundRightClickCancelsJobAndWorkerPanelConsumesClicks()
        {
            SelectFirst(); hud.Refresh();
            var panel = hud.transform.Find("Worker Status").GetComponent<RectTransform>();
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, panel.TransformPoint(panel.rect.center));
            input.ProcessPointer(screen, false, false, false, true, false); game.World.Step(); game.World.Step();
            Assert.That(game.World.GetUnit(1).IsMoving, Is.False);
            ResourceNodeState node = Order(ResourceType.Wood); var worker = game.World.GetUnit(1).Villager;
            for (int i = 0; i < 300 && worker.CarriedAmount < 3; i++) game.World.Step();
            Assert.That(worker.CarriedAmount, Is.EqualTo(3)); int remaining = node.Remaining;
            input.ProcessPointer(Camera.main.WorldToScreenPoint(new Vector3(-12, -5, 0)), false, false, false, true, false);
            for (int i = 0; i < 100; i++) game.World.Step();
            Assert.That(worker.Activity, Is.EqualTo(VillagerActivity.Idle)); Assert.That(node.Remaining, Is.EqualTo(remaining));
            Assert.That(worker.CarriedAmount, Is.EqualTo(3)); yield return null;
        }

        [UnityTest]
        public IEnumerator DeliveryInterruptedByHouseAutomaticallyDepositsThenReturnsToTheTree()
        {
            SelectFirst(); ResourceNodeState node = Order(ResourceType.Wood); var worker = game.World.GetUnit(1).Villager;
            for (int i = 0; i < 500 && worker.Activity != VillagerActivity.GoingToDropOff; i++) game.World.Step();
            Assert.That(worker.CarriedAmount, Is.EqualTo(10));
            var buildingInput = Object.FindFirstObjectByType<BuildingPlacementInput>();
            Assert.That(buildingInput.BeginPlacement(), Is.True);
            Assert.That(buildingInput.ConfirmAtScreen(Camera.main.WorldToScreenPoint(new Vector3(5.5f, -6.5f, 0))), Is.True);
            game.World.Step(); game.World.Step();
            BuildingState house = game.World.Buildings.Single();
            for (int i = 0; i < 700 && !house.IsComplete; i++) game.World.Step();
            Assert.That(house.IsComplete, Is.True); Assert.That(worker.Activity, Is.EqualTo(VillagerActivity.GoingToDropOff));
            Assert.That(worker.CarriedAmount, Is.EqualTo(10)); Assert.That(game.World.GetPlayerEconomy(1).Wood, Is.EqualTo(150));
            for (int i = 0; i < 700 && game.World.GetPlayerEconomy(1).Wood < 160; i++) game.World.Step();
            Assert.That(game.World.GetPlayerEconomy(1).Wood, Is.EqualTo(160)); Assert.That(worker.CarriedAmount, Is.Zero);
            for (int i = 0; i < 700 && worker.CarriedAmount == 0; i++) game.World.Step();
            Assert.That(worker.ResourceId, Is.EqualTo(node.Id)); Assert.That(worker.CarriedAmount, Is.GreaterThan(0));
            game.Units[0].Render(game.World.GetUnit(1), 1); hud.Refresh();
            Assert.That(hud.WoodText, Is.EqualTo("160")); yield return null;
        }

        [UnityTest]
        public IEnumerator DepletedTreeBanksPartialLoadThenChangesSourceAndWorkerHud()
        {
            SelectFirst(); ResourceNodeState node = Order(ResourceType.Wood); var worker = game.World.GetUnit(1).Villager;
            for (int i = 0; i < 300 && worker.CarriedAmount < 3; i++) game.World.Step();
            Assert.That(worker.CarriedAmount, Is.EqualTo(3));
            // Remove the remaining stock through the existing debug command, preserving the real cargo.
            game.Submit(new GameRTS.Commands.ResourceDebugCommand(game.NextCommandTick, 1, game.NextLocalSequence(), node.Id, 1000));
            for (int i = 0; i < 600 && game.World.GetPlayerEconomy(1).Wood < 203; i++) game.World.Step();
            Assert.That(node.IsDepleted, Is.True); Assert.That(game.World.GetPlayerEconomy(1).Wood, Is.EqualTo(203));
            Assert.That(worker.CarriedAmount, Is.Zero); Assert.That(worker.ResourceId, Is.Not.EqualTo(node.Id));
            var next = game.World.GetResource(worker.ResourceId);
            Assert.That(next.ResourceType, Is.EqualTo(ResourceType.Wood)); int before = next.Remaining;
            for (int i = 0; i < 600 && worker.CarriedAmount == 0; i++) game.World.Step();
            Assert.That(next.Remaining, Is.LessThan(before)); hud.Refresh();
            Assert.That(hud.transform.Find("Worker Status/Details").GetComponent<Text>().text, Does.Contain("Nguồn còn: " + next.Remaining));
            Assert.That(hud.WoodText, Is.EqualTo("203")); yield return null;
        }
    }
}
