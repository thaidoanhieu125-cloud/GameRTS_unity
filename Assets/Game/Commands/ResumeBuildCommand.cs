using System.Collections.Generic;

namespace GameRTS.Commands
{
    public sealed class ResumeBuildCommand : ISimulationCommand
    {
        public int Tick { get; }
        public int PlayerId { get; }
        public int Sequence { get; }
        public int BuildingId { get; }
        public IReadOnlyList<int> BuilderIds { get; }
        public ResumeBuildCommand(int tick, int playerId, int sequence, int buildingId, IEnumerable<int> builderIds)
        {
            if (tick <= 0 || playerId <= 0 || sequence < 0 || buildingId <= 0) throw new System.ArgumentException("Invalid resume-build command.");
            Tick = tick; PlayerId = playerId; Sequence = sequence; BuildingId = buildingId;
            BuilderIds = BuildCommand.CopyIds(builderIds);
        }
    }
}
