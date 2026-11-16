using GameRTS.Commands;
using GameRTS.Data;
using NUnit.Framework;
using UnityEditor;

namespace GameRTS.Tests
{
    public sealed class ProgressionAssetTests
    {
        [TestCase("Deer", 250, 32)] [TestCase("Boar", 400, 200)] [TestCase("Elephant", 600, 450)]
        public void AnimalAssets(string name, int food, int hp)
        {
            var d = AssetDatabase.LoadAssetAtPath<AnimalDataSO>("Assets/Game/Data/Animals/" + name + "Data.asset");
            Assert.That(d, Is.Not.Null); Assert.That(d.CreateDefinition().Food, Is.EqualTo(food)); Assert.That(d.CreateDefinition().Stats.MaxHP, Is.EqualTo(hp));
            Assert.That(d.AliveSprite, Is.Not.Null); Assert.That(d.CarcassSprite, Is.Not.Null); Assert.That(d.ViewPrefab, Is.Not.Null);
        }
        [TestCase(TechnologyId.DoubleAxe)] [TestCase(TechnologyId.LeatherBag)] [TestCase(TechnologyId.Fletching)]
        [TestCase(TechnologyId.ScaleArmor)] [TestCase(TechnologyId.BronzeAge)]
        public void TechnologyAssets(TechnologyId id)
        {
            var d = AssetDatabase.LoadAssetAtPath<TechnologyDataSO>("Assets/Game/Data/Technologies/" + id + ".asset");
            Assert.That(d, Is.Not.Null); var def = d.CreateDefinition(); Assert.That(def.Id, Is.EqualTo(id)); Assert.That(d.DisplayName, Is.Not.Empty);
            if (id == TechnologyId.BronzeAge) { Assert.That(def.Food, Is.EqualTo(500)); Assert.That(def.Duration, Is.EqualTo(1200)); }
        }
        [Test] public void BronzeTownHasDistinctSprite()
        {
            var d = AssetDatabase.LoadAssetAtPath<DropOffDataSO>("Assets/Game/Data/Buildings/TownHallData.asset");
            Assert.That(d.BronzeSprite, Is.Not.Null); Assert.That(d.BronzeSprite, Is.Not.SameAs(d.Sprite));
        }
    }
}
