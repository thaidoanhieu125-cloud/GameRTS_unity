using GameRTS.Simulation;
using UnityEngine;
namespace GameRTS.View
{
    // Presentation only. Visibility and explored cells are decided by the deterministic world.
    public sealed class FogOfWarView : MonoBehaviour
    {
        private RtsGameView game;
        private Texture2D texture;
        private SpriteRenderer layer;
        private int lastTick=-1;
        private readonly System.Collections.Generic.Dictionary<int,GameObject> blasts=new System.Collections.Generic.Dictionary<int,GameObject>();
        private Sprite ring;
        private Color32[] pixels;
        public byte AlphaAt(int cell) => pixels[cell].a;
        private void LateUpdate()
        {
            if(game==null) game=GetComponent<RtsGameView>();
            var w=game.World; if(!w.VisionEnabled) return;
            if(texture==null)
            {
                var map=w.Map; texture=new Texture2D(map.Width,map.Height,TextureFormat.RGBA32,false); texture.filterMode=FilterMode.Point; texture.wrapMode=TextureWrapMode.Clamp;
                pixels=new Color32[map.Count];
                var go=new GameObject("Fog of War"); go.transform.SetParent(transform);
                layer=go.AddComponent<SpriteRenderer>(); layer.sprite=Sprite.Create(texture,new Rect(0,0,map.Width,map.Height),Vector2.zero,1);
                go.transform.position=new Vector3(map.CenterX(0)/1000f-.5f,map.CenterY(0)/1000f-.5f,0); layer.sortingOrder=30000;
            }
            if(w.Tick==lastTick) return; lastTick=w.Tick;
            for(int cell=0;cell<pixels.Length;cell++) pixels[cell]=new Color32(0,0,0,w.IsCellVisible(RtsGameView.LocalPlayerId,cell)?(byte)0:w.IsCellExplored(RtsGameView.LocalPlayerId,cell)?(byte)150:(byte)255);
            for(int cell=0;cell<pixels.Length;cell++) if(pixels[cell].a==150) pixels[cell]=new Color32(45,45,45,150);
            texture.SetPixels32(pixels); texture.Apply(false);
            var alive=new System.Collections.Generic.HashSet<int>();
            foreach(var e in w.Explosions)
            {
                alive.Add(e.BuildingId);
                if(!blasts.TryGetValue(e.BuildingId,out var obj))
                {
                    if(game.Units.Count==0) continue;
                    if(ring==null) ring=game.Units[0].transform.Find("SelectionCircle").GetComponent<SpriteRenderer>().sprite;
                    obj=new GameObject("Mine explosion"); obj.transform.SetParent(transform); var renderer=obj.AddComponent<SpriteRenderer>(); renderer.sprite=ring; renderer.color=new Color(1,.5f,.05f,.8f); renderer.sortingOrder=50; blasts.Add(e.BuildingId,obj);
                }
                obj.transform.position=new Vector3(e.X/1000f,e.Y/1000f,0); obj.transform.localScale=Vector3.one*(1f+(w.Tick-e.Tick)*.2f);
                obj.SetActive(w.IsCellVisible(RtsGameView.LocalPlayerId,w.Map.CellAt(e.X,e.Y)));
            }
            foreach(var id in new System.Collections.Generic.List<int>(blasts.Keys)) if(!alive.Contains(id)) {Destroy(blasts[id]);blasts.Remove(id);}
        }
        private void OnDestroy() { if(layer!=null) Destroy(layer.sprite); if(texture!=null) Destroy(texture); }
    }
}
