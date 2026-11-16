using GameRTS.Data;
using NUnit.Framework;
using UnityEditor;
namespace GameRTS.Tests
{
    public sealed class DefenseAssetTests
    {
        [TestCase("Wall",5,0,400,1)]
        [TestCase("Outpost",100,0,300,1)]
        [TestCase("WatchTower",125,0,600,2)]
        [TestCase("Landmine",50,30,1,1)]
        public void DefenseData(string name,int wood,int gold,int hp,int width)
        {
            var data=AssetDatabase.LoadAssetAtPath<BuildingDataSO>("Assets/Game/Data/Buildings/"+name+"Data.asset");Assert.That(data,Is.Not.Null);
            var def=data.CreateDefinition();Assert.That(def.WoodCost,Is.EqualTo(wood));Assert.That(def.GoldCost,Is.EqualTo(gold));
            Assert.That(def.MaxHitPoints,Is.EqualTo(hp));Assert.That(def.Width,Is.EqualTo(width));Assert.That(def.Height,Is.EqualTo(width));
            Assert.That(data.CompletedSprite,Is.Not.Null);Assert.That(data.ViewPrefab,Is.Not.Null);
            if(name=="Outpost") Assert.That(def.SightRadius,Is.EqualTo(15));
            if(name=="WatchTower") {Assert.That(def.TowerRange,Is.EqualTo(6));Assert.That(def.TowerDamage,Is.EqualTo(20));}
            if(name=="Landmine") Assert.That(def.BlocksMovement,Is.False);
        }
    }
}
