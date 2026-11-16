using GameRTS.Commands;
namespace GameRTS.Simulation
{
    public static class DefenseScenario
    {
        public static SimulationWorld CreateWorld()
        {
            var w=new SimulationWorld(new GridMap(32,32));w.AddPlayer(1,1000,1000,1000,100);w.AddPlayer(2,1000,1000,1000,100);
            w.RegisterBuildingDefinition(new BuildingDefinition("wall",1,1,5,0,0,40,400));
            w.RegisterBuildingDefinition(new BuildingDefinition("outpost",1,1,100,0,0,100,300,sightRadius:15));
            w.RegisterBuildingDefinition(new BuildingDefinition("watch_tower",2,2,125,0,0,200,600,towerDamage:20));
            w.RegisterBuildingDefinition(new BuildingDefinition("landmine",1,1,50,0,30,60,1,sightRadius:0,isMine:true));
            for(int i=0;i<3;i++) w.AddUnit("villager",1,3000,450,3500,3500+i*5000,10);
            var trigger=w.AddUnit("enemy",2,3000,450,16500,8500,stats:new CombatStats(100));
            var archerTarget=w.AddUnit("enemy",2,3000,450,12500,13500,stats:new CombatStats(100));
            var attacker=w.AddUnit("enemy",2,3000,450,7500,3500,stats:new CombatStats(100,0,200));
            w.ConfigureVision();
            w.Submit(new BuildCommand(1,1,1,"wall",w.Map.Index(5,3),new[]{1}));
            w.Submit(new BuildCommand(1,1,2,"outpost",w.Map.Index(5,8),new[]{2}));
            w.Submit(new BuildCommand(1,1,3,"watch_tower",w.Map.Index(5,13),new[]{3}));
            w.Submit(new BuildCommand(180,1,4,"landmine",w.Map.Index(9,8),new[]{2}));
            w.Submit(new MoveCommand(300,2,1,new[]{trigger.Id},9500,8500));
            w.Submit(new MoveCommand(350,2,2,new[]{archerTarget.Id},7500,13500));
            w.Submit(new AttackCommand(450,2,3,new[]{attacker.Id},AttackTargetKind.Building,w.Map.Index(5,3)+1));
            return w;
        }
    }
}
