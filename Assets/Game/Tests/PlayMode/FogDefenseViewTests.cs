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
    public sealed class FogDefenseViewTests
    {
        [UnityTest]
        public IEnumerator FogAndRememberedTown()
        {
            yield return SceneManager.LoadSceneAsync("AiScene");yield return null;
            var game=Object.FindFirstObjectByType<RtsGameView>();game.enabled=false;var w=game.World;
            var fog=game.GetComponent<FogOfWarView>();yield return new WaitForEndOfFrame();
            Assert.That(fog.AlphaAt(0),Is.EqualTo(255));Assert.That(fog.AlphaAt(w.DropOffs.First(d=>d.OwnerId==1).Cell),Is.Zero);
            var scout=w.Units.First(u=>u.OwnerId==1);int site=w.Map.Index(43,35);
            w.Submit(new MoveCommand(w.Tick+1,1,100000,new[]{scout.Id},w.Map.CenterX(site),w.Map.CenterY(site)));
            for(int i=0;i<320;i++) w.Step();
            Assert.That(w.IsCellVisible(1,w.DropOffs.First(d=>d.OwnerId==2).Cell),Is.True);
            game.enabled=true;yield return null;yield return new WaitForEndOfFrame();game.enabled=false;
            var enemy=game.DropOffViews.Single(d=>d.State.OwnerId==2);var memory=game.transform.Find("Last seen "+enemy.State.Id).GetComponent<SpriteRenderer>();var oldSprite=memory.sprite;
            w.Submit(new MoveCommand(w.Tick+1,1,100001,new[]{scout.Id},w.Map.CenterX(w.Map.Index(14,27)),w.Map.CenterY(w.Map.Index(14,27))));
            for(int i=0;i<360;i++) w.Step();
            game.enabled=true;yield return null;yield return new WaitForEndOfFrame();game.enabled=false;
            Assert.That(enemy.gameObject.activeSelf,Is.False);Assert.That(memory.gameObject.activeSelf,Is.True);Assert.That(memory.sprite,Is.SameAs(oldSprite));
            Assert.That(fog.AlphaAt(site),Is.EqualTo(150));
            Assert.That(game.Units.Where(v=>w.GetUnit(v.InstanceId).OwnerId==2).All(v=>!v.gameObject.activeSelf),Is.True);
        }
        [UnityTest]
        public IEnumerator DefenseButtonsBuildRealAssets()
        {
            foreach(string type in new[]{"wall","outpost","watch_tower","landmine"})
            {
                yield return SceneManager.LoadSceneAsync("GameScene");yield return null;
                var game=Object.FindFirstObjectByType<RtsGameView>();game.enabled=false;
                var selection=Object.FindFirstObjectByType<UnitSelectionView>();selection.SelectAt(Camera.main.WorldToScreenPoint(game.Units[0].transform.position));
                if (type == "watch_tower")
                {
                    game.Submit(new EconomyDebugCommand(game.NextCommandTick,1,game.NextLocalSequence(),EconomyDebugOperation.GrantResources,500));
                    game.World.Step();game.World.Step();
                    game.Submit(new ResearchCommand(game.NextCommandTick,1,game.NextLocalSequence(),game.DropOffViews.Single().State.Id,TechnologyId.BronzeAge));
                    for(int i=0;i<1202;i++) game.World.Step();
                }
                var hud=Object.FindFirstObjectByType<ConstructionHudView>();hud.RefreshBuildOptions();
                var button=hud.transform.Find("Top Bar/Build "+type).GetComponent<Button>();Assert.That(button.gameObject.activeSelf,Is.True);
                button.onClick.Invoke();var preview=Object.FindFirstObjectByType<BuildingPlacementView>();Assert.That(preview.Data.TypeId,Is.EqualTo(type));
                var input=Object.FindFirstObjectByType<BuildingPlacementInput>();Assert.That(input.ConfirmAtScreen(Camera.main.WorldToScreenPoint(new Vector3(-5.5f,-4.5f,0))),Is.True);
                game.World.Step();game.World.Step();var b=game.World.GetBuilding(game.World.LastPlacedBuildingId);
                for(int i=0;i<800 && !b.IsComplete;i++) game.World.Step();Assert.That(b.IsComplete,Is.True);
                game.RenderBuildings();Assert.That(game.BuildingViews[b.Id].DisplayedSprite,Is.SameAs(game.GetBuildingData(type).CompletedSprite));
            }
        }
    }
}
