using GameRTS.Commands;

namespace GameRTS.Simulation
{
    // A shared, repeatable economy slice for both Editor and Windows replay checks.
    public static class TrainingScenario
    {
        public static SimulationWorld CreateWorld()
        {
            var w = new SimulationWorld(new GridMap(24, 20)); w.AddPlayer(1);
            int town = w.Map.Index(4, 5); w.AddDropOff(1, town, 2, 2, ResourceAcceptance.All);
            w.AddTrainingCenter(town + 1, new TrainingDefinition("villager", 3000, 450, 10, 10));
            var wood = w.AddResource("tree", ResourceType.Wood, 300, w.Map.Index(3, 3));
            var food = w.AddResource("bush", ResourceType.Food, 200, w.Map.Index(3, 8));
            var gold = w.AddResource("gold", ResourceType.Gold, 800, w.Map.Index(16, 6));
            w.RegisterBuildingDefinition(new BuildingDefinition("mining_camp", 2, 2, 100, 0, 0, 200, 400, 0, ResourceAcceptance.Gold));
            w.AddUnit("villager", 1, 3000, 450, 2500, 3500, 10, 10);
            w.AddUnit("villager", 1, 3000, 450, 14500, 6500, 10, 10);
            w.AddUnit("villager", 1, 3000, 450, 2500, 8500, 10, 10);
            w.Submit(new GatherCommand(1, 1, 1, new[] { 1 }, wood.Id));
            w.Submit(new GatherCommand(1, 1, 2, new[] { 3 }, food.Id));
            w.Submit(new BuildCommand(180, 1, 3, "mining_camp", w.Map.Index(18, 6), new[] { 2 }));
            w.Submit(new GatherCommand(500, 1, 4, new[] { 2 }, gold.Id));
            w.Submit(new TrainingCommand(1, 1, 5, town + 1, TrainingOperation.SetRally, wood.Cell));
            w.Submit(new TrainingCommand(10, 1, 6, town + 1, TrainingOperation.Enqueue));
            w.Submit(new TrainingCommand(11, 1, 7, town + 1, TrainingOperation.Enqueue));
            w.Submit(new TrainingCommand(12, 1, 8, town + 1, TrainingOperation.Enqueue));
            return w;
        }
    }
}
