using System;

namespace GameRTS.Simulation
{
    // Integer state only. A rejected transaction changes nothing.
    public sealed class PlayerEconomyState
    {
        public int Wood { get; private set; }
        public int Food { get; private set; }
        public int Gold { get; private set; }
        public int Population { get; private set; }
        public int MaxPopulation { get; private set; }

        public PlayerEconomyState(int wood, int food, int gold, int population, int maxPopulation)
        {
            if (wood < 0 || food < 0 || gold < 0 || population < 0 || maxPopulation < population)
                throw new ArgumentOutOfRangeException(nameof(wood));
            Wood = wood; Food = food; Gold = gold; Population = population; MaxPopulation = maxPopulation;
        }

        public bool TryAddResources(int wood, int food, int gold)
        {
            if (wood < 0 || food < 0 || gold < 0 || (long)Wood + wood > int.MaxValue
                || (long)Food + food > int.MaxValue || (long)Gold + gold > int.MaxValue) return false;
            Wood += wood; Food += food; Gold += gold; return true;
        }

        public bool TrySpendResources(int wood, int food, int gold)
        {
            if (wood < 0 || food < 0 || gold < 0 || Wood < wood || Food < food || Gold < gold) return false;
            Wood -= wood; Food -= food; Gold -= gold; return true;
        }

        public bool TryReservePopulation(int amount)
        {
            if (amount < 0 || (long)Population + amount > MaxPopulation) return false;
            Population += amount; return true;
        }

        public bool TryReleasePopulation(int amount)
        {
            if (amount < 0 || amount > Population) return false;
            Population -= amount; return true;
        }

        public bool TryIncreaseMaxPopulation(int amount)
        {
            if (amount < 0 || (long)MaxPopulation + amount > int.MaxValue) return false;
            MaxPopulation += amount; return true;
        }
        // Losing housing can leave existing units above the cap. New spawns must wait.
        public bool TryDecreaseMaxPopulation(int amount)
        {
            if (amount < 0 || amount > MaxPopulation) return false;
            MaxPopulation -= amount; return true;
        }
    }
}
