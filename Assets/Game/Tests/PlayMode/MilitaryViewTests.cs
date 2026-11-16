using System.Collections;
using System.Linq;
using GameRTS.Commands;
using GameRTS.Input;
using GameRTS.Simulation;
using GameRTS.UI;
using GameRTS.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GameRTS.Tests
{
    public sealed class MilitaryViewTests
    {
        private RtsGameView game;
        private UnitSelectionInput input;
        private TrainingHudView trainingHud;
        private IEnumerator Load(string scene = "GameScene")
        {
            yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single); yield return null;
            game = Object.FindFirstObjectByType<RtsGameView>(); game.enabled = false;
            input = Object.FindFirstObjectByType<UnitSelectionInput>(); trainingHud = Object.FindFirstObjectByType<TrainingHudView>();
        }
        private Vector2 Screen(Vector3 point) => Camera.main.WorldToScreenPoint(point);
        private void Click(Vector2 point)
        { input.ProcessPointer(point, true, false, false, false, false); input.ProcessPointer(point, false, false, true, false, false); }
        private void Step(int count)
        { for (int i = 0; i < count; i++) game.World.Step(); game.RenderUnits(); game.RenderBuildings(); trainingHud.Refresh(); }
        private IEnumerator Build(string name)
        {
            Click(Screen(game.Units[0].transform.position)); var hud = Object.FindFirstObjectByType<ConstructionHudView>(); hud.RefreshBuildOptions();
            hud.transform.Find("Top Bar/Build " + name).GetComponent<Button>().onClick.Invoke();
            var placement = Object.FindFirstObjectByType<BuildingPlacementInput>(); Assert.That(placement.ConfirmAtScreen(Screen(new Vector3(5.5f, -6.5f))), Is.True);
            Step(2); var building = game.World.Buildings.Single(); Assert.That(game.World.GetTraining(building.Id), Is.Null);
            for (int i = 0; i < 900 && !building.IsComplete; i++) game.World.Step(); game.RenderUnits(); game.RenderBuildings();
            // Placement consumes its release gesture through the following frame.
            Assert.That(building.IsComplete, Is.True); yield return null; yield return null;
            Click(Screen(game.BuildingViews[building.Id].transform.position)); trainingHud.Refresh();
            Assert.That(game.SelectedTrainingBuildingId, Is.EqualTo(building.Id)); Assert.That(trainingHud.IsVisible, Is.True);
        }
        private void Train() => trainingHud.transform.Find("Training Panel/Train Villager").GetComponent<Button>().onClick.Invoke();
        [UnityTest]
        public IEnumerator AllThreeButtonsBuildTrainAndRallyTheirSoldier()
        {
            string[] names = { "Barracks", "ArcheryRange", "Stable" };
            UnitClass[] classes = { UnitClass.Spearman, UnitClass.Archer, UnitClass.Cavalry };
            for (int kind = 0; kind < 3; kind++)
            {
                yield return Load();
                if (kind == 2)
                {
                    game.Submit(new EconomyDebugCommand(game.NextCommandTick, 1, game.NextLocalSequence(), EconomyDebugOperation.GrantResources, 500)); Step(2);
                    int town = game.DropOffViews.Single().State.Id;
                    game.Submit(new ResearchCommand(game.NextCommandTick, 1, game.NextLocalSequence(), town, TechnologyId.BronzeAge)); Step(1202);
                    Assert.That(game.World.GetAge(1), Is.EqualTo(2));
                }
                yield return Build(names[kind]);
                Vector3 point = new Vector3(11.5f, -.5f, 0); input.ProcessPointer(Screen(point), false, false, false, true, false);
                Train(); Step(202); var unit = game.World.Units.Last(u => u.OwnerId == 1 && u.Animal == null);
                Assert.That(unit, Is.Not.Null); Assert.That(unit.Stats.Class, Is.EqualTo(classes[kind])); Assert.That(unit.Villager, Is.Null);
                Step(350); Assert.That(unit.IsMoving, Is.False); Assert.That(unit.X, Is.EqualTo(11500));
                Assert.That(game.World.GetPlayerEconomy(1).Population, Is.EqualTo(4));
                Assert.That(trainingHud.transform.Find("Training Panel/Title").GetComponent<Text>().text, Does.Not.Contain("TÒA THỊ CHÍNH"));
                game.RenderBuildings(); Assert.That(game.BuildingViews[game.World.Buildings.Single().Id].transform.Find("Rally Flag").gameObject.activeSelf, Is.True);
            }
        }
        [UnityTest]
        public IEnumerator MilitarySlotRightClickRefundsWoodAndFood()
        {
            yield return Load(); yield return Build("Barracks"); Train(); Step(2);
            var economy = game.World.GetPlayerEconomy(1); Assert.That(economy.Wood, Is.EqualTo(25)); Assert.That(economy.Food, Is.EqualTo(150));
            var slot = trainingHud.transform.Find("Training Panel/Queue Slot 1").GetComponent<TrainingQueueSlotView>();
            slot.OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Right }); Step(2);
            Assert.That(economy.Wood, Is.EqualTo(50)); Assert.That(economy.Food, Is.EqualTo(200)); Assert.That(game.World.QueuedPopulation(1), Is.Zero);
        }
        [UnityTest]
        public IEnumerator VictoryDefeatFreezeInputAndRestartFreshMatch()
        {
            for (int side = 0; side < 2; side++)
            {
                yield return Load("CombatScene");
                int player = side == 0 ? 1 : 2, killer = side == 0 ? 8 : 9;
                var town = game.DropOffViews.Single(v => v.State.OwnerId != player);
                game.Submit(new MoveCommand(game.NextCommandTick, 1, game.NextLocalSequence(), side == 0 ? new[] { 4, 5 } : new[] { 4, 5, 8 }, -20500, 20500));
                game.Submit(new MoveCommand(game.NextCommandTick, 2, 1, side == 0 ? new[] { 6, 7, 9 } : new[] { 6, 7 }, 20500, 20500));
                game.Submit(new AttackCommand(game.NextCommandTick, player, side == 0 ? game.NextLocalSequence() : 2, new[] { killer }, AttackTargetKind.Building, town.State.Id));
                for (int i = 0; i < 2600 && !game.World.IsMatchOver; i++) game.World.Step(); game.RenderUnits();
                var overlay = Object.FindFirstObjectByType<MatchHudView>(); overlay.Refresh();
                Assert.That(game.World.IsMatchOver, Is.True); Assert.That(overlay.IsVisible, Is.True);
                Assert.That(overlay.Title, Does.Contain(side == 0 ? "VICTORY" : "DEFEAT")); Assert.That(EconomyHudView.BlocksPointer(new Vector2(400, 300)), Is.True);
                int tick = game.World.Tick; Step(100); Assert.That(game.World.Tick, Is.EqualTo(tick));
                overlay.transform.Find("Match Result/Panel/Restart").GetComponent<Button>().onClick.Invoke(); yield return null; yield return null;
                var fresh = Object.FindFirstObjectByType<RtsGameView>(); Assert.That(fresh.World.IsMatchOver, Is.False);
                Assert.That(fresh.World.Units.Count, Is.EqualTo(9)); Assert.That(fresh.World.GetPlayerEconomy(1).Population, Is.EqualTo(6));
                Assert.That(Object.FindFirstObjectByType<MatchHudView>().IsVisible, Is.False);
            }
        }
    }
}
