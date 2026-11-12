using System.Collections.Generic;

namespace GameRTS.Simulation
{
    public sealed class UnitState
    {
        public int Id { get; }
        public int OwnerId { get; }
        public string TypeId { get; }
        public int Speed { get; }
        public int Radius { get; }
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public int PreviousX { get; internal set; }
        public int PreviousY { get; internal set; }
        public int Cell { get; internal set; }
        public int GoalCell { get; internal set; }
        public bool IsMoving => NextCell >= 0 || Cell != GoalCell || PathIndex < Path.Count;
        internal int NextCell = -1, PathIndex, WaitTicks, Remainder, Carry, SegmentProgress;
        internal List<int> Path = new List<int>();

        internal UnitState(int id, int ownerId, string typeId, int speed, int radius, int cell, GridMap map)
        {
            Id = id; OwnerId = ownerId; TypeId = typeId; Speed = speed; Radius = radius;
            Cell = GoalCell = cell;
            X = PreviousX = map.CenterX(cell); Y = PreviousY = map.CenterY(cell);
        }
    }
}
