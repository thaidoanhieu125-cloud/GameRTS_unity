using System;

namespace GameRTS.Simulation
{
    public enum PlacementIssue { None, UnknownBuilding, InvalidPlayer, NoBuilders, InvalidBuilder, OutsideMap, BlockedTerrain, Occupied, NoAccess, InsufficientResources, PopulationOverflow }

    // Trusted match data. Commands send an ID, never their own costs or footprint.
    public sealed class BuildingDefinition
    {
        public string TypeId { get; }
        public int Width { get; }
        public int Height { get; }
        public int WoodCost { get; }
        public int FoodCost { get; }
        public int GoldCost { get; }
        public int RequiredWork { get; }
        public int MaxHitPoints { get; }
        public int PopulationBonus { get; }
        public ResourceAcceptance Accepts { get; }
        public int Armor { get; }
        public BuildingDefinition(string typeId, int width, int height, int woodCost, int foodCost, int goldCost,
            int requiredWork, int maxHitPoints, int populationBonus = 0, ResourceAcceptance accepts = 0, int armor = 0)
        {
            if (string.IsNullOrWhiteSpace(typeId) || width < 1 || height < 1 || width > 8 || height > 8
                || woodCost < 0 || foodCost < 0 || goldCost < 0 || requiredWork < 1 || requiredWork > 1000000
                || maxHitPoints < 1 || maxHitPoints > 10000000 || populationBonus < 0 || populationBonus > 1000
                || armor < 0 || armor > 10000000 || (accepts & ~ResourceAcceptance.All) != 0) throw new ArgumentException("Invalid building definition.");
            TypeId = typeId; Width = width; Height = height; WoodCost = woodCost; FoodCost = foodCost; GoldCost = goldCost;
            RequiredWork = requiredWork; MaxHitPoints = maxHitPoints; PopulationBonus = populationBonus; Accepts = accepts; Armor = armor;
        }
    }

    public sealed class BuildingState
    {
        public int Id { get; }
        public int OwnerId { get; }
        public int Cell { get; }
        public BuildingDefinition Definition { get; }
        public int Width => Definition.Width;
        public int Height => Definition.Height;
        public int CompletedWork { get; internal set; }
        public int HitPoints { get; internal set; } = 1;
        public int MaxHitPoints => Definition.MaxHitPoints;
        public int Armor => Definition.Armor;
        public bool IsDestroyed { get; internal set; }
        public bool IsComplete => CompletedWork == Definition.RequiredWork;
        public int ProgressPermille => (int)((long)CompletedWork * 1000 / Definition.RequiredWork);
        public int ProgressPercent => ProgressPermille / 10;
        internal BuildingState(int ownerId, int cell, BuildingDefinition definition, int id = 0)
        { Id = id == 0 ? cell + 1 : id; OwnerId = ownerId; Cell = cell; Definition = definition; }
    }
}
