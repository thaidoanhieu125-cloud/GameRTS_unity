using System;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public enum ResearchIssue { None, Disabled, InvalidBuilding, NotOwner, RequiresAge, Busy, AlreadyDone, InsufficientResources, InvalidJob, RefundOverflow }

    public sealed class ResearchDefinition
    {
        public TechnologyId Id { get; }
        public string BuildingType { get; }
        public int RequiredAge { get; }
        public int Wood { get; }
        public int Food { get; }
        public int Gold { get; }
        public int Duration { get; }

        public ResearchDefinition(TechnologyId id, string buildingType, int age, int wood, int food, int gold, int duration)
        {
            if (!Enum.IsDefined(typeof(TechnologyId), id) || string.IsNullOrWhiteSpace(buildingType)
                || age < 1 || age > 2 || wood < 0 || food < 0 || gold < 0 || duration < 1 || duration > 1000000)
                throw new ArgumentException("Invalid research definition.");
            Id = id; BuildingType = buildingType; RequiredAge = age;
            Wood = wood; Food = food; Gold = gold; Duration = duration;
        }

        // Prototype prices for technologies; Bronze Age is the requested 500 food / 60 seconds.
        public static ResearchDefinition[] Defaults() => new[]
        {
            new ResearchDefinition(TechnologyId.DoubleAxe, "lumber_camp", 1, 50, 100, 0, 400),
            new ResearchDefinition(TechnologyId.LeatherBag, "town_center", 2, 75, 150, 0, 600),
            new ResearchDefinition(TechnologyId.Fletching, "barracks", 2, 0, 100, 100, 400),
            new ResearchDefinition(TechnologyId.ScaleArmor, "barracks", 2, 0, 100, 100, 400),
            new ResearchDefinition(TechnologyId.BronzeAge, "town_center", 1, 0, 500, 0, 1200)
        };
    }

    public sealed class ResearchJob
    {
        public int BuildingId { get; }
        public int PlayerId { get; }
        public ResearchDefinition Definition { get; }
        public int Progress { get; internal set; }
        public int Percent => Progress * 100 / Definition.Duration;
        internal ResearchJob(int buildingId, int playerId, ResearchDefinition definition)
        { BuildingId = buildingId; PlayerId = playerId; Definition = definition; }
    }
}
