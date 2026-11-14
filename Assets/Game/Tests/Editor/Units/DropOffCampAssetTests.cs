using GameRTS.Data;
using GameRTS.Simulation;
using GameRTS.UI;
using GameRTS.View;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameRTS.Tests
{
    public sealed class DropOffCampAssetTests
    {
        [TestCase("LumberCamp", "lumber_camp", ResourceAcceptance.Wood)]
        [TestCase("Mill", "mill", ResourceAcceptance.Food)]
        [TestCase("MiningCamp", "mining_camp", ResourceAcceptance.Gold)]
        public void CampAssetsHaveCorrectCostsAndFilters(string name, string id, ResourceAcceptance accepts)
        {
            var data = AssetDatabase.LoadAssetAtPath<BuildingDataSO>("Assets/Game/Data/Buildings/" + name + "Data.asset");
            Assert.That(data, Is.Not.Null); var definition = data.CreateDefinition();
            Assert.That(definition.TypeId, Is.EqualTo(id)); Assert.That(definition.Width, Is.EqualTo(2)); Assert.That(definition.Height, Is.EqualTo(2));
            Assert.That(definition.WoodCost, Is.EqualTo(100)); Assert.That(definition.FoodCost + definition.GoldCost, Is.Zero);
            Assert.That(definition.Accepts, Is.EqualTo(accepts)); Assert.That(definition.PopulationBonus, Is.Zero);
            Assert.That(definition.RequiredWork, Is.EqualTo(200)); Assert.That(definition.MaxHitPoints, Is.EqualTo(400));
            Assert.That(data.CompletedSprite, Is.Not.Null); Assert.That(data.FoundationSprite, Is.Not.Null);
            Assert.That(data.CompletedSprite, Is.Not.SameAs(data.FoundationSprite));
            Assert.That(data.ViewPrefab.GetComponent<BuildingView>(), Is.Not.Null);
        }
        [Test]
        public void HudButtonsHaveAllFourSerializedBindings()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/UI/Prefabs/EconomyHud.prefab");
            var settings = new SerializedObject(prefab.GetComponent<ConstructionHudView>());
            string[] fields = { "buildButton", "lumberCampButton", "millButton", "miningCampButton" };
            foreach (string field in fields)
            {
                var button = settings.FindProperty(field).objectReferenceValue as Button;
                Assert.That(button, Is.Not.Null, field); Assert.That(button.gameObject.activeSelf, Is.False);
                Assert.That(button.GetComponentInChildren<Text>(true).text, Does.Contain("Gỗ"));
            }
        }
    }
}
