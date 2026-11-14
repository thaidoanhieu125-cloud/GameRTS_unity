using System;

namespace GameRTS.Simulation
{
    [Flags]
    public enum ResourceAcceptance { Wood = 1, Food = 2, Gold = 4, All = Wood | Food | Gold }

    public sealed class DropOffState
    {
        public int Id { get; }
        public int OwnerId { get; }
        public int Cell { get; }
        public int Width { get; }
        public int Height { get; }
        public ResourceAcceptance Accepts { get; }
        private int currentHP;
        internal BuildingState Building;
        public int MaxHP { get; }
        public int Armor { get; }
        public int CurrentHP { get => Building != null ? Building.HitPoints : currentHP; internal set { if (Building != null) Building.HitPoints = value; else currentHP = value; } }
        public bool IsDestroyed => CurrentHP <= 0;
        internal DropOffState(int ownerId, int cell, int width, int height, ResourceAcceptance accepts, int maxHP = 1000, int armor = 2, int id = 0)
        { Id = id == 0 ? cell + 1 : id; OwnerId = ownerId; Cell = cell; Width = width; Height = height; Accepts = accepts; MaxHP = currentHP = maxHP; Armor = armor; }
        public bool AcceptsResource(ResourceType type) => (Accepts & (ResourceAcceptance)(1 << (int)type)) != 0;
    }
}
