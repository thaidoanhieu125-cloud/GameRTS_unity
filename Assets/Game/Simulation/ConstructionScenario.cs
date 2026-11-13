using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public static class ConstructionScenario
    {
        public static SimulationWorld CreateWorld()
        {
            var world = new SimulationWorld(new GridMap(20, 16)); world.AddPlayer(1, 500, 200, 100);
            world.RegisterBuildingDefinition(new BuildingDefinition("house", 2, 2, 50, 0, 0, 80, 300, 5));
            world.RegisterBuildingDefinition(new BuildingDefinition("store", 3, 3, 75, 0, 30, 100, 500, 0, ResourceAcceptance.All));
            world.AddUnit("villager", 1, 3000, 450, 2500, 5500, 10, 10);
            world.AddUnit("villager", 1, 3000, 450, 2500, 9500, 10, 10);
            world.AddUnit("soldier", 1, 3000, 450, 2500, 3500);
            int house = world.Map.Index(6, 5);
            world.Submit(new BuildCommand(1, 1, 1, "house", house, new[] { 1 }));
            world.Submit(new MoveCommand(35, 1, 2, new[] { 1 }, 1500, 1500));
            world.Submit(new ResumeBuildCommand(60, 1, 3, house + 1, new[] { 1 }));
            world.Submit(new MoveCommand(170, 1, 4, new[] { 3 }, 16500, 8500));
            world.Submit(new BuildCommand(200, 1, 5, "store", world.Map.Index(10, 5), new[] { 2 }));
            return world;
        }
    }
}
