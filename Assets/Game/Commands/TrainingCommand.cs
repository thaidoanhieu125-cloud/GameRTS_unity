using System;

namespace GameRTS.Commands
{
    public enum TrainingOperation { Enqueue, Cancel, SetRally }
    public sealed class TrainingCommand : ISimulationCommand
    {
        public int Tick { get; }
        public int PlayerId { get; }
        public int Sequence { get; }
        public int BuildingId { get; }
        public TrainingOperation Operation { get; }
        public int Target { get; }
        public TrainingCommand(int tick, int playerId, int sequence, int buildingId, TrainingOperation operation, int target = 0)
        {
            if (tick <= 0 || playerId <= 0 || sequence < 0 || buildingId <= 0 || !Enum.IsDefined(typeof(TrainingOperation), operation)
                || (operation == TrainingOperation.Cancel && target <= 0) || (operation == TrainingOperation.SetRally && target < 0))
                throw new ArgumentOutOfRangeException(nameof(tick));
            Tick = tick; PlayerId = playerId; Sequence = sequence; BuildingId = buildingId; Operation = operation; Target = target;
        }
    }
}
