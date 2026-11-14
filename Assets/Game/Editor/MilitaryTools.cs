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
    public static class MilitaryTools
    {
        private const string GamePath = "Assets/Game/Scenes/GameScene.unity", CombatPath = "Assets/Game/Scenes/CombatScene.unity", HudPath = "Assets/Game/UI/Prefabs/EconomyHud.prefab";
        private static readonly string[] Files = { "Barracks", "ArcheryRange", "Stable" };
        private static readonly string[] Ids = { "barracks", "archery_range", "stable" };
        private static readonly string[] Names = { "Trại lính", "Trường bắn", "Chuồng ngựa" };
        private static readonly string[] Units = { "Melee", "Archer", "Cavalry" };
        [MenuItem("GameRTS/Set up Military and Victory")]
        public static void Setup()
        {
            var scene = SceneManager.GetSceneByPath(GamePath);
            if (EditorApplication.isPlayingOrWillChangePlaymode || !scene.isLoaded || scene.isDirty)
                throw new InvalidOperationException("Stop Play, open and save GameScene before military setup.");
            ConfigureCavalry();
            ConfigureUnit("Melee", UnitClass.Spearman, 120, 2, 12, 20, 2, 1, 0);
            ConfigureUnit("Archer", UnitClass.Archer, 45, 0, 18, 12, 3, 6, 10000);
            ConfigureUnit("Cavalry", UnitClass.Cavalry, 180, 1, 16, 20, 6, 1, 0);
            var villager = AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/Game/Data/Units/VillagerData.asset");
            var worker = new SerializedObject(villager); worker.FindProperty("unitClass").intValue = (int)UnitClass.Villager; worker.ApplyModifiedPropertiesWithoutUndo();
            for (int i = 0; i < 3; i++) { MakeTraining(i); MakeBuilding(i); }
            ConfigureHud(); AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(HudPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            foreach (string path in new[] { GamePath, CombatPath })
            {
                var loaded = path == GamePath ? SceneManager.GetSceneByPath(path) : EditorSceneManager.OpenScene(path);
                var game = Find<RtsGameView>(loaded); var settings = new SerializedObject(game); var catalog = settings.FindProperty("buildingCatalog");
                for (int kind = 0; kind < 3; kind++)
                {
                    var data = AssetDatabase.LoadAssetAtPath<BuildingDataSO>("Assets/Game/Data/Buildings/" + Files[kind] + "Data.asset"); bool exists = false;
                    for (int i = 0; i < catalog.arraySize; i++) if (catalog.GetArrayElementAtIndex(i).objectReferenceValue == data) exists = true;
                    if (!exists) { catalog.arraySize++; catalog.GetArrayElementAtIndex(catalog.arraySize - 1).objectReferenceValue = data; }
                }
                settings.FindProperty("enableVictoryCondition").boolValue = path == CombatPath; settings.ApplyModifiedPropertiesWithoutUndo();
                Assign(Find<MatchHudView>(loaded), "game", game);
                if (path == CombatPath) Assign(Find<CombatPreviewSpawner>(loaded), "cavalry", AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/Game/Data/Units/CavalryData.asset"));
                EditorSceneManager.MarkSceneDirty(loaded); if (!EditorSceneManager.SaveScene(loaded)) throw new IOException("Could not save " + path);
            }
            EditorSceneManager.OpenScene(GamePath); AssetDatabase.SaveAssets();
            File.WriteAllText("Logs/Milestone3.military-setup.txt", "3 unit classes and counters, 3 military buildings/queues/rally, victory/defeat/restart connected in both maps.");
        }
        private static T Find<T>(Scene scene) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects()) { var value = root.GetComponentInChildren<T>(true); if (value != null) return value; }
            throw new InvalidOperationException("Missing " + typeof(T).Name);
        }
        private static void ConfigureCavalry()
        {
            Sprite sprite = PixelSprite("Assets/Game/Art/Units/Cavalry.png", 3);
            const string path = "Assets/Game/View/Prefabs/Cavalry.prefab";
            if (!File.Exists(path))
            {
                var root = PrefabUtility.LoadPrefabContents("Assets/Game/View/Prefabs/SampleSoldier.prefab");
                try { root.name = "Cavalry"; root.GetComponent<SpriteRenderer>().sprite = sprite; PrefabUtility.SaveAsPrefabAsset(root, path); }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            const string dataPath = "Assets/Game/Data/Units/CavalryData.asset";
            if (!File.Exists(dataPath))
            {
                var data = ScriptableObject.CreateInstance<UnitDataSO>(); var settings = new SerializedObject(data);
                settings.FindProperty("unitId").stringValue = "cavalry"; settings.FindProperty("displayName").stringValue = "Kị binh";
                settings.FindProperty("viewPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                settings.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.CreateAsset(data, dataPath);
            }
        }
        private static void ConfigureUnit(string file, UnitClass type, int hp, int armor, int damage, int interval, int speed, int range, int projectile)
        {
            var data = AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/Game/Data/Units/" + file + "Data.asset"); var settings = new SerializedObject(data);
            settings.FindProperty("unitClass").intValue = (int)type; settings.FindProperty("maxHP").intValue = hp; settings.FindProperty("armor").intValue = armor;
            settings.FindProperty("attackDamage").intValue = damage; settings.FindProperty("attackIntervalTicks").intValue = interval;
            settings.FindProperty("moveSpeed").floatValue = speed; settings.FindProperty("attackRangeCells").intValue = range;
            settings.FindProperty("projectileSpeed").intValue = projectile; settings.FindProperty("aggroRangeCells").intValue = 6;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void MakeTraining(int kind)
        {
            string path = "Assets/Game/Data/Buildings/" + Files[kind] + "TrainingData.asset";
            if (File.Exists(path)) return;
            var data = ScriptableObject.CreateInstance<TrainingDataSO>(); var settings = new SerializedObject(data);
            settings.FindProperty("unitData").objectReferenceValue = AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/Game/Data/Units/" + Units[kind] + "Data.asset");
            settings.FindProperty("woodCost").intValue = kind == 2 ? 0 : 25;
            settings.FindProperty("foodCost").intValue = kind == 0 ? 50 : kind == 2 ? 70 : 0;
            settings.FindProperty("goldCost").intValue = kind == 0 ? 0 : kind == 1 ? 45 : 60;
            settings.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.CreateAsset(data, path);
        }
        private static void MakeBuilding(int kind)
        {
            Sprite sprite = PixelSprite("Assets/Game/Art/Buildings/" + Files[kind] + ".png", kind);
            string prefabPath = "Assets/Game/View/Prefabs/Buildings/" + Files[kind] + ".prefab";
            if (!File.Exists(prefabPath))
            {
                var root = PrefabUtility.LoadPrefabContents("Assets/Game/View/Prefabs/Buildings/House.prefab");
                var town = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/Buildings/TownHall.prefab");
                try
                {
                    root.name = Files[kind]; var select = UnityEngine.Object.Instantiate(town.transform.Find("Selection Marker").gameObject, root.transform).transform;
                    var flag = UnityEngine.Object.Instantiate(town.transform.Find("Rally Flag").gameObject, root.transform).transform;
                    select.name = "Selection Marker"; flag.name = "Rally Flag"; select.gameObject.SetActive(false); flag.gameObject.SetActive(false);
                    Assign(root.GetComponent<BuildingView>(), "selectionMarker", select, "rallyMarker", flag); PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            string path = "Assets/Game/Data/Buildings/" + Files[kind] + "Data.asset"; if (File.Exists(path)) return;
            var data = ScriptableObject.CreateInstance<BuildingDataSO>(); var settings = new SerializedObject(data);
            settings.FindProperty("typeId").stringValue = Ids[kind]; settings.FindProperty("displayName").stringValue = Names[kind];
            settings.FindProperty("woodCost").intValue = 150; settings.FindProperty("populationBonus").intValue = 0;
            settings.FindProperty("maxHitPoints").intValue = 600; settings.FindProperty("armor").intValue = 2;
            settings.FindProperty("foundationSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Buildings/HouseFoundation.png");
            settings.FindProperty("completedSprite").objectReferenceValue = sprite; settings.FindProperty("viewPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            settings.FindProperty("trainingData").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TrainingDataSO>("Assets/Game/Data/Buildings/" + Files[kind] + "TrainingData.asset");
            settings.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.CreateAsset(data, path);
        }
        private static void ConfigureHud()
        {
            var root = PrefabUtility.LoadPrefabContents(HudPath);
            try
            {
                var construction = root.GetComponent<ConstructionHudView>(); string[] fields = { "barracksButton", "archeryRangeButton", "stableButton" };
                for (int i = 0; i < 3; i++)
                {
                    string name = "Build " + Files[i]; var child = root.transform.Find("Top Bar/" + name);
                    if (child == null)
                    {
                        var obj = UnityEngine.Object.Instantiate(root.transform.Find("Top Bar/Build House").gameObject, root.transform.Find("Top Bar")); obj.name = name; child = obj.transform;
                        child.GetComponent<RectTransform>().anchoredPosition = new Vector2(16 + 168 * i, -78);
                        child.GetComponentInChildren<Text>(true).text = Names[i].ToUpperInvariant() + "\n150 Gỗ"; child.gameObject.SetActive(false);
                    }
                    Assign(construction, fields[i], child.GetComponent<Button>());
                }
                var notice = root.transform.Find("Construction Notice") as RectTransform;
                if (notice != null) notice.anchoredPosition = new Vector2(16, -232);
                var training = root.GetComponent<TrainingHudView>();
                Assign(training, "title", root.transform.Find("Training Panel/Title").GetComponent<Text>(), "trainCaption", root.transform.Find("Training Panel/Train Villager/Caption").GetComponent<Text>());
                root.transform.Find("Training Panel/Train Villager/Caption").GetComponent<Text>().fontSize = 14;
                if (root.GetComponent<MatchHudView>() == null)
                {
                    var view = root.AddComponent<MatchHudView>(); var overlay = Box(root.transform, "Match Result", Vector2.zero, Vector2.zero, new Color(0, 0, 0, .78f));
                    overlay.anchorMin = Vector2.zero; overlay.anchorMax = Vector2.one; overlay.offsetMin = overlay.offsetMax = Vector2.zero;
                    var panel = Box(overlay, "Panel", Vector2.zero, new Vector2(540, 300), new Color(.07f, .12f, .15f)); panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(.5f, .5f);
                    var title = Label(panel, "Title", "VICTORY\nCHIẾN THẮNG", new Vector2(20, 135), new Vector2(500, 130), 36);
                    var buttonRect = Box(panel, "Restart", new Vector2(150, 40), new Vector2(240, 65), new Color(.25f, .4f, .27f));
                    var button = buttonRect.gameObject.AddComponent<Button>(); button.targetGraphic = buttonRect.GetComponent<Image>(); button.navigation = new Navigation { mode = Navigation.Mode.None };
                    Label(buttonRect, "Caption", "CHƠI LẠI", Vector2.zero, new Vector2(240, 65), 24);
                    Assign(view, "overlay", overlay.gameObject, "title", title, "restart", button); overlay.gameObject.SetActive(false);
                }
                PrefabUtility.SaveAsPrefabAsset(root, HudPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static RectTransform Box(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image)); obj.transform.SetParent(parent, false); var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero; rect.anchoredPosition = position; rect.sizeDelta = size; obj.GetComponent<Image>().color = color; return rect;
        }
        private static Text Label(Transform parent, string name, string caption, Vector2 position, Vector2 size, int fontSize)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Text)); obj.transform.SetParent(parent, false); var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero; rect.anchoredPosition = position; rect.sizeDelta = size;
            var text = obj.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = fontSize; text.text = caption; text.color = Color.white; text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false; return text;
        }
        private static Sprite PixelSprite(string path, int kind)
        {
            if (!File.Exists(path))
            {
                int size = kind == 3 ? 64 : 128; var texture = new Texture2D(size, size, TextureFormat.RGBA32, false); texture.SetPixels(new Color[size * size]);
                Action<int, int, int, int, Color> rect = (left, bottom, right, top, color) => { for (int y = bottom; y <= top; y++) for (int x = left; x <= right; x++) texture.SetPixel(x, y, color); };
                if (kind == 3)
                {
                    rect(10, 22, 49, 39, new Color(.55f, .3f, .13f)); rect(42, 32, 53, 49, new Color(.65f, .38f, .18f));
                    rect(12, 8, 17, 23, Color.black); rect(40, 8, 45, 23, Color.black); rect(5, 28, 10, 40, Color.black);
                    rect(24, 36, 36, 48, new Color(.2f, .55f, 1)); rect(27, 48, 34, 56, new Color(.95f, .8f, .55f)); rect(38, 40, 40, 61, new Color(.7f, .7f, .7f));
                }
                else
                {
                    rect(6, 7, 121, 21, new Color(.28f, .26f, .21f)); rect(15, 21, 112, 79, new Color(.67f, .5f, .3f));
                    rect(9, 80, 118, 92, kind == 0 ? new Color(.55f, .2f, .15f) : kind == 1 ? new Color(.2f, .38f, .6f) : new Color(.45f, .3f, .15f));
                    rect(50, 21, 78, 63, new Color(.2f, .14f, .1f)); rect(21, 44, 38, 62, new Color(.8f, .83f, .65f)); rect(89, 44, 105, 62, new Color(.8f, .83f, .65f));
                    rect(61, 92, 64, 116, new Color(.7f, .6f, .4f)); rect(65, 103, 87, 115, kind == 0 ? Color.red : kind == 1 ? Color.cyan : Color.yellow);
                    if (kind == 0) { rect(22, 24, 24, 78, Color.white); rect(33, 24, 35, 78, Color.white); }
                    else if (kind == 1) { rect(24, 66, 36, 69, Color.red); rect(27, 63, 32, 72, Color.white); }
                    else { rect(87, 22, 115, 38, new Color(.45f, .24f, .1f)); rect(107, 35, 117, 52, new Color(.45f, .24f, .1f)); }
                }
                texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture); AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite; importer.spritePixelsPerUnit = 64;
                importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.filterMode = FilterMode.Point; importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static void Assign(UnityEngine.Object target, params object[] pairs)
        {
            var settings = new SerializedObject(target);
            for (int i = 0; i < pairs.Length; i += 2) settings.FindProperty((string)pairs[i]).objectReferenceValue = (UnityEngine.Object)pairs[i + 1];
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
