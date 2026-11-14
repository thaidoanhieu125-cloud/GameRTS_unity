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
    public sealed class TrainingViewTests
    {
        private RtsGameView game;
        private UnitSelectionInput input;
        private TrainingHudView hud;
        private TrainingState center;
        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single); yield return null;
            game = Object.FindFirstObjectByType<RtsGameView>(); game.enabled = false;
            input = Object.FindFirstObjectByType<UnitSelectionInput>(); hud = Object.FindFirstObjectByType<TrainingHudView>();
            center = game.World.TrainingCenters.Single();
        }
        private void SelectTown()
        {
            Vector2 point = Camera.main.WorldToScreenPoint(game.DropOffViews[0].transform.position);
            input.ProcessPointer(point, true, false, false, false, false); input.ProcessPointer(point, false, false, true, false, false); hud.Refresh();
            Assert.That(game.SelectedTrainingBuildingId, Is.EqualTo(center.BuildingId)); Assert.That(game.SelectedCount, Is.Zero);
        }
        private void Train() => hud.transform.Find("Training Panel/Train Villager").GetComponent<Button>().onClick.Invoke();
        private void Steps(int count) { for (int i = 0; i < count; i++) game.World.Step(); game.RenderUnits(); hud.Refresh(); }
        [UnityTest]
        public IEnumerator TownSelectionTrainsNewVisibleSelectableVillager()
        {
            Assert.That(hud.IsVisible, Is.False); SelectTown(); Train(); Steps(2);
            Assert.That(hud.IsVisible, Is.True); Assert.That(hud.QueueText, Does.Contain("[1/5]"));
            Assert.That(game.World.GetPlayerEconomy(1).Food, Is.EqualTo(150)); Steps(99);
            Assert.That(hud.Progress, Is.EqualTo(.5f).Within(.01f)); Steps(100);
            Assert.That(game.Units.Count, Is.EqualTo(4)); Assert.That(center.Queue.Count, Is.Zero);
            var economy = Object.FindFirstObjectByType<EconomyHudView>(); economy.Refresh();
            Assert.That(economy.PopulationText, Is.EqualTo("4/10"));
            Object.FindFirstObjectByType<UnitSelectionView>().Clear(); hud.Refresh();
            Vector2 point = Camera.main.WorldToScreenPoint(game.Units[3].transform.position);
            input.ProcessPointer(point, true, false, false, false, false); input.ProcessPointer(point, false, false, true, false, false);
            Assert.That(game.Units[3].IsSelected, Is.True); Assert.That(game.SelectedTrainingBuildingId, Is.Zero);
            hud.Refresh(); Assert.That(hud.IsVisible, Is.False); yield return null;
        }
        [UnityTest]
        public IEnumerator QueueIconRightClickRefundsAndConsumesMapInput()
        {
            SelectTown(); Train(); Train(); Steps(2);
            var slot = hud.transform.Find("Training Panel/Queue Slot 2").GetComponent<TrainingQueueSlotView>();
            var rect = slot.GetComponent<RectTransform>(); Vector2 point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            Assert.That(EconomyHudView.BlocksPointer(point), Is.True);
            input.ProcessPointer(point, false, false, false, true, false);
            slot.OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Right }); Steps(2);
            Assert.That(center.Queue.Count, Is.EqualTo(1)); Assert.That(game.World.GetPlayerEconomy(1).Food, Is.EqualTo(150));
            Assert.That(center.RallyCell, Is.EqualTo(-1)); yield return null;
        }
        [UnityTest]
        public IEnumerator ResourceRallyShowsFlagAndNewVillagerBanksWood()
        {
            SelectTown(); var tree = game.ResourceViews.First(v => v.State.ResourceType == ResourceType.Wood);
            input.ProcessPointer(Camera.main.WorldToScreenPoint(tree.transform.position), false, false, false, true, false);
            Train(); Steps(2); Assert.That(center.RallyResourceId, Is.EqualTo(tree.State.Id));
            game.DropOffViews[0].Render(game.World, true);
            Assert.That(game.DropOffViews[0].transform.Find("Rally Flag").gameObject.activeSelf, Is.True);
            Steps(650); Assert.That(game.World.GetPlayerEconomy(1).Wood, Is.GreaterThan(200));
            Assert.That(game.World.GetUnit(4).Villager.ResourceId, Is.EqualTo(tree.State.Id)); yield return null;
        }
        [UnityTest]
        public IEnumerator FullPopulationShowsHouseMessageWithoutCharging()
        {
            SelectTown(); game.World.GetPlayerEconomy(1).TryReservePopulation(7); Train(); Steps(2);
            Assert.That(hud.StatusText, Is.EqualTo("Cần xây thêm Nhà dân")); Assert.That(center.Queue.Count, Is.Zero);
            Assert.That(game.World.GetPlayerEconomy(1).Food, Is.EqualTo(200)); yield return null;
        }
    }
}
