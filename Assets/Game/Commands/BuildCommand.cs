using System;
using System.Collections.Generic;

namespace GameRTS.Commands
{
    public sealed class BuildCommand : ISimulationCommand
    {
        public int Tick { get; }
        public int PlayerId { get; }
        public int Sequence { get; }
        public string TypeId { get; }
        public int Cell { get; }
        public IReadOnlyList<int> BuilderIds { get; }
        public BuildCommand(int tick, int playerId, int sequence, string typeId, int cell, IEnumerable<int> builderIds)
        {
            if (tick <= 0 || playerId <= 0 || sequence < 0 || string.IsNullOrWhiteSpace(typeId)) throw new ArgumentException("Invalid build command.");
            Tick = tick; PlayerId = playerId; Sequence = sequence; TypeId = typeId; Cell = cell;
            BuilderIds = CopyIds(builderIds);
        }
        internal static IReadOnlyList<int> CopyIds(IEnumerable<int> ids)
        {
            if (ids == null) throw new ArgumentNullException(nameof(ids));
            var sorted = new SortedSet<int>(ids);
            if (sorted.Count == 0 || sorted.Count > 1024 || sorted.Min <= 0) throw new ArgumentException("Build needs 1 to 1024 positive builder IDs.");
            var copy = new int[sorted.Count]; sorted.CopyTo(copy); return Array.AsReadOnly(copy);
        }
    }
}
