using System;
using System.IO;
using System.Linq;
using GameRTS.Data;
using GameRTS.Simulation;
using GameRTS.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

namespace GameRTS.Editor
{
    public static class LandscapeMapTools
    {
        private const string Art="Assets/Game/Art/Landscape/";
        [MenuItem("GameRTS/Set up expanded landscape")]
        public static void Setup()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Stop Play and save your current scene before landscape setup.");
            Directory.CreateDirectory("Logs/SceneBackups");
            foreach(string name in new[]{"GameScene","AiScene"})
                File.Copy("Assets/Game/Scenes/"+name+".unity","Logs/SceneBackups/"+name+"-before-landscape-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+".unity",true);

            Sprite oak=ImportTree("Oak.png",3.05f), pine=ImportTree("Pine.png",3.5f);
            Sprite[] rocks=ImportRocks();string[] selectedRockNames=rocks.Select(r=>r.name).ToArray();
            Texture2D grass=ImportSurface("Grass.png"),water=ImportSurface("Water.png");
            Material sprites=AssetDatabase.LoadAssetAtPath<Material>(Art+"LandscapeSprites.mat");
            if(sprites==null)
            { sprites=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));AssetDatabase.CreateAsset(sprites,Art+"LandscapeSprites.mat"); }
            Material surface=AssetDatabase.LoadAssetAtPath<Material>(Art+"LandscapeGround.mat");
            if(surface==null) { surface=new Material(Shader.Find("GameRTS/Landscape Ground"));AssetDatabase.CreateAsset(surface,Art+"LandscapeGround.mat"); }
            surface.SetTexture("_GrassTex",grass);surface.SetTexture("_WaterTex",water);EditorUtility.SetDirty(surface);
            Mesh mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"LandscapePlane.asset");
            if(mesh==null) { mesh=new Mesh();AssetDatabase.CreateAsset(mesh,Art+"LandscapePlane.asset"); }
            mesh.name="128 by 192 north-south ground";
            float minX=MeadowMapLayout.MinimumX,minY=MeadowMapLayout.MinimumY;
            float maxX=minX+MeadowMapLayout.Width,maxY=minY+MeadowMapLayout.Height;
            mesh.vertices=new[]{new Vector3(minX,minY,0),new Vector3(maxX,minY,0),new Vector3(maxX,maxY,0),new Vector3(minX,maxY,0)};
            mesh.triangles=new[]{0,2,1,0,3,2};mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
            var tree=AssetDatabase.LoadAssetAtPath<ResourceDataSO>("Assets/Game/Data/Resources/TreeData.asset");
            Set(tree,"sprite",oak);
            string pinePath="Assets/Game/Data/Resources/PineData.asset";
            var pineData=AssetDatabase.LoadAssetAtPath<ResourceDataSO>(pinePath);
            if(pineData==null)
            { pineData=ScriptableObject.CreateInstance<ResourceDataSO>();AssetDatabase.CreateAsset(pineData,pinePath); }
            var pd=new SerializedObject(pineData);
            pd.FindProperty("resourceId").stringValue="tree_pine";pd.FindProperty("displayName").stringValue="Cây thông";
            pd.FindProperty("resourceType").enumValueIndex=(int)ResourceType.Wood;pd.FindProperty("initialAmount").intValue=100;
            pd.FindProperty("sprite").objectReferenceValue=pine;pd.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pineData);EditorUtility.SetDirty(tree);AssetDatabase.SaveAssets();
            foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Game/View/Prefabs"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                var root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if(root.GetComponent<UnitView>()!=null || root.GetComponent<ResourceNodeView>()!=null || root.GetComponent<DropOffView>()!=null || root.GetComponent<BuildingView>()!=null || root.GetComponent<AnimalView>()!=null)
                    {
                        if(root.GetComponent<WorldDepthView>()==null) root.AddComponent<WorldDepthView>();
                        if(root.GetComponent<ResourceNodeView>()!=null)
                        {
                            var r=root.GetComponent<SpriteRenderer>();r.sprite=root.GetComponent<ResourceNodeView>().Data.Sprite;r.sharedMaterial=sprites;
                        }
                        PrefabUtility.SaveAsPrefabAsset(root,path);
                    }
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            foreach(string name in new[]{"GameScene","AiScene"})
            {
                Scene scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/"+name+".unity");
                // Opening another scene may unload editor-stack asset references. Reload persisted SOs.
                tree=AssetDatabase.LoadAssetAtPath<ResourceDataSO>("Assets/Game/Data/Resources/TreeData.asset");
                pineData=AssetDatabase.LoadAssetAtPath<ResourceDataSO>(pinePath);
                surface=AssetDatabase.LoadAssetAtPath<Material>(Art+"LandscapeGround.mat");
                mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"LandscapePlane.asset");
                sprites=AssetDatabase.LoadAssetAtPath<Material>(Art+"LandscapeSprites.mat");
                rocks=AssetDatabase.LoadAllAssetsAtPath(Art+"Rocks.png").OfType<Sprite>().Where(r=>selectedRockNames.Contains(r.name)).OrderBy(r=>Array.IndexOf(selectedRockNames,r.name)).ToArray();
                var map=Find<RtsMapView>(scene);Tilemap ground=map.Ground;
                ground.ClearAllTiles();
                Tile tile=AssetDatabase.LoadAssetAtPath<Tile>("Assets/Game/Art/Terrain/GrassLight.asset");
                if(tile==null) throw new InvalidOperationException("Missing base ground tile.");
                // Bounds markers keep the saved scene compact; the continuous plane fills the map.
                ground.SetTile(new Vector3Int(MeadowMapLayout.MinimumX,MeadowMapLayout.MinimumY,0),tile);
                ground.SetTile(new Vector3Int(MeadowMapLayout.MinimumX+MeadowMapLayout.Width-1,MeadowMapLayout.MinimumY+MeadowMapLayout.Height-1,0),tile);ground.CompressBounds();
                ground.GetComponent<TilemapRenderer>().enabled=false;
                if(map.Obstacles!=null) { map.Obstacles.ClearAllTiles();map.Obstacles.GetComponent<TilemapRenderer>().enabled=false; }
                var ms=new SerializedObject(map);ms.FindProperty("useMeadowLandscape").boolValue=true;ms.ApplyModifiedPropertiesWithoutUndo();
                var view=map.GetComponent<LandscapeSurfaceView>();if(view==null)view=map.gameObject.AddComponent<LandscapeSurfaceView>();
                var vs=new SerializedObject(view);vs.FindProperty("surfaceMaterial").objectReferenceValue=surface;
                vs.FindProperty("surfaceMesh").objectReferenceValue=mesh;vs.FindProperty("spriteMaterial").objectReferenceValue=sprites;
                var rs=vs.FindProperty("rockSprites");rs.arraySize=rocks.Length;for(int i=0;i<rocks.Length;i++)rs.GetArrayElementAtIndex(i).objectReferenceValue=rocks[i];
                vs.ApplyModifiedPropertiesWithoutUndo();view.Rebuild();
                var game=Find<RtsGameView>(scene);
                Transform resourceRoot=(Transform)new SerializedObject(game).FindProperty("resourceRoot").objectReferenceValue;
                var simulationMap=MeadowMapLayout.CreateMap();
                Transform oldForest=resourceRoot.Find("Landscape Forest");
                if(oldForest!=null)UnityEngine.Object.DestroyImmediate(oldForest.gameObject);
                Transform oldDeposits=resourceRoot.Find("Central Deposits");
                if(oldDeposits!=null)UnityEngine.Object.DestroyImmediate(oldDeposits.gameObject);
                if(name=="AiScene") ConfigureNorthSouthMatch(scene,game,resourceRoot,simulationMap);
                var forest=new GameObject("Landscape Forest");forest.transform.SetParent(resourceRoot,false);
                GameObject treePrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/Resources/Tree.prefab");
                foreach(var cell in MeadowMapLayout.Trees())
                {
                    var obj=(GameObject)PrefabUtility.InstantiatePrefab(treePrefab,forest.transform);
                    obj.name="Forest tree "+cell.X+","+cell.Y;obj.transform.position=new Vector3(cell.X+.5f,cell.Y+.5f,0);
                    ResourceDataSO definition=(cell.X+cell.Y)%6==0?pineData:tree;
                    if(definition==null || definition.Sprite==null)throw new InvalidOperationException("Persisted forest data is missing.");
                    Set(obj.GetComponent<ResourceNodeView>(),"data",definition);HideStock(obj);
                    obj.GetComponent<SpriteRenderer>().sprite=definition.Sprite;
                }
                var deposits=new GameObject("Central Deposits");deposits.transform.SetParent(resourceRoot,false);
                foreach(var node in MeadowMapLayout.CentralDeposits())
                    PlaceResource(deposits.transform,node.Type,new Vector3(node.Position.X+.5f,node.Position.Y+.5f,0),true);
                var camera=Find<Camera>(scene);camera.transform.position=new Vector3(0,0,-10);camera.orthographicSize=10;
                EditorSceneManager.MarkSceneDirty(scene);
                if(!EditorSceneManager.SaveScene(scene))throw new IOException("Failed saving expanded "+name);
            }
            AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Game/Scenes/GameScene.unity");
            File.WriteAllText("Logs/Landscape.setup.txt","128x192 north-south GameScene/AiScene, two forest belts, four rock clusters, six lakes, contested central deposits. Forest trees="+MeadowMapLayout.Trees().Count()+"; rocks="+MeadowMapLayout.Rocks.Count);
        }

        [MenuItem("GameRTS/Capture landscape preview")]
        public static void CapturePreview()
        {
            var scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/AiScene.unity");
            var camera=Find<Camera>(scene);Vector3 oldPosition=camera.transform.position;float oldSize=camera.orthographicSize;
            RenderTexture target=null;Texture2D pixels=null;
            var previous=RenderTexture.active;
            try
            {
                foreach(var shot in new[]{new Vector3(0,0,13.5f),new Vector3(0,34,18),new Vector3(0,64,32),new Vector3(0,64,104)})
                {
                    int width=shot.z>100?1000:1600,height=shot.z>100?1600:1000;
                    target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);target.Create();
                    pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
                    camera.transform.position=new Vector3(shot.x,shot.y,-10);camera.orthographicSize=shot.z;
                    var request=new RenderPipeline.StandardRequest { destination=target };
                    RenderPipeline.SubmitRenderRequest(camera,request);
                    RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();
                    File.WriteAllBytes("Logs/Landscape.preview-"+shot.x+"-"+shot.y+(shot.z>100?"-overview":"")+".png",pixels.EncodeToPNG());
                    target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);target=null;pixels=null;
                }
            }
            finally
            { camera.transform.position=oldPosition;camera.orthographicSize=oldSize;RenderTexture.active=previous;if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}if(pixels!=null)UnityEngine.Object.DestroyImmediate(pixels); }
            EditorSceneManager.OpenScene("Assets/Game/Scenes/GameScene.unity");
        }

        private static void HideStock(GameObject obj)
        {
            Set(obj.GetComponent<ResourceNodeView>(),"stockLabel",null);
            var label=obj.transform.Find("Stock");if(label!=null)label.gameObject.SetActive(false);
        }
        private static void PlaceResource(Transform parent,ResourceType type,Vector3 position,bool hideStock)
        {
            string prefab=type==ResourceType.Wood?"Tree":type==ResourceType.Food?"BerryBush":"GoldMine";
            var obj=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/Resources/"+prefab+".prefab"),parent);
            obj.transform.position=position;obj.name=prefab+" "+position.x+","+position.y;
            if(hideStock)HideStock(obj);
        }
        private static void ConfigureNorthSouthMatch(Scene scene,RtsGameView game,Transform resourceRoot,GridMap map)
        {
            // Replace only this scene's authored starting nodes; generated forest is rebuilt separately.
            while(resourceRoot.childCount>0)UnityEngine.Object.DestroyImmediate(resourceRoot.GetChild(0).gameObject);
            foreach(var node in AiScenario.Resources(map))
                PlaceResource(resourceRoot,node.Type,new Vector3(map.CenterX(node.Cell)/1000f,map.CenterY(node.Cell)/1000f,0),false);
            var gs=new SerializedObject(game);
            var depots=(Transform)gs.FindProperty("dropOffRoot").objectReferenceValue;
            foreach(var depot in depots.GetComponentsInChildren<DropOffView>(true))
            {
                int owner=new SerializedObject(depot).FindProperty("ownerId").intValue;
                int c=AiScenario.TownCell(map,owner);
                depot.transform.position=new Vector3(map.CenterX(c)/1000f+.5f,map.CenterY(c)/1000f+.5f,0);
            }
            var spawner=new SerializedObject(Find<UnitPreviewSpawner>(scene));
            var positions=spawner.FindProperty("spawnPositions");var starts=AiScenario.VillagerCells(map,1);
            positions.arraySize=starts.Length;
            for(int i=0;i<starts.Length;i++)positions.GetArrayElementAtIndex(i).vector2Value=new Vector2(map.CenterX(starts[i])/1000f,map.CenterY(starts[i])/1000f);
            spawner.ApplyModifiedPropertiesWithoutUndo();
            var fauna=new SerializedObject(Find<FaunaSpawner>(scene));var animals=fauna.FindProperty("positions");
            Vector2[] home={new Vector2(-6.5f,-9.5f),new Vector2(-4.5f,-10.5f),new Vector2(-11.5f,-13.5f),new Vector2(13.5f,-7.5f)};
            for(int i=0;i<animals.arraySize;i++)
            { var p=home[i%4];animals.GetArrayElementAtIndex(i).vector2Value=i<4?p:new Vector2(-p.x,128-p.y); }
            fauna.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite ImportTree(string name,float height)
        {
            string path=Art+name;var texture=new Texture2D(2,2);texture.LoadImage(File.ReadAllBytes(path));
            int minX=texture.width,minY=texture.height,maxX=0,maxY=0;
            Color32[] pixels=texture.GetPixels32();
            for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)
            { if(pixels[y*texture.width+x].a<90)continue;minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y); }
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.filterMode=FilterMode.Bilinear;importer.mipmapEnabled=false;
            importer.maxTextureSize=2048;importer.spritePixelsPerUnit=(maxY-minY+1)/height;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;
            settings.spritePivot=new Vector2((minX+maxX)*.5f/texture.width,(minY+8f)/texture.height);importer.SetTextureSettings(settings);
            importer.SaveAndReimport();UnityEngine.Object.DestroyImmediate(texture);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static Sprite[] ImportRocks()
        {
            string path=Art+"Rocks.png";var texture=new Texture2D(2,2);texture.LoadImage(File.ReadAllBytes(path));
            int width=texture.width,height=texture.height;UnityEngine.Object.DestroyImmediate(texture);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;importer.spritePixelsPerUnit=width/6f/1.35f;
            importer.SaveAndReimport();
            var factory=new SpriteDataProviderFactories();factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var prior=provider.GetSpriteRects().ToDictionary(s=>s.name,s=>s.spriteID);
            var sheet=new SpriteRect[30];for(int row=0;row<5;row++)for(int col=0;col<6;col++)
            {
                string n="Rock_"+row+"_"+col;
                sheet[row*6+col]=new SpriteRect {name=n,spriteID=prior.TryGetValue(n,out var id)?id:GUID.Generate(),rect=new Rect(col*width/6f,height-(row+1)*height/5f,width/6f,height/5f),alignment=SpriteAlignment.Custom,pivot=new Vector2(.5f,.12f)};
            }
            provider.SetSpriteRects(sheet);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(sheet.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
            provider.Apply();importer.SaveAndReimport();
            var all=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            // Natural grey and moss rocks only; keep crystal, lava and ice variants out of grassland.
            string[] wanted={"Rock_0_0","Rock_0_1","Rock_0_2","Rock_0_3","Rock_0_4","Rock_0_5","Rock_1_2","Rock_1_3","Rock_1_4","Rock_2_1","Rock_2_2","Rock_2_3","Rock_2_4","Rock_4_0","Rock_4_1","Rock_4_2"};
            return wanted.Select(n=>all.Single(s=>s.name==n)).ToArray();
        }
        private static Texture2D ImportSurface(string name)
        {
            var imp=(TextureImporter)AssetImporter.GetAtPath(Art+name);imp.textureType=TextureImporterType.Default;
            imp.wrapMode=TextureWrapMode.Repeat;imp.filterMode=FilterMode.Trilinear;imp.mipmapEnabled=true;imp.maxTextureSize=2048;
            imp.textureCompression=TextureImporterCompression.CompressedHQ;imp.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(Art+name);
        }
        private static void Set(UnityEngine.Object obj,string field,UnityEngine.Object value)
        { var s=new SerializedObject(obj);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo(); }
        private static T Find<T>(Scene scene) where T:Component
        { foreach(var root in scene.GetRootGameObjects()) { var c=root.GetComponentInChildren<T>(true);if(c!=null)return c; } throw new InvalidOperationException("Missing "+typeof(T).Name); }
    }
}
