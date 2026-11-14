using GameRTS.Data;
using GameRTS.View;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameRTS.Tests
{
    public sealed class CombatAssetTests
    {
        [TestCase("Melee", 120, 2, 12, 1, false)]
        [TestCase("Archer", 70, 0, 10, 5, true)]
        public void CombatDataAndHealthBarAreConnected(string name, int hp, int armor, int damage, int range, bool ranged)
        {
            var data = AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/Game/Data/Units/" + name + "Data.asset"); var stats = data.CreateCombatStats();
            Assert.That(stats.MaxHP, Is.EqualTo(hp)); Assert.That(stats.Armor, Is.EqualTo(armor)); Assert.That(stats.AttackDamage, Is.EqualTo(damage));
            Assert.That(stats.AttackRange, Is.EqualTo(range * 1000)); Assert.That(stats.IsRanged, Is.EqualTo(ranged));
            Assert.That(data.ViewPrefab.GetComponentInChildren<HealthBarView>(), Is.Not.Null);
        }
        [Test]
        public void ProjectilePrefabHasSpriteWithoutPhysics()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/Arrow.prefab");
            Assert.That(prefab.GetComponent<ProjectileView>(), Is.Not.Null); Assert.That(prefab.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
            Assert.That(prefab.GetComponent<Collider2D>(), Is.Null); Assert.That(prefab.GetComponent<Rigidbody2D>(), Is.Null);
        }
    }
}
