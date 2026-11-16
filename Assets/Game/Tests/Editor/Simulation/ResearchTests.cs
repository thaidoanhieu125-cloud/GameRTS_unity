using System.Linq;
using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;

namespace GameRTS.Tests
{
    public sealed class ResearchTests
    {
        private static void Step(SimulationWorld w, int count) { for (int i = 0; i < count; i++) w.Step(); }
        private static int Town(SimulationWorld w) => w.DropOffs.First(d => d.OwnerId == 1).Id;
        private static SimulationWorld World()
        {
            var w = new SimulationWorld(new GridMap(28, 24)); w.AddPlayer(1, 1000, 3000, 1000, 100); w.AddPlayer(2);
            int tc = w.AddDropOff(1, w.Map.Index(1, 1), 2, 2, ResourceAcceptance.All).Id;
            w.AddTrainingCenter(tc, new TrainingDefinition("villager", 3000, 450, 10, 10, durationTicks: 2));
            w.AddUnit("villager", 1, 3000, 450, 4500, 2500, 10);
            w.AddUnit("archer", 1, 3000, 450, 4500, 4500, stats: new CombatStats(45, 0, 18, 12, 6000, 10000, unitClass: UnitClass.Archer));
            w.AddUnit("spear", 1, 3000, 450, 4500, 6500, stats: new CombatStats(120, 2, 12, unitClass: UnitClass.Spearman));
            w.AddUnit("horse", 1, 3000, 450, 4500, 8500, stats: new CombatStats(180, 1, 16, unitClass: UnitClass.Cavalry));
            w.AddUnit("enemy", 2, 3000, 450, 7500, 1500, stats: new CombatStats(100, 0, 10000));
            w.RegisterBuildingDefinition(new BuildingDefinition("lumber_camp", 1, 1, 100, 0, 0, 1, 400));
            w.RegisterBuildingDefinition(new BuildingDefinition("barracks", 1, 1, 150, 0, 0, 1, 600));
            w.RegisterBuildingDefinition(new BuildingDefinition("stable", 2, 2, 150, 0, 0, 1, 600));
            w.RegisterBuildingDefinition(new BuildingDefinition("watch_tower", 2, 2, 125, 0, 0, 1, 600));
            w.ConfigureTechnologies(ResearchDefinition.Defaults().Select(d => new ResearchDefinition(d.Id, d.BuildingType, d.RequiredAge, d.Wood, d.Food, d.Gold, d.Id == TechnologyId.BronzeAge ? 1200 : 4)));
            return w;
        }
        private static void Age(SimulationWorld w, int seq = 1)
        { w.Submit(new ResearchCommand(w.Tick + 1, 1, seq, Town(w), TechnologyId.BronzeAge)); Step(w, 1200); }
        private static int Build(SimulationWorld w, string type, int x = 6, int y = 1, int seq = 100)
        {
            w.Submit(new BuildCommand(w.Tick + 1, 1, seq, type, w.Map.Index(x, y), new[] { 1 })); Step(w, 70);
            return w.Buildings.Last().Id;
        }
        [Test] public void AgeCostsAndSixtySeconds()
        {
            var w = World(); w.Submit(new ResearchCommand(1, 1, 1, Town(w), TechnologyId.BronzeAge)); w.Step();
            Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(2500)); Assert.That(w.GetAge(1), Is.EqualTo(1));
            Step(w, 1198); Assert.That(w.GetAge(1), Is.EqualTo(1)); w.Step(); Assert.That(w.GetAge(1), Is.EqualTo(2));
            Assert.That(w.GetAge(2), Is.EqualTo(1));
            w.Submit(new ResearchCommand(w.Tick + 1, 1, 2, Town(w), TechnologyId.BronzeAge)); w.Step();
            Assert.That(w.LastResearchIssue, Is.EqualTo(ResearchIssue.AlreadyDone)); Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(2500));
        }
        [Test] public void AgeLocksBothBuildings()
        {
            var w = World();
            foreach (string type in new[] { "stable", "watch_tower" }) Assert.That(w.CheckBuildingPlacement(1, type, w.Map.Index(14, 2), new[] { 1 }), Is.EqualTo(PlacementIssue.RequiresAge));
            Age(w);
            foreach (string type in new[] { "stable", "watch_tower" }) Assert.That(w.CheckBuildingPlacement(1, type, w.Map.Index(14, 2), new[] { 1 }), Is.EqualTo(PlacementIssue.None));
        }
        [Test] public void ResearchAndTrainingAreExclusive()
        {
            var w = World(); w.Submit(new ResearchCommand(1, 1, 1, Town(w), TechnologyId.BronzeAge));
            w.Submit(new TrainingCommand(1, 1, 2, Town(w), TrainingOperation.Enqueue)); w.Step();
            Assert.That(w.LastTrainingIssue, Is.EqualTo(TrainingIssue.ResearchBusy)); Assert.That(w.GetTraining(Town(w)).Queue.Count, Is.Zero);
            w.Submit(new ResearchCommand(2, 1, 3, Town(w), TechnologyId.BronzeAge, ResearchOperation.Cancel)); w.Step();
            Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(3000));
            w.Submit(new TrainingCommand(3, 1, 4, Town(w), TrainingOperation.Enqueue));
            w.Submit(new ResearchCommand(3, 1, 5, Town(w), TechnologyId.BronzeAge)); w.Step();
            Assert.That(w.LastResearchIssue, Is.EqualTo(ResearchIssue.Busy));
        }
        [Test] public void CancellationCannotRefundTwice()
        {
            var w = World(); w.Submit(new ResearchCommand(1, 1, 1, Town(w), TechnologyId.BronzeAge)); w.Step();
            w.Submit(new ResearchCommand(2, 2, 1, Town(w), TechnologyId.BronzeAge, ResearchOperation.Cancel)); w.Step();
            Assert.That(w.LastResearchIssue, Is.EqualTo(ResearchIssue.NotOwner));
            w.Submit(new ResearchCommand(3, 1, 2, Town(w), TechnologyId.BronzeAge, ResearchOperation.Cancel)); w.Step();
            w.Submit(new ResearchCommand(4, 1, 3, Town(w), TechnologyId.BronzeAge, ResearchOperation.Cancel)); w.Step();
            Assert.That(w.LastResearchIssue, Is.EqualTo(ResearchIssue.InvalidJob)); Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(3000));
        }
        [Test] public void AdvancedTechRequiresAge()
        {
            var w = World(); int b = Build(w, "barracks");
            Assert.That(w.CheckResearch(1, b, TechnologyId.Fletching), Is.EqualTo(ResearchIssue.RequiresAge));
            Assert.That(w.CheckResearch(2, b, TechnologyId.Fletching), Is.EqualTo(ResearchIssue.NotOwner));
            Assert.That(w.CheckResearch(1, Town(w), TechnologyId.DoubleAxe), Is.EqualTo(ResearchIssue.InvalidBuilding));
        }
        [Test] public void BonusesApplyToExistingAndNewUnits()
        {
            var w = World(); int b = Build(w, "barracks"); Age(w);
            w.Submit(new ResearchCommand(w.Tick + 1, 1, 2, Town(w), TechnologyId.LeatherBag));
            w.Submit(new ResearchCommand(w.Tick + 1, 1, 3, b, TechnologyId.Fletching)); Step(w, 4);
            Assert.That(w.GetUnit(1).Villager.Capacity, Is.EqualTo(15)); Assert.That(w.GetUnit(2).Stats.AttackDamage, Is.EqualTo(19));
            Assert.That(w.GetUnit(2).Stats.AttackRange, Is.EqualTo(7000)); Assert.That(w.GetUnit(3).Armor, Is.EqualTo(2));
            w.Submit(new ResearchCommand(w.Tick + 1, 1, 4, b, TechnologyId.ScaleArmor)); Step(w, 4);
            Assert.That(w.GetUnit(3).Armor, Is.EqualTo(3)); Assert.That(w.GetUnit(4).Armor, Is.EqualTo(2)); Assert.That(w.GetUnit(2).Armor, Is.Zero);
            w.Submit(new TrainingCommand(w.Tick + 1, 1, 5, Town(w), TrainingOperation.Enqueue)); Step(w, 2);
            Assert.That(w.Units.Last().Villager.Capacity, Is.EqualTo(15));
        }
        [Test] public void DestroyedResearchDoesNotCompleteOrRefund()
        {
            var w = World(); int camp = Build(w, "lumber_camp");
            w.Submit(new ResearchCommand(w.Tick + 1, 1, 1, camp, TechnologyId.DoubleAxe));
            w.Submit(new AttackCommand(w.Tick + 1, 2, 1, new[] { 5 }, AttackTargetKind.Building, camp)); Step(w, 12);
            Assert.That(w.GetBuilding(camp).IsDestroyed, Is.True); Assert.That(w.HasTechnology(1, TechnologyId.DoubleAxe), Is.False);
            Assert.That(w.GetResearch(camp), Is.Null); Assert.That(w.GetPlayerEconomy(1).Wood, Is.EqualTo(850));
        }
        [Test] public void ProgressionReplayCompletesTechnologies()
        {
            var w = ProgressionScenario.CreateWorld(); Step(w, 3000);
            foreach (TechnologyId id in System.Enum.GetValues(typeof(TechnologyId))) Assert.That(w.HasTechnology(1, id), Is.True, id.ToString());
            Assert.That(w.Buildings.Any(b => b.Definition.TypeId == "stable" && b.IsComplete), Is.True);
            Assert.That(w.GetUnit(8).IsAlive, Is.False); Assert.That(w.GetUnit(9).IsAlive, Is.False);
        }
        [Test] public void AxeGivesExactlyTwentyPercent()
        {
            var worlds = new SimulationWorld[2];
            for (int i = 0; i < 2; i++)
            {
                var w = worlds[i] = new SimulationWorld(new GridMap(20, 20)); w.AddPlayer(1, 1000, 1000, 1000, 10);
                w.AddUnit("worker", 1, 3000, 450, 3500, 4500, 1000);
                w.AddUnit("builder", 1, 3000, 450, 1500, 1500, 10);
                w.AddResource("tree", ResourceType.Wood, 1000, w.Map.Index(4, 4));
                w.RegisterBuildingDefinition(new BuildingDefinition("lumber_camp", 1, 1, 0, 0, 0, 1, 400));
                w.ConfigureTechnologies(new[] { new ResearchDefinition(TechnologyId.DoubleAxe, "lumber_camp", 1, 0, 0, 0, 1) });
                int cell = w.Map.Index(1, 2); w.Submit(new BuildCommand(1, 1, 1, "lumber_camp", cell, new[] { 2 })); Step(w, 40);
                if (i == 1) w.Submit(new ResearchCommand(41, 1, 2, cell + 1, TechnologyId.DoubleAxe)); w.Step();
                w.Submit(new GatherCommand(42, 1, 3, new[] { 1 }, w.Map.Index(4, 4) + 1)); Step(w, 100);
            }
            Assert.That(worlds[0].GetUnit(1).Villager.CarriedAmount, Is.EqualTo(10));
            Assert.That(worlds[1].GetUnit(1).Villager.CarriedAmount, Is.EqualTo(12));
        }
        [Test] public void CostsAreAtomicAndConcurrentCopyIsRejected()
        {
            var w = new SimulationWorld(new GridMap(20, 20)); w.AddPlayer(1, 50, 99, 0, 10);
            w.AddDropOff(1, w.Map.Index(1, 1), 2, 2, ResourceAcceptance.All);
            w.AddUnit("worker", 1, 3000, 450, 4500, 4500, 10);
            w.RegisterBuildingDefinition(new BuildingDefinition("lumber_camp", 1, 1, 0, 0, 0, 1, 400));
            w.ConfigureTechnologies(ResearchDefinition.Defaults()); int camp = Build(w, "lumber_camp");
            w.Submit(new ResearchCommand(w.Tick + 1, 1, 1, camp, TechnologyId.DoubleAxe)); w.Step();
            Assert.That(w.LastResearchIssue, Is.EqualTo(ResearchIssue.InsufficientResources));
            Assert.That(w.GetPlayerEconomy(1).Wood, Is.EqualTo(50)); Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(99));
            var other = World(); int first = Build(other, "lumber_camp"); int second = Build(other, "lumber_camp", 10, 1, 101);
            other.Submit(new ResearchCommand(other.Tick + 1, 1, 1, first, TechnologyId.DoubleAxe));
            other.Submit(new ResearchCommand(other.Tick + 1, 1, 2, second, TechnologyId.DoubleAxe)); other.Step();
            Assert.That(other.LastResearchIssue, Is.EqualTo(ResearchIssue.Busy)); Assert.That(other.GetResearch(second), Is.Null);
        }
        [Test] public void PendingResearchAndHuntAffectChecksum()
        {
            var a = World(); var b = World();
            a.Submit(new ResearchCommand(1, 1, 1, Town(a), TechnologyId.BronzeAge));
            b.Submit(new ResearchCommand(1, 1, 1, Town(b), TechnologyId.LeatherBag));
            Assert.That(a.ComputeChecksum(), Is.Not.EqualTo(b.ComputeChecksum()));
            a = World(); b = World();
            a.Submit(new HuntCommand(1, 1, 1, new[] { 1 }, 5)); b.Submit(new HuntCommand(1, 1, 1, new[] { 1 }, 4));
            Assert.That(a.ComputeChecksum(), Is.Not.EqualTo(b.ComputeChecksum()));
        }
    }
}
