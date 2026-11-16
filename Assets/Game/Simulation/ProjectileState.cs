using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public sealed class ProjectileState
    {
        public int Id { get; }
        public int OwnerId { get; }
        public int SourceId { get; }
        public AttackTargetKind TargetKind { get; }
        public int TargetId { get; }
        public int Damage { get; }
        public UnitClass SourceClass { get; }
        public int Speed { get; }
        public int LaunchTick { get; }
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public int PreviousX { get; internal set; }
        public int PreviousY { get; internal set; }
        internal int Remainder;
        internal ProjectileState(int id, BuildingState source, UnitState target, int x, int y, int tick)
        {
            Id=id; OwnerId=source.OwnerId; SourceId=-source.Id; TargetKind=AttackTargetKind.Unit; TargetId=target.Id;
            Damage=source.Definition.TowerDamage; SourceClass=UnitClass.Generic; Speed=10000; LaunchTick=tick;
            X=PreviousX=x; Y=PreviousY=y;
        }
        internal ProjectileState(int id, UnitState source, int tick, CombatStats overrideStats = null)
        {
            Id = id; OwnerId = source.OwnerId; SourceId = source.Id; TargetKind = source.TargetKind; TargetId = source.TargetId;
            Damage = (overrideStats ?? source.Stats).AttackDamage; SourceClass = (overrideStats ?? source.Stats).Class; Speed = (overrideStats ?? source.Stats).ProjectileSpeed; LaunchTick = tick;
            X = PreviousX = source.X; Y = PreviousY = source.Y;
        }
    }
}
