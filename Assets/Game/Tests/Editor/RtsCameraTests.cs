using System.Reflection;
using GameRTS.Input;
using GameRTS.View;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Tilemaps;

namespace GameRTS.Tests
{
    public sealed class RtsCameraTests
    {
        private GameObject gridObject;
        private GameObject cameraObject;
        private Tilemap ground;
        private Tile tile;
        private Camera camera;
        private RtsCameraView view;

        [SetUp]
        public void SetUp()
        {
            gridObject = new GameObject("Test Grid", typeof(Grid));
            var groundObject = new GameObject("Test Ground", typeof(Tilemap));
            groundObject.transform.SetParent(gridObject.transform);
            ground = groundObject.GetComponent<Tilemap>();
            tile = ScriptableObject.CreateInstance<Tile>();
            SetMap(64, 64);
            cameraObject = new GameObject("Test Camera", typeof(Camera));
            camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.aspect = 16f / 9f;
            camera.orthographicSize = 10f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            view = cameraObject.AddComponent<RtsCameraView>();
            var settings = new SerializedObject(view);
            settings.FindProperty("ground").objectReferenceValue = ground;
            settings.ApplyModifiedPropertiesWithoutUndo();
            InvokeCameraMethod("Awake");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(cameraObject);
            Object.DestroyImmediate(gridObject);
            Object.DestroyImmediate(tile);
        }

        [TestCase(100f, 100f)]
        [TestCase(-100f, -100f)]
        public void PanKeepsEveryViewportCornerInsideMap(float x, float y)
        {
            camera.transform.position = new Vector3(x, y, -10f);
            InvokeCameraMethod("LateUpdate");
            AssertViewportInside(-32f, -32f, 32f, 32f);
            Assert.That(camera.transform.position.z, Is.EqualTo(-10f));
        }

        [Test]
        public void ScrollHasZoomLimitsAndDoesNotDependOnDeltaTime()
        {
            view.ApplyInput(Vector2.zero, 1f, 0f);
            InvokeCameraMethod("LateUpdate");
            Assert.That(camera.orthographicSize, Is.EqualTo(8.5f).Within(0.0001f));
            view.ApplyInput(Vector2.zero, 10000f, 0f);
            InvokeCameraMethod("LateUpdate");
            Assert.That(camera.orthographicSize, Is.EqualTo(4f).Within(0.0001f));
            view.ApplyInput(Vector2.zero, -10000f, 0f);
            InvokeCameraMethod("LateUpdate");
            Assert.That(camera.orthographicSize, Is.LessThanOrEqualTo(20f));
            AssertViewportInside(-32f, -32f, 32f, 32f);
        }

        [TestCase(0.5f)]
        [TestCase(3.5f)]
        [TestCase(10f)]
        public void ResizingViewportStillKeepsCameraInsideMap(float aspect)
        {
            camera.aspect = aspect;
            camera.orthographicSize = 20f;
            camera.transform.position = new Vector3(30f, 30f, -10f);
            InvokeCameraMethod("LateUpdate");
            AssertViewportInside(-32f, -32f, 32f, 32f);
        }

        [Test]
        public void SmallMapCanOverrideMinimumZoomToFitViewport()
        {
            SetMap(4, 2);
            InvokeCameraMethod("LateUpdate");
            AssertViewportInside(-2f, -1f, 2f, 1f);
            Assert.That(camera.orthographicSize, Is.GreaterThan(0f));
        }

        [Test]
        public void CombinedPanIsNormalizedAndUsesElapsedTime()
        {
            view.ApplyInput(new Vector2(2f, 2f), 0f, 0.5f);
            Assert.That(((Vector2)camera.transform.position).magnitude,
                Is.EqualTo(10f).Within(0.0001f));
        }

        [TestCase(Key.W, Key.D, 1f, 1f)]
        [TestCase(Key.DownArrow, Key.LeftArrow, -1f, -1f)]
        [TestCase(Key.A, Key.D, 0f, 0f)]
        public void KeyboardBindingsReadWASDArrowsAndOpposingKeys(Key first, Key second,
            float expectedX, float expectedY)
        {
            Keyboard previousKeyboard = Keyboard.current;
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            try
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(first, second));
                InputSystem.Update();
                var pan = (Vector2)typeof(RtsCameraInput)
                    .GetMethod("ReadKeyboardPan", BindingFlags.Static | BindingFlags.NonPublic)
                    .Invoke(null, new object[] { keyboard });
                Assert.That(pan, Is.EqualTo(new Vector2(expectedX, expectedY)));
            }
            finally
            {
                InputSystem.RemoveDevice(keyboard);
                previousKeyboard?.MakeCurrent();
            }
        }

        private void InvokeCameraMethod(string name)
        {
            // EditMode objects do not receive runtime lifecycle callbacks.
            typeof(RtsCameraView).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(view, null);
        }
        private void SetMap(int width, int height)
        {
            ground.ClearAllTiles();
            ground.SetTile(new Vector3Int(-width / 2, -height / 2, 0), tile);
            ground.SetTile(new Vector3Int(width / 2 - 1, height / 2 - 1, 0), tile);
            ground.CompressBounds();
        }

        private void AssertViewportInside(float xMin, float yMin, float xMax, float yMax)
        {
            Vector3 lower = camera.ViewportToWorldPoint(new Vector3(0f, 0f, 10f));
            Vector3 upper = camera.ViewportToWorldPoint(new Vector3(1f, 1f, 10f));
            Assert.That(lower.x, Is.GreaterThanOrEqualTo(xMin - 0.0001f));
            Assert.That(lower.y, Is.GreaterThanOrEqualTo(yMin - 0.0001f));
            Assert.That(upper.x, Is.LessThanOrEqualTo(xMax + 0.0001f));
            Assert.That(upper.y, Is.LessThanOrEqualTo(yMax + 0.0001f));
        }
    }
}

