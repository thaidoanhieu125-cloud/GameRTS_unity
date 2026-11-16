namespace GameRTS.Simulation
{
    public enum ResourceType { Wood, Food, Gold }

    public sealed class ResourceNodeState
    {
        public int Id { get; }
        public string TypeId { get; }
        public ResourceType ResourceType { get; }
        public int Cell { get; }
        public int InitialAmount { get; }
        public int GatherRatePercent { get; }
        public int Remaining { get; internal set; }
        public bool IsDepleted => Remaining == 0;

        internal ResourceNodeState(int cell, string typeId, ResourceType resourceType, int amount, int id = 0, int gatherRatePercent = 100)
        {
            GatherRatePercent = gatherRatePercent; Id = id == 0 ? cell + 1 : id; Cell = cell; TypeId = typeId;
            ResourceType = resourceType; InitialAmount = Remaining = amount;
        }
    }
}
