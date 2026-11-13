using System;
using System.Collections.Generic;

namespace GameRTS.Commands
{
    // Coordinates are integer thousandths of a world unit. No Unity types cross this boundary.
    public sealed class MoveCommand : ISimulationCommand
    {
        private readonly int[] unitIds;
        private readonly IReadOnlyList<int> readOnlyIds;
        public int Tick { get; }
        public int PlayerId { get; }
        public int Sequence { get; }
        public int TargetX { get; }
        public int TargetY { get; }
        public IReadOnlyList<int> UnitIds => readOnlyIds;

        public MoveCommand(int tick, int playerId, int sequence, IEnumerable<int> ids, int targetX, int targetY)
        {
            if (tick <= 0 || playerId <= 0 || sequence < 0)
                throw new ArgumentOutOfRangeException(nameof(tick));
            if (ids == null) throw new ArgumentNullException(nameof(ids));
            var sorted = new SortedSet<int>(ids);
            if (sorted.Count == 0 || sorted.Count > 1024 || sorted.Min <= 0)
                throw new ArgumentException("A move needs 1 to 1024 positive unit IDs.", nameof(ids));
            unitIds = new int[sorted.Count];
            sorted.CopyTo(unitIds);
            readOnlyIds = Array.AsReadOnly(unitIds);
            Tick = tick;
            PlayerId = playerId;
            Sequence = sequence;
            TargetX = targetX;
            TargetY = targetY;
        }
    }
}
