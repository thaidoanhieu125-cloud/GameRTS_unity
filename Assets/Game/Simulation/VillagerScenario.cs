using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public static class VillagerScenario
    {
        public static SimulationWorld CreateWorld()
        {
            var world = new SimulationWorld(new GridMap(18, 14)); world.AddPlayer(1);
            var wood = world.AddResource("tree", ResourceType.Wood, 25, world.Map.Index(4, 4));
            var food = world.AddResource("bush", ResourceType.Food, 25, world.Map.Index(4, 8));
            var gold = world.AddResource("gold", ResourceType.Gold, 25, world.Map.Index(9, 4));
            world.AddDropOff(1, world.Map.Index(8, 7), 2, 2, ResourceAcceptance.All);
            world.AddUnit("villager", 1, 3000, 450, 3500, 4500, 10, 10);
            world.AddUnit("villager", 1, 3000, 450, 3500, 8500, 10, 10);
            world.AddUnit("villager", 1, 3000, 450, 9500, 3500, 10, 10);
            world.Submit(new GatherCommand(1, 1, 1, new[] { 1 }, wood.Id));
            world.Submit(new GatherCommand(1, 1, 2, new[] { 2 }, food.Id));
            world.Submit(new GatherCommand(1, 1, 3, new[] { 3 }, gold.Id));
            return world;
        }
    }
}
