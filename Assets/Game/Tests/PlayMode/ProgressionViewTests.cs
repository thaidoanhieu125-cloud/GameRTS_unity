using System.Collections;
using System.Linq;
using GameRTS.Commands;
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
    public sealed class ProgressionViewTests
    {
        private RtsGameView game;
        private UnitSelectionInput input;
        private IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("GameScene"); yield return null;
            game = Object.FindFirstObjectByType<RtsGameView>(); game.enabled = false;
            input = Object.FindFirstObjectByType<UnitSelectionInput>();
        }
        private Vector2 Screen(Vector3 v) => Camera.main.WorldToScreenPoint(v);
        private void Click(Vector3 v)
        { input.ProcessPointer(Screen(v), true, false, false, false, false); input.ProcessPointer(Screen(v), false, false, true, false, false); }
        private void Step(int count) { for (int i = 0; i < count; i++) game.World.Step(); game.RenderUnits(); game.RenderBuildings(); }
        [UnityTest] public IEnumerator RightClickHuntsAndBanksMeat()
        {
            yield return Load(); var animal = Object.FindObjectsByType<AnimalView>(FindObjectsSortMode.None).First(v => v.State.Animal.Definition.Kind == AnimalKind.Deer);
            Click(game.Units[0].transform.position); input.ProcessPointer(Screen(animal.transform.position), false, false, false, true, false);
            bool arrows = false, fled = false; int x = animal.State.X, y = animal.State.Y;
            for (int i = 0; i < 3000; i++) { game.World.Step(); arrows |= game.World.ProjectileCount > 0; fled |= animal.State.X != x || animal.State.Y != y; }
            animal.Render(); game.RenderUnits(); Object.FindFirstObjectByType<EconomyHudView>().Refresh();
            Assert.That(arrows && fled, Is.True); Assert.That(animal.State.IsAlive, Is.False);
            Assert.That(game.World.GetPlayerEconomy(1).Food, Is.GreaterThan(200)); Assert.That(game.World.GetPlayerEconomy(1).Population, Is.EqualTo(3));
            var meat = game.World.GetResource(animal.State.Animal.CorpseResourceId);
            Assert.That(meat.Remaining + game.World.GetPlayerEconomy(1).Food - 200 + game.World.GetUnit(1).Villager.CarriedAmount, Is.EqualTo(250));
        }
        [UnityTest] public IEnumerator AgeButtonCountsDownChangesTownAndUnlocks()
        {
            yield return Load(); var w = game.World;
            game.Submit(new EconomyDebugCommand(game.NextCommandTick, 1, game.NextLocalSequence(), EconomyDebugOperation.GrantResources, 500)); Step(2);
            var town = game.DropOffViews.Single(); Click(town.transform.position);
            var hud = Object.FindFirstObjectByType<ResearchHudView>(); hud.Refresh(); Assert.That(hud.IsVisible, Is.True);
            Assert.That(w.CanBuildInAge(1, "stable"), Is.False); hud.GetButton(TechnologyId.BronzeAge).onClick.Invoke(); Step(2); hud.Refresh();
            Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(200)); Assert.That(hud.StatusText, Does.Contain("giây"));
            var job = w.GetResearch(town.State.Id); Assert.That(job, Is.Not.Null);
            while (job.Progress < 1199) w.Step(); Assert.That(w.GetAge(1), Is.EqualTo(1)); w.Step(); Assert.That(w.GetAge(1), Is.EqualTo(2));
            town.Render(w, true); Assert.That(town.GetComponent<SpriteRenderer>().sprite, Is.SameAs(town.Data.BronzeSprite));
            Click(game.Units[0].transform.position); var build = Object.FindFirstObjectByType<ConstructionHudView>(); build.RefreshBuildOptions();
            Assert.That(build.transform.Find("Top Bar/Build Stable").GetComponent<Button>().interactable, Is.True);
            Assert.That(build.transform.Find("Top Bar/Build watch_tower").GetComponent<Button>().interactable, Is.True);
            yield return null;
        }
        [UnityTest] public IEnumerator LumberCampCanBeSelectedToResearchAxe()
        {
            yield return Load(); game.Submit(new EconomyDebugCommand(game.NextCommandTick, 1, game.NextLocalSequence(), EconomyDebugOperation.GrantResources, 500)); Step(2);
            Click(game.Units[0].transform.position); var build = Object.FindFirstObjectByType<ConstructionHudView>(); build.RefreshBuildOptions();
            build.transform.Find("Top Bar/Build Lumber Camp").GetComponent<Button>().onClick.Invoke();
            Assert.That(Object.FindFirstObjectByType<BuildingPlacementInput>().ConfirmAtScreen(Screen(new Vector3(-5.5f, -4.5f, 0))), Is.True);
            Step(900); var camp = game.World.Buildings.Single(); Assert.That(camp.IsComplete, Is.True);
            yield return null; yield return null; Click(game.BuildingViews[camp.Id].transform.position);
            var hud = Object.FindFirstObjectByType<ResearchHudView>(); hud.Refresh(); Assert.That(hud.IsVisible, Is.True);
            Assert.That(game.SelectedTrainingBuildingId, Is.EqualTo(camp.Id)); hud.GetButton(TechnologyId.DoubleAxe).onClick.Invoke(); Step(402);
            Assert.That(game.World.HasTechnology(1, TechnologyId.DoubleAxe), Is.True); yield return null;
        }
    }
}
