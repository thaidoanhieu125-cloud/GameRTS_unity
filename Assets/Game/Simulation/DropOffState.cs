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
        internal DropOffState(int ownerId, int cell, int width, int height, ResourceAcceptance accepts)
        { Id = cell + 1; OwnerId = ownerId; Cell = cell; Width = width; Height = height; Accepts = accepts; }
        public bool AcceptsResource(ResourceType type) => (Accepts & (ResourceAcceptance)(1 << (int)type)) != 0;
    }
}
