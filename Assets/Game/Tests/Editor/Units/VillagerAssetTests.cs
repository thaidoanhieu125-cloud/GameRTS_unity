using GameRTS.Data;
using GameRTS.Simulation;
using GameRTS.View;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameRTS.Tests
{
    public sealed class VillagerAssetTests
    {
        [Test]
        public void VillagerDataHasGatheringSettingsAndCargoView()
        {
            UnitDataSO data = AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/Game/Data/Units/VillagerData.asset");
            Assert.That(data, Is.Not.Null); Assert.That(data.CanGather, Is.True);
            Assert.That(data.CarryCapacity, Is.EqualTo(10)); Assert.That(data.GatherIntervalTicks, Is.EqualTo(10));
            Assert.That(data.ViewPrefab.GetComponent<UnitView>(), Is.Not.Null);
            Assert.That(data.ViewPrefab.transform.Find("Cargo").GetComponent<TextMesh>(), Is.Not.Null);
            Assert.That(data.ViewPrefab.transform.Find("Cargo Marker").GetComponent<SpriteRenderer>(), Is.Not.Null);
            var soldier = AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/Game/Data/Units/SampleSoldierData.asset");
            Assert.That(soldier.CanGather, Is.False); Assert.That(soldier.CarryCapacity, Is.Zero);
        }

        [Test]
        public void TownHallAcceptsAllResourceTypesAndHasTwoByTwoFootprint()
        {
            var data = AssetDatabase.LoadAssetAtPath<DropOffDataSO>("Assets/Game/Data/Buildings/TownHallData.asset");
            Assert.That(data, Is.Not.Null); Assert.That(data.Width, Is.EqualTo(2)); Assert.That(data.Height, Is.EqualTo(2));
            Assert.That(data.Accepts, Is.EqualTo(ResourceAcceptance.All)); Assert.That(data.Sprite, Is.Not.Null);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/Buildings/TownHall.prefab");
            Assert.That(prefab.GetComponent<DropOffView>().Data, Is.SameAs(data));
        }
    }
}
