using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public static class ProgressionScenario
    {
        public struct CellPosition { public int x, y; public CellPosition(int xx, int yy) { x = xx; y = yy; } }
        public static CellPosition[] AnimalCells()
        {
            var first = new[] { new CellPosition(14, 22), new CellPosition(16, 21), new CellPosition(9, 18), new CellPosition(28, 26) };
            var all = new CellPosition[8];
            for (int i = 0; i < 4; i++) { all[i] = first[i]; all[i + 4] = new CellPosition(63 - first[i].x, 63 - first[i].y); }
            return all;
        }
        public static AnimalDefinition Animal(AnimalKind kind) => kind == AnimalKind.Deer
            ? new AnimalDefinition("deer", kind, 250, 4000, new CombatStats(32)) : kind == AnimalKind.Boar
            ? new AnimalDefinition("boar", kind, 400, 4000, new CombatStats(200, 0, 12, 15))
            : new AnimalDefinition("elephant", kind, 600, 3500, new CombatStats(450, 2, 24));

        public static SimulationWorld CreateAiWorld(AiSettings settings = null)
        {
            var w = AiScenario.CreateWorld(settings); w.ConfigureTechnologies(ResearchDefinition.Defaults());
            var cells = AnimalCells();
            for (int i = 0; i < cells.Length; i++) w.AddAnimal(Animal(i % 4 < 2 ? AnimalKind.Deer : i % 4 == 2 ? AnimalKind.Boar : AnimalKind.Elephant), w.Map.Index(cells[i].x, cells[i].y));
            return w;
        }
        public static SimulationWorld CreateWorld()
        {
            var w = new SimulationWorld(new GridMap(40, 32)); w.AddPlayer(1, 2000, 5000, 2000, 100); w.AddPlayer(2);
            int tc = w.AddDropOff(1, w.Map.Index(2, 2), 2, 2, ResourceAcceptance.All).Id;
            w.AddTrainingCenter(tc, new TrainingDefinition("villager", 3000, 450, 10, 10));
            for (int i = 0; i < 4; i++) w.AddUnit("villager", 1, 3000, 450, 5500 + i * 2000, 6500, 10, stats: new CombatStats(40, 0, 3, unitClass: UnitClass.Villager));
            w.AddUnit("melee", 1, 2000, 450, 15500, 11500, stats: new CombatStats(120, 2, 12, unitClass: UnitClass.Spearman));
            w.AddUnit("archer", 1, 3000, 450, 15500, 13500, stats: new CombatStats(45, 0, 18, 12, 6000, 10000, unitClass: UnitClass.Archer));
            w.AddUnit("cavalry", 1, 6000, 450, 17500, 11500, stats: new CombatStats(180, 1, 16, unitClass: UnitClass.Cavalry));
            var deer = w.AddAnimal(Animal(AnimalKind.Deer), w.Map.Index(5, 10));
            var boar = w.AddAnimal(Animal(AnimalKind.Boar), w.Map.Index(20, 12));
            w.AddAnimal(Animal(AnimalKind.Elephant), w.Map.Index(30, 20));
            w.AddResource("tree", ResourceType.Wood, 300, w.Map.Index(8, 8));
            w.RegisterBuildingDefinition(new BuildingDefinition("lumber_camp", 2, 2, 100, 0, 0, 200, 400, accepts: ResourceAcceptance.Wood));
            w.RegisterBuildingDefinition(new BuildingDefinition("barracks", 2, 2, 150, 0, 0, 200, 600));
            w.RegisterBuildingDefinition(new BuildingDefinition("stable", 2, 2, 150, 0, 0, 200, 600));
            w.ConfigureTechnologies(ResearchDefinition.Defaults());
            int camp = w.Map.Index(6, 2), barracks = w.Map.Index(11, 2);
            w.Submit(new ResearchCommand(1, 1, 1, tc, TechnologyId.BronzeAge));
            w.Submit(new HuntCommand(2, 1, 2, new[] { 1 }, deer.Id));
            w.Submit(new BuildCommand(1, 1, 3, "lumber_camp", camp, new[] { 2 }));
            w.Submit(new BuildCommand(1, 1, 4, "barracks", barracks, new[] { 3 }));
            w.Submit(new ResearchCommand(300, 1, 5, camp + 1, TechnologyId.DoubleAxe));
            w.Submit(new AttackCommand(450, 1, 6, new[] { 5, 7 }, AttackTargetKind.Unit, boar.Id));
            w.Submit(new GatherCommand(800, 1, 7, new[] { 2 }, w.Map.Index(8, 8) + 1));
            w.Submit(new ResearchCommand(1300, 1, 8, tc, TechnologyId.LeatherBag));
            w.Submit(new ResearchCommand(1300, 1, 9, barracks + 1, TechnologyId.Fletching));
            w.Submit(new ResearchCommand(1800, 1, 10, barracks + 1, TechnologyId.ScaleArmor));
            w.Submit(new BuildCommand(1500, 1, 11, "stable", w.Map.Index(16, 2), new[] { 3 }));
            return w;
        }
    }
}
