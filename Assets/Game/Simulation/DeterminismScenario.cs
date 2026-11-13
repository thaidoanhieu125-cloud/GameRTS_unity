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
            var world = new SimulationWorld(map, true);
            world.AddPlayer(1, maxPopulation: Math.Max(200, count));
            for (int i = 0; i < count; i++)
                world.AddUnit("sample_soldier", 1, 3000, 450, (i % 10 + 2) * 1000 + 500, (i / 10 + 2) * 1000 + 500);
            return world;
        }

        public static string Run()
        {
            SimulationWorld world = CreateWorld();
            var ids = new int[world.Units.Count];
            for (int i = 0; i < ids.Length; i++) ids[i] = i + 1;
            world.Submit(new MoveCommand(1, 1, 1, ids, 33500, 25500));
            world.Submit(new MoveCommand(201, 1, 2, ids, 6500, 30500));
            world.Submit(new MoveCommand(401, 1, 3, ids, 30500, 8500));
            world.Submit(new EconomyDebugCommand(10, 1, 4, EconomyDebugOperation.GrantResources, 50));
            world.Submit(new EconomyDebugCommand(50, 1, 5, EconomyDebugOperation.SpendResources, 50));
            world.Submit(new EconomyDebugCommand(100, 1, 6, EconomyDebugOperation.SpendResources, 1000));
            world.Submit(new EconomyDebugCommand(300, 1, 7, EconomyDebugOperation.IncreasePopulationLimit, 5));
            var report = new StringBuilder("RTS-REPLAY-3\nUnits=100;Ticks=600;Rate=20;Directions=8;Economy=1\n");
            for (int i = 0; i < 600; i++)
            {
                world.Step();
                report.Append(world.Tick.ToString(CultureInfo.InvariantCulture)).Append(':')
                    .Append(world.ComputeChecksum().ToString("X16", CultureInfo.InvariantCulture)).Append('\n');
            }
            return report.ToString();
        }
    }
}
