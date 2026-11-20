using System.Collections.Generic;
using System.Linq;
using GameRTS.Simulation;
using NUnit.Framework;

namespace GameRTS.Tests
{
    public sealed class MeadowMapTests
    {
        [Test]
        public void ExpandedLandKeepsBothBasesAndEveryLandRegionConnected()
        {
            GridMap map=MeadowMapLayout.CreateMap();
            Assert.That(map.Width,Is.EqualTo(128));Assert.That(map.Height,Is.EqualTo(192));
            Assert.That(MeadowMapLayout.LakeCount,Is.EqualTo(6));
            var reached=map.ReachableFrom(map.CellAt(0,0));
            for(int c=0;c<map.Count;c++) if(map.IsWalkable(c)) Assert.That(reached[c],Is.True,"Isolated land cell "+c);
            foreach(int player in new[]{1,2})
            {
                int town=AiScenario.TownCell(map,player);Assert.That(map.IsWalkable(town),Is.True);
                foreach(int c in AiScenario.VillagerCells(map,player)) Assert.That(reached[c],Is.True);
            }
        }
        [Test]
        public void LakesBlockPathsAndKeepAReachableDetour()
        {
            GridMap map=MeadowMapLayout.CreateMap();
            for(int i=0;i<MeadowMapLayout.LakeCount;i++)
            {
                var lake=MeadowMapLayout.GetLake(i);
                Assert.That(map.IsWalkable(map.CellAt(lake.X*1000,lake.Y*1000)),Is.False);
                int a=map.CellAt((lake.X-lake.RadiusX-3)*1000,lake.Y*1000);
                int b=map.CellAt((lake.X+lake.RadiusX+3)*1000,lake.Y*1000);
                if(!map.IsWalkable(a)||!map.IsWalkable(b))continue;
                var path=GridPathfinder.FindPath(map,a,b);Assert.That(path,Is.Not.Null);
                foreach(int c in path)Assert.That(map.IsWalkable(c),Is.True);
            }
        }
        [Test]
        public void ForestPlacementIsStableAndNeverOverlapsTerrainOrStartingResources()
        {
            GridMap map=MeadowMapLayout.CreateMap();var trees=MeadowMapLayout.Trees().ToArray();
            Assert.That(trees.Length,Is.GreaterThan(100));
            var keys=new HashSet<int>();
            foreach(var p in trees)
            { int c=map.CellAt(p.X*1000+500,p.Y*1000+500);Assert.That(map.IsWalkable(c),Is.True);Assert.That(keys.Add(c),Is.True); }
            foreach(var node in AiScenario.Resources(map))Assert.That(keys.Contains(node.Cell),Is.False);
            foreach(var node in MeadowMapLayout.CentralDeposits())
            { var p=node.Position;int c=map.CellAt(p.X*1000+500,p.Y*1000+500);Assert.That(map.IsWalkable(c),Is.True);Assert.That(keys.Add(c),Is.True); }
            var again=MeadowMapLayout.Trees().Select(p=>p.X+","+p.Y).ToArray();
            Assert.That(again,Is.EqualTo(trees.Select(p=>p.X+","+p.Y).ToArray()));
            foreach(int c in keys)map.SetWalkable(c%map.Width,c/map.Width,false);
            var reached=map.ReachableFrom(map.CellAt(0,0));
            for(int c=0;c<map.Count;c++)if(map.IsWalkable(c))Assert.That(reached[c],Is.True,"Forest sealed land at "+c);
        }
        [Test]
        public void NorthSouthBasesMirrorEachOtherWithoutChangingOldScenario()
        {
            var small=new GridMap(64,64,-32000,-32000);var large=MeadowMapLayout.CreateMap();
            Assert.That(AiScenario.TownCell(small,1),Is.EqualTo(small.Index(16,26)));
            Assert.That(AiScenario.TownCell(small,2),Is.EqualTo(small.Index(46,36)));
            int home=AiScenario.TownCell(large,1),enemy=AiScenario.TownCell(large,2);
            Assert.That(large.CenterY(home),Is.LessThan(0));Assert.That(large.CenterY(enemy),Is.GreaterThan(128000));
            Assert.That(home%large.Width+enemy%large.Width,Is.EqualTo(large.Width-2));
            Assert.That(home/large.Width+enemy/large.Width,Is.EqualTo(large.Height-2));
            foreach(var node in AiScenario.Resources(large))Assert.That(large.IsWalkable(node.Cell),Is.True);
        }
    }
}
