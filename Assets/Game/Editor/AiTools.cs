using System;
using System.IO;
using System.Linq;
using GameRTS.Data;
using GameRTS.Simulation;
using GameRTS.UI;
using GameRTS.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace GameRTS.Editor
{
    public static class AiTools
    {
        public const string ScenePath = "Assets/Game/Scenes/AiScene.unity";
        private const string GamePath = "Assets/Game/Scenes/GameScene.unity";
        [MenuItem("GameRTS/Set up AI opponent")]
        public static void Setup()
        {
            var current = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || current.isDirty)
                throw new InvalidOperationException("Stop Play and save the current scene before AI setup.");
            const string profilePath = "Assets/Game/Data/AiStandard.asset";
            if (!File.Exists(profilePath)) AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AiProfileSO>(), profilePath);
            ConfigureNavigation(); AssetDatabase.SaveAssets();
            if (!File.Exists(ScenePath)) CreateScene();
            var scenes = EditorBuildSettings.scenes.ToList();
            if (!scenes.Any(s => s.path == ScenePath)) scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray(); AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(GamePath);
            File.WriteAllText("Logs/Ai.setup.txt", "AiScene ready: equal 200W/200F/100G, 1 TC and 3 Villagers per side; shared sight; AI player2; standard commands.");
        }
        private static void ConfigureNavigation()
        {
            const string path = "Assets/Game/UI/Prefabs/EconomyHud.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var view = root.GetComponent<SceneSwitchView>();
                var existing = root.transform.Find("AI Match");
                if (existing == null)
                {
                    var button = UnityEngine.Object.Instantiate(root.transform.Find("Switch Map").gameObject, root.transform);
                    button.name = "AI Match";
                    button.GetComponentInChildren<Text>().text = "ĐẤU VỚI MÁY";
                    existing = button.transform;
                }
                existing.GetComponent<RectTransform>().anchoredPosition = new Vector2(-239, 34);
                Assign(view, "aiButton", existing.GetComponent<Button>());
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void CreateScene()
        {
            if (!AssetDatabase.CopyAsset(GamePath, ScenePath)) throw new IOException("Could not create AiScene from the base map.");
            var scene = EditorSceneManager.OpenScene(ScenePath); var game = Find<RtsGameView>(scene);
            var map = new GridMap(64, 64, -32000, -32000);
            var settings = new SerializedObject(game);
            var players = settings.FindProperty("additionalPlayers"); players.arraySize = 1; players.GetArrayElementAtIndex(0).intValue = 2;
            settings.FindProperty("enableEconomyTesting").boolValue = false; settings.FindProperty("enableResourceTesting").boolValue = false;
            settings.FindProperty("enableVictoryCondition").boolValue = true;
            settings.FindProperty("startingWood").intValue = 200; settings.FindProperty("startingFood").intValue = 200;
            settings.FindProperty("startingGold").intValue = 100; settings.FindProperty("startingMaxPopulation").intValue = 10;
            settings.ApplyModifiedPropertiesWithoutUndo();
            var resourceRoot = (Transform)settings.FindProperty("resourceRoot").objectReferenceValue;
            ClearChildren(resourceRoot);
            foreach (var node in AiScenario.Resources(map))
            {
                string name = node.Type == ResourceType.Wood ? "Tree" : node.Type == ResourceType.Food ? "BerryBush" : "GoldMine";
                var obj = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/Resources/" + name + ".prefab"), resourceRoot);
                obj.name = name + " " + node.Cell; obj.transform.position = Position(map, node.Cell);
            }
            var depotRoot = (Transform)settings.FindProperty("dropOffRoot").objectReferenceValue;
            ClearChildren(depotRoot);
            for (int player = 1; player <= 2; player++)
            {
                var obj = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/Buildings/TownHall.prefab"), depotRoot);
                obj.name = player == 1 ? "Player Town Center" : "AI Town Center";
                obj.transform.position = Position(map, AiScenario.TownCell(map, player)) + new Vector3(.5f, .5f);
                var owner = new SerializedObject(obj.GetComponent<DropOffView>()); owner.FindProperty("ownerId").intValue = player; owner.ApplyModifiedPropertiesWithoutUndo();
                if (player == 2) obj.GetComponent<SpriteRenderer>().color = new Color(1, .65f, .65f);
            }
            var villager = AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/Game/Data/Units/VillagerData.asset");
            var spawner = new SerializedObject(Find<UnitPreviewSpawner>(scene));
            spawner.FindProperty("unitData").objectReferenceValue = villager;
            var positions = spawner.FindProperty("spawnPositions"); var overrides = spawner.FindProperty("unitDataOverrides");
            int[] starts = AiScenario.VillagerCells(map, 1); positions.arraySize = overrides.arraySize = starts.Length;
            for (int i = 0; i < starts.Length; i++)
            { positions.GetArrayElementAtIndex(i).vector2Value = Position(map, starts[i]); overrides.GetArrayElementAtIndex(i).objectReferenceValue = villager; }
            spawner.ApplyModifiedPropertiesWithoutUndo();
            foreach (var tilemap in Find<RtsMapView>(scene).GetComponentsInChildren<Tilemap>()) if (tilemap.name == "Obstacles") tilemap.ClearAllTiles();
            var ai = new GameObject("AI Player 2", typeof(AiOpponentView)); SceneManager.MoveGameObjectToScene(ai, scene);
            Assign(ai.GetComponent<AiOpponentView>(), "game", game, "profile", AssetDatabase.LoadAssetAtPath<AiProfileSO>("Assets/Game/Data/AiStandard.asset"), "villager", villager);
            var camera = Find<Camera>(scene); camera.transform.position = new Vector3(-15, -3, -10); camera.orthographicSize = 10;
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }
        private static Vector3 Position(GridMap map, int cell) => new Vector3(map.CenterX(cell) / 1000f, map.CenterY(cell) / 1000f, 0);
        private static void ClearChildren(Transform root)
        { while (root.childCount > 0) UnityEngine.Object.DestroyImmediate(root.GetChild(0).gameObject); }
        private static T Find<T>(Scene scene) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects()) { var item = root.GetComponentInChildren<T>(true); if (item != null) return item; }
            throw new InvalidOperationException("Missing " + typeof(T).Name);
        }
        private static void Assign(UnityEngine.Object target, params object[] pairs)
        {
            var settings = new SerializedObject(target);
            for (int i = 0; i < pairs.Length; i += 2) settings.FindProperty((string)pairs[i]).objectReferenceValue = (UnityEngine.Object)pairs[i + 1];
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
