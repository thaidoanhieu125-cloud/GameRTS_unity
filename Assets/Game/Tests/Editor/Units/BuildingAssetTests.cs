using GameRTS.Data;
using GameRTS.UI;
using GameRTS.View;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameRTS.Tests
{
    public sealed class BuildingAssetTests
    {
        [Test]
        public void HouseHasTrustedCostsFootprintWorkAndTwoVisualStages()
        {
            var data = AssetDatabase.LoadAssetAtPath<BuildingDataSO>("Assets/Game/Data/Buildings/HouseData.asset");
            Assert.That(data, Is.Not.Null); var definition = data.CreateDefinition();
            Assert.That(definition.Width, Is.EqualTo(2)); Assert.That(definition.Height, Is.EqualTo(2));
            Assert.That(definition.WoodCost, Is.EqualTo(50)); Assert.That(definition.RequiredWork, Is.EqualTo(200));
            Assert.That(data.FoundationSprite, Is.Not.Null); Assert.That(data.CompletedSprite, Is.Not.Null);
            Assert.That(data.FoundationSprite, Is.Not.SameAs(data.CompletedSprite));
            Assert.That(data.ViewPrefab.GetComponent<BuildingView>(), Is.Not.Null);
            Assert.That(data.ViewPrefab.transform.Find("Progress Fill"), Is.Not.Null);
        }
        [Test]
        public void HudBuildButtonAndVillagerHammerAreConnected()
        {
            var hud = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/UI/Prefabs/EconomyHud.prefab");
            Assert.That(hud.GetComponent<ConstructionHudView>(), Is.Not.Null);
            Assert.That(hud.transform.Find("Top Bar/Build House").GetComponent<Button>(), Is.Not.Null);
            var villager = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/Villager.prefab");
            Assert.That(villager.transform.Find("Hammer").gameObject.activeSelf, Is.False);
            var settings = new SerializedObject(villager.GetComponent<UnitView>());
            Assert.That(settings.FindProperty("hammer").objectReferenceValue, Is.Not.Null);
        }
    }
}
