using System;
using System.Collections.Generic;
using System.Reflection;
using GameRTS.Data;
using GameRTS.View;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace GameRTS.Tests
{
    public sealed class UnitPreviewTests
    {
        private const string DataPath = "Assets/Game/Data/Units/SampleSoldierData.asset";
        private GameObject root;

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void SamplePrefabHasVisibleBodyAndInitiallyHiddenSelectionCircle()
        {
            UnitDataSO data = LoadSample();
            Assert.That(data.ViewPrefab.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
            Transform circle = data.ViewPrefab.transform.Find("SelectionCircle");
            Assert.That(circle, Is.Not.Null);
            Assert.That(circle.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
            Assert.That(circle.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void SelectionCanBeToggledAndReinitializationClearsIt()
        {
            UnitDataSO data = LoadSample();
            root = Object.Instantiate(data.ViewPrefab);
            UnitView unit = root.GetComponent<UnitView>();
            unit.Initialize(data, 1);
            unit.SetSelected(true);
            Assert.That(unit.IsSelected, Is.True);
            unit.SetSelected(false);
            Assert.That(unit.IsSelected, Is.False);
            unit.SetSelected(true);
            unit.Initialize(data, 2);
            Assert.That(unit.InstanceId, Is.EqualTo(2));
            Assert.That(unit.IsSelected, Is.False);
            Assert.That(data.ViewPrefab.transform.Find("SelectionCircle").gameObject.activeSelf, Is.False);
        }

        [Test]
        public void PreviewSpawnsFiveUniqueInstancesSharingOneDefinition()
        {
            UnitDataSO data = LoadSample();
            root = new GameObject("Test Spawner");
            var spawner = root.AddComponent<UnitPreviewSpawner>();
            var settings = new SerializedObject(spawner);
            settings.FindProperty("unitData").objectReferenceValue = data;
            settings.ApplyModifiedPropertiesWithoutUndo();
            typeof(UnitPreviewSpawner).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(spawner, null);
            UnitView[] units = root.GetComponentsInChildren<UnitView>();
            Assert.That(units.Length, Is.EqualTo(5));
            var ids = new HashSet<int>();
            for (int i = 0; i < units.Length; i++)
            {
                Assert.That(units[i].Data, Is.SameAs(data));
                Assert.That(units[i].InstanceId, Is.EqualTo(i + 1));
                Assert.That(units[i].IsSelected, Is.EqualTo(i == 0));
                ids.Add(units[i].InstanceId);
            }
            Assert.That(ids.Count, Is.EqualTo(5));
        }

        [Test]
        public void InvalidInitializationKeepsExistingBinding()
        {
            UnitDataSO data = LoadSample();
            root = Object.Instantiate(data.ViewPrefab);
            UnitView unit = root.GetComponent<UnitView>();
            unit.Initialize(data, 7);
            Assert.Throws<ArgumentNullException>(() => unit.Initialize(null, 8));
            Assert.Throws<ArgumentOutOfRangeException>(() => unit.Initialize(data, 0));
            Assert.That(unit.InstanceId, Is.EqualTo(7));
            Assert.That(unit.Data, Is.SameAs(data));
        }

        [Test]
        public void MissingDataDoesNotCreatePartialPreview()
        {
            root = new GameObject("Invalid Spawner");
            var spawner = root.AddComponent<UnitPreviewSpawner>();
            LogAssert.Expect(LogType.Error, "Unit preview needs Unit Data with an ID and a view prefab.");
            typeof(UnitPreviewSpawner).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(spawner, null);
            Assert.That(root.transform.childCount, Is.Zero);
        }

        [Test]
        public void DataValidationTrimsIdAndClampsNegativeSettings()
        {
            UnitDataSO data = ScriptableObject.CreateInstance<UnitDataSO>();
            try
            {
                var settings = new SerializedObject(data);
                settings.FindProperty("unitId").stringValue = " sample_soldier ";
                settings.FindProperty("moveSpeed").floatValue = -3f;
                settings.FindProperty("collisionRadius").floatValue = -1f;
                settings.ApplyModifiedPropertiesWithoutUndo();
                typeof(UnitDataSO).GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(data, null);
                Assert.That(data.UnitId, Is.EqualTo("sample_soldier"));
                Assert.That(data.MoveSpeed, Is.Zero);
                Assert.That(data.CollisionRadius, Is.EqualTo(0.01f));
            }
            finally
            {
                Object.DestroyImmediate(data);
            }
        }

        private static UnitDataSO LoadSample()
        {
            UnitDataSO data = AssetDatabase.LoadAssetAtPath<UnitDataSO>(DataPath);
            Assert.That(data, Is.Not.Null);
            Assert.That(data.ViewPrefab, Is.Not.Null);
            return data;
        }
    }
}
