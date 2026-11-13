using System;

namespace GameRTS.Commands
{
    public enum EconomyDebugOperation { GrantResources, SpendResources, IncreasePopulationLimit }

    // Prototype controls only. A world must explicitly enable these commands.
    public sealed class EconomyDebugCommand : ISimulationCommand
    {
        public int Tick { get; }
        public int PlayerId { get; }
        public int Sequence { get; }
        public EconomyDebugOperation Operation { get; }
        public int Amount { get; }

        public EconomyDebugCommand(int tick, int playerId, int sequence, EconomyDebugOperation operation, int amount)
        {
            if (tick <= 0 || playerId <= 0 || sequence < 0 || amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (!Enum.IsDefined(typeof(EconomyDebugOperation), operation))
                throw new ArgumentOutOfRangeException(nameof(operation));
            Tick = tick; PlayerId = playerId; Sequence = sequence; Operation = operation; Amount = amount;
        }
    }
}
