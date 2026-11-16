using System.Linq;
using GameRTS.Commands;
using GameRTS.Simulation;
using NUnit.Framework;

namespace GameRTS.Tests
{
    public sealed class HuntingTests
    {
        private static void Step(SimulationWorld w, int n) { for (int i = 0; i < n; i++) w.Step(); }
        private static SimulationWorld World(AnimalKind kind)
        {
            var w = new SimulationWorld(new GridMap(32, 24)); w.AddPlayer(1, 200, 0, 100, 10);
            w.AddDropOff(1, w.Map.Index(1, 1), 2, 2, ResourceAcceptance.All);
            w.AddUnit("villager", 1, 3000, 450, 5500, 5500, 10);
            w.AddAnimal(ProgressionScenario.Animal(kind), w.Map.Index(8, 5)); return w;
        }
        [Test] public void WildlifeDoesNotUsePopulation()
        {
            var w = World(AnimalKind.Deer); Assert.That(w.GetPlayerEconomy(1).Population, Is.EqualTo(1));
            Assert.That(w.GetPlayerEconomy(0), Is.Null); Assert.That(w.GetUnit(2).OwnerId, Is.Zero);
        }
        [Test] public void HuntingUsesArrowsAndDeerFlees()
        {
            var w = World(AnimalKind.Deer); var deer = w.GetUnit(2); int x = deer.X, y = deer.Y;
            w.Submit(new HuntCommand(1, 1, 1, new[] { 1 }, deer.Id)); w.Step();
            Assert.That(w.ProjectileCount, Is.EqualTo(1)); Assert.That(deer.CurrentHP, Is.EqualTo(32));
            Step(w, 15); Assert.That(deer.CurrentHP, Is.LessThan(32)); Assert.That(deer.X != x || deer.Y != y, Is.True);
        }
        [Test] public void HunterDepositsAndConservesMeat()
        {
            var w = World(AnimalKind.Deer); w.Submit(new HuntCommand(1, 1, 1, new[] { 1 }, 2));
            Step(w, 3000); var meat = w.GetResource(w.GetUnit(2).Animal.CorpseResourceId);
            Assert.That(meat, Is.Not.Null); Assert.That(meat.InitialAmount, Is.EqualTo(250));
            Assert.That(w.GetPlayerEconomy(1).Food, Is.GreaterThan(0));
            Assert.That(meat.Remaining + w.GetPlayerEconomy(1).Food + w.GetUnit(1).Villager.CarriedAmount, Is.EqualTo(250));
            Assert.That(w.GetPlayerEconomy(1).Population, Is.EqualTo(1));
        }
        [TestCase(AnimalKind.Boar)] [TestCase(AnimalKind.Elephant)]
        public void DangerousAnimalRetaliates(AnimalKind kind)
        {
            var w = World(kind); w.Submit(new HuntCommand(1, 1, 1, new[] { 1 }, 2)); Step(w, 200);
            Assert.That(w.GetUnit(1).IsAlive, Is.False); Assert.That(w.GetUnit(2).IsAlive, Is.True);
            Assert.That(w.GetPlayerEconomy(1).Population, Is.Zero);
        }
        [Test] public void MoveCancelsHunting()
        {
            var w = World(AnimalKind.Deer); w.Submit(new HuntCommand(1, 1, 1, new[] { 1 }, 2)); w.Step();
            w.Submit(new MoveCommand(2, 1, 2, new[] { 1 }, 2500, 18500)); Step(w, 200);
            Assert.That(w.GetUnit(1).Villager.HuntAnimalId, Is.Zero); Assert.That(w.GetUnit(1).TargetId, Is.Zero);
        }
        [Test] public void MeatHarvestsFasterThanBerries()
        {
            int[] harvested = new int[2];
            for (int i = 0; i < 2; i++)
            {
                var w = new SimulationWorld(new GridMap(20, 20)); w.AddPlayer(1);
                w.AddUnit("worker", 1, 3000, 450, 3500, 4500, 1000);
                w.AddUnit("soldier", 1, 3000, 450, 5500, 4500, stats: new CombatStats(100, 0, 10000));
                int cell = w.Map.Index(4, 4);
                if (i == 0) w.AddResource("berries", ResourceType.Food, 250, cell);
                else
                {
                    var deer = w.AddAnimal(ProgressionScenario.Animal(AnimalKind.Deer), cell);
                    w.Submit(new AttackCommand(1, 1, 1, new[] { 2 }, AttackTargetKind.Unit, deer.Id));
                }
                Step(w, 2); var source = w.GetResourceAt(cell);
                Assert.That(source, Is.Not.Null); w.Submit(new GatherCommand(3, 1, 2, new[] { 1 }, source.Id)); Step(w, 100);
                harvested[i] = w.GetUnit(1).Villager.CarriedAmount;
            }
            Assert.That(harvested[0], Is.EqualTo(10)); Assert.That(harvested[1], Is.EqualTo(15));
        }
        [Test] public void WildlifeAndTechAreDeterministic()
        {
            var a = ProgressionScenario.CreateWorld(); var b = ProgressionScenario.CreateWorld();
            for (int i = 0; i < 2500; i++) { a.Step(); b.Step(); Assert.That(a.ComputeChecksum(), Is.EqualTo(b.ComputeChecksum()), "Tick " + i); }
        }
        [Test] public void ExhaustedCarcassReopensCell()
        {
            var w = World(AnimalKind.Deer); w.Submit(new HuntCommand(1, 1, 1, new[] { 1 }, 2)); Step(w, 14000);
            var meat = w.GetResource(w.GetUnit(2).Animal.CorpseResourceId);
            Assert.That(meat.IsDepleted, Is.True); Assert.That(w.Map.IsWalkable(meat.Cell), Is.True);
            Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(250)); Assert.That(w.GetUnit(1).Villager.CarriedAmount, Is.Zero);
        }
        [TestCase(AiDifficulty.Easy)] [TestCase(AiDifficulty.Medium)] [TestCase(AiDifficulty.Hard)]
        public void AiCanAgeAndWinWithRealCosts(AiDifficulty level)
        {
            var w = ProgressionScenario.CreateAiWorld(AiDifficulties.Create(level));
            for (int i = 0; i < 14000 && !w.IsMatchOver; i++) w.Step();
            Assert.That(w.GetAge(2), Is.EqualTo(2)); Assert.That(w.WinnerId, Is.EqualTo(2));
            Assert.That(w.Buildings.Any(b => b.OwnerId == 2 && b.Definition.TypeId == "stable" && b.IsComplete), Is.True);
            Assert.That(w.GetPlayerEconomy(1).Food, Is.EqualTo(200));
        }
    }
}
