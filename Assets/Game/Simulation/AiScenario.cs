using System.Collections.Generic;

namespace GameRTS.Simulation
{
    public static class AiScenario
    {
        public struct ResourcePlacement
        {
            public int Cell;
            public ResourceType Type;
            public int Amount;
            public string TypeId => Type == ResourceType.Wood ? "tree" : Type == ResourceType.Food ? "berrybush" : "goldmine";
        }
        public static int TownCell(GridMap map, int player) => player == 1 ? map.Index(16, 26) : map.Index(46, 36);
        public static int[] VillagerCells(GridMap map, int player)
        {
            int town = TownCell(map, player), x = town % map.Width, y = town / map.Width;
            return player == 1 ? new[] { map.Index(x - 2, y + 1), map.Index(x, y + 3), map.Index(x + 3, y + 1) }
                : new[] { map.Index(x + 3, y), map.Index(x + 1, y - 2), map.Index(x - 2, y) };
        }
        public static IEnumerable<ResourcePlacement> Resources(GridMap map)
        {
            for (int player = 1; player <= 2; player++)
            {
                int town = TownCell(map, player), x = town % map.Width, y = town / map.Width;
                int sign = player == 1 ? 1 : -1, offset = player == 1 ? 0 : 1;
                for (int xx = -7; xx <= -3; xx += 2) for (int yy = -4; yy <= 2; yy += 2)
                    yield return new ResourcePlacement { Cell = map.Index(x + offset + xx * sign, y + offset + yy * sign), Type = ResourceType.Wood, Amount = 100 };
                for (int xx = 3; xx <= 9; xx += 2) for (int yy = -5; yy <= -3; yy += 2)
                    yield return new ResourcePlacement { Cell = map.Index(x + offset + xx * sign, y + offset + yy * sign), Type = ResourceType.Food, Amount = 150 };
                for (int xx = 5; xx <= 7; xx += 2) for (int yy = 4; yy <= 6; yy += 2)
                    yield return new ResourcePlacement { Cell = map.Index(x + offset + xx * sign, y + offset + yy * sign), Type = ResourceType.Gold, Amount = 800 };
            }
        }
        public static SimulationWorld CreateWorld(AiSettings settings = null)
        {
            var w = new SimulationWorld(new GridMap(64, 64, -32000, -32000)); w.AddPlayer(1); w.AddPlayer(2);
            foreach (var node in Resources(w.Map)) w.AddResource(node.TypeId, node.Type, node.Amount, node.Cell);
            var villager = new CombatStats(40, 0, 3, 30, unitClass: UnitClass.Villager);
            var workerTraining = new TrainingDefinition("villager", 3000, 450, 10, 10, stats: villager);
            int first = w.AddDropOff(1, TownCell(w.Map, 1), 2, 2, ResourceAcceptance.All).Id;
            int second = w.AddDropOff(2, TownCell(w.Map, 2), 2, 2, ResourceAcceptance.All).Id;
            w.AddTrainingCenter(first, workerTraining); w.AddTrainingCenter(second, workerTraining);
            w.RegisterBuildingDefinition(new BuildingDefinition("house", 2, 2, 50, 0, 0, 200, 300, 5));
            w.RegisterBuildingDefinition(new BuildingDefinition("lumber_camp", 2, 2, 100, 0, 0, 200, 400, accepts: ResourceAcceptance.Wood));
            w.RegisterBuildingDefinition(new BuildingDefinition("mining_camp", 2, 2, 100, 0, 0, 200, 400, accepts: ResourceAcceptance.Gold));
            var stats = new[] { new CombatStats(120, 2, 12, aggroRange: 6000, unitClass: UnitClass.Spearman),
                new CombatStats(45, 0, 18, 12, 6000, 10000, 6000, UnitClass.Archer), new CombatStats(180, 1, 16, aggroRange: 6000, unitClass: UnitClass.Cavalry) };
            string[] types = { "barracks", "archery_range", "stable" }, unitTypes = { "melee", "archer", "cavalry" };
            for (int i = 0; i < 3; i++)
            {
                var train = new TrainingDefinition(unitTypes[i], i == 0 ? 2000 : i == 1 ? 3000 : 6000, 450, 0, 10,
                    i == 0 ? 50 : i == 1 ? 0 : 70, stats: stats[i], woodCost: i == 2 ? 0 : 25, goldCost: i == 0 ? 0 : i == 1 ? 45 : 60);
                w.RegisterBuildingDefinition(new BuildingDefinition(types[i], 2, 2, 150, 0, 0, 200, 600, armor: 2, training: train));
            }
            for (int player = 1; player <= 2; player++) foreach (int cell in VillagerCells(w.Map, player))
                w.AddUnit("villager", player, 3000, 450, w.Map.CenterX(cell), w.Map.CenterY(cell), 10, stats: villager);
            w.ConfigureTownCenterVictory(first, second); w.ConfigureVision(); w.AddAiController(2, second, settings); return w;
        }

        // Replay/test fixture only: a strong stationary defender repels waves without altering gameplay assets.
        public static SimulationWorld CreateRepelledWorld()
        {
            var world = CreateWorld(); int cell = world.Map.Index(18, 25);
            world.AddUnit("test_defender", 1, 0, 450, world.Map.CenterX(cell), world.Map.CenterY(cell),
                stats: new CombatStats(100000, 0, 100, 10, 8000, 20000, 8000));
            return world;
        }
    }
}
