using System;
using System.IO;
using GameRTS.Data;
using GameRTS.Input;
using GameRTS.UI;
using GameRTS.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameRTS.Editor
{
    public static class BuildingTools
    {
        private const string HudPath = "Assets/Game/UI/Prefabs/EconomyHud.prefab";
        [MenuItem("GameRTS/Set up Building System")]
        public static void Setup()
        {
            const string path = "Assets/Game/Scenes/GameScene.unity";
            Scene scene = SceneManager.GetSceneByPath(path); if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(path);
            if (scene.isDirty) throw new InvalidOperationException("Save GameScene before setting up buildings.");
            Sprite foundation = MakeSprite("Assets/Game/Art/Buildings/HouseFoundation.png", 0);
            Sprite completed = MakeSprite("Assets/Game/Art/Buildings/House.png", 1);
            Sprite hammer = MakeSprite("Assets/Game/Art/Units/Hammer.png", 2);
            GameObject prefab = MakeHousePrefab(foundation);
            BuildingDataSO data = MakeHouseData(foundation, completed, prefab);
            AddHammer(hammer); AddHud();
            AssetDatabase.ImportAsset(HudPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            RtsGameView game = null; ConstructionHudView hud = null; Camera camera = null; GameObject root = null;
            foreach (GameObject obj in scene.GetRootGameObjects())
            {
                if (game == null) game = obj.GetComponentInChildren<RtsGameView>(true);
                if (hud == null) hud = obj.GetComponentInChildren<ConstructionHudView>(true);
                if (camera == null) camera = obj.GetComponentInChildren<Camera>(true);
                if (obj.name == "Constructed Buildings") root = obj;
            }
            if (game == null || hud == null || camera == null) throw new InvalidOperationException("Scene needs RTS Session, HUD and camera.");
            if (root == null) { root = new GameObject("Constructed Buildings"); SceneManager.MoveGameObjectToScene(root, scene); }
            Assign(game, "constructedBuildingRoot", root.transform);
            var gameSettings = new SerializedObject(game); var catalog = gameSettings.FindProperty("buildingCatalog"); bool found = false;
            for (int i = 0; i < catalog.arraySize; i++) if (catalog.GetArrayElementAtIndex(i).objectReferenceValue == data) found = true;
            if (!found) { catalog.arraySize++; catalog.GetArrayElementAtIndex(catalog.arraySize - 1).objectReferenceValue = data; }
            gameSettings.ApplyModifiedPropertiesWithoutUndo();
            BuildingPlacementView placement = game.GetComponent<BuildingPlacementView>();
            if (placement == null) placement = game.gameObject.AddComponent<BuildingPlacementView>();
            Assign(placement, "game", game, "viewCamera", camera, "cellSprite", Ground(), "spriteMaterial", Material());
            BuildingPlacementInput input = game.GetComponent<BuildingPlacementInput>();
            if (input == null) input = game.gameObject.AddComponent<BuildingPlacementInput>();
            Assign(input, "game", game, "selection", game.GetComponent<UnitSelectionView>(), "placement", placement, "hud", hud);
            Assign(hud, "game", game, "placement", placement);
            EditorSceneManager.MarkSceneDirty(scene); if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save GameScene.");
            AssetDatabase.SaveAssets(); Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/Milestone2.buildings-setup.txt", "House data/prefab, foundation/finished sprites, hammer, ghost, build button and placement input connected.");
        }
        private static Sprite Ground() => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Terrain/GroundTile.png");
        private static Material Material() => AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/SampleSoldier.prefab").GetComponent<SpriteRenderer>().sharedMaterial;
        private static GameObject MakeHousePrefab(Sprite foundation)
        {
            const string path = "Assets/Game/View/Prefabs/Buildings/House.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path); if (prefab != null) return prefab;
            var root = new GameObject("House", typeof(BuildingView));
            try
            {
                SpriteRenderer body = SpriteChild(root.transform, "Body", foundation, Color.white, 0);
                SpriteRenderer background = SpriteChild(root.transform, "Progress Background", Ground(), new Color(.06f, .09f, .1f), 13);
                SpriteRenderer fill = SpriteChild(root.transform, "Progress Fill", Ground(), new Color(.94f, .74f, .3f), 14);
                var labelObject = new GameObject("Progress Label", typeof(TextMesh)); labelObject.transform.SetParent(root.transform, false);
                TextMesh label = labelObject.GetComponent<TextMesh>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.fontSize = 48; label.characterSize = .07f; label.anchor = TextAnchor.MiddleCenter; label.text = "Nhà 0%";
                label.GetComponent<MeshRenderer>().sharedMaterial = label.font.material; label.GetComponent<MeshRenderer>().sortingOrder = 15;
                Assign(root.GetComponent<BuildingView>(), "body", body, "progressBackground", background.transform, "progressFill", fill.transform, "progressLabel", label);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        private static BuildingDataSO MakeHouseData(Sprite foundation, Sprite complete, GameObject prefab)
        {
            const string path = "Assets/Game/Data/Buildings/HouseData.asset";
            BuildingDataSO data = AssetDatabase.LoadAssetAtPath<BuildingDataSO>(path); if (data != null) return data;
            data = ScriptableObject.CreateInstance<BuildingDataSO>();
            Assign(data, "foundationSprite", foundation, "completedSprite", complete, "viewPrefab", prefab);
            AssetDatabase.CreateAsset(data, path); return data;
        }
        private static SpriteRenderer SpriteChild(Transform parent, string name, Sprite sprite, Color color, int order)
        {
            var obj = new GameObject(name, typeof(SpriteRenderer)); obj.transform.SetParent(parent, false);
            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.color = color;
            renderer.sharedMaterial = Material(); renderer.sortingOrder = order; return renderer;
        }
        private static void AddHammer(Sprite sprite)
        {
            const string path = "Assets/Game/View/Prefabs/Villager.prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (root.transform.Find("Hammer") != null) return;
                SpriteRenderer hammer = SpriteChild(root.transform, "Hammer", sprite, Color.white, 14);
                hammer.transform.localPosition = new Vector3(.35f, .18f, -.1f); hammer.transform.localScale = Vector3.one * .4f;
                Assign(root.GetComponent<UnitView>(), "hammer", hammer.transform); hammer.gameObject.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void AddHud()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(HudPath);
            try
            {
                if (root.GetComponent<ConstructionHudView>() != null) return;
                ConstructionHudView hud = root.AddComponent<ConstructionHudView>(); Transform bar = root.transform.Find("Top Bar");
                RectTransform test = bar.Find("Test Toggle").GetComponent<RectTransform>();
                test.anchorMin = new Vector2(.85f, .55f); test.anchorMax = new Vector2(.985f, .92f); test.offsetMin = test.offsetMax = Vector2.zero;
                var buttonObject = new GameObject("Build House", typeof(RectTransform), typeof(Image), typeof(Button)); buttonObject.transform.SetParent(bar, false);
                RectTransform rect = buttonObject.GetComponent<RectTransform>(); rect.anchorMin = new Vector2(.85f, .08f); rect.anchorMax = new Vector2(.985f, .46f);
                rect.offsetMin = rect.offsetMax = Vector2.zero; buttonObject.GetComponent<Image>().color = new Color(.2f, .29f, .25f);
                Button button = buttonObject.GetComponent<Button>(); button.targetGraphic = buttonObject.GetComponent<Image>(); button.navigation = new Navigation { mode = Navigation.Mode.None };
                Text caption = UiText(buttonObject.transform, "Caption", 17); caption.text = "XÂY NHÀ · B"; caption.alignment = TextAnchor.MiddleCenter;
                var panelObject = new GameObject("Construction Notice", typeof(RectTransform), typeof(Image)); panelObject.transform.SetParent(root.transform, false);
                RectTransform panel = panelObject.GetComponent<RectTransform>(); panel.anchorMin = panel.anchorMax = Vector2.up; panel.pivot = Vector2.up;
                panel.sizeDelta = new Vector2(690, 70); panel.anchoredPosition = new Vector2(16, -100);
                panelObject.GetComponent<Image>().color = new Color(.065f, .105f, .12f, .96f);
                Text notice = UiText(panel, "Text", 18); notice.rectTransform.offsetMin = new Vector2(12, 4); notice.rectTransform.offsetMax = new Vector2(-8, -4);
                Assign(hud, "buildButton", button, "noticePanel", panel, "notice", notice); panelObject.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, HudPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static Text UiText(Transform parent, string name, int size)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Text)); obj.transform.SetParent(parent, false);
            Text text = obj.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size;
            text.color = Color.white; text.alignment = TextAnchor.MiddleLeft; text.raycastTarget = false;
            text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one; text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
            return text;
        }
        private static void Assign(UnityEngine.Object target, params object[] pairs)
        {
            var serialized = new SerializedObject(target);
            for (int i = 0; i < pairs.Length; i += 2) serialized.FindProperty((string)pairs[i]).objectReferenceValue = (UnityEngine.Object)pairs[i + 1];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static Sprite MakeSprite(string path, int kind)
        {
            if (!File.Exists(path))
            {
                int size = kind == 2 ? 16 : 128; var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.SetPixels(new Color[size * size]);
                for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
                {
                    Color color = Color.clear;
                    if (kind == 2)
                    {
                        if (x >= 6 && x <= 8 && y >= 1 && y <= 12) color = new Color(.55f, .34f, .16f);
                        if (x >= 2 && x <= 13 && y >= 10 && y <= 14) color = new Color(.72f, .78f, .78f);
                    }
                    else
                    {
                        if (x >= 7 && x <= 120 && y >= 9 && y <= 116) color = new Color(.23f, .2f, .14f);
                        if (x >= 13 && x <= 114 && y >= 15 && y <= 110) color = new Color(.51f, .4f, .25f);
                        if (kind == 0)
                        {
                            if (x >= 18 && x <= 109 && y >= 22 && y <= 101) color = new Color(.34f, .31f, .25f);
                            if ((x >= 17 && x <= 23 || x >= 104 && x <= 110) && y >= 17 && y <= 108
                                || (y >= 20 && y <= 26 || y >= 99 && y <= 105) && x >= 17 && x <= 110) color = new Color(.77f, .56f, .28f);
                            if (y >= 48 && y <= 53 && x >= 23 && x <= 103) color = new Color(.64f, .49f, .29f);
                        }
                        else
                        {
                            if (x >= 17 && x <= 110 && y >= 20 && y <= 67) color = new Color(.83f, .71f, .46f);
                            if (x >= 52 && x <= 75 && y >= 17 && y <= 51) color = new Color(.28f, .2f, .13f);
                            if (x >= 13 && x <= 114 && y >= 62 && y <= 110) color = y % 12 < 2 || x % 19 == 0
                                ? new Color(.14f, .26f, .32f) : new Color(.27f, .45f, .52f);
                            if (y >= 84 && y <= 88 && x >= 13 && x <= 114) color = new Color(.47f, .63f, .6f);
                            if ((x >= 25 && x <= 39 || x >= 89 && x <= 102) && y >= 31 && y <= 48) color = new Color(.24f, .41f, .4f);
                        }
                    }
                    texture.SetPixel(x, y, color);
                }
                texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture); AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = kind == 2 ? 16 : 64; importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
