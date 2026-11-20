using GameRTS.Simulation;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameRTS.View
{
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class LandscapeSurfaceView : MonoBehaviour
    {
        [SerializeField] private Material surfaceMaterial;
        [SerializeField] private Mesh surfaceMesh;
        [SerializeField] private Sprite[] rockSprites;
        [SerializeField] private Material spriteMaterial;
        private GameObject preview;
        private void OnEnable() => Rebuild();
        public void Rebuild()
        {
            Clear();
            if(surfaceMaterial==null || surfaceMesh==null) return;
            preview=new GameObject("Landscape presentation") { hideFlags=HideFlags.DontSave };
            preview.transform.SetParent(transform,false);
            var floor=new GameObject("Grass and curved lakes",typeof(MeshFilter),typeof(MeshRenderer));
            floor.transform.SetParent(preview.transform,false);
            floor.GetComponent<MeshFilter>().sharedMesh=surfaceMesh;
            // North of y=100, world-depth orders become negative. Ground stays below every prop.
            var renderer=floor.GetComponent<MeshRenderer>();renderer.sharedMaterial=surfaceMaterial;renderer.sortingOrder=-20000;
            var data=new Vector4[12];
            for(int i=0;i<MeadowMapLayout.LakeCount;i++)
            { var lake=MeadowMapLayout.GetLake(i);data[i]=new Vector4(lake.X,lake.Y,lake.RadiusX,lake.RadiusY); }
            var block=new MaterialPropertyBlock();block.SetVectorArray("_Lakes",data);block.SetInt("_LakeCount",MeadowMapLayout.LakeCount);renderer.SetPropertyBlock(block);
            if(rockSprites==null || rockSprites.Length==0) return;
            foreach(var cell in MeadowMapLayout.Rocks) AddRock(cell.X,cell.Y);
            var map=GetComponent<RtsMapView>();
            if(map!=null && map.Obstacles!=null)
                foreach(var pos in map.Obstacles.cellBounds.allPositionsWithin)
                    if(map.Obstacles.HasTile(pos) && !MeadowMapLayout.IsRockCell(pos.x,pos.y)) AddRock(pos.x,pos.y);
        }
        private void AddRock(int x,int y)
        {
            var go=new GameObject("Rock "+x+","+y,typeof(SpriteRenderer),typeof(WorldDepthView));
            uint hash=(uint)(x*73856093 ^ y*19349663);
            go.transform.SetParent(preview.transform,false);
            go.transform.localPosition=new Vector3(x+.5f+(hash%41-20f)/100f,y+.5f+((hash/41)%41-20f)/100f,0);
            float ridge=1f-Mathf.Max(Mathf.Abs(Mathf.Abs(x)-35)/11f,Mathf.Min(Mathf.Abs(y-22),Mathf.Abs(y-106))/5f);
            go.transform.localScale=Vector3.one*(1.2f+(hash%60)/100f+Mathf.Max(0,ridge)*1.2f);
            int index=(int)(hash%(uint)rockSprites.Length);
            var r=go.GetComponent<SpriteRenderer>();r.sprite=rockSprites[index];r.sharedMaterial=spriteMaterial;
        }
        private void OnDisable() => Clear();
        private void Clear() { if(preview!=null) Dispose(preview);preview=null; }
        private static void Dispose(Object obj) { if(Application.isPlaying) Destroy(obj);else DestroyImmediate(obj); }
    }
}
