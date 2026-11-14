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
    public static class TrainingTools
    {
        private const string HudPath = "Assets/Game/UI/Prefabs/EconomyHud.prefab";
        [MenuItem("GameRTS/Set up Villager Training")]
        public static void Setup()
        {
            Scene scene = SceneManager.GetSceneByPath("Assets/Game/Scenes/GameScene.unity");
            if (EditorApplication.isPlayingOrWillChangePlaymode || !scene.isLoaded || scene.isDirty)
                throw new InvalidOperationException("Stop Play, open GameScene and save it before training setup.");
            var unit = AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/Game/Data/Units/VillagerData.asset");
            const string dataPath = "Assets/Game/Data/Buildings/VillagerTrainingData.asset";
            var data = AssetDatabase.LoadAssetAtPath<TrainingDataSO>(dataPath);
            if (data == null) { data = ScriptableObject.CreateInstance<TrainingDataSO>(); Assign(data, "unitData", unit); AssetDatabase.CreateAsset(data, dataPath); }
            Sprite ring = SpriteAsset("Assets/Game/Art/UI/TrainingRing.png", false);
            Sprite flag = SpriteAsset("Assets/Game/Art/UI/RallyFlag.png", true);
            ConfigureTownHall(data, flag); ConfigureHud(ring);
            AssetDatabase.ImportAsset("Assets/Game/View/Prefabs/Buildings/TownHall.prefab", ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(HudPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            RtsGameView game = null; UnitPreviewSpawner spawner = null; TrainingHudView hud = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (game == null) game = root.GetComponentInChildren<RtsGameView>(true);
                if (spawner == null) spawner = root.GetComponentInChildren<UnitPreviewSpawner>(true);
                if (hud == null) hud = root.GetComponentInChildren<TrainingHudView>(true);
            }
            if (game == null || spawner == null || hud == null) throw new InvalidOperationException("Missing gameplay scene bindings.");
            var settings = new SerializedObject(spawner); var positions = settings.FindProperty("spawnPositions");
            var overrides = settings.FindProperty("unitDataOverrides"); positions.arraySize = overrides.arraySize = 3;
            Vector2[] initial = { new Vector2(-4, -1.5f), new Vector2(-2, 1), Vector2.zero };
            for (int i = 0; i < 3; i++) { positions.GetArrayElementAtIndex(i).vector2Value = initial[i]; overrides.GetArrayElementAtIndex(i).objectReferenceValue = unit; }
            settings.ApplyModifiedPropertiesWithoutUndo(); Assign(hud, "game", game);
            var input = game.GetComponent<TrainingInput>(); if (input == null) input = game.gameObject.AddComponent<TrainingInput>();
            Assign(input, "game", game, "hud", hud);
            EditorSceneManager.MarkSceneDirty(scene); if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save training setup.");
            AssetDatabase.SaveAssets(); File.WriteAllText("Logs/Milestone2.training-setup.txt", "Town Center queue, ring, 5 cancelable slots, rally flag, training input, and 3 initial Villagers connected.");
        }
        private static void ConfigureTownHall(TrainingDataSO data, Sprite flag)
        {
            const string path = "Assets/Game/View/Prefabs/Buildings/TownHall.prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Transform marker = root.transform.Find("Selection Marker");
                if (marker == null)
                {
                    marker = new GameObject("Selection Marker").transform; marker.SetParent(root.transform, false);
                    for (int i = 0; i < 4; i++)
                    {
                        var edge = new GameObject("Edge", typeof(SpriteRenderer)); edge.transform.SetParent(marker, false);
                        var renderer = edge.GetComponent<SpriteRenderer>(); renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Terrain/GroundTile.png");
                        renderer.sharedMaterial = root.GetComponent<SpriteRenderer>().sharedMaterial; renderer.color = new Color(.25f, 1, .4f); renderer.sortingOrder = 8;
                        edge.transform.localScale = i < 2 ? new Vector3(2.12f, .055f, 1) : new Vector3(.055f, 2.12f, 1);
                        edge.transform.localPosition = i < 2 ? new Vector3(0, i == 0 ? -1.06f : 1.06f, -.1f) : new Vector3(i == 2 ? -1.06f : 1.06f, 0, -.1f);
                    }
                }
                Transform rally = root.transform.Find("Rally Flag");
                if (rally == null)
                {
                    var obj = new GameObject("Rally Flag", typeof(SpriteRenderer)); rally = obj.transform; rally.SetParent(root.transform, false);
                    var renderer = obj.GetComponent<SpriteRenderer>(); renderer.sprite = flag; renderer.sortingOrder = 30;
                    renderer.sharedMaterial = root.GetComponent<SpriteRenderer>().sharedMaterial;
                    rally.localScale = Vector3.one * .8f;
                }
                marker.gameObject.SetActive(false); rally.gameObject.SetActive(false);
                Assign(root.GetComponent<DropOffView>(), "trainingData", data, "selectionMarker", marker, "rallyMarker", rally);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void ConfigureHud(Sprite ring)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(HudPath);
            try
            {
                if (root.GetComponent<TrainingHudView>() != null) return;
                var hud = root.AddComponent<TrainingHudView>();
                var panel = Box(root.transform, "Training Panel", new Vector2(16, 34), new Vector2(650, 150), new Color(.065f, .105f, .12f, .98f));
                Label(panel, "Title", "TÒA THỊ CHÍNH · Chuột phải map: tập kết", new Vector2(16, 118), new Vector2(620, 24), 18);
                var train = Box(panel, "Train Villager", new Vector2(16, 37), new Vector2(155, 66), new Color(.22f, .34f, .24f));
                var button = train.gameObject.AddComponent<Button>(); button.targetGraphic = train.GetComponent<Image>(); button.navigation = new Navigation { mode = Navigation.Mode.None };
                Label(train, "Caption", "HUẤN LUYỆN DÂN\n50 Thực · 10 giây", Vector2.zero, new Vector2(155, 66), 16, TextAnchor.MiddleCenter);
                var background = Box(panel, "Progress Background", new Vector2(187, 40), new Vector2(64, 64), new Color(.25f, .31f, .32f));
                background.GetComponent<Image>().sprite = ring;
                var progress = Box(panel, "Progress Ring", new Vector2(187, 40), new Vector2(64, 64), new Color(.96f, .75f, .32f)).GetComponent<Image>();
                progress.sprite = ring; progress.type = Image.Type.Filled; progress.fillMethod = Image.FillMethod.Radial360; progress.fillOrigin = 2; progress.fillAmount = 0;
                var percent = Label(panel, "Progress Text", "0%", new Vector2(187, 40), new Vector2(64, 64), 16, TextAnchor.MiddleCenter);
                var queue = Label(panel, "Queue Text", "Queue: [0/5]", new Vector2(268, 99), new Vector2(365, 24), 16);
                var status = Label(panel, "Status", "", new Vector2(16, 6), new Vector2(615, 28), 15); status.color = new Color(1, .81f, .52f);
                var serialized = new SerializedObject(hud); var slots = serialized.FindProperty("slots"); slots.arraySize = 5;
                for (int i = 0; i < 5; i++)
                {
                    var slot = Box(panel, "Queue Slot " + (i + 1), new Vector2(268 + i * 70, 43), new Vector2(60, 50), new Color(.17f, .24f, .27f));
                    var view = slot.gameObject.AddComponent<TrainingQueueSlotView>();
                    var icon = Box(slot, "Icon", new Vector2(10, 3), new Vector2(40, 44), Color.white).GetComponent<Image>(); icon.raycastTarget = false; icon.preserveAspect = true;
                    var number = Label(slot, "Order", "", new Vector2(3, 29), new Vector2(18, 18), 13);
                    Assign(view, "icon", icon, "order", number); slots.GetArrayElementAtIndex(i).objectReferenceValue = view;
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assign(hud, "panel", panel, "trainButton", button, "progress", progress, "progressText", percent, "queueText", queue, "status", status);
                panel.gameObject.SetActive(false); PrefabUtility.SaveAsPrefabAsset(root, HudPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static RectTransform Box(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image)); obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero; rect.anchoredPosition = position; rect.sizeDelta = size;
            obj.GetComponent<Image>().color = color; return rect;
        }
        private static Text Label(Transform parent, string name, string value, Vector2 position, Vector2 size, int font, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Text)); obj.transform.SetParent(parent, false);
            var text = obj.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = font; text.text = value;
            text.color = Color.white; text.alignment = alignment; text.raycastTarget = false;
            var rect = text.rectTransform; rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero; rect.anchoredPosition = position; rect.sizeDelta = size; return text;
        }
        private static void Assign(UnityEngine.Object target, params object[] pairs)
        {
            var settings = new SerializedObject(target);
            for (int i = 0; i < pairs.Length; i += 2) settings.FindProperty((string)pairs[i]).objectReferenceValue = (UnityEngine.Object)pairs[i + 1];
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
        private static Sprite SpriteAsset(string path, bool flag)
        {
            if (!File.Exists(path))
            {
                int size = flag ? 32 : 64; var texture = new Texture2D(size, size, TextureFormat.RGBA32, false); texture.SetPixels(new Color[size * size]);
                for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
                {
                    int dx = x * 2 - size + 1, dy = y * 2 - size + 1; int distance = dx * dx + dy * dy;
                    Color c = !flag && distance >= 44 * 44 && distance <= 60 * 60 ? Color.white : Color.clear;
                    if (flag && x >= 7 && x <= 9 && y >= 2 && y <= 29) c = new Color(.69f, .47f, .24f);
                    if (flag && x >= 10 && x <= 25 && y >= 19 && y <= 29 - (x - 10) / 3) c = new Color(.3f, 1, .45f);
                    texture.SetPixel(x, y, c);
                }
                texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture); AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite; importer.spritePixelsPerUnit = size;
                importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.filterMode = FilterMode.Point; importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
