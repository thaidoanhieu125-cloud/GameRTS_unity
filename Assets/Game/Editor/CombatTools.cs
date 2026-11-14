using System;
using System.IO;
using GameRTS.Data;
using GameRTS.UI;
using GameRTS.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameRTS.Editor
{
    public static class CombatTools
    {
        private const string GamePath = "Assets/Game/Scenes/GameScene.unity", CombatPath = "Assets/Game/Scenes/CombatScene.unity";
        private static Sprite tile;
        private static Material material;
        [MenuItem("GameRTS/Set up Combat")]
        public static void Setup()
        {
            var scene = SceneManager.GetSceneByPath(GamePath);
            if (EditorApplication.isPlayingOrWillChangePlaymode || !scene.isLoaded || scene.isDirty)
                throw new InvalidOperationException("Stop Play, open GameScene and save before combat setup.");
            tile = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Terrain/GroundTile.png");
            material = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/SampleSoldier.prefab").GetComponent<SpriteRenderer>().sharedMaterial;
            Sprite arrow = MakeArrow(); ConfigureArrow(arrow);
            foreach (string path in new[] { "SampleSoldier", "Villager" }) ConfigureHealth("Assets/Game/View/Prefabs/" + path + ".prefab", false);
            foreach (string path in new[] { "TownHall", "House", "LumberCamp", "Mill", "MiningCamp" }) ConfigureHealth("Assets/Game/View/Prefabs/Buildings/" + path + ".prefab", true);
            ConfigureArcher();
            var melee = MakeUnit("Melee", "melee", "Lính giáo", "SampleSoldier", 120, 2, 12, 20, 1, 0);
            var archer = MakeUnit("Archer", "archer", "Cung thủ", "Archer", 70, 0, 10, 24, 5, 8000);
            ConfigureHud(); AssetDatabase.SaveAssets();
            var game = Find<RtsGameView>(scene); var arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/Arrow.prefab").GetComponent<ProjectileView>();
            Assign(game, "arrowPrefab", arrowPrefab); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            if (!File.Exists(CombatPath))
            {
                if (!AssetDatabase.CopyAsset(GamePath, CombatPath)) throw new IOException("Could not copy economy map for combat.");
                var combat = EditorSceneManager.OpenScene(CombatPath); var combatGame = Find<RtsGameView>(combat);
                var settings = new SerializedObject(combatGame); var players = settings.FindProperty("additionalPlayers"); players.arraySize = 1; players.GetArrayElementAtIndex(0).intValue = 2;
                settings.ApplyModifiedPropertiesWithoutUndo();
                var obj = new GameObject("Combat Units", typeof(CombatPreviewSpawner)); SceneManager.MoveGameObjectToScene(obj, combat);
                Assign(obj.GetComponent<CombatPreviewSpawner>(), "game", combatGame,
                    "melee", AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/Game/Data/Units/MeleeData.asset"),
                    "archer", AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/Game/Data/Units/ArcherData.asset"));
                var root = new SerializedObject(combatGame).FindProperty("dropOffRoot").objectReferenceValue as Transform;
                var town = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/Buildings/TownHall.prefab"), root);
                town.name = "Enemy Town Center"; town.transform.position = new Vector3(12, -5, 0);
                var owner = new SerializedObject(town.GetComponent<DropOffView>()); owner.FindProperty("ownerId").intValue = 2; owner.ApplyModifiedPropertiesWithoutUndo();
                town.GetComponent<SpriteRenderer>().color = new Color(1, .65f, .65f);
                var camera = Find<Camera>(combat); camera.transform.position = new Vector3(4, 0, -10); camera.orthographicSize = 10;
                EditorSceneManager.MarkSceneDirty(combat); EditorSceneManager.SaveScene(combat);
                EditorSceneManager.OpenScene(GamePath);
            }
            // Reload assets after opening the scene: importing/copying can invalidate temporary references.
            var savedCombat = EditorSceneManager.OpenScene(CombatPath);
            var savedGame = Find<RtsGameView>(savedCombat);
            Assign(Find<CombatPreviewSpawner>(savedCombat), "game", savedGame,
                "melee", AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/Game/Data/Units/MeleeData.asset"),
                "archer", AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/Game/Data/Units/ArcherData.asset"));
            EditorSceneManager.MarkSceneDirty(savedCombat); EditorSceneManager.SaveScene(savedCombat);
            EditorSceneManager.OpenScene(GamePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(GamePath, true), new EditorBuildSettingsScene(CombatPath, true) };
            AssetDatabase.SaveAssets(); File.WriteAllText("Logs/Milestone3.setup.txt", "Health bars, melee/archer stats, arrow, CombatScene and navigation connected. Main map still starts with 3 Villagers.");
        }
        private static T Find<T>(Scene scene) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects()) { var value = root.GetComponentInChildren<T>(true); if (value != null) return value; }
            throw new InvalidOperationException("Missing " + typeof(T).Name);
        }
        private static void ConfigureHealth(string path, bool building)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (root.transform.Find("HealthBar") == null)
                {
                    float width = building ? 1.7f : .85f;
                    var bar = new GameObject("HealthBar", typeof(HealthBarView)); bar.transform.SetParent(root.transform, false); bar.transform.localPosition = new Vector3(0, building ? 1.15f : .67f, -.2f);
                    var bg = SpriteChild(bar.transform, "Background", new Color(.08f, .09f, .1f), 40); bg.transform.localScale = new Vector3(width + .06f, .13f, 1);
                    var fill = SpriteChild(bar.transform, "Fill", new Color(.25f, .95f, .35f), 41); fill.transform.localScale = new Vector3(width, .07f, 1);
                    var label = new GameObject("HP", typeof(TextMesh)); label.transform.SetParent(bar.transform, false); label.transform.localPosition = new Vector3(0, .18f, -.03f);
                    var text = label.GetComponent<TextMesh>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = 40; text.characterSize = .045f; text.anchor = TextAnchor.MiddleCenter; text.text = "HP";
                    text.GetComponent<MeshRenderer>().sharedMaterial = text.font.material; text.GetComponent<MeshRenderer>().sortingOrder = 42;
                    Assign(bar.GetComponent<HealthBarView>(), "fill", fill.transform, "fillRenderer", fill, "label", text);
                    var settings = new SerializedObject(bar.GetComponent<HealthBarView>()); settings.FindProperty("width").floatValue = width; settings.ApplyModifiedPropertiesWithoutUndo();
                    Component view = building ? (Component)root.GetComponent<DropOffView>() ?? root.GetComponent<BuildingView>() : root.GetComponent<UnitView>();
                    Assign(view, "healthBar", bar.GetComponent<HealthBarView>());
                }
                if (!building && root.transform.Find("Attack Flash") == null)
                {
                    var flash = SpriteChild(root.transform, "Attack Flash", new Color(1, .85f, .3f), 20); flash.transform.localScale = new Vector3(.12f, .5f, 1); flash.transform.localPosition = new Vector3(.38f, .05f, -.1f); flash.transform.localRotation = Quaternion.Euler(0, 0, -25); flash.enabled = false;
                    Assign(root.GetComponent<UnitView>(), "attackFlash", flash);
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void ConfigureArcher()
        {
            const string path = "Assets/Game/View/Prefabs/Archer.prefab";
            if (File.Exists(path)) return;
            AssetDatabase.CopyAsset("Assets/Game/View/Prefabs/SampleSoldier.prefab", path);
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                root.name = "Archer";
                for (int i = 0; i < 2; i++)
                {
                    var limb = SpriteChild(root.transform, "Bow Limb", new Color(.65f, .35f, .12f), 16);
                    limb.transform.localPosition = new Vector3(.32f, i == 0 ? .16f : -.16f, -.1f); limb.transform.localScale = new Vector3(.07f, .4f, 1); limb.transform.localRotation = Quaternion.Euler(0, 0, i == 0 ? 35 : -35);
                }
                var line = SpriteChild(root.transform, "Bow String", Color.white, 15); line.transform.localPosition = new Vector3(.21f, 0, -.1f); line.transform.localScale = new Vector3(.025f, .6f, 1);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static UnitDataSO MakeUnit(string file, string id, string name, string prefab, int hp, int armor, int damage, int interval, int range, int speed)
        {
            string path = "Assets/Game/Data/Units/" + file + "Data.asset";
            var data = AssetDatabase.LoadAssetAtPath<UnitDataSO>(path); if (data != null) return data;
            data = ScriptableObject.CreateInstance<UnitDataSO>(); var settings = new SerializedObject(data);
            settings.FindProperty("unitId").stringValue = id; settings.FindProperty("displayName").stringValue = name;
            settings.FindProperty("maxHP").intValue = hp; settings.FindProperty("armor").intValue = armor; settings.FindProperty("attackDamage").intValue = damage;
            settings.FindProperty("attackIntervalTicks").intValue = interval; settings.FindProperty("attackRangeCells").intValue = range;
            settings.FindProperty("projectileSpeed").intValue = speed; settings.FindProperty("aggroRangeCells").intValue = 6;
            settings.FindProperty("viewPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/" + prefab + ".prefab");
            settings.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.CreateAsset(data, path); return data;
        }
        private static SpriteRenderer SpriteChild(Transform parent, string name, Color color, int order)
        {
            var obj = new GameObject(name, typeof(SpriteRenderer)); obj.transform.SetParent(parent, false);
            var renderer = obj.GetComponent<SpriteRenderer>(); renderer.sprite = tile; renderer.sharedMaterial = material; renderer.color = color; renderer.sortingOrder = order; return renderer;
        }
        private static void ConfigureArrow(Sprite sprite)
        {
            const string path = "Assets/Game/View/Prefabs/Arrow.prefab"; if (File.Exists(path)) return;
            var obj = new GameObject("Arrow", typeof(SpriteRenderer), typeof(ProjectileView)); var renderer = obj.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite; renderer.sharedMaterial = material; renderer.sortingOrder = 35;
            PrefabUtility.SaveAsPrefabAsset(obj, path); UnityEngine.Object.DestroyImmediate(obj);
        }
        private static Sprite MakeArrow()
        {
            const string path = "Assets/Game/Art/Units/Arrow.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(32, 8, TextureFormat.RGBA32, false); texture.SetPixels(new Color[256]);
                for (int x = 1; x < 28; x++) for (int y = 3; y < 5; y++) texture.SetPixel(x, y, new Color(.8f, .6f, .3f));
                for (int x = 25; x < 32; x++) for (int y = 0; y < 8; y++) if (Math.Abs(y - 4) <= (31 - x) / 2) texture.SetPixel(x, y, Color.white);
                texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture); AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite; importer.spritePixelsPerUnit = 32;
                importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.filterMode = FilterMode.Point; importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static void ConfigureHud()
        {
            const string path = "Assets/Game/UI/Prefabs/EconomyHud.prefab"; var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (root.GetComponent<SceneSwitchView>() != null) return;
                var view = root.AddComponent<SceneSwitchView>(); var obj = new GameObject("Switch Map", typeof(RectTransform), typeof(Image), typeof(Button)); obj.transform.SetParent(root.transform, false);
                var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1, 0); rect.anchoredPosition = new Vector2(-16, 34); rect.sizeDelta = new Vector2(215, 42);
                obj.GetComponent<Image>().color = new Color(.13f, .26f, .34f); var button = obj.GetComponent<Button>(); button.targetGraphic = obj.GetComponent<Image>(); button.navigation = new Navigation { mode = Navigation.Mode.None };
                var label = new GameObject("Caption", typeof(RectTransform), typeof(Text)); label.transform.SetParent(rect, false); var text = label.GetComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.text = "THỬ CHIẾN ĐẤU"; text.fontSize = 17; text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
                var labelRect = label.GetComponent<RectTransform>(); labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one; labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
                Assign(view, "button", button, "caption", text); PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void Assign(UnityEngine.Object target, params object[] pairs)
        {
            var settings = new SerializedObject(target);
            for (int i = 0; i < pairs.Length; i += 2) settings.FindProperty((string)pairs[i]).objectReferenceValue = (UnityEngine.Object)pairs[i + 1];
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
