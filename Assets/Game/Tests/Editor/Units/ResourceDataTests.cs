using GameRTS.Data;
using GameRTS.Simulation;
using GameRTS.View;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameRTS.Tests
{
    public sealed class ResourceDataTests
    {
        [TestCase("Tree", ResourceType.Wood, 100)]
        [TestCase("BerryBush", ResourceType.Food, 150)]
        [TestCase("GoldMine", ResourceType.Gold, 800)]
        public void ResourceAssetAndPrefabContainRequiredData(string name, ResourceType type, int stock)
        {
            var data = AssetDatabase.LoadAssetAtPath<ResourceDataSO>("Assets/Game/Data/Resources/" + name + "Data.asset");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/View/Prefabs/Resources/" + name + ".prefab");
            Assert.That(data, Is.Not.Null); Assert.That(data.Sprite, Is.Not.Null);
            Assert.That(data.ResourceType, Is.EqualTo(type)); Assert.That(data.InitialAmount, Is.EqualTo(stock));
            Assert.That(prefab.GetComponent<ResourceNodeView>().Data, Is.SameAs(data));
            Assert.That(prefab.GetComponent<SpriteRenderer>().sprite, Is.SameAs(data.Sprite));
            Assert.That(prefab.GetComponent<Collider2D>(), Is.Null);
            Assert.That(prefab.GetComponentInChildren<TextMesh>().text, Is.EqualTo(stock.ToString()));
        }
    }
}
