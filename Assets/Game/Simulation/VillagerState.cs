namespace GameRTS.Simulation
{
    public enum VillagerActivity { Idle, GoingToResource, Gathering, GoingToDropOff, WaitingForResource, WaitingForDropOff, GoingToBuild, Building, WaitingToBuild, SearchingForResource }

    public sealed class VillagerState
    {
        public int Capacity { get; internal set; }
        internal int BaseCapacity { get; }
        public int HuntAnimalId { get; internal set; }
        public int GatherIntervalTicks { get; }
        public VillagerActivity Activity { get; internal set; }
        public int ResourceId { get; internal set; }
        public int DropOffId { get; internal set; }
        public int BuildingId { get; internal set; }
        public ResourceType CarriedType { get; internal set; }
        public int CarriedAmount { get; internal set; }
        public int WorkTicks { get; internal set; }
        public int RetryTicks { get; internal set; }

        internal VillagerState(int capacity, int gatherIntervalTicks)
        { BaseCapacity = Capacity = capacity; GatherIntervalTicks = gatherIntervalTicks; }
    }
}
