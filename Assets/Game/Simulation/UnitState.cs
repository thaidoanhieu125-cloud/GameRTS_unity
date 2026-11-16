using System.Collections.Generic;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public sealed class UnitState
    {
        public int Id { get; }
        public int OwnerId { get; }
        public string TypeId { get; }
        public int Speed { get; }
        public int Radius { get; }
        public VillagerState Villager { get; }
        public CombatStats Stats { get; internal set; }
        internal CombatStats BaseStats { get; }
        public AnimalState Animal { get; internal set; }
        public int MaxHP => Stats.MaxHP;
        public int Armor => Stats.Armor;
        public int CurrentHP { get; internal set; }
        public bool IsAlive => CurrentHP > 0;
        public AttackTargetKind TargetKind { get; internal set; }
        public int TargetId { get; internal set; }
        public int LastAttackTick { get; internal set; } = -1;
        public int LastDamageTick { get; internal set; } = -1;
        internal int NextAttackTick, RepathAt;
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public int PreviousX { get; internal set; }
        public int PreviousY { get; internal set; }
        public int Cell { get; internal set; }
        public int GoalCell { get; internal set; }
        public bool IsMoving => NextCell >= 0 || Cell != GoalCell || PathIndex < Path.Count;
        internal int NextCell = -1, PathIndex, WaitTicks, Remainder, Carry, SegmentProgress;
        internal List<int> Path = new List<int>();

        internal UnitState(int id, int ownerId, string typeId, int speed, int radius, int cell, GridMap map, int carryCapacity, int gatherIntervalTicks, CombatStats stats = null)
        {
            Id = id; OwnerId = ownerId; TypeId = typeId; Speed = speed; Radius = radius;
            BaseStats = Stats = stats ?? CombatStats.Passive; CurrentHP = Stats.MaxHP;
            if (carryCapacity > 0) Villager = new VillagerState(carryCapacity, gatherIntervalTicks);
            Cell = GoalCell = cell;
            X = PreviousX = map.CenterX(cell); Y = PreviousY = map.CenterY(cell);
        }
    }
}
