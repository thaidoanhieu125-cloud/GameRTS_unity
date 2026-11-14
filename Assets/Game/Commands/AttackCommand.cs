using System;
using System.Collections.Generic;

namespace GameRTS.Commands
{
    public enum AttackTargetKind { Unit, Building }
    public sealed class AttackCommand : ISimulationCommand
    {
        public int Tick { get; }
        public int PlayerId { get; }
        public int Sequence { get; }
        public IReadOnlyList<int> UnitIds { get; }
        public AttackTargetKind TargetKind { get; }
        public int TargetId { get; }
        public AttackCommand(int tick, int playerId, int sequence, IEnumerable<int> ids, AttackTargetKind kind, int targetId)
        {
            if (tick <= 0 || playerId <= 0 || sequence < 0 || targetId <= 0 || !Enum.IsDefined(typeof(AttackTargetKind), kind))
                throw new ArgumentOutOfRangeException(nameof(tick));
            if (ids == null) throw new ArgumentNullException(nameof(ids));
            var sorted = new SortedSet<int>(ids);
            if (sorted.Count == 0 || sorted.Count > 1024 || sorted.Min <= 0) throw new ArgumentException("Attack needs 1..1024 positive IDs.");
            var copy = new int[sorted.Count]; sorted.CopyTo(copy); UnitIds = Array.AsReadOnly(copy);
            Tick = tick; PlayerId = playerId; Sequence = sequence; TargetKind = kind; TargetId = targetId;
        }
    }
}
