using System;
using System.IO;
using GameRTS.Input;
using GameRTS.UI;
using GameRTS.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameRTS.Editor
{
    public static class MilestoneTwoTools
    {
        private const string PrefabPath = "Assets/Game/UI/Prefabs/EconomyHud.prefab";
        private static readonly Color Ink = new Color(.065f, .105f, .12f, .97f);
        private static readonly Color Card = new Color(.12f, .18f, .19f);
        private static readonly Color Gold = new Color(.92f, .74f, .39f);

        [MenuItem("GameRTS/Set up Resource HUD")]
        public static void SetupEconomy()
        {
            const string path = "Assets/Game/Scenes/GameScene.unity";
            Scene scene = SceneManager.GetSceneByPath(path);
            if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(path);
            if (scene.isDirty) throw new InvalidOperationException("Save GameScene before setting up the HUD.");
            EnsureFolder("Assets/Game/Art/UI"); EnsureFolder("Assets/Game/UI/Prefabs");
            var sprites = new Sprite[4];
            string[] names = { "FoodIcon", "WoodIcon", "GoldIcon", "PopulationIcon" };
            for (int i = 0; i < 4; i++) sprites[i] = CreateIcon(names[i], i);
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null) CreatePrefab(sprites);
            RtsGameView game = null; EconomyHudView hud = null; EventSystem events = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (game == null) game = root.GetComponentInChildren<RtsGameView>(true);
                if (hud == null) hud = root.GetComponentInChildren<EconomyHudView>(true);
                if (events == null) events = root.GetComponentInChildren<EventSystem>(true);
            }
            if (game == null) throw new InvalidOperationException("GameScene needs RTS Session.");
            if (hud == null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene);
                hud = instance.GetComponent<EconomyHudView>();
            }
            Assign(hud, "game", game);
            EconomyTestInput input = game.GetComponent<EconomyTestInput>();
            if (input == null) input = game.gameObject.AddComponent<EconomyTestInput>();
            Assign(input, "game", game, "hud", hud);
            if (events == null)
            {
                var system = new GameObject("UI EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                SceneManager.MoveGameObjectToScene(system, scene);
            }
            else if (events.GetComponent<InputSystemUIInputModule>() == null)
                events.gameObject.AddComponent<InputSystemUIInputModule>();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save GameScene.");
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/Milestone2.setup.txt", "Resource HUD prefab, 4 icons, test input and UI EventSystem connected.");
        }

        private static void CreatePrefab(Sprite[] icons)
        {
            var root = new GameObject("Economy HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            try
            {
                root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                root.GetComponent<Canvas>().sortingOrder = 100;
                CanvasScaler scaler = root.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = .5f;
                EconomyHudView hud = root.AddComponent<EconomyHudView>();
                RectTransform bar = Box("Top Bar", root.transform, Ink, new Vector2(0, 1), Vector2.one, new Vector2(0, -88), Vector2.zero);
                Box("Gold Rule", bar, Gold, Vector2.zero, Vector2.right, Vector2.zero, new Vector2(0, 2));
                string[] titles = { "THỰC", "GỖ", "VÀNG", "DÂN SỐ" };
                var numbers = new Text[4];
                for (int i = 0; i < 4; i++)
                {
                    RectTransform card = Box(titles[i], bar, Card, new Vector2(.014f + i * .205f, 0), new Vector2(.20f + i * .205f, 1), new Vector2(0, 13), new Vector2(0, -13));
                    RectTransform icon = Box("Icon", card, Color.white, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(12, -22), new Vector2(56, 22));
                    icon.GetComponent<Image>().sprite = icons[i]; icon.GetComponent<Image>().preserveAspect = true;
                    Label("Label", card, titles[i], 15, Gold, new Vector2(0, 1), Vector2.one, new Vector2(68, -28), new Vector2(-4, -5));
                    numbers[i] = Label("Amount", card, i == 3 ? "0/10" : "0", 27, Color.white, Vector2.zero, Vector2.one, new Vector2(68, 1), new Vector2(-4, -25));
                }
                Button toggle = MakeButton("Test Toggle", bar, "THỬ VÍ · F9", new Vector2(.85f, .24f), new Vector2(.985f, .76f), Vector2.zero, Vector2.zero);
                RectTransform panel = Box("Wallet Test Panel", root.transform, Ink, new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -252), new Vector2(658, -100));
                Label("Title", panel, "THỬ VÍ TÀI NGUYÊN  /  F9 để đóng", 17, Gold, new Vector2(0, 1), Vector2.one, new Vector2(16, -36), new Vector2(-8, -7));
                Button grant = MakeButton("Grant", panel, "Nạp +50 / loại", new Vector2(0, 1), new Vector2(0, 1), new Vector2(16, -91), new Vector2(209, -46));
                Button spend = MakeButton("Spend", panel, "Chi -50 / loại", new Vector2(0, 1), new Vector2(0, 1), new Vector2(223, -91), new Vector2(416, -46));
                Button capacity = MakeButton("Capacity", panel, "Sức chứa +5", new Vector2(0, 1), new Vector2(0, 1), new Vector2(430, -91), new Vector2(623, -46));
                Text feedback = Label("Feedback", panel, "Lệnh thử chạy qua Simulation theo tick; không tự sinh quân.", 16, new Color(.7f, .8f, .8f), Vector2.zero, Vector2.one, new Vector2(16, 10), new Vector2(-12, -103));
                RectTransform helpBox = Box("Controls", root.transform, Ink, Vector2.zero, Vector2.right, Vector2.zero, new Vector2(0, 38));
                Text help = Label("Help", helpBox, "", 16, new Color(.8f, .86f, .85f), Vector2.zero, Vector2.one, new Vector2(18, 3), new Vector2(-8, -3));
                helpBox.GetComponent<Image>().raycastTarget = false;
                Assign(hud, "topBar", bar, "testPanel", panel, "foodValue", numbers[0], "woodValue", numbers[1],
                    "goldValue", numbers[2], "populationValue", numbers[3], "feedback", feedback, "help", help,
                    "toggleButton", toggle, "grantButton", grant, "spendButton", spend, "capacityButton", capacity);
                panel.gameObject.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static RectTransform Box(string name, Transform parent, Color color, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image)); obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = low; rect.offsetMax = high;
            obj.GetComponent<Image>().color = color; return rect;
        }

        private static Text Label(string name, Transform parent, string text, int size, Color color, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Text)); obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = low; rect.offsetMax = high;
            Text label = obj.GetComponent<Text>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = text; label.fontSize = size; label.color = color; label.alignment = TextAnchor.MiddleLeft;
            label.raycastTarget = false; label.horizontalOverflow = HorizontalWrapMode.Overflow; return label;
        }

        private static Button MakeButton(string name, Transform parent, string text, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
        {
            RectTransform rect = Box(name, parent, Card, min, max, low, high);
            Button button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>();
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            Text label = Label("Caption", rect, text, 17, Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            label.alignment = TextAnchor.MiddleCenter; return button;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path));
        }

        private static void Assign(UnityEngine.Object target, params object[] pairs)
        {
            var serialized = new SerializedObject(target);
            for (int i = 0; i < pairs.Length; i += 2)
                serialized.FindProperty((string)pairs[i]).objectReferenceValue = (UnityEngine.Object)pairs[i + 1];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // Small original placeholder icons, rasterized from circles and polygons. No external artwork.
        private static Sprite CreateIcon(string name, int kind)
        {
            string path = "Assets/Game/Art/UI/" + name + ".png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
                {
                    Color c = Color.clear;
                    if (kind == 0)
                    {
                        if (Circle(x, y, 24, 28, 17) || Circle(x, y, 40, 28, 17)) c = new Color(.86f, .3f, .23f);
                        if (Circle(x, y, 20, 34, 6)) c = new Color(1, .52f, .35f);
                        if (x >= 30 && x <= 34 && y >= 43 && y <= 55) c = new Color(.45f, .28f, .12f);
                        if (Circle(x, y, 42, 50, 7) && x + y > 85) c = new Color(.4f, .75f, .3f);
                    }
                    else if (kind == 1)
                    {
                        for (int log = 0; log < 2; log++)
                        {
                            int cy = 20 + 23 * log;
                            if (x >= 15 && x <= 49 && Mathf.Abs(y - cy) <= 9) c = new Color(.48f, .28f, .13f);
                            if (Circle(x, y, 16, cy, 10)) c = new Color(.91f, .68f, .36f);
                            if (Circle(x, y, 16, cy, 6) && !Circle(x, y, 16, cy, 4)) c = new Color(.6f, .36f, .17f);
                            if (x >= 29 && x <= 48 && y == cy + 4) c = new Color(.72f, .47f, .21f);
                        }
                    }
                    else if (kind == 2)
                    {
                        if ((y >= 13 && y <= 29 && x >= 6 + (y - 13) / 3 && x <= 57 - (y - 13) / 3)
                            || (y >= 33 && y <= 49 && x >= 18 + (y - 33) / 3 && x <= 47 - (y - 33) / 3))
                            c = y < 29 || y < 49 && y > 32 ? new Color(.93f, .64f, .12f) : new Color(1, .87f, .4f);
                        if (y == 29 && x >= 11 && x <= 52 || y == 49 && x >= 23 && x <= 42) c = new Color(1, .9f, .48f);
                    }
                    else
                    {
                        if (Circle(x, y, 18, 37, 8) || Circle(x, y, 47, 37, 8)
                            || Circle(x, y, 18, 17, 12) || Circle(x, y, 47, 17, 12)) c = new Color(.39f, .64f, .7f);
                        if (Circle(x, y, 32, 45, 11) || (Circle(x, y, 32, 16, 17) && y >= 6)) c = new Color(.85f, .9f, .82f);
                    }
                    texture.SetPixel(x, y, c);
                }
                texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite; importer.spritePixelsPerUnit = 64;
                importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static bool Circle(int x, int y, int cx, int cy, int radius) => (x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius;
    }
}
