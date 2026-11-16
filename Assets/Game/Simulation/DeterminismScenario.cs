using System;
using System.Globalization;
using System.Text;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    // Shared by EditMode tests and the Windows player: 100 units and the exact same commands.
    public static class DeterminismScenario
    {
        public static SimulationWorld CreateWorld(int count = 100)
        {
            var map = new GridMap(40, 40);
            for (int y = 4; y < 36; y++)
                if (y < 18 || y > 21)
                    for (int x = 19; x <= 20; x++) map.SetWalkable(x, y, false);
            var world = new SimulationWorld(map, true, true);
            world.AddPlayer(1, maxPopulation: Math.Max(200, count));
            for (int i = 0; i < count; i++)
                world.AddUnit("sample_soldier", 1, 3000, 450, (i % 10 + 2) * 1000 + 500, (i / 10 + 2) * 1000 + 500);
            return world;
        }

        public static string Run()
        {
            SimulationWorld world = CreateWorld();
            ResourceNodeState tree = world.AddResource("tree", ResourceType.Wood, 100, world.Map.Index(17, 18));
            ResourceNodeState food = world.AddResource("berrybush", ResourceType.Food, 150, world.Map.Index(18, 21));
            ResourceNodeState gold = world.AddResource("goldmine", ResourceType.Gold, 800, world.Map.Index(25, 23));
            var ids = new int[world.Units.Count];
            for (int i = 0; i < ids.Length; i++) ids[i] = i + 1;
            world.Submit(new MoveCommand(1, 1, 1, ids, 33500, 25500));
            world.Submit(new MoveCommand(201, 1, 2, ids, 6500, 30500));
            world.Submit(new MoveCommand(401, 1, 3, ids, 30500, 8500));
            world.Submit(new EconomyDebugCommand(10, 1, 4, EconomyDebugOperation.GrantResources, 50));
            world.Submit(new EconomyDebugCommand(50, 1, 5, EconomyDebugOperation.SpendResources, 50));
            world.Submit(new EconomyDebugCommand(100, 1, 6, EconomyDebugOperation.SpendResources, 1000));
            world.Submit(new EconomyDebugCommand(300, 1, 7, EconomyDebugOperation.IncreasePopulationLimit, 5));
            world.Submit(new ResourceDebugCommand(150, 1, 8, tree.Id, 50));
            world.Submit(new ResourceDebugCommand(175, 1, 9, tree.Id, 50));
            world.Submit(new ResourceDebugCommand(250, 1, 10, food.Id, 1000));
            world.Submit(new ResourceDebugCommand(350, 1, 11, gold.Id, 50));
            SimulationWorld workers = VillagerScenario.CreateWorld();
            SimulationWorld construction = ConstructionScenario.CreateWorld();
            SimulationWorld recovery = VillagerScenario.CreateRecoveryWorld();
            SimulationWorld camps = ConstructionScenario.CreateCampWorld();
            SimulationWorld training = TrainingScenario.CreateWorld();
            SimulationWorld combat = CombatScenario.CreateWorld();
            var military = MilitaryScenario.CreateWorld(); var defeat = MilitaryScenario.CreateWorld(true);
            var defense = DefenseScenario.CreateWorld();
            var progression = ProgressionScenario.CreateWorld(); var progressedAi = ProgressionScenario.CreateAiWorld(AiDifficulties.Create(AiDifficulty.Medium));
            var ai = AiScenario.CreateWorld(); var waves = AiScenario.CreateRepelledWorld();
            var levels = new[] { AiScenario.CreateWorld(AiDifficulties.Create(AiDifficulty.Easy)), AiScenario.CreateWorld(AiDifficulties.Create(AiDifficulty.Medium)), AiScenario.CreateWorld(AiDifficulties.Create(AiDifficulty.Hard)) };
            var report = new StringBuilder("RTS-REPLAY-16\nUnits=100;Villagers=3;Construction=2;Recovery=1;Camps=3;Training=3;Combat=4;Military=3;Victory=1;Defeat=1;Defenses=4;Hunting=3;Technologies=5;Age=2;AI=6;AiStepsPerSample=20;Ticks=600;Rate=20;Directions=8;Economy=1;Resources=3;SearchRadius=8\n");
            for (int i = 0; i < 600; i++)
            {
                defense.Step(); world.Step(); workers.Step(); construction.Step(); recovery.Step(); camps.Step(); training.Step(); combat.Step(); military.Step(); defeat.Step();
                for (int step = 0; step < 20; step++) { progression.Step(); progressedAi.Step(); ai.Step(); waves.Step(); foreach (var level in levels) level.Step(); }
                report.Append(world.Tick.ToString(CultureInfo.InvariantCulture)).Append(':')
                    .Append(world.ComputeChecksum().ToString("X16", CultureInfo.InvariantCulture)).Append('/')
                    .Append(workers.ComputeChecksum().ToString("X16", CultureInfo.InvariantCulture)).Append('/')
                    .Append(construction.ComputeChecksum().ToString("X16", CultureInfo.InvariantCulture)).Append('/')
                    .Append(recovery.ComputeChecksum().ToString("X16", CultureInfo.InvariantCulture)).Append('/')
                    .Append(camps.ComputeChecksum().ToString("X16", CultureInfo.InvariantCulture)).Append('/')
                    .Append(training.ComputeChecksum().ToString("X16", CultureInfo.InvariantCulture)).Append('/')
                    .Append(combat.ComputeChecksum().ToString("X16", CultureInfo.InvariantCulture)).Append('/')
                    .Append(military.ComputeChecksum().ToString("X16", CultureInfo.InvariantCulture)).Append('/')
                    .Append(defeat.ComputeChecksum().ToString("X16", CultureInfo.InvariantCulture)).Append('/')
                    .Append(ai.ComputeChecksum().ToString("X16", CultureInfo.InvariantCulture)).Append('/')
                    .Append(waves.ComputeChecksum().ToString("X16", CultureInfo.InvariantCulture));
                foreach (var level in levels) report.Append('/').Append(level.ComputeChecksum().ToString("X16", CultureInfo.InvariantCulture));
                report.Append('/').Append(defense.ComputeChecksum().ToString("X16", CultureInfo.InvariantCulture));
                report.Append('/').Append(progression.ComputeChecksum().ToString("X16", CultureInfo.InvariantCulture)).Append('/').Append(progressedAi.ComputeChecksum().ToString("X16", CultureInfo.InvariantCulture));
                report.Append('\n');
            }
            return report.ToString();
        }
    }
}
