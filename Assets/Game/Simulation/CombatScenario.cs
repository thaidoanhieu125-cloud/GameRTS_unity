using System;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public static class CombatScenario
    {
        public static SimulationWorld CreateWorld(bool reverseCommands = false)
        {
            var w = new SimulationWorld(new GridMap(24, 20)); w.AddPlayer(1); w.AddPlayer(2);
            w.AddDropOff(1, w.Map.Index(2, 2), 2, 2, ResourceAcceptance.All);
            var town = w.AddDropOff(2, w.Map.Index(17, 3), 2, 2, ResourceAcceptance.All, 100, 2);
            for (int y = 2; y < 12; y++) if (y != 8) w.Map.SetWalkable(10, y, false);
            w.AddUnit("melee", 1, 3000, 450, 3500, 6500, stats: new CombatStats(120, 2, 12));
            w.AddUnit("archer", 1, 3000, 450, 3500, 8500, stats: new CombatStats(70, 0, 10, 24, 5000, 8000));
            w.AddUnit("melee", 2, 3000, 450, 12500, 6500, stats: new CombatStats(60, 0, 4));
            w.AddUnit("archer", 2, 3000, 450, 14500, 8500, stats: new CombatStats(40, 0, 4, 30, 5000, 6000));
            var commands = new[] { new AttackCommand(1, 1, 1, new[] { 1 }, AttackTargetKind.Unit, 3),
                new AttackCommand(1, 1, 2, new[] { 2 }, AttackTargetKind.Unit, 4),
                new AttackCommand(350, 1, 3, new[] { 1, 2 }, AttackTargetKind.Building, town.Id) };
            if (reverseCommands) Array.Reverse(commands); foreach (var command in commands) w.Submit(command); return w;
        }
    }
}
