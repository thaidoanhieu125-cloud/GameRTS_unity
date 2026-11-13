using System;
using System.IO;
using GameRTS.Data;
using GameRTS.Input;
using GameRTS.Simulation;
using GameRTS.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameRTS.Editor
{
    public static class ResourceNodeTools
    {
        [MenuItem("GameRTS/Set up Resource Nodes")]
        public static void SetupResources()
        {
            const string path = "Assets/Game/Scenes/GameScene.unity";
            Scene scene = SceneManager.GetSceneByPath(path);
            if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(path);
            if (scene.isDirty) throw new InvalidOperationException("Save GameScene before setting up resource nodes.");
            EnsureFolder("Assets/Game/Art/Resources"); EnsureFolder("Assets/Game/Data/Resources");
            EnsureFolder("Assets/Game/View/Prefabs/Resources");
            var prefabs = new GameObject[3];
            string[] names = { "Tree", "BerryBush", "GoldMine" };
            string[] titles = { "Cây", "Bụi quả", "Mỏ vàng" };
            int[] amounts = { 100, 150, 800 };
            for (int i = 0; i < 3; i++)
            {
                Sprite sprite = CreateSprite(names[i], i);
                ResourceDataSO data = CreateData(names[i], titles[i], (ResourceType)i, amounts[i], sprite);
                prefabs[i] = CreatePrefab(names[i], data);
            }
            RtsGameView game = null; GameObject root = null;
            foreach (GameObject obj in scene.GetRootGameObjects())
            {
                if (game == null) game = obj.GetComponentInChildren<RtsGameView>(true);
                if (obj.name == "Resource Nodes") root = obj;
            }
            if (game == null) throw new InvalidOperationException("GameScene needs RTS Session.");
            if (root == null)
            {
                root = new GameObject("Resource Nodes"); SceneManager.MoveGameObjectToScene(root, scene);
                Place(prefabs[0], root.transform, -3.5f, -3.5f);
                Place(prefabs[0], root.transform, -2.5f, -3.5f);
                Place(prefabs[0], root.transform, -1.5f, -3.5f);
                Place(prefabs[0], root.transform, -5.5f, 2.5f);
                Place(prefabs[0], root.transform, -4.5f, 2.5f);
                Place(prefabs[1], root.transform, -7.5f, -.5f);
                Place(prefabs[1], root.transform, -6.5f, -.5f);
                Place(prefabs[2], root.transform, 3.5f, 4.5f);
                Place(prefabs[2], root.transform, 4.5f, 4.5f);
            }
            Assign(game, "resourceRoot", root.transform);
            ResourceTestInput test = game.GetComponent<ResourceTestInput>();
            if (test == null) test = game.gameObject.AddComponent<ResourceTestInput>();
            Assign(test, "game", game, "viewCamera", FindCamera(scene));
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save GameScene.");
            AssetDatabase.SaveAssets(); Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/Milestone2.resources-setup.txt", "3 data assets, sprites and prefabs; 9 map nodes and F8 test input connected.");
        }

        private static Camera FindCamera(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            { Camera camera = root.GetComponentInChildren<Camera>(true); if (camera != null) return camera; }
            throw new InvalidOperationException("GameScene needs a camera.");
        }

        private static void Place(GameObject prefab, Transform parent, float x, float y)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            obj.transform.position = new Vector3(x, y, 0); obj.name = prefab.name + " [" + x + ", " + y + "]";
        }

        private static ResourceDataSO CreateData(string name, string title, ResourceType type, int amount, Sprite sprite)
        {
            string path = "Assets/Game/Data/Resources/" + name + "Data.asset";
            ResourceDataSO data = AssetDatabase.LoadAssetAtPath<ResourceDataSO>(path);
            if (data != null) return data;
            data = ScriptableObject.CreateInstance<ResourceDataSO>();
            var serialized = new SerializedObject(data);
            serialized.FindProperty("resourceId").stringValue = name.ToLowerInvariant();
            serialized.FindProperty("displayName").stringValue = title;
            serialized.FindProperty("resourceType").enumValueIndex = (int)type;
            serialized.FindProperty("initialAmount").intValue = amount;
            serialized.FindProperty("sprite").objectReferenceValue = sprite;
            serialized.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.CreateAsset(data, path); return data;
        }

        private static GameObject CreatePrefab(string name, ResourceDataSO data)
        {
            string path = "Assets/Game/View/Prefabs/Resources/" + name + ".prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;
            var root = new GameObject(name, typeof(SpriteRenderer), typeof(ResourceNodeView));
            try
            {
                SpriteRenderer renderer = root.GetComponent<SpriteRenderer>(); renderer.sprite = data.Sprite;
                var soldier = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/SampleSoldier.prefab");
                renderer.sharedMaterial = soldier.GetComponent<SpriteRenderer>().sharedMaterial; renderer.sortingOrder = 0;
                var labelObject = new GameObject("Stock", typeof(TextMesh)); labelObject.transform.SetParent(root.transform, false);
                labelObject.transform.localPosition = new Vector3(0, -.36f, -.1f);
                TextMesh label = labelObject.GetComponent<TextMesh>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.fontSize = 48; label.characterSize = .08f; label.anchor = TextAnchor.MiddleCenter;
                label.alignment = TextAlignment.Center; label.text = data.InitialAmount.ToString(); label.color = Color.white;
                MeshRenderer mesh = label.GetComponent<MeshRenderer>(); mesh.sharedMaterial = label.font.material; mesh.sortingOrder = 4;
                Assign(root.GetComponent<ResourceNodeView>(), "data", data, "stockLabel", label);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static void Assign(UnityEngine.Object target, params object[] pairs)
        {
            var serialized = new SerializedObject(target);
            for (int i = 0; i < pairs.Length; i += 2)
                serialized.FindProperty((string)pairs[i]).objectReferenceValue = (UnityEngine.Object)pairs[i + 1];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/'); EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        // Original temporary world sprites generated from simple pixel shapes.
        private static Sprite CreateSprite(string name, int kind)
        {
            string path = "Assets/Game/Art/Resources/" + name + ".png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                texture.SetPixels(new Color[64 * 64]);
                Color dark = new Color(.10f, .23f, .12f), green = new Color(.21f, .46f, .17f), light = new Color(.40f, .64f, .23f);
                if (kind == 0)
                {
                    Rect(texture, 28, 5, 10, 32, new Color(.28f, .17f, .09f));
                    Rect(texture, 31, 7, 4, 29, new Color(.59f, .36f, .16f));
                    Disc(texture, 32, 34, 24, dark); Disc(texture, 20, 40, 16, dark); Disc(texture, 43, 42, 17, dark);
                    Disc(texture, 31, 38, 22, green); Disc(texture, 20, 43, 13, green); Disc(texture, 41, 44, 15, green);
                    Disc(texture, 28, 48, 12, light); Disc(texture, 17, 40, 7, light); Disc(texture, 43, 41, 7, light);
                }
                else if (kind == 1)
                {
                    Disc(texture, 20, 24, 16, dark); Disc(texture, 43, 24, 16, dark); Disc(texture, 31, 38, 18, dark);
                    Disc(texture, 20, 27, 14, green); Disc(texture, 43, 27, 14, green); Disc(texture, 31, 39, 15, light);
                    int[] fruitX = { 15, 26, 39, 48, 33, 21, 43 }, fruitY = { 31, 20, 26, 34, 42, 42, 17 };
                    for (int i = 0; i < fruitX.Length; i++)
                    {
                        Disc(texture, fruitX[i], fruitY[i], 4, new Color(.39f, .08f, .12f));
                        Disc(texture, fruitX[i] - 1, fruitY[i] + 1, 2, new Color(.94f, .28f, .28f));
                    }
                }
                else
                {
                    Disc(texture, 18, 23, 14, new Color(.22f, .25f, .25f));
                    Disc(texture, 41, 24, 18, new Color(.22f, .25f, .25f));
                    Disc(texture, 30, 39, 18, new Color(.22f, .25f, .25f));
                    Disc(texture, 18, 26, 11, new Color(.47f, .49f, .46f));
                    Disc(texture, 41, 27, 14, new Color(.38f, .41f, .40f));
                    Disc(texture, 29, 42, 14, new Color(.57f, .59f, .53f));
                    Rect(texture, 19, 40, 9, 8, new Color(.95f, .68f, .13f));
                    Rect(texture, 33, 25, 10, 8, new Color(1f, .81f, .26f));
                    Rect(texture, 13, 21, 7, 6, new Color(.91f, .58f, .08f));
                    Rect(texture, 37, 41, 6, 5, new Color(1f, .85f, .37f));
                }
                texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite; importer.spritePixelsPerUnit = 64;
                importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void Rect(Texture2D texture, int x, int y, int width, int height, Color color)
        { for (int iy = y; iy < y + height; iy++) for (int ix = x; ix < x + width; ix++) texture.SetPixel(ix, iy, color); }
        private static void Disc(Texture2D texture, int cx, int cy, int radius, Color color)
        {
            for (int y = Mathf.Max(0, cy - radius); y <= Mathf.Min(63, cy + radius); y++)
            for (int x = Mathf.Max(0, cx - radius); x <= Mathf.Min(63, cx + radius); x++)
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius) texture.SetPixel(x, y, color);
        }
    }
}
