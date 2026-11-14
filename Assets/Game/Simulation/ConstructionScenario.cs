using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public static class ConstructionScenario
    {
        public static SimulationWorld CreateCampWorld()
        {
            var world = new SimulationWorld(new GridMap(28, 20)); world.AddPlayer(1, 500, 200, 100);
            var wood = world.AddResource("tree", ResourceType.Wood, 25, world.Map.Index(4, 4));
            var food = world.AddResource("bush", ResourceType.Food, 25, world.Map.Index(4, 11));
            var gold = world.AddResource("gold", ResourceType.Gold, 25, world.Map.Index(16, 4));
            world.AddDropOff(1, world.Map.Index(17, 14), 2, 2, ResourceAcceptance.All);
            string[] ids = { "lumber_camp", "mill", "mining_camp" };
            int[] sites = { world.Map.Index(7, 4), world.Map.Index(7, 11), world.Map.Index(18, 4) };
            int[] sources = { wood.Id, food.Id, gold.Id };
            for (int i = 0; i < 3; i++)
            {
                world.RegisterBuildingDefinition(new BuildingDefinition(ids[i], 2, 2, 100, 0, 0, 80, 400, 0, (ResourceAcceptance)(1 << i)));
                world.AddUnit("villager", 1, 3000, 450, world.Map.CenterX(sites[i] - 1), world.Map.CenterY(sites[i] - 1), 10, 10);
                world.Submit(new BuildCommand(1, 1, i + 1, ids[i], sites[i], new[] { i + 1 }));
                world.Submit(new GatherCommand(100, 1, i + 4, new[] { i + 1 }, sources[i]));
            }
            return world;
        }
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
