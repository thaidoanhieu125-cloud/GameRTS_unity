using System.Collections;
using GameRTS.Input;
using GameRTS.Simulation;
using GameRTS.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Rendering;
using System.IO;

namespace GameRTS.Tests
{
    public sealed class LandscapeViewTests
    {
        [UnityTest]
        public IEnumerator ExpandedSceneMatchesVisibleLakesAndKeepsForestHarvestable()
        {
            yield return SceneManager.LoadSceneAsync("GameScene");yield return null;
            var game=Object.FindFirstObjectByType<RtsGameView>();game.enabled=false;
            Assert.That(game.World.Map.Width,Is.EqualTo(128));Assert.That(game.World.Map.Height,Is.EqualTo(192));
            Assert.That(Object.FindFirstObjectByType<RtsMapView>().UsesMeadowLandscape,Is.True);
            Assert.That(Object.FindFirstObjectByType<LandscapeSurfaceView>(),Is.Not.Null);
            Assert.That(game.ResourceViews.Count,Is.GreaterThan(100));
            for(int i=0;i<MeadowMapLayout.LakeCount;i++)
            { var l=MeadowMapLayout.GetLake(i);Assert.That(game.World.Map.IsWalkable(game.World.Map.CellAt(l.X*1000,l.Y*1000)),Is.False); }
            foreach(var view in game.ResourceViews)
            { Assert.That(game.World.Map.HasResourceBlocker(view.State.Cell),Is.True);Assert.That(view.IsVisible,Is.True); }
            game.RenderUnits();game.RenderBuildings();
            Capture(Camera.main,new Vector3(0,0,-10),10,"Logs/Landscape.play-start.png");
            Capture(Camera.main,new Vector3(0,34,-10),18,"Logs/Landscape.play-lake.png");
            Capture(Camera.main,new Vector3(0,64,-10),32,"Logs/Landscape.play-center.png");
        }
        [UnityTest]
        public IEnumerator ClickingTreeCrownOrdersGatherAtTheTrunkCell()
        {
            yield return SceneManager.LoadSceneAsync("GameScene");yield return null;
            var game=Object.FindFirstObjectByType<RtsGameView>();game.enabled=false;
            var input=Object.FindFirstObjectByType<UnitSelectionInput>();var unit=game.Units[0];
            Vector2 selected=Camera.main.WorldToScreenPoint(unit.transform.position);
            input.ProcessPointer(selected,true,false,true,false,false);
            var tree=game.ResourceViews[0];Vector2 crown=(Vector2)tree.transform.position+new Vector2(0,1.6f);
            Assert.That(game.World.GetResourceAt(game.World.Map.CellAt(RtsGameView.ToSimulationCoordinate(crown.x),RtsGameView.ToSimulationCoordinate(crown.y))),Is.Null);
            Assert.That(game.ResourceAtVisualPoint(crown)?.Id,Is.EqualTo(tree.State.Id));
            Vector2 screen=Camera.main.WorldToScreenPoint(crown);input.ProcessPointer(screen,false,false,false,true,false);
            game.World.Step();game.World.Step();
            Assert.That(game.World.GetUnit(unit.InstanceId).Villager.ResourceId,Is.EqualTo(tree.State.Id));
        }
        private static void Capture(Camera camera,Vector3 position,float zoom,string file)
        {
            Vector3 oldPosition=camera.transform.position;float oldZoom=camera.orthographicSize;
            var rt=new RenderTexture(1600,1000,24,RenderTextureFormat.ARGB32);
            var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);var previous=RenderTexture.active;
            try
            {
                camera.transform.position=position;camera.orthographicSize=zoom;rt.Create();
                RenderPipeline.SubmitRenderRequest(camera,new RenderPipeline.StandardRequest{destination=rt});
                RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();
                File.WriteAllBytes(file,image.EncodeToPNG());
            }
            finally
            { camera.transform.position=oldPosition;camera.orthographicSize=oldZoom;RenderTexture.active=previous;rt.Release();Object.Destroy(rt);Object.Destroy(image); }
        }
    }
}
