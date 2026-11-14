using System.Collections;
using GameRTS.Commands;
using GameRTS.Input;
using GameRTS.UI;
using GameRTS.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GameRTS.Tests
{
    public sealed class EconomyHudTests
    {
        private RtsGameView game;
        private EconomyHudView hud;
        private EconomyTestInput input;

        [UnitySetUp]
        public IEnumerator LoadGame()
        {
            yield return SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single);
            yield return null;
            game = Object.FindFirstObjectByType<RtsGameView>();
            hud = Object.FindFirstObjectByType<EconomyHudView>();
            input = Object.FindFirstObjectByType<EconomyTestInput>();
            Assert.That(hud, Is.Not.Null); game.enabled = false;
        }

        [UnityTest]
        public IEnumerator InitialHudShowsRealWalletAndFiveSpawnedUnits()
        {
            hud.Refresh();
            Assert.That(hud.FoodText, Is.EqualTo("200")); Assert.That(hud.WoodText, Is.EqualTo("200"));
            Assert.That(hud.GoldText, Is.EqualTo("100")); Assert.That(hud.PopulationText, Is.EqualTo("3/10"));
            Assert.That(game.World.GetPlayerEconomy(1).Population, Is.EqualTo(game.World.Units.Count));
            Assert.That(hud.TestPanelOpen, Is.False);
            Assert.That(hud.GetComponentsInChildren<Image>(true).Length, Is.GreaterThanOrEqualTo(8));
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestButtonsQueueTransactionsThenHudFollowsSimulation()
        {
            hud.ToggleTestPanel();
            hud.transform.Find("Wallet Test Panel/Grant").GetComponent<Button>().onClick.Invoke();
            Assert.That(game.World.GetPlayerEconomy(1).Gold, Is.EqualTo(100));
            game.World.Step(); game.World.Step(); yield return null;
            Assert.That(hud.GoldText, Is.EqualTo("150")); Assert.That(hud.WoodText, Is.EqualTo("250"));
            for (int i = 0; i < 3; i++)
            {
                input.Request(EconomyDebugOperation.SpendResources); game.World.Step(); game.World.Step();
            }
            yield return null;
            Assert.That(hud.GoldText, Is.EqualTo("0")); Assert.That(hud.WoodText, Is.EqualTo("100"));
            input.Request(EconomyDebugOperation.SpendResources); game.World.Step(); game.World.Step(); yield return null;
            Assert.That(hud.GoldText, Is.EqualTo("0")); Assert.That(hud.WoodText, Is.EqualTo("100"));
            Assert.That(hud.FeedbackText, Does.Contain("Từ chối"));
            input.Request(EconomyDebugOperation.IncreasePopulationLimit); game.World.Step(); game.World.Step(); yield return null;
            Assert.That(hud.PopulationText, Is.EqualTo("3/15"));
            Assert.That(game.World.Units.Count, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator HudConsumesClicksAndMovementStillUsesUniqueSequences()
        {
            var selection = Object.FindFirstObjectByType<UnitSelectionView>();
            var controls = Object.FindFirstObjectByType<UnitSelectionInput>();
            Vector2 unit = Camera.main.WorldToScreenPoint(game.Units[0].transform.position);
            controls.ProcessPointer(unit, true, false, false, false, false);
            controls.ProcessPointer(unit, false, false, true, false, false);
            Assert.That(selection.SelectedIds.Count, Is.EqualTo(1));
            Vector2 bar = RectTransformUtility.WorldToScreenPoint(null, hud.TopBar.TransformPoint(hud.TopBar.rect.center));
            Assert.That(EconomyHudView.BlocksPointer(bar), Is.True);
            controls.ProcessPointer(bar, true, false, false, false, false);
            controls.ProcessPointer(bar, false, false, true, true, false);
            Assert.That(selection.SelectedIds.Count, Is.EqualTo(1));
            input.Request(EconomyDebugOperation.GrantResources);
            Vector2 target = Camera.main.WorldToScreenPoint(new Vector3(-11, 0, 0));
            controls.ProcessPointer(target, false, false, false, true, false);
            game.World.Step(); game.World.Step(); yield return null;
            Assert.That(hud.GoldText, Is.EqualTo("150"));
            Assert.That(game.World.GetUnit(1).IsMoving, Is.True);
        }
    }
}
