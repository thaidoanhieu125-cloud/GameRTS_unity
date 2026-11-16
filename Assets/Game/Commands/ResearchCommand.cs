using System;

namespace GameRTS.Commands
{
    public enum TechnologyId { DoubleAxe, LeatherBag, Fletching, ScaleArmor, BronzeAge }
    public enum ResearchOperation { Start, Cancel }

    public sealed class ResearchCommand : ISimulationCommand
    {
        public int Tick { get; }
        public int PlayerId { get; }
        public int Sequence { get; }
        public int BuildingId { get; }
        public TechnologyId Technology { get; }
        public ResearchOperation Operation { get; }

        public ResearchCommand(int tick, int playerId, int sequence, int buildingId, TechnologyId technology,
            ResearchOperation operation = ResearchOperation.Start)
        {
            if (tick <= 0 || playerId <= 0 || sequence < 0 || buildingId <= 0
                || !Enum.IsDefined(typeof(TechnologyId), technology) || !Enum.IsDefined(typeof(ResearchOperation), operation))
                throw new ArgumentOutOfRangeException(nameof(tick));
            Tick = tick; PlayerId = playerId; Sequence = sequence; BuildingId = buildingId;
            Technology = technology; Operation = operation;
        }
    }
}
