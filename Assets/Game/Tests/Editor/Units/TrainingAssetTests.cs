using GameRTS.Data;
using GameRTS.UI;
using GameRTS.View;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameRTS.Tests
{
    public sealed class TrainingAssetTests
    {
        [Test]
        public void TownHallHasTrainingDefinitionSelectionAndRallyVisuals()
        {
            var town = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/Buildings/TownHall.prefab");
            var definition = town.GetComponent<DropOffView>().TrainingData.CreateDefinition();
            Assert.That(definition.UnitTypeId, Is.EqualTo("villager")); Assert.That(definition.FoodCost, Is.EqualTo(50));
            Assert.That(definition.DurationTicks, Is.EqualTo(200)); Assert.That(definition.QueueLimit, Is.EqualTo(5));
            Assert.That(town.transform.Find("Selection Marker").gameObject.activeSelf, Is.False);
            Assert.That(town.transform.Find("Rally Flag").GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
        }
        [Test]
        public void HudHasFiveCancelableSlotsAndRadialProgress()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/UI/Prefabs/EconomyHud.prefab");
            Assert.That(prefab.GetComponent<TrainingHudView>(), Is.Not.Null);
            Assert.That(prefab.GetComponentsInChildren<TrainingQueueSlotView>(true).Length, Is.EqualTo(5));
            var progress = prefab.transform.Find("Training Panel/Progress Ring").GetComponent<Image>();
            Assert.That(progress.type, Is.EqualTo(Image.Type.Filled)); Assert.That(progress.fillMethod, Is.EqualTo(Image.FillMethod.Radial360));
            Assert.That(progress.sprite, Is.Not.Null);
        }
    }
}
