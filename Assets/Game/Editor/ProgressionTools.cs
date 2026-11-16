using System;
using System.IO;
using System.Linq;
using GameRTS.Commands;
using GameRTS.Data;
using GameRTS.Input;
using GameRTS.Simulation;
using GameRTS.UI;
using GameRTS.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameRTS.Editor
{
    public static class ProgressionTools
    {
        private const string AnimalArt = "Assets/Game/Art/Animals/";
        private const string AnimalData = "Assets/Game/Data/Animals/";
        private const string AnimalPrefabs = "Assets/Game/View/Prefabs/Animals/";
        private const string TechData = "Assets/Game/Data/Technologies/";
        private static readonly string[] AnimalNames = { "Deer", "Boar", "Elephant" };
        private static readonly string[] AnimalLabels = { "Nai", "Heo rừng", "Voi" };
        [MenuItem("GameRTS/Set up Hunting and Technologies")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before setup.");
            for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scenes before setup.");
            Folder("Assets/Game/Art", "Animals"); Folder("Assets/Game/Data", "Animals"); Folder("Assets/Game/Data", "Technologies"); Folder("Assets/Game/View/Prefabs", "Animals");
            Sprite carcass = MakeSprite(AnimalArt + "Carcass.png", 3);
            for (int i = 0; i < 3; i++) MakeAnimal(i, carcass);
            string[] labels = { "Rìu đôi (+20% Gỗ)", "Túi da (gánh 15)", "Mũi tên rèn (+1 Công/+1 Tầm)", "Giáp vảy (+1 Giáp)", "Lên Đời 2 — Đồ đồng" };
            foreach (var d in ResearchDefinition.Defaults())
            {
                string path = TechData + d.Id + ".asset"; if (File.Exists(path)) continue;
                var data = ScriptableObject.CreateInstance<TechnologyDataSO>();
                Assign(data, "technology", (int)d.Id, "displayName", labels[(int)d.Id], "buildingType", d.BuildingType,
                    "requiredAge", d.RequiredAge, "wood", d.Wood, "food", d.Food, "gold", d.Gold, "durationTicks", d.Duration);
                AssetDatabase.CreateAsset(data, path);
            }
            Sprite stone = MakeSprite("Assets/Game/Art/Buildings/TownHallBronze.png", 4);
            Assign(AssetDatabase.LoadAssetAtPath<DropOffDataSO>("Assets/Game/Data/Buildings/TownHallData.asset"), "bronzeSprite", stone);
            AssetDatabase.SaveAssets();
            foreach (var entry in EditorBuildSettings.scenes.Where(e => e.enabled))
            {
                var scene = EditorSceneManager.OpenScene(entry.path);
                var game = Find<RtsGameView>(scene); if (game == null) continue;
                bool enabled = scene.name != "CombatScene";
                var serialized = new SerializedObject(game); serialized.FindProperty("enableTechnologies").boolValue = enabled;
                var catalog = serialized.FindProperty("technologyCatalog"); catalog.arraySize = 5;
                for (int i = 0; i < 5; i++) catalog.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<TechnologyDataSO>(TechData + (TechnologyId)i + ".asset");
                serialized.ApplyModifiedPropertiesWithoutUndo();
                var hudRoot = Find<EconomyHudView>(scene).gameObject;
                var hud = hudRoot.GetComponent<ResearchHudView>() ?? hudRoot.AddComponent<ResearchHudView>(); Assign(hud, "game", game);
                var input = game.GetComponent<ResearchInput>() ?? game.gameObject.AddComponent<ResearchInput>(); Assign(input, "game", game, "hud", hud);
                if (enabled)
                {
                    var root = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "Wildlife");
                    if (root == null) root = new GameObject("Wildlife");
                    var spawner = root.GetComponent<FaunaSpawner>() ?? root.AddComponent<FaunaSpawner>();
                    Assign(spawner, "game", game);
                    var so = new SerializedObject(spawner); var data = so.FindProperty("data"); var positions = so.FindProperty("positions");
                    int count = scene.name == "AiScene" ? 8 : 4; data.arraySize = positions.arraySize = count;
                    Vector2[] sample = { new Vector2(-8.5f, -.5f), new Vector2(-10.5f, 2.5f), new Vector2(7.5f, 6.5f), new Vector2(13.5f, -9.5f) };
                    for (int i = 0; i < count; i++)
                    {
                        int kind = i % 4 < 2 ? 0 : i % 4 - 1;
                        data.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AnimalDataSO>(AnimalData + AnimalNames[kind] + "Data.asset");
                        if (scene.name == "AiScene")
                        {
                            var cell = ProgressionScenario.AnimalCells()[i]; positions.GetArrayElementAtIndex(i).vector2Value = new Vector2(cell.x - 31.5f, cell.y - 31.5f);
                        }
                        else positions.GetArrayElementAtIndex(i).vector2Value = sample[i];
                    }
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                EditorSceneManager.MarkSceneDirty(scene); if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Cannot save " + entry.path);
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene("Assets/Game/Scenes/GameScene.unity");
            File.WriteAllText("Logs/Progression.setup.txt", "Animal assets/placements, research HUD/commands, age gates and bronze TC connected. CombatScene remains a combat-only prototype.");
        }
        private static T Find<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).FirstOrDefault();
        private static void Folder(string parent, string child) { if (!AssetDatabase.IsValidFolder(parent + "/" + child)) AssetDatabase.CreateFolder(parent, child); }
        private static void MakeAnimal(int i, Sprite carcass)
        {
            Sprite alive = MakeSprite(AnimalArt + AnimalNames[i] + ".png", i);
            string prefab = AnimalPrefabs + AnimalNames[i] + ".prefab";
            if (!File.Exists(prefab))
            {
                var root = new GameObject(AnimalNames[i], typeof(SpriteRenderer), typeof(AnimalView));
                try
                {
                    var body = root.GetComponent<SpriteRenderer>(); body.sprite = alive; body.sortingOrder = 10;
                    var label = new GameObject("Animal status", typeof(TextMesh)); label.transform.SetParent(root.transform, false); label.transform.localPosition = new Vector3(0, .65f, -.1f);
                    var t = label.GetComponent<TextMesh>(); t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.fontSize = 40; t.characterSize = .05f; t.anchor = TextAnchor.LowerCenter;
                    t.GetComponent<MeshRenderer>().sharedMaterial = t.font.material; t.GetComponent<MeshRenderer>().sortingOrder = 42;
                    Assign(root.GetComponent<AnimalView>(), "body", body, "label", t); PrefabUtility.SaveAsPrefabAsset(root, prefab);
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            string path = AnimalData + AnimalNames[i] + "Data.asset";
            if (File.Exists(path)) return;
            var d = ScriptableObject.CreateInstance<AnimalDataSO>();
            Assign(d, "typeId", AnimalNames[i].ToLowerInvariant(), "displayName", AnimalLabels[i], "kind", i,
                "food", new[] { 250, 400, 600 }[i], "maxHP", new[] { 32, 200, 450 }[i], "armor", i == 2 ? 2 : 0,
                "damage", new[] { 0, 12, 24 }[i], "speed", new[] { 4000, 4000, 3500 }[i], "attackInterval", i == 1 ? 15 : 20,
                "aliveSprite", alive, "carcassSprite", carcass, "viewPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(prefab));
            AssetDatabase.CreateAsset(d, path);
        }
        private static Sprite MakeSprite(string path, int kind)
        {
            if (!File.Exists(path))
            {
                var t = new Texture2D(64, 64, TextureFormat.RGBA32, false); t.SetPixels(new Color[4096]);
                for (int y = 3; y < 61; y++) for (int x = 3; x < 61; x++)
                {
                    bool body = (x - 30) * (x - 30) * 2 + (y - 30) * (y - 30) < (kind == 2 ? 620 : 420);
                    Color color = kind == 0 ? new Color(.62f, .37f, .17f) : kind == 1 ? new Color(.26f, .17f, .12f) : new Color(.56f, .58f, .59f);
                    if (kind < 3)
                    {
                        if (body || x > 43 && x < 58 && y > 26 && y < 39 || (x > 16 && x < 22 || x > 34 && x < 40) && y > 10 && y < 30) t.SetPixel(x, y, color);
                        if (kind == 0 && (x == 49 || x == 53) && y > 36 && y < 53) t.SetPixel(x, y, new Color(.85f, .73f, .51f));
                        if (kind == 1 && x > 51 && y > 23 && y < 28) t.SetPixel(x, y, Color.white);
                        if (kind == 2 && x > 52 && x < 59 && y > 13 && y < 36) t.SetPixel(x, y, color);
                        if (x > 49 && x < 53 && y > 32 && y < 36) t.SetPixel(x, y, Color.black);
                    }
                    else if (kind == 3 && body) t.SetPixel(x, y, (x + y) % 9 < 3 ? new Color(.85f, .64f, .54f) : new Color(.48f, .12f, .1f));
                    else if (kind == 4 && x > 7 && x < 57 && y > 6 && y < 57)
                    {
                        bool wall = x < 15 || x > 49 || y < 15 || y > 49;
                        t.SetPixel(x, y, wall ? (x % 12 < 2 || y % 8 < 2 ? new Color(.28f, .31f, .34f) : new Color(.66f, .69f, .72f)) : new Color(.22f, .34f, .46f));
                        if (x > 26 && x < 38 && y < 20) t.SetPixel(x, y, new Color(.3f, .18f, .08f));
                    }
                }
                File.WriteAllBytes(path, t.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(t); AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = kind == 4 ? 32 : 64; importer.filterMode = FilterMode.Point; importer.alphaIsTransparency = true; importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static void Assign(UnityEngine.Object target, params object[] pairs)
        {
            var so = new SerializedObject(target);
            for (int i = 0; i < pairs.Length; i += 2)
            {
                var p = so.FindProperty((string)pairs[i]); object value = pairs[i + 1];
                if (value is int number) p.intValue = number; else if (value is string text) p.stringValue = text; else p.objectReferenceValue = (UnityEngine.Object)value;
            }
            so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(target);
        }
    }
}
