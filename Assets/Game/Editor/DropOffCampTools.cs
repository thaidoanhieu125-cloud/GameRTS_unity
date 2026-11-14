using System;
using System.IO;
using GameRTS.Data;
using GameRTS.Simulation;
using GameRTS.UI;
using GameRTS.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameRTS.Editor
{
    public static class DropOffCampTools
    {
        private const string HudPath = "Assets/Game/UI/Prefabs/EconomyHud.prefab";
        [MenuItem("GameRTS/Set up Drop-off Camps")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
            Scene scene = SceneManager.GetSceneByPath("Assets/Game/Scenes/GameScene.unity");
            if (!scene.isLoaded) throw new InvalidOperationException("Open GameScene first.");
            if (scene.isDirty) throw new InvalidOperationException("Save GameScene before setting up camps.");
            string[] names = { "LumberCamp", "Mill", "MiningCamp" };
            string[] ids = { "lumber_camp", "mill", "mining_camp" };
            string[] titles = { "Xưởng Gỗ", "Xưởng Thực", "Xưởng Vàng" };
            var data = new BuildingDataSO[3];
            for (int i = 0; i < 3; i++) data[i] = CreateCamp(names[i], ids[i], titles[i], i);
            ConfigureHud();
            AssetDatabase.ImportAsset(HudPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            RtsGameView game = null;
            foreach (var root in scene.GetRootGameObjects()) if (game == null) game = root.GetComponentInChildren<RtsGameView>(true);
            if (game == null) throw new InvalidOperationException("Missing RTS Session.");
            var settings = new SerializedObject(game); var catalog = settings.FindProperty("buildingCatalog");
            foreach (var camp in data)
            {
                bool found = false;
                for (int i = 0; i < catalog.arraySize; i++) if (catalog.GetArrayElementAtIndex(i).objectReferenceValue == camp) found = true;
                if (!found) { catalog.arraySize++; catalog.GetArrayElementAtIndex(catalog.arraySize - 1).objectReferenceValue = camp; }
            }
            settings.ApplyModifiedPropertiesWithoutUndo(); EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save camp catalog in GameScene.");
            AssetDatabase.SaveAssets();
            File.WriteAllText("Logs/Milestone2.camps-setup.txt", "3 camps: 2x2, 100 Wood, 200 work, 400 HP, one accepted resource each; four worker build buttons connected.");
        }

        private static BuildingDataSO CreateCamp(string name, string id, string title, int kind)
        {
            Sprite sprite = CampSprite("Assets/Game/Art/Buildings/" + name + ".png", kind);
            Sprite foundation = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Buildings/HouseFoundation.png");
            string prefabPath = "Assets/Game/View/Prefabs/Buildings/" + name + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
            {
                GameObject root = PrefabUtility.LoadPrefabContents("Assets/Game/View/Prefabs/Buildings/House.prefab");
                try
                {
                    root.name = name; root.transform.Find("Progress Label").GetComponent<TextMesh>().text = title + " 0%";
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            string path = "Assets/Game/Data/Buildings/" + name + "Data.asset";
            var data = AssetDatabase.LoadAssetAtPath<BuildingDataSO>(path);
            if (data != null) return data;
            data = ScriptableObject.CreateInstance<BuildingDataSO>(); var settings = new SerializedObject(data);
            settings.FindProperty("typeId").stringValue = id; settings.FindProperty("displayName").stringValue = title;
            settings.FindProperty("width").intValue = settings.FindProperty("height").intValue = 2;
            settings.FindProperty("woodCost").intValue = 100;
            settings.FindProperty("requiredWork").intValue = 200; settings.FindProperty("maxHitPoints").intValue = 400;
            settings.FindProperty("populationBonus").intValue = 0;
            settings.FindProperty("accepts").intValue = 1 << kind;
            settings.FindProperty("foundationSprite").objectReferenceValue = foundation;
            settings.FindProperty("completedSprite").objectReferenceValue = sprite;
            settings.FindProperty("viewPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            settings.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.CreateAsset(data, path); return data;
        }

        private static void ConfigureHud()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(HudPath);
            try
            {
                Transform bar = root.transform.Find("Top Bar"); var hud = root.GetComponent<ConstructionHudView>();
                if (hud == null) throw new InvalidOperationException("Set up the base Building System first.");
                string[] names = { "Build House", "Build Lumber Camp", "Build Mill", "Build Mining Camp" };
                string[] captions = { "XÂY NHÀ · B\n50 Gỗ", "XƯỞNG GỖ\n100 Gỗ", "XƯỞNG THỰC\n100 Gỗ", "XƯỞNG VÀNG\n100 Gỗ" };
                string[] fields = { "buildButton", "lumberCampButton", "millButton", "miningCampButton" };
                var settings = new SerializedObject(hud);
                for (int i = 0; i < 4; i++)
                {
                    Transform existing = bar.Find(names[i]); GameObject obj;
                    if (existing == null) obj = UnityEngine.Object.Instantiate(bar.Find("Build House").gameObject, bar, false);
                    else obj = existing.gameObject;
                    obj.name = names[i]; var rect = obj.GetComponent<RectTransform>();
                    rect.anchorMin = rect.anchorMax = Vector2.zero; rect.pivot = Vector2.up;
                    rect.anchoredPosition = new Vector2(16 + i * 168, -12); rect.sizeDelta = new Vector2(160, 58);
                    obj.GetComponentInChildren<Text>(true).text = captions[i]; obj.GetComponentInChildren<Text>(true).fontSize = 16;
                    obj.GetComponent<Image>().color = i == 0 ? new Color(.2f, .29f, .25f) : new Color(.17f, .24f, .27f);
                    settings.FindProperty(fields[i]).objectReferenceValue = obj.GetComponent<Button>();
                    obj.SetActive(false);
                }
                settings.ApplyModifiedPropertiesWithoutUndo();
                var test = bar.Find("Test Toggle").GetComponent<RectTransform>();
                test.anchorMin = new Vector2(.85f, .16f); test.anchorMax = new Vector2(.985f, .84f); test.offsetMin = test.offsetMax = Vector2.zero;
                root.transform.Find("Construction Notice").GetComponent<RectTransform>().anchoredPosition = new Vector2(16, -168);
                PrefabUtility.SaveAsPrefabAsset(root, HudPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static Sprite CampSprite(string path, int kind)
        {
            if (!File.Exists(path))
            {
                var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false); texture.SetPixels(new Color[128 * 128]);
                Color roof = kind == 0 ? new Color(.24f, .42f, .27f) : kind == 1 ? new Color(.66f, .24f, .2f) : new Color(.28f, .34f, .43f);
                for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
                {
                    Color c = Color.clear;
                    if (x >= 6 && x <= 121 && y >= 8 && y <= 117) c = new Color(.27f, .25f, .17f);
                    if (x >= 11 && x <= 116 && y >= 13 && y <= 112) c = new Color(.49f, .4f, .26f);
                    if (x >= 16 && x <= 111 && y >= 54 && y <= 106) c = y % 13 < 2 || x % 23 == 0 ? roof * .65f : roof;
                    if ((x >= 18 && x <= 25 || x >= 102 && x <= 109) && y >= 22 && y <= 53) c = new Color(.75f, .57f, .33f);
                    if (kind == 0)
                    {
                        if (x >= 28 && x <= 92 && (y >= 20 && y <= 29 || y >= 34 && y <= 43)) c = new Color(.56f, .29f, .13f);
                        if ((x - 31) * (x - 31) + (y - 25) * (y - 25) < 35 || (x - 31) * (x - 31) + (y - 39) * (y - 39) < 35) c = new Color(.88f, .68f, .37f);
                    }
                    else if (kind == 1)
                    {
                        if (y >= 19 && y <= 43 && (x >= 32 && x <= 47 || x >= 53 && x <= 68 || x >= 74 && x <= 89)) c = new Color(.91f, .73f, .34f);
                        if (y >= 41 && y <= 44 && x >= 32 && x <= 89) c = new Color(.58f, .4f, .17f);
                    }
                    else
                    {
                        if ((x - 44) * (x - 44) + (y - 31) * (y - 31) < 170 || (x - 79) * (x - 79) + (y - 29) * (y - 29) < 210) c = new Color(.57f, .61f, .64f);
                        if ((x >= 39 && x <= 46 && y >= 27 && y <= 34) || (x >= 74 && x <= 84 && y >= 24 && y <= 31)) c = new Color(1, .74f, .18f);
                    }
                    texture.SetPixel(x, y, c);
                }
                texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture); AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 64; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
