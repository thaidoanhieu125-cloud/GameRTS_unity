using System;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public static class MilitaryScenario
    {
        public static SimulationWorld CreateWorld(bool defeat = false, bool reverse = false)
        {
            var w = new SimulationWorld(new GridMap(24, 20)); w.AddPlayer(1, 500, 500, 500, 20); w.AddPlayer(2);
            int own = w.AddDropOff(1, w.Map.Index(2, 2), 2, 2, ResourceAcceptance.All, 60).Id;
            int enemy = w.AddDropOff(2, w.Map.Index(18, 2), 2, 2, ResourceAcceptance.All, 60).Id;
            w.ConfigureTownCenterVictory(own, enemy);
            var spear = new CombatStats(120, 2, 12, unitClass: UnitClass.Spearman);
            var archer = new CombatStats(45, 0, 18, 12, 6000, 10000, unitClass: UnitClass.Archer);
            var horse = new CombatStats(180, 1, 16, unitClass: UnitClass.Cavalry);
            string[] names = { "barracks", "archery_range", "stable" }; int[] sites = { w.Map.Index(5, 5), w.Map.Index(5, 10), w.Map.Index(10, 10) };
            for (int i = 0; i < 3; i++)
            {
                var training = new TrainingDefinition(i == 0 ? "melee" : i == 1 ? "archer" : "cavalry", i == 0 ? 2000 : i == 1 ? 3000 : 6000, 450, 0, 10,
                    i == 0 ? 50 : i == 1 ? 0 : 70, stats: i == 0 ? spear : i == 1 ? archer : horse, woodCost: i == 2 ? 0 : 25, goldCost: i == 0 ? 0 : i == 1 ? 45 : 60);
                w.RegisterBuildingDefinition(new BuildingDefinition(names[i], 2, 2, 150, 0, 0, 200, 600, training: training));
                w.AddUnit("villager", 1, 3000, 450, w.Map.CenterX(sites[i] - 1), w.Map.CenterY(sites[i] - 1), 10);
            }
            if (defeat) w.AddUnit("cavalry", 2, 6000, 450, 5500, 2500, stats: horse);
            var commands = new ISimulationCommand[10];
            for (int i = 0; i < 3; i++)
            {
                commands[i] = new BuildCommand(1, 1, i + 1, names[i], sites[i], new[] { i + 1 });
                commands[i + 3] = new TrainingCommand(202, 1, i + 4, sites[i] + 1, TrainingOperation.SetRally, w.Map.Index(16, 4 + i * 2));
                commands[i + 6] = new TrainingCommand(203, 1, i + 7, sites[i] + 1, TrainingOperation.Enqueue);
            }
            commands[9] = defeat ? new AttackCommand(450, 2, 1, new[] { 4 }, AttackTargetKind.Building, own)
                : new AttackCommand(450, 1, 10, new[] { 4, 5, 6 }, AttackTargetKind.Building, enemy);
            if (reverse) Array.Reverse(commands); foreach (var command in commands) w.Submit(command); return w;
        }
    }
}
