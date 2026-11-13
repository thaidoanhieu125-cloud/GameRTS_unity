using System;
using System.Collections.Generic;

namespace GameRTS.Commands
{
    public sealed class GatherCommand : ISimulationCommand
    {
        public int Tick { get; }
        public int PlayerId { get; }
        public int Sequence { get; }
        public int NodeId { get; }
        public IReadOnlyList<int> UnitIds { get; }

        public GatherCommand(int tick, int playerId, int sequence, IEnumerable<int> ids, int nodeId)
        {
            if (tick <= 0 || playerId <= 0 || sequence < 0 || nodeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(tick));
            if (ids == null) throw new ArgumentNullException(nameof(ids));
            var sorted = new SortedSet<int>(ids);
            if (sorted.Count == 0 || sorted.Count > 1024 || sorted.Min <= 0)
                throw new ArgumentException("Gather needs 1 to 1024 positive unit IDs.", nameof(ids));
            var copy = new int[sorted.Count]; sorted.CopyTo(copy); UnitIds = Array.AsReadOnly(copy);
            Tick = tick; PlayerId = playerId; Sequence = sequence; NodeId = nodeId;
        }
    }
}
