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
    public static class VillagerTools
    {
        [MenuItem("GameRTS/Set up Villager Economy")]
        public static void Setup()
        {
            const string path = "Assets/Game/Scenes/GameScene.unity";
            Scene scene = SceneManager.GetSceneByPath(path);
            if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(path);
            if (scene.isDirty) throw new InvalidOperationException("Save GameScene before setting up villagers.");
            Folder("Assets/Game/Art/Buildings"); Folder("Assets/Game/Data/Buildings"); Folder("Assets/Game/View/Prefabs/Buildings");
            Sprite villagerSprite = MakeSprite("Assets/Game/Art/Units/Villager.png", false);
            Sprite hallSprite = MakeSprite("Assets/Game/Art/Buildings/TownHall.png", true);
            UnitDataSO villager = MakeVillager(villagerSprite);
            GameObject hallPrefab = MakeTownHall(hallSprite);
            AddWorkerPanel();
            RtsGameView game = null; UnitPreviewSpawner spawner = null; GameObject buildings = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (game == null) game = root.GetComponentInChildren<RtsGameView>(true);
                if (spawner == null) spawner = root.GetComponentInChildren<UnitPreviewSpawner>(true);
                if (root.name == "Drop Off Buildings") buildings = root;
            }
            if (game == null || spawner == null) throw new InvalidOperationException("Scene needs RTS Session and Unit Preview.");
            if (buildings == null)
            {
                buildings = new GameObject("Drop Off Buildings"); SceneManager.MoveGameObjectToScene(buildings, scene);
                GameObject hall = (GameObject)PrefabUtility.InstantiatePrefab(hallPrefab, buildings.transform);
                hall.transform.position = new Vector3(2, -5, 0);
            }
            Assign(game, "dropOffRoot", buildings.transform);
            var serialized = new SerializedObject(spawner); SerializedProperty overrides = serialized.FindProperty("unitDataOverrides");
            if (overrides.arraySize == 0)
            {
                overrides.arraySize = 5;
                for (int i = 0; i < 5; i++) overrides.GetArrayElementAtIndex(i).objectReferenceValue = i < 3 ? villager : null;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save GameScene.");
            AssetDatabase.SaveAssets(); Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/Milestone2.villagers-setup.txt", "Villager data/prefab, 3 villager overrides, 2x2 Town Hall, worker status panel connected.");
        }

        private static UnitDataSO MakeVillager(Sprite sprite)
        {
            const string prefabPath = "Assets/Game/View/Prefabs/Villager.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
            {
                GameObject root = PrefabUtility.LoadPrefabContents("Assets/Game/View/Prefabs/SampleSoldier.prefab");
                try
                {
                    root.name = "Villager"; root.GetComponent<SpriteRenderer>().sprite = sprite;
                    TextMesh label = MakeLabel(root.transform, "Cargo", "0/10", new Vector3(0, -.64f, -.1f), .065f);
                    var bag = new GameObject("Cargo Marker", typeof(SpriteRenderer)); bag.transform.SetParent(root.transform, false);
                    bag.transform.localPosition = new Vector3(.23f, -.1f, 0); bag.transform.localScale = new Vector3(.28f, .25f, 1);
                    SpriteRenderer marker = bag.GetComponent<SpriteRenderer>();
                    marker.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Terrain/GroundTile.png");
                    marker.sharedMaterial = root.GetComponent<SpriteRenderer>().sharedMaterial; marker.sortingOrder = 11; marker.enabled = false;
                    Assign(root.GetComponent<UnitView>(), "cargoLabel", label, "cargoMarker", marker);
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            const string path = "Assets/Game/Data/Units/VillagerData.asset";
            UnitDataSO data = AssetDatabase.LoadAssetAtPath<UnitDataSO>(path);
            if (data != null) return data;
            data = ScriptableObject.CreateInstance<UnitDataSO>(); var serialized = new SerializedObject(data);
            serialized.FindProperty("unitId").stringValue = "villager"; serialized.FindProperty("displayName").stringValue = "Dân";
            serialized.FindProperty("canGather").boolValue = true; serialized.FindProperty("carryCapacity").intValue = 10;
            serialized.FindProperty("gatherIntervalTicks").intValue = 10;
            serialized.FindProperty("viewPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            serialized.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.CreateAsset(data, path); return data;
        }

        private static GameObject MakeTownHall(Sprite sprite)
        {
            const string dataPath = "Assets/Game/Data/Buildings/TownHallData.asset";
            DropOffDataSO data = AssetDatabase.LoadAssetAtPath<DropOffDataSO>(dataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<DropOffDataSO>(); Assign(data, "sprite", sprite);
                AssetDatabase.CreateAsset(data, dataPath);
            }
            const string path = "Assets/Game/View/Prefabs/Buildings/TownHall.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path); if (prefab != null) return prefab;
            var root = new GameObject("Town Hall", typeof(SpriteRenderer), typeof(DropOffView));
            try
            {
                root.GetComponent<SpriteRenderer>().sprite = data.Sprite;
                root.GetComponent<SpriteRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/SampleSoldier.prefab").GetComponent<SpriteRenderer>().sharedMaterial;
                MakeLabel(root.transform, "Name", "TÒA THỊ CHÍNH", new Vector3(0, -.8f, -.1f), .04f);
                Assign(root.GetComponent<DropOffView>(), "data", data);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static TextMesh MakeLabel(Transform parent, string name, string text, Vector3 position, float size)
        {
            var obj = new GameObject(name, typeof(TextMesh)); obj.transform.SetParent(parent, false); obj.transform.localPosition = position;
            TextMesh label = obj.GetComponent<TextMesh>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 48; label.characterSize = size; label.anchor = TextAnchor.MiddleCenter; label.text = text;
            label.GetComponent<MeshRenderer>().sharedMaterial = label.font.material; label.GetComponent<MeshRenderer>().sortingOrder = 12;
            return label;
        }

        private static void AddWorkerPanel()
        {
            const string path = "Assets/Game/UI/Prefabs/EconomyHud.prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (root.transform.Find("Worker Status") != null) return;
                var obj = new GameObject("Worker Status", typeof(RectTransform), typeof(Image)); obj.transform.SetParent(root.transform, false);
                RectTransform panel = obj.GetComponent<RectTransform>(); panel.anchorMin = panel.anchorMax = Vector2.one; panel.pivot = Vector2.one;
                panel.sizeDelta = new Vector2(410, 84); panel.anchoredPosition = new Vector2(-16, -100);
                obj.GetComponent<Image>().color = new Color(.065f, .105f, .12f, .96f);
                var textObject = new GameObject("Details", typeof(RectTransform), typeof(Text)); textObject.transform.SetParent(panel, false);
                RectTransform rect = textObject.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
                rect.offsetMin = new Vector2(14, 7); rect.offsetMax = new Vector2(-8, -7);
                Text label = textObject.GetComponent<Text>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.fontSize = 19; label.color = Color.white; label.alignment = TextAnchor.MiddleLeft; label.raycastTarget = false;
                Assign(root.GetComponent<EconomyHudView>(), "workerPanel", panel, "workerDetails", label);
                obj.SetActive(false); PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void Assign(UnityEngine.Object target, params object[] pairs)
        {
            var serialized = new SerializedObject(target);
            for (int i = 0; i < pairs.Length; i += 2) serialized.FindProperty((string)pairs[i]).objectReferenceValue = (UnityEngine.Object)pairs[i + 1];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/'); Folder(parent); AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static Sprite MakeSprite(string path, bool hall)
        {
            if (!File.Exists(path))
            {
                int size = hall ? 128 : 64;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false); texture.SetPixels(new Color[size * size]);
                for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
                {
                    Color color = Color.clear;
                    if (hall)
                    {
                        if (x >= 8 && x <= 119 && y >= 10 && y <= 112) color = new Color(.24f, .20f, .14f);
                        if (x >= 13 && x <= 114 && y >= 15 && y <= 106) color = new Color(.72f, .6f, .36f);
                        if (x >= 16 && x <= 111 && y >= 45 && y <= 108) color = (y % 12 < 2 || x % 16 == 0)
                            ? new Color(.35f, .18f, .12f) : new Color(.63f, .28f, .17f);
                        if (y >= 70 && y <= 76 && x >= 15 && x <= 112) color = new Color(.86f, .48f, .22f);
                        if (x >= 51 && x <= 76 && y >= 15 && y <= 42) color = new Color(.26f, .19f, .12f);
                        if ((x >= 23 && x <= 39 || x >= 88 && x <= 104) && y >= 22 && y <= 35) color = new Color(.26f, .44f, .43f);
                    }
                    else
                    {
                        int dx = x - 31, dy = y - 29;
                        if (dx * dx + dy * dy < 25 * 25) color = new Color(.18f, .16f, .12f);
                        if (dx * dx + dy * dy < 22 * 22) color = new Color(.66f, .40f, .20f);
                        if (Math.Abs(x - 31) < 12 && y >= 14 && y <= 35) color = new Color(.85f, .75f, .48f);
                        if ((x - 31) * (x - 31) + (y - 43) * (y - 43) < 12 * 12) color = new Color(.96f, .76f, .43f);
                        if (y >= 44 && y <= 49 && x >= 16 && x <= 46) color = new Color(.95f, .86f, .57f);
                        if (x >= 47 && x <= 50 && y >= 18 && y <= 42) color = new Color(.45f, .28f, .13f);
                        if (x >= 42 && x <= 57 && y >= 36 && y <= 40) color = new Color(.65f, .72f, .73f);
                    }
                    texture.SetPixel(x, y, color);
                }
                texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture); AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 64; importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
