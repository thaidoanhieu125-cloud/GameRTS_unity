using System.Linq;
using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;
namespace GameRTS.Tests
{
    public sealed class DefenseTests
    {
        private static SimulationWorld World(string type,int work=2,bool vision=false)
        {
            var w=new SimulationWorld(new GridMap(40,24)); w.AddPlayer(1,1000,1000,1000,100); w.AddPlayer(2,1000,1000,1000,100);
            w.AddUnit("builder",1,3000,450,2500,2500,10);
            w.RegisterBuildingDefinition(new BuildingDefinition(type,type=="tower"?2:1,type=="tower"?2:1,type=="wall"?5:type=="outpost"?100:type=="tower"?125:50,0,type=="mine"?30:0,work,type=="wall"?400:type=="tower"?600:300,
                sightRadius:type=="outpost"?15:type=="mine"?0:-1,towerDamage:type=="tower"?20:0,isMine:type=="mine"));
            if(vision) w.ConfigureVision(); return w;
        }
        private static UnitState Enemy(SimulationWorld w,int x,int y,int damage=0,int armor=0,int hp=100)
            => w.AddUnit("enemy",2,3000,450,x*1000+500,y*1000+500,stats:new CombatStats(hp,armor,damage));
        private static BuildingState Build(SimulationWorld w,string type)
        {
            w.Submit(new BuildCommand(1,1,1,type,w.Map.Index(4,2),new[]{1})); w.Step();
            return w.GetBuilding(w.LastPlacedBuildingId);
        }
        private static void Step(SimulationWorld w,int ticks) {for(int i=0;i<ticks;i++) w.Step();}
        [Test]
        public void ThreeFogStates()
        {
            var w=new SimulationWorld(new GridMap(40,24));w.AddPlayer(1);var scout=w.AddUnit("scout",1,3000,450,2500,2500);w.ConfigureVision();
            Assert.That(w.GetFogState(1,w.Map.Index(10,2)),Is.EqualTo(FogState.Visible));
            Assert.That(w.GetFogState(1,w.Map.Index(11,2)),Is.EqualTo(FogState.Unexplored));
            w.Submit(new MoveCommand(1,1,1,new[]{scout.Id},20500,2500));Step(w,200);
            Assert.That(w.GetFogState(1,w.Map.Index(2,2)),Is.EqualTo(FogState.Explored));
            Assert.That(w.GetFogState(1,scout.Cell),Is.EqualTo(FogState.Visible));
        }
        [Test]
        public void OutpostSeesFifteen()
        {
            var w=World("outpost",100,true);var b=Build(w,"outpost");
            Assert.That(w.IsCellVisible(1,w.Map.Index(19,2)),Is.False);Step(w,140);Assert.That(b.IsComplete,Is.True);
            Assert.That(w.IsCellVisible(1,w.Map.Index(19,2)),Is.True);Assert.That(w.IsCellVisible(1,w.Map.Index(20,2)),Is.False);
        }
        [Test]
        public void WallBlocksAndOpens()
        {
            var w=World("wall");var attacker=Enemy(w,5,2,1000);var b=Build(w,"wall");Step(w,60);
            Assert.That(w.GetPlayerEconomy(1).Wood,Is.EqualTo(995));Assert.That(b.HitPoints,Is.EqualTo(400));
            Assert.That(w.Map.IsWalkable(b.Cell),Is.False);
            CollectionAssert.DoesNotContain(GridPathfinder.FindPath(w.Map,w.Map.Index(3,2),w.Map.Index(5,2)),b.Cell);
            w.Submit(new AttackCommand(w.Tick+1,2,1,new[]{attacker.Id},AttackTargetKind.Building,b.Id));Step(w,2);
            Assert.That(b.IsDestroyed,Is.True);Assert.That(w.Map.IsWalkable(b.Cell),Is.True);
        }
        [Test]
        public void TowerUsesFlyingArrows()
        {
            var w=World("tower",2,true);var enemy=Enemy(w,9,2);var far=Enemy(w,12,2);var b=Build(w,"tower");
            for(int i=0;i<100 && w.ProjectileCount==0;i++) w.Step();
            Assert.That(b.IsComplete,Is.True);Assert.That(w.ProjectileCount,Is.EqualTo(1));
            Assert.That(w.Projectiles.Single().SourceId,Is.EqualTo(-b.Id));Assert.That(enemy.CurrentHP,Is.EqualTo(100));
            Step(w,110);Assert.That(enemy.IsAlive,Is.False);Assert.That(far.CurrentHP,Is.EqualTo(100));
        }
        [Test]
        public void FoundationCannotShoot()
        {
            var w=World("tower",200);var enemy=Enemy(w,9,2);var b=Build(w,"tower");Step(w,40);
            Assert.That(b.IsComplete,Is.False);Assert.That(w.ProjectileCount,Is.Zero);Assert.That(enemy.CurrentHP,Is.EqualTo(100));
        }
        [Test]
        public void MineIsHiddenAndPassable()
        {
            var w=World("mine",2,true);var enemy=Enemy(w,8,2);var b=Build(w,"mine");
            Assert.That(w.CanObserveBuilding(2,b.Id),Is.True);Step(w,60);
            Assert.That(w.CanObserveBuilding(1,b.Id),Is.True);Assert.That(w.CanObserveBuilding(2,b.Id),Is.False);
            Assert.That(w.Map.IsWalkable(b.Cell),Is.True);
            w.Submit(new AttackCommand(w.Tick+1,2,1,new[]{enemy.Id},AttackTargetKind.Building,b.Id));w.Step();Assert.That(enemy.TargetId,Is.Zero);
            Assert.That(w.CheckBuildingPlacement(1,"mine",b.Cell,new[]{1}),Is.EqualTo(PlacementIssue.Occupied));
        }
        [Test]
        public void ArmingCancelsEnemyAttack()
        {
            var w=World("mine",200);var enemy=Enemy(w,5,2,1);var b=Build(w,"mine");Step(w,120);
            w.Submit(new AttackCommand(w.Tick+1,2,1,new[]{enemy.Id},AttackTargetKind.Building,b.Id));Step(w,140);
            Assert.That(b.IsComplete,Is.True);Assert.That(b.IsDestroyed,Is.False);Assert.That(enemy.TargetId,Is.Zero);
        }
        [Test]
        public void MineHitsEnemiesOnce()
        {
            var w=World("mine");var trigger=Enemy(w,8,2,armor:100);var near=Enemy(w,4,3);var far=Enemy(w,6,4);
            var ally=w.AddUnit("friend",1,3000,450,3500,3500,stats:new CombatStats(100));var b=Build(w,"mine");Step(w,60);
            w.Submit(new MoveCommand(w.Tick+1,2,1,new[]{trigger.Id},4500,2500));
            for(int i=0;i<100 && !b.IsDestroyed;i++) w.Step();
            Assert.That(b.IsDestroyed,Is.True);Assert.That(trigger.CurrentHP,Is.EqualTo(40));Assert.That(near.CurrentHP,Is.EqualTo(40));
            Assert.That(ally.CurrentHP,Is.EqualTo(100));Assert.That(w.GetUnit(1).CurrentHP,Is.EqualTo(40));Assert.That(far.CurrentHP,Is.EqualTo(100));
            Assert.That(w.Explosions.Count,Is.EqualTo(1));Step(w,30);Assert.That(trigger.CurrentHP,Is.EqualTo(40));Assert.That(w.Explosions.Count,Is.Zero);
            Assert.That(w.GetPlayerEconomy(1).Wood,Is.EqualTo(950));Assert.That(w.GetPlayerEconomy(1).Gold,Is.EqualTo(970));
        }
        [Test]
        public void MineNeedsCompletion()
        {
            var w=World("mine",200);var enemy=Enemy(w,5,2);var b=Build(w,"mine");
            w.Submit(new MoveCommand(2,2,1,new[]{enemy.Id},4500,2500));Step(w,60);
            Assert.That(b.IsComplete,Is.False);Assert.That(enemy.CurrentHP,Is.EqualTo(100));Step(w,220);
            Assert.That(b.IsDestroyed,Is.True);Assert.That(enemy.CurrentHP,Is.EqualTo(40));
        }
        [Test]
        public void DestroyedOutpostLeavesExploredGround()
        {
            var w=World("outpost",2,true);var enemy=Enemy(w,5,2,10000);var b=Build(w,"outpost");Step(w,60);
            Assert.That(w.IsCellVisible(1,w.Map.Index(19,2)),Is.True);
            w.Submit(new AttackCommand(w.Tick+1,2,1,new[]{enemy.Id},AttackTargetKind.Building,b.Id));Step(w,2);
            Assert.That(b.IsDestroyed,Is.True);Assert.That(w.GetFogState(1,w.Map.Index(19,2)),Is.EqualTo(FogState.Explored));
        }
        [Test]
        public void MineDeathReleasesPopulation()
        {
            var w=World("mine");var enemy=Enemy(w,8,2,hp:40);var b=Build(w,"mine");Step(w,60);
            w.Submit(new MoveCommand(w.Tick+1,2,1,new[]{enemy.Id},4500,2500));Step(w,100);
            Assert.That(enemy.IsAlive,Is.False);Assert.That(w.GetPlayerEconomy(2).Population,Is.Zero);
            Assert.That(w.Map.IsWalkable(b.Cell),Is.True);
        }
        [Test]
        public void ReplayCoversDefenses()
        {
            var w=DefenseScenario.CreateWorld();Step(w,600);
            Assert.That(w.BuildingCount,Is.EqualTo(4));
            Assert.That(w.Buildings.Single(b=>b.Definition.TypeId=="wall").IsDestroyed,Is.True);
            Assert.That(w.Buildings.Single(b=>b.Definition.TypeId=="landmine").IsDestroyed,Is.True);
            Assert.That(w.Buildings.Single(b=>b.Definition.TypeId=="outpost").IsComplete,Is.True);
            Assert.That(w.Buildings.Single(b=>b.Definition.TypeId=="watch_tower").IsComplete,Is.True);
            Assert.That(w.GetUnit(4).CurrentHP,Is.EqualTo(40));Assert.That(w.GetUnit(5).IsAlive,Is.False);
            Assert.That(w.GetPlayerEconomy(1).Wood,Is.EqualTo(720));Assert.That(w.GetPlayerEconomy(1).Gold,Is.EqualTo(970));
        }
        [Test]
        public void DefensesAreDeterministic()
        {
            var a=World("tower",2,true);var b=World("tower",2,true);Enemy(a,9,2);Enemy(b,9,2);Build(a,"tower");Build(b,"tower");
            for(int i=0;i<160;i++){a.Step();b.Step();Assert.That(a.ComputeChecksum(),Is.EqualTo(b.ComputeChecksum()));}
        }
    }
}

