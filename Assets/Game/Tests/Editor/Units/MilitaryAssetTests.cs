using GameRTS.Data;
using GameRTS.Simulation;
using GameRTS.UI;
using GameRTS.View;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameRTS.Tests
{
    public sealed class MilitaryAssetTests
    {
        [TestCase("Barracks", "melee", 25, 50, 0, UnitClass.Spearman)]
        [TestCase("ArcheryRange", "archer", 25, 0, 45, UnitClass.Archer)]
        [TestCase("Stable", "cavalry", 0, 70, 60, UnitClass.Cavalry)]
        public void MilitaryBuildingHasCorrectCostsTrainingAndVisuals(string name, string unit, int wood, int food, int gold, UnitClass type)
        {
            var data = AssetDatabase.LoadAssetAtPath<BuildingDataSO>("Assets/Game/Data/Buildings/" + name + "Data.asset"); var building = data.CreateDefinition();
            Assert.That(building.WoodCost, Is.EqualTo(150)); Assert.That(building.FoodCost + building.GoldCost, Is.Zero);
            Assert.That(building.Width, Is.EqualTo(2)); Assert.That(building.Height, Is.EqualTo(2)); Assert.That((int)building.Accepts, Is.Zero);
            var training = building.Training; Assert.That(training.UnitTypeId, Is.EqualTo(unit)); Assert.That(training.Stats.Class, Is.EqualTo(type));
            Assert.That(new[] { training.WoodCost, training.FoodCost, training.GoldCost }, Is.EqualTo(new[] { wood, food, gold }));
            Assert.That(training.CarryCapacity, Is.Zero); Assert.That(training.QueueLimit, Is.EqualTo(5)); Assert.That(training.DurationTicks, Is.EqualTo(200));
            Assert.That(data.ViewPrefab.transform.Find("Selection Marker"), Is.Not.Null); Assert.That(data.ViewPrefab.transform.Find("Rally Flag"), Is.Not.Null);
        }
        [Test]
        public void HudHasThreeMilitaryBindingsAndEndOverlay()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/UI/Prefabs/EconomyHud.prefab"); var settings = new SerializedObject(prefab.GetComponent<ConstructionHudView>());
            foreach (string field in new[] { "barracksButton", "archeryRangeButton", "stableButton" }) Assert.That(settings.FindProperty(field).objectReferenceValue, Is.Not.Null);
            Assert.That(prefab.GetComponent<MatchHudView>(), Is.Not.Null); Assert.That(prefab.transform.Find("Match Result").gameObject.activeSelf, Is.False);
        }
    }
}
