using System;
using System.IO;
using System.Text;
using GameRTS.Input;
using GameRTS.Simulation;
using GameRTS.View;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace GameRTS.Editor
{
    [InitializeOnLoad]
    public static class MilestoneOneTools
    {
        private const string ScenePath = "Assets/Game/Scenes/GameScene.unity";
        private static TestRunnerApi runner;

        static MilestoneOneTools()
        {
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/Milestone1.runner-ready.txt", "m1-v1");
            runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            runner.RegisterCallbacks(new TestResults());
            EditorApplication.update += ProcessRequest;
        }

        // Allows the same checks to run in an already open Editor without a second project instance.
        private static void ProcessRequest()
        {
            const string path = "Library/Milestone1.request";
            if (!File.Exists(path) || EditorApplication.isCompiling || EditorApplication.isUpdating
                || EditorApplication.isPlayingOrWillChangePlaymode) return;
            string action;
            try { action = File.ReadAllText(path).Trim(); File.Delete(path); }
            catch (IOException) { return; } // A writer or scanner may briefly hold the request file.
            try
            {
                switch (action)
                {
                    case "setup": SetupScene(); break;
                    case "economy-setup": MilestoneTwoTools.SetupEconomy(); break;
                    case "resources-setup": ResourceNodeTools.SetupResources(); break;
                    case "villagers-setup": VillagerTools.Setup(); break;
                    case "building-setup": BuildingTools.Setup(); break;
                    case "camps-setup": DropOffCampTools.Setup(); break;
                    case "training-setup": TrainingTools.Setup(); break;
                    case "refresh": AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate); break;
                    case "game-scene": OpenGameScene(); break;
                    case "edit-tests": RunEditModeTests(); break;
                    case "play-tests": RunPlayModeTests(); break;
                    case "baseline": WriteReplayBaseline(); break;
                    case "build": BuildWindows(); break;
                    default: throw new ArgumentException("Unknown verification request: " + action);
                }
            }
            catch (Exception e) { File.WriteAllText("Logs/Milestone1.error.txt", e.ToString()); Debug.LogException(e); }
        }

        [MenuItem("GameRTS/Run EditMode checks")]
        public static void RunEditModeTests() => RunTests(TestMode.EditMode, "editmode");
        [MenuItem("GameRTS/Run PlayMode checks")]
        public static void RunPlayModeTests() => RunTests(TestMode.PlayMode, "playmode");

        [MenuItem("GameRTS/Open GameScene")]
        public static void OpenGameScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before opening GameScene.");
            string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            string backupFolder = "Logs/SceneBackups";
            Directory.CreateDirectory(backupFolder);
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                string name = string.IsNullOrEmpty(scene.name) ? "Untitled" : scene.name;
                string backup = $"{backupFolder}/{name}-{stamp}-{i}.unity";
                if (!EditorSceneManager.SaveScene(scene, backup, true))
                    throw new IOException("Could not back up the current scene; kept it open: " + backup);
            }
            Scene opened = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!opened.isLoaded) throw new IOException("Could not open GameScene.");
            File.WriteAllText("Logs/GameScene.opened.txt", "Opened=" + opened.path + "\nSceneBackups=" + backupFolder);
        }

        private static void RunTests(TestMode mode, string suite)
        {
            SessionState.SetString("Milestone1.TestSuite", suite);
            runner.Execute(new ExecutionSettings(new Filter { testMode = mode }));
        }

        private sealed class TestResults : ICallbacks
        {
            public void RunStarted(ITestAdaptor test) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                string suite = SessionState.GetString("Milestone1.TestSuite", "");
                if (string.IsNullOrEmpty(suite)) return;
                TestRunnerApi.SaveResultToFile(result, "Logs/Milestone1." + suite + ".xml");
                File.WriteAllText("Logs/Milestone1." + suite + ".txt", $"Passed={result.PassCount};Failed={result.FailCount}");
                SessionState.EraseString("Milestone1.TestSuite");
            }
        }

        [MenuItem("GameRTS/Set up Milestone 1 scene")]
        public static void SetupScene()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(ScenePath);
            if (scene.isDirty) throw new InvalidOperationException("Save GameScene before running scene setup.");
            GameObject grid = FindRoot(scene, "Map Grid");
            Tilemap ground = grid.transform.Find("Ground").GetComponent<Tilemap>();
            Transform existing = grid.transform.Find("Obstacles");
            Tilemap obstacles;
            if (existing == null)
            {
                var obstacleObject = new GameObject("Obstacles", typeof(Tilemap), typeof(TilemapRenderer));
                obstacleObject.transform.SetParent(grid.transform, false);
                obstacles = obstacleObject.GetComponent<Tilemap>();
                var renderer = obstacleObject.GetComponent<TilemapRenderer>();
                renderer.sharedMaterial = ground.GetComponent<TilemapRenderer>().sharedMaterial;
                renderer.sortingOrder = -5;
                const string path = "Assets/Game/Art/Terrain/RockObstacle.asset";
                Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
                if (tile == null)
                {
                    tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Terrain/GroundTile.png");
                    tile.color = new Color(0.36f, 0.32f, 0.26f);
                    tile.colliderType = Tile.ColliderType.None;
                    AssetDatabase.CreateAsset(tile, path);
                }
                Paint(obstacles, tile, 6, -4, 2, 8);
                Paint(obstacles, tile, -10, 5, 5, 2);
                Paint(obstacles, tile, -9, -7, 3, 2);
            }
            else obstacles = existing.GetComponent<Tilemap>();

            RtsMapView map = GetOrAdd<RtsMapView>(grid);
            Assign(map, "ground", ground, "obstacles", obstacles);
            GameObject session = FindRoot(scene, "RTS Session", false);
            if (session == null) { session = new GameObject("RTS Session"); SceneManager.MoveGameObjectToScene(session, scene); }
            RtsGameView game = GetOrAdd<RtsGameView>(session);
            Assign(game, "mapView", map);
            Camera camera = FindRoot(scene, "Main Camera").GetComponent<Camera>();
            UnitSelectionView selection = GetOrAdd<UnitSelectionView>(session);
            Assign(selection, "viewCamera", camera, "game", game);
            UnitSelectionInput input = GetOrAdd<UnitSelectionInput>(session);
            Assign(input, "viewCamera", camera, "game", game, "selection", selection);
            UnitPreviewSpawner spawner = FindRoot(scene, "Unit Preview").GetComponent<UnitPreviewSpawner>();
            Assign(spawner, "game", game);
            var settings = new SerializedObject(spawner);
            settings.FindProperty("selectFirstForPreview").boolValue = false;
            settings.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save GameScene.");
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/Milestone1.setup.txt", "Simulation, selection input and obstacle Tilemap connected in GameScene.");
        }

        [MenuItem("GameRTS/Build Windows")]
        public static void BuildWindows()
        {
            Directory.CreateDirectory("Logs");
            UnityEngine.Object[] previousPreloads = PlayerSettings.GetPreloadedAssets();
            try
            {
                BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath }, target = BuildTarget.StandaloneWindows64,
                    locationPathName = "Builds/Windows/GameRTS.exe", options = BuildOptions.None
                });
                File.WriteAllText("Logs/Milestone1.build.txt", $"Result={report.summary.result};Errors={report.summary.totalErrors}");
                if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Windows build failed.");
            }
            finally { PlayerSettings.SetPreloadedAssets(previousPreloads); AssetDatabase.SaveAssets(); }
        }

        [MenuItem("GameRTS/Write replay verification baseline")]
        public static void WriteReplayBaseline()
        {
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/Milestone1.editor-replay.txt", DeterminismScenario.Run(), new UTF8Encoding(false));
        }

        private static GameObject FindRoot(Scene scene, string name, bool required = true)
        {
            foreach (GameObject root in scene.GetRootGameObjects()) if (root.name == name) return root;
            if (required) throw new InvalidOperationException("Missing scene root: " + name);
            return null;
        }
        private static T GetOrAdd<T>(GameObject obj) where T : Component => obj.GetComponent<T>() ?? obj.AddComponent<T>();
        private static void Assign(UnityEngine.Object obj, params object[] fields)
        {
            var settings = new SerializedObject(obj);
            for (int i = 0; i < fields.Length; i += 2)
                settings.FindProperty((string)fields[i]).objectReferenceValue = (UnityEngine.Object)fields[i + 1];
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Paint(Tilemap map, Tile tile, int x, int y, int width, int height)
        {
            for (int j = y; j < y + height; j++)
            for (int i = x; i < x + width; i++) map.SetTile(new Vector3Int(i, j, 0), tile);
        }
    }
}
