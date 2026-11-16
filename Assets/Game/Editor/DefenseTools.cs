using System;
using System.IO;
using System.Linq;
using GameRTS.Data;
using GameRTS.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace GameRTS.Editor
{
    public static class DefenseTools
    {
        private static readonly string[] Names={"Wall","Outpost","WatchTower","Landmine"};
        private static readonly string[] Ids={"wall","outpost","watch_tower","landmine"};
        private static readonly string[] Labels={"Tường thành","Trạm soi","Tháp canh","Bẫy bom"};
        [MenuItem("GameRTS/Set up Fog and Defenses")]
        public static void Setup()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before setup.");
            for(int i=0;i<SceneManager.sceneCount;i++) if(SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scenes before setup.");
            for(int i=0;i<4;i++) Make(i);
            AssetDatabase.SaveAssets();
            foreach(var entry in EditorBuildSettings.scenes.Where(s=>s.enabled))
            {
                var scene=EditorSceneManager.OpenScene(entry.path);
                var game=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<RtsGameView>(true)).FirstOrDefault(); if(game==null) continue;
                var so=new SerializedObject(game); var list=so.FindProperty("buildingCatalog");
                foreach(string name in Names)
                {
                    var data=AssetDatabase.LoadAssetAtPath<BuildingDataSO>("Assets/Game/Data/Buildings/"+name+"Data.asset");
                    bool found=false; for(int i=0;i<list.arraySize;i++) if(list.GetArrayElementAtIndex(i).objectReferenceValue==data) found=true;
                    if(!found) {list.arraySize++;list.GetArrayElementAtIndex(list.arraySize-1).objectReferenceValue=data;}
                }
                so.ApplyModifiedPropertiesWithoutUndo(); EditorSceneManager.MarkSceneDirty(scene); if(!EditorSceneManager.SaveScene(scene)) throw new IOException("Save failed: "+entry.path);
            }
            var hud=PrefabUtility.LoadPrefabContents("Assets/Game/UI/Prefabs/EconomyHud.prefab");
            try {hud.transform.Find("Construction Notice").GetComponent<RectTransform>().anchoredPosition=new Vector2(16,-292); PrefabUtility.SaveAsPrefabAsset(hud,"Assets/Game/UI/Prefabs/EconomyHud.prefab");}
            finally {PrefabUtility.UnloadPrefabContents(hud);}
            EditorSceneManager.OpenScene("Assets/Game/Scenes/GameScene.unity"); AssetDatabase.SaveAssets();
            File.WriteAllText("Logs/Defense.setup.txt","Wall, outpost, tower and mine assets/catalogs ready in all gameplay scenes.");
        }
        private static void Make(int kind)
        {
            string png="Assets/Game/Art/Buildings/"+Names[kind]+".png";
            if(!File.Exists(png))
            {
                var t=new Texture2D(64,64,TextureFormat.RGBA32,false); t.SetPixels(new Color[4096]);
                for(int y=6;y<58;y++) for(int x=5;x<59;x++)
                {
                    bool draw=kind==0 ? y<42 || x%16<10 : kind==1 ? (x>28 && x<36) || y>36 && x>16 && x<49 : kind==2 ? y<43 && x>14 && x<51 || y>40 && x>8 && x<57 : (x-32)*(x-32)+(y-28)*(y-28)<190 || x>31 && x<36 && y<51;
                    if(draw) t.SetPixel(x,y,kind==0?new Color(.45f,.48f,.5f):kind==1?new Color(.6f,.4f,.22f):kind==2?new Color(.55f,.37f,.23f):new Color(.22f,.23f,.2f));
                    if(kind==1 && x>35 && x<51 && y>47) t.SetPixel(x,y,Color.red);
                    if(kind==2 && y>44 && x>8 && x<57) t.SetPixel(x,y,new Color(.6f,.17f,.12f));
                    if(kind==3 && (x-34)*(x-34)+(y-51)*(y-51)<10) t.SetPixel(x,y,Color.yellow);
                }
                File.WriteAllBytes(png,t.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(t); AssetDatabase.ImportAsset(png);
                var importer=(TextureImporter)AssetImporter.GetAtPath(png); importer.textureType=TextureImporterType.Sprite; importer.spritePixelsPerUnit=64; importer.filterMode=FilterMode.Point; importer.alphaIsTransparency=true; importer.SaveAndReimport();
            }
            string prefab="Assets/Game/View/Prefabs/Buildings/"+Names[kind]+".prefab";
            if(!File.Exists(prefab))
            {
                var root=PrefabUtility.LoadPrefabContents("Assets/Game/View/Prefabs/Buildings/House.prefab");
                try {root.name=Names[kind]; PrefabUtility.SaveAsPrefabAsset(root,prefab);} finally {PrefabUtility.UnloadPrefabContents(root);}
            }
            string path="Assets/Game/Data/Buildings/"+Names[kind]+"Data.asset";
            if(File.Exists(path)) return;
            var d=ScriptableObject.CreateInstance<BuildingDataSO>(); var so=new SerializedObject(d);
            so.FindProperty("typeId").stringValue=Ids[kind]; so.FindProperty("displayName").stringValue=Labels[kind];
            so.FindProperty("width").intValue=kind==2?2:1; so.FindProperty("height").intValue=kind==2?2:1;
            so.FindProperty("woodCost").intValue=new[]{5,100,125,50}[kind]; so.FindProperty("goldCost").intValue=kind==3?30:0;
            so.FindProperty("populationBonus").intValue=0; so.FindProperty("requiredWork").intValue=new[]{40,100,200,60}[kind];
            so.FindProperty("maxHitPoints").intValue=new[]{400,300,600,1}[kind];
            so.FindProperty("sightRadius").intValue=kind==1?15:kind==3?0:-1;
            so.FindProperty("towerDamage").intValue=kind==2?20:0; so.FindProperty("isMine").boolValue=kind==3;
            so.FindProperty("foundationSprite").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Buildings/HouseFoundation.png");
            so.FindProperty("completedSprite").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>(png);
            so.FindProperty("viewPrefab").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(prefab);
            so.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.CreateAsset(d,path);
        }
    }
}
