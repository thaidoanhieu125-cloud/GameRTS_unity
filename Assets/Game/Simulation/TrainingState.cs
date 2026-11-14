using System;
using System.Collections.Generic;

namespace GameRTS.Simulation
{
    public enum TrainingIssue { None, InvalidBuilding, NotOwner, QueueFull, PopulationLimit, NotEnoughFood, InvalidTicket, RefundOverflow, InvalidRally }
    public sealed class TrainingDefinition
    {
        public string UnitTypeId { get; }
        public int Speed { get; }
        public int Radius { get; }
        public int CarryCapacity { get; }
        public int GatherIntervalTicks { get; }
        public int FoodCost { get; }
        public int DurationTicks { get; }
        public int QueueLimit { get; }
        public TrainingDefinition(string unitTypeId, int speed, int radius, int carryCapacity, int gatherIntervalTicks,
            int foodCost = 50, int durationTicks = 200, int queueLimit = 5)
        {
            if (string.IsNullOrWhiteSpace(unitTypeId) || speed <= 0 || speed > 20000 || radius <= 0 || radius >= 500
                || carryCapacity <= 0 || carryCapacity > 1000 || gatherIntervalTicks <= 0 || gatherIntervalTicks > 1200
                || foodCost < 0 || durationTicks <= 0 || durationTicks > 1000000 || queueLimit <= 0 || queueLimit > 5)
                throw new ArgumentException("Invalid villager training definition.");
            UnitTypeId = unitTypeId; Speed = speed; Radius = radius; CarryCapacity = carryCapacity; GatherIntervalTicks = gatherIntervalTicks;
            FoodCost = foodCost; DurationTicks = durationTicks; QueueLimit = queueLimit;
        }
    }
    public sealed class TrainingTicket
    {
        public int Id { get; }
        public int PaidFood { get; }
        public int ProgressTicks { get; internal set; }
        internal TrainingTicket(int id, int food) { Id = id; PaidFood = food; }
    }
    public sealed class TrainingState
    {
        internal readonly List<TrainingTicket> Jobs = new List<TrainingTicket>();
        public int BuildingId { get; }
        public int OwnerId { get; }
        public TrainingDefinition Definition { get; }
        public IReadOnlyList<TrainingTicket> Queue { get; }
        public int RallyCell { get; internal set; } = -1;
        public int RallyResourceId { get; internal set; }
        public int ProgressPercent => Jobs.Count == 0 ? 0 : (int)((long)Jobs[0].ProgressTicks * 100 / Definition.DurationTicks);
        public bool WaitingForSpawn { get; internal set; }
        internal TrainingState(int buildingId, int ownerId, TrainingDefinition definition)
        { BuildingId = buildingId; OwnerId = ownerId; Definition = definition; Queue = Jobs.AsReadOnly(); }
    }
}
