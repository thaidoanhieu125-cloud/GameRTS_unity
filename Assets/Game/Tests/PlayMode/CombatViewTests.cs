using System.Collections;
using System.Linq;
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
    public sealed class CombatViewTests
    {
        private RtsGameView game;
        private UnitSelectionInput input;
        private UnitSelectionView selection;
        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("CombatScene", LoadSceneMode.Single); yield return null;
            game = Object.FindFirstObjectByType<RtsGameView>(); game.enabled = false;
            input = Object.FindFirstObjectByType<UnitSelectionInput>(); selection = Object.FindFirstObjectByType<UnitSelectionView>();
        }
        private static Vector2 Screen(Vector3 point) => Camera.main.WorldToScreenPoint(point);
        private void Click(Vector2 point)
        { input.ProcessPointer(point, true, false, false, false, false); input.ProcessPointer(point, false, false, true, false, false); }
        [UnityTest]
        public IEnumerator SceneSeparatesTeamsAndEnemyCannotBeSelected()
        {
            Assert.That(game.World.Units.Count, Is.EqualTo(7)); Assert.That(game.World.GetPlayerEconomy(1).Population, Is.EqualTo(5));
            Assert.That(game.World.GetPlayerEconomy(2).Population, Is.EqualTo(2));
            Click(Screen(game.Units[5].transform.position)); Assert.That(game.SelectedCount, Is.Zero);
            var fill = game.Units[5].transform.Find("HealthBar/Fill").GetComponent<SpriteRenderer>(); Assert.That(fill.color.r, Is.GreaterThan(fill.color.g));
            Click(Screen(game.Units[4].transform.position)); Assert.That(selection.SelectedIds.Single(), Is.EqualTo(5)); yield return null;
        }
        [UnityTest]
        public IEnumerator RightClickEnemyLaunchesVisibleArrowThenHealthFalls()
        {
            Click(Screen(game.Units[4].transform.position)); var target = game.World.GetUnit(6);
            input.ProcessPointer(Screen(game.Units[5].transform.position), false, false, false, true, false);
            for (int i = 0; i < 500 && game.World.ProjectileCount == 0; i++) game.World.Step();
            Assert.That(game.World.ProjectileCount, Is.GreaterThan(0)); Assert.That(target.CurrentHP, Is.EqualTo(target.MaxHP));
            game.RenderProjectiles(); Assert.That(game.VisibleProjectileCount, Is.GreaterThan(0));
            for (int i = 0; i < 100 && target.CurrentHP == target.MaxHP; i++) game.World.Step(); game.RenderUnits(); game.RenderProjectiles();
            Assert.That(target.CurrentHP, Is.LessThan(target.MaxHP));
            Assert.That(game.Units[5].GetComponentInChildren<HealthBarView>().Fraction, Is.LessThan(1)); yield return null;
        }
        [UnityTest]
        public IEnumerator RightClickEnemyTownSendsBuildingAttack()
        {
            Click(Screen(game.Units[4].transform.position)); var town = game.DropOffViews.Single(v => v.State.OwnerId == 2);
            input.ProcessPointer(Screen(town.transform.position), false, false, false, true, false); game.World.Step(); game.World.Step();
            Assert.That(game.World.GetUnit(5).TargetKind, Is.EqualTo(AttackTargetKind.Building));
            Assert.That(game.World.GetUnit(5).TargetId, Is.EqualTo(town.State.Id)); yield return null;
        }
        [UnityTest]
        public IEnumerator DeadVillagerDisappearsAndSelectionPopulationUpdate()
        {
            Click(Screen(game.Units[0].transform.position));
            game.Submit(new MoveCommand(game.NextCommandTick, 1, game.NextLocalSequence(), new[] { 4, 5 }, -20500, 20500));
            game.Submit(new MoveCommand(game.NextCommandTick, 2, 1, new[] { 7 }, 20500, 20500));
            game.Submit(new AttackCommand(game.NextCommandTick, 2, 2, new[] { 6 }, AttackTargetKind.Unit, 1));
            for (int i = 0; i < 1000 && game.World.GetUnit(1).IsAlive; i++) game.World.Step();
            Assert.That(game.World.GetUnit(1).IsAlive, Is.False); game.RenderUnits(); selection.RefreshSelection();
            Assert.That(game.Units[0].gameObject.activeSelf, Is.False); Assert.That(selection.SelectedIds.Count, Is.Zero);
            Assert.That(game.World.GetPlayerEconomy(1).Population, Is.EqualTo(4));
            var hud = Object.FindFirstObjectByType<EconomyHudView>(); hud.Refresh(); Assert.That(hud.PopulationText, Is.EqualTo("4/10")); yield return null;
        }
        [UnityTest]
        public IEnumerator NavigationButtonConsumesInputAndLoadsEconomyMap()
        {
            var view = Object.FindFirstObjectByType<SceneSwitchView>(); var button = view.transform.Find("Switch Map").GetComponent<Button>();
            var rect = button.GetComponent<RectTransform>(); var point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            Assert.That(EconomyHudView.BlocksPointer(point), Is.True); button.onClick.Invoke(); yield return null; yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("GameScene"));
            Assert.That(Object.FindFirstObjectByType<RtsGameView>().World.Units.Count, Is.EqualTo(3));
        }
    }
}
